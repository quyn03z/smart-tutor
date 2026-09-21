using SmartTutor.BusinessLogic.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static SmartTutor.BusinessLogic.Models.ReportModels;

namespace SmartTutor.BusinessLogic.Services.Impl
{
    public interface IRepoortService
    {
        Task<IEnumerable<MonthlyReportResponseDto>> GenerateMonthlyReportsAsync(GenerateReportRequestDto dto);
        Task<IEnumerable<MonthlyReportResponseDto>> GetMonthlyReportsAsync(string? reportMonth, string? paymentStatus, int? classId);
        Task<MonthlyReportDetailResponseDto> GetMonthlyReportDetailAsync(int reportId);
        Task<MonthlyReportResponseDto> UpdateMonthlyReportAsync(int reportId, UpdateMonthlyReportRequestDto dto);
    }
}
