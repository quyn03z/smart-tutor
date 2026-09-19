using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartTutor.BusinessLogic.Models;
using SmartTutor.BusinessLogic.Services.Impl;
using static SmartTutor.BusinessLogic.Models.AttendanceModels;

namespace SmartTutor.API.Controllers
{
    [Authorize]
    [Route("api/attendance")]
    [ApiController]
    public class AttendanceController : BaseController
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAttendanceRecordAsync([FromBody] UpdateAttendanceRequestDto updateAttendanceRequestDto)
        {
            if (!ModelState.IsValid)
                return ValidationError();
            return Ok(ApiResult<UpdateAttendanceResponseDto>
                .Success(await _attendanceService.UpdateAttendanceAsync(updateAttendanceRequestDto)));
        }

    }
}
