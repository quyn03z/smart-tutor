using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartTutor.BusinessLogic.Models;
using SmartTutor.BusinessLogic.Services.Impl;
using SmartTutor.BusinessLogic.Services.Serv;
using static SmartTutor.BusinessLogic.Models.StudentModels;
using static SmartTutor.BusinessLogic.Models.UserModels;

namespace SmartTutor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet("my-students")]
        public async Task<IActionResult> GetMyStudentsAsync()
        {
            return Ok(ApiResult<IEnumerable<StudentsResponseModel>>
                .Success(await _studentService.GetStudentsByCurrentUserAsync()));
        }


    }
}
