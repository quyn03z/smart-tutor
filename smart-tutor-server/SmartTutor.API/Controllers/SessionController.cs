using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartTutor.BusinessLogic.Models;
using SmartTutor.BusinessLogic.Services.Impl;
using SmartTutor.BusinessLogic.Services.Serv;
using static SmartTutor.BusinessLogic.Models.SessionModels;

namespace SmartTutor.API.Controllers
{
    [Authorize]
    [Route("api/session")]
    [ApiController]
    public class SessionController : BaseController
    {
        private readonly ISessionService _sessionService;

        public SessionController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        [HttpGet("teaching")]
        public async Task<IActionResult> GetMyTeacherSessionsAsync([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
        {
            return Ok(ApiResult<IEnumerable<SessionRespondModel>>
                .Success(await _sessionService.GetMyTeacherSessionsAsync(fromDate, toDate)));
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSessionsAsync(SessionRequestModel sessionRequestModel)
        {
            if (!ModelState.IsValid)
                return ValidationError();
            return Ok(ApiResult<SessionRespondModel>
                .Success(await _sessionService.CreateSessionsAsync(sessionRequestModel)));
        }

        [HttpPut("edit")]
        public async Task<IActionResult> EditSessionAsync(SessionRequestModel sessionRequestModel)
        {
            if (!ModelState.IsValid)
                return ValidationError();
            return Ok(ApiResult<SessionRespondModel>
                .Success(await _sessionService.EditSessionsAsync(sessionRequestModel)));
        }

    }
}
