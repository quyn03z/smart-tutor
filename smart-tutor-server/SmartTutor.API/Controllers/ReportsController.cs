using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartTutor.BusinessLogic.Models;
using SmartTutor.BusinessLogic.Services.Impl;
using SmartTutor.BusinessLogic.Services.Serv;
using static SmartTutor.BusinessLogic.Models.UserModels;

namespace SmartTutor.API.Controllers
{
    [Route("api/reports")]
    [ApiController]
    public class ReportsController : BaseController
    {
        private readonly IRepoortService _repoortService;

        public ReportsController(IRepoortService repoortService)
        {
            _repoortService = repoortService;
        }

        //[HttpPost("generate")]
        //public async Task<IActionResult> GenerateMonthlyReports([FromBody] GenerateReportRequestDto dto)
        //{
        //    if (!ModelState.IsValid)
        //        return ValidationError();
        //    return Ok(ApiResult<CreateUserResponseModel>
        //        .Success(await _repoortService.CreateUserAsync(createUserModel)));
        //}

    }
}
