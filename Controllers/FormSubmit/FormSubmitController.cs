using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using FileUploadApp.CommonLayer;
using FileUploadApp.DataAccessLayer;

namespace FileUploadApp.Controllers
{
    [Route("api/form/[controller]")]
    [ApiController]
    public class FormSubmitController : ControllerBase
    {
        private readonly IFormDL _FormDL;

        public FormSubmitController(IFormDL FormDL)
        {
            _FormDL = FormDL;
        }

        [HttpPost]
        public async Task<IActionResult> addData(FormRequest request)
        {
            FormResponse response = new FormResponse();
            try
            {
                response = await _FormDL.FormValue(request);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return Ok(response);
        }


        [HttpGet]
        public async Task<IActionResult> GetFormValues()
        {
            try
            {
                List<String> response = await _FormDL.GetFormValues();
                return Ok(response);
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }


        [HttpDelete]
        public async Task<IActionResult> DeleteUser(int id)
        {
           
            try
            {
                string res = await _FormDL.DeleteUser(id);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPut]
        public async Task<IActionResult> UpdateUserById (int id , FormRequest request)
        {
            try
            {
                string res = await _FormDL.UpdateUserById(id,request);
                Console.WriteLine(res);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
