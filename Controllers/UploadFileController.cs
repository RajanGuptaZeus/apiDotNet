using FileUploadApp.CommonLayer.Model;
using FileUploadApp.DataAccessLayer;
using Microsoft.AspNetCore.Mvc;

namespace FileUploadApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class UploadFileController : ControllerBase
    {
        // interface create kiya
        public readonly IUploadFileDL _uploadFileDL;



        public UploadFileController(IUploadFileDL uploadFileDL)
        {
            // dependency injection implement kiya - DI
            _uploadFileDL = uploadFileDL;
        }

        [HttpGet]
        public string Test()
        {
            return "Hello World";
        }

        [HttpPost]
        public async Task<IActionResult> UploadCsvFile(UploadFileRequest request)
        {
            UploadFileResponse response = new UploadFileResponse();
            string path = "upload/" + request.File.FileName;
            try
            {
                if (request.File.FileName.ToLower().EndsWith(".csv"))
                {

                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                    using (FileStream stream = new FileStream(path, FileMode.CreateNew))
                    {
                        await request.File.CopyToAsync(stream);
                    }
                    response = await _uploadFileDL.UploadCsvFile(request, path);
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Invalid file type. Only CSV files are allowed.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured");
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return Ok(response);
        }
    }
}
