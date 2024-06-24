using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ExcelDataReader;
using FileUploadApp.CommonLayer.Model;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Mysqlx.Resultset;

namespace FileUploadApp.DataAccessLayer
{
    public class UploadFileDL : IUploadFileDL
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlConnection _mySqlConnection;

        public UploadFileDL(IConfiguration configuration)
        {
            _configuration = configuration;
            _mySqlConnection = new MySqlConnection(_configuration["ConnectionStrings:MySqlDBConnectionString"]);
        }

        protected async Task<MySqlCommand> GetMySqlCommandForBatch(String sCommand, int rows, int cols, MySqlConnection sqlConnection)
        {
            
            List<string> rowParam = new List<string>();

            for (int i = 0; i < rows; ++i)
            {
                List<string> col = new List<string>();
                
                for(int j = 0; j < cols; j++)
                {
                    col.Add($"@r{i}c{j}");
                }
                String sRow = "(" + String.Join(",", col) + ")";                                            
                rowParam.Add(sRow);
            }

            sCommand += String.Join(",",rowParam) + ";";             

            MySqlCommand cmd = new MySqlCommand(sCommand, sqlConnection);
            for (int i = 0; i < rows; ++i)
            {                             
                for (int j = 0; j < cols; ++j)
                {           
                    cmd.Parameters.AddWithValue($"@r{i}c{j}", "");
                }
            } 

            await cmd.PrepareAsync();
            return cmd;
        }

        protected void UpdateParamsOfCommandUsers(MySqlCommand cmd, DataTable dataTable, int iFrom, int iTo)
        {
            for (int i = iFrom; i < iTo; ++i)
            {
                var row = dataTable.Rows[i];                                        
                cmd.Parameters[(i-iFrom) * 15 + 0].Value = row["UserId"];
                cmd.Parameters[(i-iFrom) * 15 + 1].Value = row["EmailId"];
                cmd.Parameters[(i-iFrom) * 15 + 2].Value = row["Name"];
                cmd.Parameters[(i-iFrom) * 15 + 3].Value = row["Country"];
                cmd.Parameters[(i-iFrom) * 15 + 4].Value = row["State"];
                cmd.Parameters[(i-iFrom) * 15 + 5].Value = row["City"];
                cmd.Parameters[(i-iFrom) * 15 + 6].Value = row["TelephoneNumber"];
                cmd.Parameters[(i-iFrom) * 15 + 7].Value = row["AddressLine1"];
                cmd.Parameters[(i-iFrom) * 15 + 8].Value = row["AddressLine2"];
                cmd.Parameters[(i-iFrom) * 15 + 9].Value = row["DateOfBirth"];
                cmd.Parameters[(i-iFrom) * 15 + 10].Value = row["GrossSalaryFY2019_20"];
                cmd.Parameters[(i-iFrom) * 15 + 11].Value = row["GrossSalaryFY2020_21"];
                cmd.Parameters[(i-iFrom) * 15 + 12].Value = row["GrossSalaryFY2021_22"];
                cmd.Parameters[(i-iFrom) * 15 + 13].Value = row["GrossSalaryFY2022_23"];
                cmd.Parameters[(i-iFrom) * 15 + 14].Value = row["GrossSalaryFY2023_24"];
                
            } 
        }
        public async Task<UploadFileResponse> UploadCsvFile(UploadFileRequest request, string path)
        {
            UploadFileResponse response = new UploadFileResponse();
            response.IsSuccess = true;
            response.Message = "Successfully";

            try
            {
                if (_mySqlConnection.State != ConnectionState.Open)
                {
                    await _mySqlConnection.OpenAsync();
                }

                if (request.File.FileName.ToLower().EndsWith(".csv"))
                {
                    using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                        using (IExcelDataReader reader = ExcelReaderFactory.CreateCsvReader(stream))
                        {
                            DataSet dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
                            {
                                UseColumnDataType = false,
                                ConfigureDataTable = tableReader => new ExcelDataTableConfiguration
                                {
                                    UseHeaderRow = true
                                }
                            });

                            var dataTable = dataSet.Tables[0];
                            Stopwatch stopwatch = Stopwatch.StartNew();

                            // Begin transaction for batch insert
                            using (var transaction = await _mySqlConnection.BeginTransactionAsync())
                            {
                                try
                                {
                                    
                                    int BATCH_SIZE = 50;
                                    int iFrom = 0;
                                    int iTo = BATCH_SIZE;
                                    if(iTo > dataTable.Rows.Count)
                                    {
                                        iTo = dataTable.Rows.Count;
                                    }

                                    

                                    String sCommand = @"INSERT INTO userrecords 
                                            (UserId, EmailId, Name, Country, State, City, TelephoneNumber, AddressLine1, AddressLine2, 
                                            DateOfBirth, GrossSalaryFY2019_20, GrossSalaryFY2020_21, GrossSalaryFY2021_22, 
                                            GrossSalaryFY2022_23, GrossSalaryFY2023_24)
                                            VALUES ";

                                    MySqlCommand cmd = await GetMySqlCommandForBatch(sCommand, BATCH_SIZE, 15, _mySqlConnection);                                    
                                                                        
                                    while(iFrom < dataTable.Rows.Count - (dataTable.Rows.Count % BATCH_SIZE))
                                    {
                                        
                                        UpdateParamsOfCommandUsers(cmd, dataTable, iFrom, iTo);
                                                                                               
                                        await cmd.ExecuteNonQueryAsync();

                                        iFrom += BATCH_SIZE;
                                        iTo += BATCH_SIZE;                                        
                                    }

                                    if(dataTable.Rows.Count % BATCH_SIZE != 0)
                                    {
                                        if(iTo > dataTable.Rows.Count)
                                        {
                                            iTo = dataTable.Rows.Count;
                                        }
                                        cmd = await GetMySqlCommandForBatch(sCommand,  dataTable.Rows.Count % BATCH_SIZE, 15, _mySqlConnection);  
                                        UpdateParamsOfCommandUsers(cmd, dataTable, iFrom, iTo);
                                                                                               
                                        await cmd.ExecuteNonQueryAsync();
                                    }

                                    await transaction.CommitAsync(); // Commit transaction if successful
                                    response.IsSuccess = true;
                                    response.Message = "CSV file processed successfully";
                                }
                                catch (Exception ex)
                                {
                                    await transaction.RollbackAsync(); // Rollback transaction on error
                                    response.IsSuccess = false;
                                    response.Message = $"An error occurred during database insert: {ex.Message}";
                                }
                            }

                            stopwatch.Stop();
                            Console.WriteLine($"Elapsed time: {stopwatch.ElapsedMilliseconds} ms");
                        }
                    }
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Invalid file type. Only CSV files are allowed.";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"An error occurred: {ex.Message}";
            }
            finally
            {
                if (_mySqlConnection.State == ConnectionState.Open)
                {
                    _mySqlConnection.Close();
                }
            }

            return response;
        }
    }
}
