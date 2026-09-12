using SmartTutor.BusinessLogic.Models;
using SmartTutor.BusinessLogic.Services.Impl;
using SmartTutor.DataAccess.Claims;
using SmartTutor.DataAccess.Repositories.Impl;
using SmartTutor.Domain.Models;
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

        public async Task<StudentsResponseModel> CreateStudentAsync(RequestStudentModel createStudentModel)
        {
            var userId = _claimService.GetUserId();
            if (!userId.HasValue)
                throw new UnauthorizedAccessException("Người dùng chưa đăng nhập hoặc token không hợp lệ.");

            var student = new Student
            {
                UserId = userId.Value,
                FullName = createStudentModel.FullName,
                GradeLevel = createStudentModel.GradeLevel,
                ParentName = createStudentModel.ParentName,
                ParentPhone = createStudentModel.ParentPhone,
                CreditBalance = 0,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            ClassEnrollment enrollment;

            // Trường hợp A: Chọn lớp có sẵn (ClassId > 0)
            if (createStudentModel.ClassId.HasValue && createStudentModel.ClassId.Value > 0)
            {
                enrollment = new ClassEnrollment
                {
                    ClassId = createStudentModel.ClassId.Value,
                    CustomFee = createStudentModel.CustomFee ?? createStudentModel.FeePerSession,
                    JoinedDate = DateTime.UtcNow,
                    Status = "Active"
                };
                student.ClassEnrollments.Add(enrollment);
            }

            // Trường hợp B: Dạy 1-1 (Nhập trực tiếp ClassName và FeePerSession)
            else if (!string.IsNullOrWhiteSpace(createStudentModel.ClassName))
            {
                var newClass = new Class
                {
                    UserId = userId.Value,
                    ClassName = createStudentModel.ClassName,
                    ClassType = createStudentModel.ClassType ?? "Individual",
                    DefaultFeePerSession = createStudentModel.FeePerSession ?? 0,
                    CreatedAt = DateTime.UtcNow
                };
                enrollment = new ClassEnrollment
                {
                    Class = newClass,
                    CustomFee = createStudentModel.FeePerSession,
                    JoinedDate = DateTime.UtcNow,
                    Status = "Active"
                };
                student.ClassEnrollments.Add(enrollment);
            }

            var createdStudent = await _studentRepository.AddAsync(student);

            var currentEnrollment = createdStudent.ClassEnrollments.FirstOrDefault();

            return new StudentsResponseModel
            {
                FullName = createdStudent.FullName,
                GradeLevel = createdStudent.GradeLevel,
                ParentName = createdStudent.ParentName,
                ParentPhone = createdStudent.ParentPhone,
                ClassName = currentEnrollment?.Class?.ClassName,
                ClassType = currentEnrollment?.Class?.ClassType,
                FeePerSession = currentEnrollment?.CustomFee
            };


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
