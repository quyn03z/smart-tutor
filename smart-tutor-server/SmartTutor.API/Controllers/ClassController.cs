using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartTutor.BusinessLogic.Models;
using SmartTutor.BusinessLogic.Services.Impl;
using SmartTutor.BusinessLogic.Services.Serv;
using static SmartTutor.BusinessLogic.Models.ClassModels;
using static SmartTutor.BusinessLogic.Models.StudentModels;

namespace SmartTutor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassController : BaseController
    {
        private readonly IClassService _classService;

        public ClassController(IClassService classService)
        {
            _classService = classService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateClassAsync(ClassRequestModel classRequestModel)
        {
            if (!ModelState.IsValid)
                return ValidationError();
            return Ok(ApiResult<ClassResponseModel>
                .Success(await _classService.CreateClassAsync(classRequestModel)));
        }

        [HttpGet("userId")]
        public async Task<IActionResult> GetMyClassAsync(int userId)
        {
            return Ok(ApiResult<IEnumerable<ClassResponseModel>>
                .Success(await _classService.GetMyClassAsync()));
        }

        [HttpDelete("classId")]          
        public async Task<IActionResult> DeleteClassAsync(int classId)
        {
            return Ok(ApiResult<string>
                .Success(await _classService.DeleteClassAsync(classId)));
        }

    }
}
