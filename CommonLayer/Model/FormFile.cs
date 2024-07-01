
namespace FileUploadApp.CommonLayer
{
    public class FormRequest 
    {
        public String? name { get; set;}
        public String phoneNumber { get; set;}
        public String? password { get; set;}
    }


    public class FormResponse
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        
    }
}