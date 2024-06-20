using FileUploadApp.CommonLayer.Model;

namespace FileUploadApp.DataAccessLayer
{
    public interface IUploadFileDL
    {
        public Task<UploadFileResponse> UploadCsvFile (UploadFileRequest request , string path);
    }
}