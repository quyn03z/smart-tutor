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
        Task<StudentsResponseModel> CreateStudentAsync(RequestStudentModel createStudentModel);
        Task<StudentsResponseModel> EditStudentAsync(RequestStudentModel studentResponseModel);
        Task<StudentDetailResponseModel> GetStudentDetailAsync(int studentId);
        Task<string> DeleteStudentAsync(int studentId);
        Task<StudentCreditHistoryResponseModel> GetStudentCreditHistoryAsync(int studentId);
    }
}
