using SmartTutor.BusinessLogic.Models;
using SmartTutor.BusinessLogic.Services.Impl;
using SmartTutor.DataAccess.Claims;
using SmartTutor.DataAccess.Repositories.Impl;
using SmartTutor.DataAccess.Repositories.Repo;
using SmartTutor.Domain.Enums;
using SmartTutor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static SmartTutor.BusinessLogic.Models.ClassModels;

namespace SmartTutor.BusinessLogic.Services.Serv
{
    public class ClassService : IClassService
    {
        private readonly IClaimService _claimService;
        private readonly IClassRepository _classRepository;

        public ClassService(IClaimService claimService, IClassRepository classRepository)
        {
            _claimService = claimService;
            _classRepository = classRepository;
        }

        public async Task<ClassResponseModel> CreateClassAsync(ClassRequestModel classRequestModel)
        {
            var userId = _claimService.GetUserId();
            if (!userId.HasValue)
                throw new UnauthorizedAccessException("Người dùng chưa đăng nhập hoặc token không hợp lệ.");

            var @class = new Class
            {
                UserId = userId.Value,
                ClassName = classRequestModel.ClassName,
                ClassType = classRequestModel.ClassType,
                DefaultFeePerSession = classRequestModel.DefaultFeePerSession,
                SchedulePattern = classRequestModel.SchedulePattern,
                CreatedAt = DateTime.UtcNow
            };

            var newClass = await _classRepository.AddAsync(@class);

            return new ClassResponseModel
            {
                Id = newClass.Id,
                UserId = newClass.UserId,
                ClassName = newClass.ClassName,
                ClassType = newClass.ClassType,
                DefaultFeePerSession = newClass.DefaultFeePerSession,
                SchedulePattern = newClass.SchedulePattern
            };
        }

    }
}
