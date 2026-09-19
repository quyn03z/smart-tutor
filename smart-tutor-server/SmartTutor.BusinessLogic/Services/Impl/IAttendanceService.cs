using System;
using System.Collections.Generic;
using System.Text;
using static SmartTutor.BusinessLogic.Models.AttendanceModels;

namespace SmartTutor.BusinessLogic.Services.Impl
{
    public interface IAttendanceService
    {
        Task<UpdateAttendanceResponseDto> UpdateAttendanceAsync(UpdateAttendanceRequestDto updateAttendanceRequestDto);

    }
}
