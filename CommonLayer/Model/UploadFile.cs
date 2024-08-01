using MongoDB.Bson;

namespace FileUploadApp.CommonLayer.Model
{
    public class UploadFileRequest
    {
        public IFormFile File { get; set; }
    }


    public class UploadFileResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }




    public class LogEntry
    {
        public ObjectId Id { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }


    public class FormDataRequest
    {
        // Properties for form data
        public string? EmailId  { get; set; }
        public string? Name { get; set; }
        public string? Country { get; set; }
        public string? State { get; set; }
        public string? City { get; set; }
        public string? TelephoneNumber { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? DateOfBirth { get; set; } // Assuming DateOfBirth is mandatory
        public decimal GrossSalaryFY2019_20 { get; set; }
        public decimal GrossSalaryFY2020_21 { get; set; }
        public decimal GrossSalaryFY2021_22 { get; set; }
        public decimal GrossSalaryFY2022_23 { get; set; }
        public decimal GrossSalaryFY2023_24 { get; set; }

        // // Constructor to initialize properties
        // public FormDataRequest()
        // {
        //     DateOfBirth = DateTime.MinValue;
        // }
    }


    public class FormCSVResponse
    {
        // Properties for form response
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
    }


    public class RowUpdate
{
    public string Email { get; set; }
    public List<ColumnChange> Changes { get; set; }
}

public class ColumnChange
{
    public int ColumnIndex { get; set; }
    public string NewValue { get; set; }
}

}