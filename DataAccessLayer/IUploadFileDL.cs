using FileUploadApp.CommonLayer.Model;

namespace FileUploadApp.DataAccessLayer
{
    public interface IUploadFileDL
    {
        public Task<UploadFileResponse> UploadCsvFile (string path);
    }
}