using Microsoft.AspNetCore.Mvc;

namespace FileUploadApp.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TestingController
    {
        [HttpPost]
        public String randomString(string request)
        {
            try
            {
                string input = request;
                string output = "Hello " + input;
                return output;
            } 
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }
    }
}