using FileUploadApp.CommonLayer.Model;
using FileUploadApp.DataAccessLayer;
using Microsoft.AspNetCore.Mvc;

namespace FileUploadApp.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]

    public class UploadFileController : ControllerBase
    {
        // interface create kiya
        public readonly IUploadFileDL _uploadFileDL;



        public UploadFileController(IUploadFileDL uploadFileDL)
        {
            // dependency injection implement kiya DI
            _uploadFileDL = uploadFileDL;
        }

        [HttpPost]
        public async Task<IActionResult> UploadCsvFile(UploadFileRequest request)
        {
            UploadFileResponse response = new UploadFileResponse();
            string path = "upload/" + request.File.FileName;
            try
            {
                response = await _uploadFileDL.UploadCsvFile(request,path);

                using(FileStream stream = new FileStream(path,FileMode.CreateNew))
                {
                    await request.File.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return Ok(response);
        }
    }
}
