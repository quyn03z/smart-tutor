using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartTutor.BusinessLogic.Models;
using SmartTutor.BusinessLogic.Services.Impl;
using System.Collections.Generic;
using System.Threading.Tasks;
using static SmartTutor.BusinessLogic.Models.ReportModels;

namespace SmartTutor.API.Controllers
{
    [Authorize]
    [Route("api/reports")]
    [ApiController]
    public class ReportsController : BaseController
    {
        private readonly IRepoortService _repoortService;

        public ReportsController(IRepoortService repoortService)
        {
            _repoortService = repoortService;
        }

        /// <summary>
        /// Chốt công và sinh báo cáo tháng cho lớp học / học sinh.
        /// </summary>
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateMonthlyReports([FromBody] GenerateReportRequestDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationError();

            var result = await _repoortService.GenerateMonthlyReportsAsync(dto);
            return Ok(ApiResult<IEnumerable<MonthlyReportResponseDto>>.Success(result));
        }
    }
}
