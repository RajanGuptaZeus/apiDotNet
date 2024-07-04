using System.Data;
using System.Data.Common;
using FileUploadApp.CommonLayer;
using MySql.Data.MySqlClient;

namespace FileUploadApp.DataAccessLayer
{
    public class FormDL : IFormDL
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlConnection _mySqlConnection;

        public FormDL(IConfiguration configuration)
        {
            _configuration = configuration;
            _mySqlConnection = new MySqlConnection(_configuration["ConnectionStrings:MySqlDBConnectionString"]);
        }

        public void GetConnection()
        {
            if (_mySqlConnection.State != ConnectionState.Open)
            {
                _mySqlConnection.Open();
            }
        }

        public async Task<FormResponse> FormValue(FormRequest request)
        {
            FormResponse response = new FormResponse();

            try
            {
                string sqlCommand = "INSERT INTO userData (name, phoneNumber, password) VALUES (@name, @phoneNumber, @password)";
                GetConnection();
                MySqlCommand cmd = new MySqlCommand(sqlCommand, _mySqlConnection);
                cmd.Parameters.AddWithValue("@name", request.name);
                cmd.Parameters.AddWithValue("@phoneNumber", request.phoneNumber);
                cmd.Parameters.AddWithValue("@password", request.password);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    response.IsSuccess = true;
                    response.Message = "Form submitted successfully";
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Failed to submit form";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            finally
            {
                _mySqlConnection.Close();
            }

            return response;
        }

        public async Task<List<string>> GetFormValues()
        {
            List<string> formValues = new List<string>();

            try
            {
                string sqlCommand = "SELECT userId , name, phoneNumber, password FROM userData";
                GetConnection();
                MySqlCommand cmd = new MySqlCommand(sqlCommand, _mySqlConnection);

                using (DbDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            Int32 id = reader.GetInt32(0);
                            string name = reader.GetString(1);
                            string phoneNumber = reader.GetString(2);
                            string password = reader.GetString(3);

                            formValues.Add($"{id}, {name}, {phoneNumber}, {password}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return formValues;
        }

        public async Task<String> DeleteUser(int id)
        {
            try
            {
                if (id < 0)
                {
                    throw new ArgumentException("Id cannot be negative");
                }
                string sqlCommand = $"DELETE FROM userData WHERE userId = {id}";
                GetConnection();
                MySqlCommand cmd = new MySqlCommand(sqlCommand, _mySqlConnection);
                cmd.Parameters.AddWithValue("@id", id);
                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    throw new Exception("User not found");
                }
                else
                {
                    return "User deleted successfully";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> UpdateUserById(int id, FormRequest request)
        {
            try
            {
                if (id < 0)
                {
                    throw new ArgumentException("Id cannot be negative");
                }

                string sqlCommand = "UPDATE userData SET name = @name, phoneNumber = @phoneNumber, password = @password WHERE userId = @id";

                GetConnection();

                MySqlCommand cmd = new MySqlCommand(sqlCommand, _mySqlConnection);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@name", request.name);
                cmd.Parameters.AddWithValue("@phoneNumber", request.phoneNumber);
                cmd.Parameters.AddWithValue("@password", request.password);
                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    throw new Exception("User not found or no changes applied");
                }
                else
                {
                    return "User updated successfully";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

    }
}
