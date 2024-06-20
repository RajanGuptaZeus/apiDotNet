using System;
using System.Data;
using System.Threading.Tasks;
using FileUploadApp.CommonLayer.Model;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

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
                    // Process CSV file using ExcelDataReader or other libraries
                    // Example using ExcelDataReader (uncomment if using):
                    // FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    // System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    // IExcelDataReader reader = ExcelReaderFactory.CreateCsvReader(stream);
                    // DataSet dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
                    // {
                    //     UseColumnDataType = false,
                    //     ConfigureDataTable = tableReader => new ExcelDataTableConfiguration
                    //     {
                    //         UseHeaderRow = true
                    //     }
                    // });

                    // Example database interaction (replace with your actual logic):
                    // var dataTable = dataSet.Tables[0];
                    // foreach (DataRow row in dataTable.Rows)
                    // {
                    //     // Example: Insert into MySQL database
                    //     MySqlCommand cmd = new MySqlCommand("INSERT INTO YourTable (...) VALUES (...)", _mySqlConnection);
                    //     await cmd.ExecuteNonQueryAsync();
                    // }

                    response.IsSuccess = true;
                    response.Message = "CSV file processed successfully";
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
