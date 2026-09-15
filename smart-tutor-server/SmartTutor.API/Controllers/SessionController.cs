using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartTutor.BusinessLogic.Models;
using SmartTutor.BusinessLogic.Services.Impl;
using SmartTutor.BusinessLogic.Services.Serv;
using static SmartTutor.BusinessLogic.Models.StudentModels;

namespace SmartTutor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController : BaseController
    {
        private readonly ISessionService _sessionService;

        public SessionController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        [HttpGet("teaching")]
        public async Task<IActionResult> GetMyTeacherSessionsAsync (DateOnly fromDate, DateOnly toDate)
        {
            return Ok(ApiResult<StudentCreditHistoryResponseModel>
                .Success(await _studentService.GetStudentCreditHistoryAsync(id)));
        }

    }
}
