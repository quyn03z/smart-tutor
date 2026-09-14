using SmartTutor.BusinessLogic.Models;
using SmartTutor.BusinessLogic.Services.Impl;
using SmartTutor.DataAccess.Claims;
using SmartTutor.DataAccess.Repositories.Impl;
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

        public async Task<ClassResponseModel> AddClassAsync(ClassRequestModel classRequestModel)
        {
            var userId = _claimService.GetUserId();
        }

    }
}
