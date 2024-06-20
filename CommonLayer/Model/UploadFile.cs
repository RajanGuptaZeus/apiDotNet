namespace FileUploadApp.CommonLayer.Model
{
    public class UploadFileRequest 
    {
        public IFormFile File { get; set;}
    }


    public class UploadFileResponse 
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}