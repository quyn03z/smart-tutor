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

        // Lấy danh sách báo cáo tháng của các học sinh (hỗ trợ lọc theo ReportMonth, trạng thái Unpaid/Paid, ClassId).
        [HttpGet]
        public async Task<IActionResult> GetMonthlyReports(
            [FromQuery] string? reportMonth,
            [FromQuery] string? paymentStatus,
            [FromQuery] int? classId)
        {
            var result = await _repoortService.GetMonthlyReportsAsync(reportMonth, paymentStatus, classId);
            return Ok(ApiResult<IEnumerable<MonthlyReportResponseDto>>.Success(result));
        }

        // Chốt công và sinh báo cáo tháng cho lớp học / học sinh.
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateMonthlyReports([FromBody] GenerateReportRequestDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationError();

            var result = await _repoortService.GenerateMonthlyReportsAsync(dto);
            return Ok(ApiResult<IEnumerable<MonthlyReportResponseDto>>.Success(result));
        }


        // Lấy chi tiết phiếu báo cáo tháng để Live Preview (kèm danh sách chi tiết các buổi học)
        [HttpGet("{reportId}")]
        public async Task<IActionResult> GetMonthlyReportDetail([FromRoute] int reportId)
        {
            var result = await _repoortService.GetMonthlyReportDetailAsync(reportId);
            return Ok(ApiResult<MonthlyReportDetailResponseDto>.Success(result));
        }

        // Cập nhật nhận xét sư phạm, lộ trình hoặc số tiền của báo cáo tháng
        [HttpPut("{reportId}")]
        public async Task<IActionResult> UpdateMonthlyReport(
            [FromRoute] int reportId, 
            [FromBody] UpdateMonthlyReportRequestDto updateMonthlyReportRequestDto)
        {
            if (!ModelState.IsValid)
                return ValidationError();

            var result = await _repoortService.UpdateMonthlyReportAsync(reportId, updateMonthlyReportRequestDto);
            return Ok(ApiResult<MonthlyReportResponseDto>.Success(result));
        }

    }
}
