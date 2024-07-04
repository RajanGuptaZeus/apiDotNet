using System;
using System.Data;
using System.Diagnostics;
using ExcelDataReader;
using FileUploadApp.CommonLayer.Model;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FileUploadApp.rabbitMQ;
using Microsoft.VisualBasic.FileIO;

namespace FileUploadApp.DataAccessLayer
{
    public class UploadFileDL
    {
        // private static readonly MySqlConnection _mySqlConnection = new MySqlConnection("server=localhost;user=root;password= ;database=upload_csv;port=3306;");
        private readonly static int fieldLength = 14;
        private readonly static string[] fieldType = ["System.String", "System.String", "System.String", "System.String", "System.String", "System.String", "System.String", "System.String", "System.String", "System.Int32", "System.Int32", "System.Int32", "System.Int32", "System.Int32"];

        // public int BATCH_SIZE { get; set; } = 1000;

        // protected static async Task<MySqlCommand> GetMySqlCommandForBatch(string sCommand, int rows, int cols, MySqlConnection sqlConnection)
        // {
        //     List<string> rowParam = new List<string>();

        //     for (int i = 0; i < rows; ++i)
        //     {
        //         List<string> col = new List<string>();

        //         for (int j = 0; j < cols; j++)
        //         {
        //             col.Add($"@r{i}c{j}");
        //         }
        //         string sRow = "(" + string.Join(",", col) + ")";
        //         rowParam.Add(sRow);
        //     }
        //     sCommand += string.Join(",", rowParam) + ";";

        //     MySqlCommand cmd = new MySqlCommand(sCommand, sqlConnection);
        //     for (int i = 0; i < rows; ++i)
        //     {
        //         for (int j = 0; j < cols; ++j)
        //         {
        //             cmd.Parameters.AddWithValue($"@r{i}c{j}", "");
        //         }
        //     }

        //     await cmd.PrepareAsync();
        //     return cmd;
        // }

        // protected static MySqlCommand UpdateParamsOfCommandUsers(MySqlCommand cmd, DataTable dataTable, int iFrom, int iTo)
        // {
        //     for (int i = iFrom; i < iTo; i++)
        //     {
        //         var row = dataTable.Rows[i];
        //         cmd.Parameters[(i - iFrom) * 14 + 0].Value = row["EmailId"];
        //         cmd.Parameters[(i - iFrom) * 14 + 1].Value = row["Name"];
        //         cmd.Parameters[(i - iFrom) * 14 + 2].Value = row["Country"];
        //         cmd.Parameters[(i - iFrom) * 14 + 3].Value = row["State"];
        //         cmd.Parameters[(i - iFrom) * 14 + 4].Value = row["City"];
        //         cmd.Parameters[(i - iFrom) * 14 + 5].Value = row["TelephoneNumber"];
        //         cmd.Parameters[(i - iFrom) * 14 + 6].Value = row["AddressLine1"];
        //         cmd.Parameters[(i - iFrom) * 14 + 7].Value = row["AddressLine2"];
        //         cmd.Parameters[(i - iFrom) * 14 + 8].Value = row["DateOfBirth"];
        //         cmd.Parameters[(i - iFrom) * 14 + 9].Value = row["GrossSalaryFY2019_20"];
        //         cmd.Parameters[(i - iFrom) * 14 + 10].Value = row["GrossSalaryFY2020_21"];
        //         cmd.Parameters[(i - iFrom) * 14 + 11].Value = row["GrossSalaryFY2021_22"];
        //         cmd.Parameters[(i - iFrom) * 14 + 12].Value = row["GrossSalaryFY2022_23"];
        //         cmd.Parameters[(i - iFrom) * 14 + 13].Value = row["GrossSalaryFY2023_24"];
        //     }
        //     return cmd;
        // }

        public static async Task<UploadFileResponse> UploadCsvFile(string path)
        {
            int malformedPackets = 0;
            bool firstFlag = true;
            List<string[]> dataTable = [];
            List<string[]> malformedRows = [];
            UploadFileResponse response = new UploadFileResponse();
            response.IsSuccess = true;
            response.Message = "Successfully";

            try
            {
                // connectToDb();
                using (TextFieldParser textFieldParser = new TextFieldParser(path))
                {
                    textFieldParser.TextFieldType = FieldType.Delimited;
                    textFieldParser.CommentTokens = ["#"];
                    // textFieldParser.HasFieldsEnclosedInQuotes = true;
                    textFieldParser.SetDelimiters(",");
                    while (!textFieldParser.EndOfData)
                    {
                        bool unsupportedDataType = false;
                        string[] fields = new string[fieldLength];
                        try
                        {
                            fields = textFieldParser.ReadFields() ?? [];
                        }
                        catch (MalformedLineException)
                        {
                            malformedPackets++;
                            continue;
                        }

                        // Handle incomplete or unexpected data
                        if (fields.Length != fieldLength)
                        {
                            malformedPackets++;
                            continue;
                        }

                        if (firstFlag)
                        {
                            firstFlag = false;
                            if (fields[0].Equals("email", StringComparison.CurrentCultureIgnoreCase)) continue;
                        }

                        for (int i = 0; i < fieldLength; i++)
                        {
                            if (fields[i].GetType().FullName != fieldType[i])
                            {
                                if (fieldType[i] == "System.Int32")
                                {
                                    if (int.TryParse(fields[i], out int value)) { }
                                    else
                                    {
                                        unsupportedDataType = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    unsupportedDataType = true;
                                    break;
                                }
                            }
                        }
                        if (unsupportedDataType)
                        {
                            malformedRows.Add(fields);
                            malformedPackets++;
                            continue;
                        }
                        dataTable.Add(fields);
                    }

                    Stopwatch stopwatch = Stopwatch.StartNew();

                    // using (var transaction = await _mySqlConnection.BeginTransactionAsync())
                    // {

                    try
                    {

                        int BATCH_SIZE = 1000;
                        int iFrom = 0;
                        int iTo = BATCH_SIZE;

                        while (iFrom < dataTable.Count - (dataTable.Count % BATCH_SIZE))
                        {
                            string cmd = CommonUtility.CreatingSqlCommand.CreateSqlCommand(dataTable, iFrom, iTo, "userrecords");
                            Sender.senderFunction("sqlCommand", cmd);
                            iFrom += BATCH_SIZE;
                            iTo += BATCH_SIZE;
                        }

                        if (dataTable.Count % BATCH_SIZE != 0)
                        {
                            if (iTo > dataTable.Count)
                            {
                                iTo = dataTable.Count;
                            }
                            string cmd = CommonUtility.CreatingSqlCommand.CreateSqlCommand(dataTable, iFrom, iTo, "userrecords");
                            Sender.senderFunction("sqlCommand", cmd);
                        }

                        // await transaction.CommitAsync();
                        response.IsSuccess = true;
                        response.Message = "CSV file processed successfully";
                    }
                    catch (Exception ex)
                    {
                        // await transaction.RollbackAsync();
                        response.IsSuccess = false;
                        response.Message = $"An error occurred during database insert: {ex.Message}";
                    } 
                    // }

                    stopwatch.Stop();
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"An error occurred: {ex}";
            }

            return response;
        }
    }
}
