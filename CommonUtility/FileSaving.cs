using FileUploadApp.CommonLayer.Model;

namespace FileUploadApp.CommonUtility;
public class FileSaving
{
    public static async Task<string> GetNameFileNameUploadFile(UploadFileRequest request)
    {
        string path = "upload/" + Path.GetRandomFileName();
        using (FileStream stream = new FileStream(path, FileMode.CreateNew))
        {
            await request.File.CopyToAsync(stream);
        }
        return path;
    }
}