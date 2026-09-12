using SmartTutor.BusinessLogic.Models;
using SmartTutor.BusinessLogic.Services.Impl;
using SmartTutor.DataAccess.Claims;
using SmartTutor.DataAccess.Repositories.Impl;
using System;
using System.Collections.Generic;
using System.Text;
using static SmartTutor.BusinessLogic.Models.StudentModels;

namespace SmartTutor.BusinessLogic.Services.Serv
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IClaimService _claimService;

        public StudentService(IStudentRepository studentRepository, IClaimService claimService)
        {
            _studentRepository = studentRepository;
            _claimService = claimService;
        }

        public async Task<IEnumerable<StudentsResponseModel>> GetStudentsByCurrentUserAsync()
        {
            var userId = _claimService.GetUserId();

            var allsStudents = await _studentRepository.GetAllStudentByUserId(userId.Value);

            return allsStudents.Select(s => {
                var enrollment = s.ClassEnrollments.FirstOrDefault();
                return new StudentsResponseModel
                    {
                        FullName = s.FullName,
                        ParentName = s.ParentName,
                        ParentPhone = s.ParentPhone,
                        GradeLevel = s.GradeLevel,
                        ClassName = enrollment.Class.ClassName,
                        ClassType = enrollment.Class.ClassType,
                        FeePerSession = enrollment.CustomFee,
                };
            });
            
        }



    }
}
