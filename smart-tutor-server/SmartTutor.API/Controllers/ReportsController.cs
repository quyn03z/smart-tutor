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

        /// Lấy danh sách báo cáo tháng của các học sinh (hỗ trợ lọc theo ReportMonth, trạng thái Unpaid/Paid, ClassId).
        [HttpGet]
        public async Task<IActionResult> GetMonthlyReports(
            [FromQuery] string? reportMonth,
            [FromQuery] string? paymentStatus,
            [FromQuery] int? classId)
        {
            var result = await _repoortService.GetMonthlyReportsAsync(reportMonth, paymentStatus, classId);
            return Ok(ApiResult<IEnumerable<MonthlyReportResponseDto>>.Success(result));
        }

        /// Chốt công và sinh báo cáo tháng cho lớp học / học sinh.
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateMonthlyReports([FromBody] GenerateReportRequestDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationError();

            var result = await _repoortService.GenerateMonthlyReportsAsync(dto);
            return Ok(ApiResult<IEnumerable<MonthlyReportResponseDto>>.Success(result));
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetMonthlyReportDetail([FromRoute] int id)
        {
            var result = await _repoortService.GetMonthlyReportDetailAsync(id);
            return Ok(ApiResult<MonthlyReportDetailResponseDto>.Success(result));
        }

    }
}
