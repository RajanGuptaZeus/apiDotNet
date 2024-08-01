using FileUploadApp.CommonLayer.Model;
using FileUploadApp.CommonUtility;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace FileUploadApp.DataAccessLayer;


public class Methods
{

    public static async Task<List<string>> getData(int pageNo, int limit)
    {
        try
        {
            List<string> employees = new List<string>();
            var sql = await SQLConnection.ConnectToDb();
            int offset = (pageNo - 1) * limit;
            string sqlCommand = $"Select * from userRecords limit {limit} offset {offset}";
            Console.WriteLine(sqlCommand);
            var cmd = new MySqlCommand(sqlCommand, sql);
            var reader = await cmd.ExecuteReaderAsync();


            while (reader.Read())
            {
                string emailId = reader["EmailId"].ToString();
                string name = reader["Name"].ToString();
                string country = reader["Country"].ToString();
                string state = reader["State"].ToString();
                string city = reader["City"].ToString();
                string telephoneNumber = reader["TelephoneNumber"].ToString();
                string addressLine1 = reader["AddressLine1"].ToString();
                string addressLine2 = reader["AddressLine2"].ToString();
                string dateOfBirth = reader["DateOfBirth"].ToString();
                decimal grossSalaryFY2019_20 = (decimal)reader["GrossSalaryFY2019_20"];
                decimal grossSalaryFY2020_21 = (decimal)reader["GrossSalaryFY2020_21"];
                decimal grossSalaryFY2021_22 = (decimal)reader["GrossSalaryFY2021_22"];
                decimal grossSalaryFY2022_23 = (decimal)reader["GrossSalaryFY2022_23"];
                decimal grossSalaryFY2023_24 = (decimal)reader["GrossSalaryFY2023_24"];

                // Constructing a string with all values for presenting it
                string employeeInfo = $"{emailId},{name},{country},{state},{city},{telephoneNumber},{addressLine1},{addressLine2},{dateOfBirth},{grossSalaryFY2019_20},{grossSalaryFY2020_21},{grossSalaryFY2021_22},{grossSalaryFY2022_23},{grossSalaryFY2023_24}";


                // Adding employee information to the list
                employees.Add(employeeInfo);
            }
            reader.Close();
            return employees;
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex);
            return null;
        }
    }




    public static async Task<UploadFileResponse> deleteAllDataFromDatabase()
    {
        UploadFileResponse response = new UploadFileResponse();
        try
        {
            var sql = await SQLConnection.ConnectToDb();
            string sqlCommand = "truncate table userrecords";
            var cmd = new MySqlCommand(sqlCommand, sql);
            var reader = await cmd.ExecuteReaderAsync();
            response.IsSuccess = true;
            response.Message = "Data deleted successfully";
            return response;
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.Message = ex.Message;
            return response;
        }
    }
    public static async Task<UploadFileResponse> DeleteDataByEmails(List<string> emailIds)
    {
        UploadFileResponse response = new UploadFileResponse();
        if (emailIds == null || emailIds.Count == 0)
        {
            response.IsSuccess = false;
            response.Message = "No email IDs provided";
            return response;
        }

        try
        {
            var sql = await SQLConnection.ConnectToDb();

            // Escape the email addresses to prevent SQL injection
            var emailParameters = string.Join(", ", emailIds.ConvertAll(email => $"'{email.Replace("'", "''")}'"));
            string sqlCommand = $"DELETE FROM userRecords WHERE EmailId IN ({emailParameters})";

            var cmd = new MySqlCommand(sqlCommand, sql);
            int rowsAffected = await cmd.ExecuteNonQueryAsync();

            response.IsSuccess = true;
            response.Message = $"{rowsAffected} rows deleted successfully";
            return response;
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.Message = ex.Message;
            return response;
        }
    }



    public static async Task<UploadFileResponse> addDataToDatabase(FormDataRequest formData)
    {
        UploadFileResponse response = new UploadFileResponse();
        try
        {
            // Connect to the database
            using (var sql = await SQLConnection.ConnectToDb())
            {
                // Check if the email already exists
                string checkEmailCommand = "SELECT COUNT(*) FROM userrecords WHERE emailId = @EmailId";
                using (var checkCmd = new MySqlCommand(checkEmailCommand, sql))
                {
                    checkCmd.Parameters.AddWithValue("@EmailId", formData.EmailId ?? (object)DBNull.Value);
                    int count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                    if (count > 0)
                    {
                        response.IsSuccess = false;
                        response.Message = "This email is already used.";
                        return response;
                    }
                }

                // Define the SQL command with parameters for insertion
                string insertCommand = @"
            INSERT INTO userrecords (
                EmailId, Name, Country, State, City, TelephoneNumber, AddressLine1, AddressLine2,
                DateOfBirth, GrossSalaryFY2019_20, GrossSalaryFY2020_21, GrossSalaryFY2021_22,
                GrossSalaryFY2022_23, GrossSalaryFY2023_24
            ) VALUES (
                @EmailId, @Name, @Country, @State, @City, @TelephoneNumber, @AddressLine1, @AddressLine2,
                @DateOfBirth, @GrossSalaryFY2019_20, @GrossSalaryFY2020_21, @GrossSalaryFY2021_22,
                @GrossSalaryFY2022_23, @GrossSalaryFY2023_24
            )";

                // Create the command and add parameters
                using (var cmd = new MySqlCommand(insertCommand, sql))
                {
                    cmd.Parameters.AddWithValue("@EmailId", formData.EmailId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Name", formData.Name ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Country", formData.Country ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@State", formData.State ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@City", formData.City ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TelephoneNumber", formData.TelephoneNumber ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AddressLine1", formData.AddressLine1 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AddressLine2", formData.AddressLine2 ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DateOfBirth", formData.DateOfBirth ?? (Object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@GrossSalaryFY2019_20", formData.GrossSalaryFY2019_20);
                    cmd.Parameters.AddWithValue("@GrossSalaryFY2020_21", formData.GrossSalaryFY2020_21);
                    cmd.Parameters.AddWithValue("@GrossSalaryFY2021_22", formData.GrossSalaryFY2021_22);
                    cmd.Parameters.AddWithValue("@GrossSalaryFY2022_23", formData.GrossSalaryFY2022_23);
                    cmd.Parameters.AddWithValue("@GrossSalaryFY2023_24", formData.GrossSalaryFY2023_24);
                    // == DateTime.MinValue ? (object)DBNull.Value : formData.DateOfBirth)
                    // Execute the insertion command
                    await cmd.ExecuteNonQueryAsync();
                }

                response.IsSuccess = true;
                response.Message = "Data added successfully";
            }
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.Message = ex.Message;
        }

        return response;
    }



    public static async Task<UploadFileResponse> UpdateRows(List<RowUpdate> updates)
    {
        UploadFileResponse response = new UploadFileResponse();
        try
        {
            using (var sql = await SQLConnection.ConnectToDb())
            {
                foreach (var update in updates)
                {
                    string updateCommand = "UPDATE userrecords SET ";
                    List<string> updateClauses = new List<string>();
                    MySqlCommand cmd = new MySqlCommand();

                    foreach (var change in update.Changes)
                    {
                        string columnName = GetColumnName(change.ColumnIndex);
                        if (!string.IsNullOrEmpty(columnName))
                        {
                            updateClauses.Add($"{columnName} = @{columnName}");
                            cmd.Parameters.AddWithValue($"@{columnName}", change.NewValue);
                        }
                    }

                    if (updateClauses.Count > 0)
                    {
                        updateCommand += string.Join(", ", updateClauses);
                        updateCommand += " WHERE EmailId = @EmailId";
                        cmd.Parameters.AddWithValue("@EmailId", update.Email);

                        cmd.CommandText = updateCommand;
                        cmd.Connection = sql;

                        await cmd.ExecuteNonQueryAsync();
                    }
                    cmd.Parameters.Clear();
                }

                response.IsSuccess = true;
                response.Message = "Updates applied successfully";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            response.IsSuccess = false;
            response.Message = ex.Message;
        }

        return response;
    }

    private static string GetColumnName(int columnIndex)
    {
        switch (columnIndex)
        {
            case 0: return "EmailId";
            case 1: return "Name";
            case 2: return "Country";
            case 3: return "State";
            case 4: return "City";
            case 5: return "TelephoneNumber";
            case 6: return "AddressLine1";
            case 7: return "AddressLine2";
            case 8: return "DateOfBirth";
            case 9: return "GrossSalaryFY2019_20";
            case 10: return "GrossSalaryFY2020_21";
            case 11: return "GrossSalaryFY2021_22";
            case 12: return "GrossSalaryFY2022_23";
            case 13: return "GrossSalaryFY2023_24";
            default: return null;
        }
    }


}