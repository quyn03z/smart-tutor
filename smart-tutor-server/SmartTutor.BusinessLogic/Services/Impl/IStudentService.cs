using SmartTutor.DataAccess.Repositories.Impl;
using System;
using System.Collections.Generic;
using System.Text;
using static SmartTutor.BusinessLogic.Models.StudentModels;

namespace SmartTutor.BusinessLogic.Services.Impl
{
    public interface IStudentService
    {
        Task<IEnumerable<StudentsResponseModel>> GetStudentsByCurrentUserAsync();
    }
}
