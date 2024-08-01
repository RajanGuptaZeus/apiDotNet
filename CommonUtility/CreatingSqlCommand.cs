using System.Data;

namespace FileUploadApp.CommonUtility
{
    public class CreatingSqlCommand
    {
        public static string CreateSqlCommand(List<string[]> data, int start, int end, string tableName , string id)
        {
            string sqlCommand = $"{id}|INSERT INTO {tableName} (EmailId, Name, Country, State, City, TelephoneNumber, AddressLine1, AddressLine2, DateOfBirth, GrossSalaryFY2019_20, GrossSalaryFY2020_21, GrossSalaryFY2021_22, GrossSalaryFY2022_23, GrossSalaryFY2023_24) VALUES ";

            for (int i = start; i < end; i++)
            {
                string emailId = data[i][0].ToString();
                string name = data[i][1].ToString();
                string country = data[i][2].ToString();
                string state = data[i][3].ToString();
                string city = data[i][4].ToString();
                string telephoneNumber = data[i][5].ToString();
                string addressLine1 = data[i][6].ToString();
                string addressLine2 = data[i][7].ToString();
                string dateOfBirth = data[i][8].ToString();
                string grossSalaryFY2019_20 = data[i][9].ToString();
                string grossSalaryFY2020_21 = data[i][10].ToString();
                string grossSalaryFY2021_22 = data[i][11].ToString();
                string grossSalaryFY2022_23 = data[i][12].ToString();
                string grossSalaryFY2023_24 = data[i][13].ToString();

                // Format strings with single quotes (assuming these are string fields)
                emailId = $"\"{emailId}\"";
                name = $"\"{name}\"";
                country = $"\"{country}\"";
                state = $"\"{state}\"";
                city = $"\"{city}\"";
                telephoneNumber = $"\"{telephoneNumber}\"";
                addressLine1 = $"\"{addressLine1}\"";
                addressLine2 = $"\"{addressLine2}\"";
                dateOfBirth = $"\"{dateOfBirth}\"";

                // Build the values part of the SQL statement
                string values = $"({emailId}, {name}, {country}, {state}, {city}, {telephoneNumber}, {addressLine1}, {addressLine2}, {dateOfBirth}, {grossSalaryFY2019_20}, {grossSalaryFY2020_21}, {grossSalaryFY2021_22}, {grossSalaryFY2022_23}, {grossSalaryFY2023_24})";

                // Append to the command
                if (i == start)
                {
                    sqlCommand += values;
                }
                else
                {
                    sqlCommand += $", {values}";
                }
            }

            return sqlCommand + " ON DUPLICATE KEY UPDATE Name = VALUES(Name),Country  = VALUES(Country), State = VALUES(State), City = VALUES(City), TelephoneNumber = VALUES(TelephoneNumber), AddressLine1 = VALUES(AddressLine1), AddressLine2 = VALUES(AddressLine2), DateOfBirth = VALUES(DateOfBirth), GrossSalaryFY2019_20 = VALUES(GrossSalaryFY2019_20), GrossSalaryFY2020_21 = VALUES(GrossSalaryFY2020_21), GrossSalaryFY2021_22 = VALUES(GrossSalaryFY2021_22), GrossSalaryFY2022_23 = VALUES(GrossSalaryFY2022_23), GrossSalaryFY2023_24 = VALUES (GrossSalaryFY2023_24); ";
        }
    }
}
