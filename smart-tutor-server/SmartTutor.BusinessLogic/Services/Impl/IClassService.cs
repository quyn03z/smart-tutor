using System;
using System.Collections.Generic;
using System.Text;
using static SmartTutor.BusinessLogic.Models.ClassModels;

namespace SmartTutor.BusinessLogic.Services.Impl
{
    public interface IClassService
    {
        Task<ClassResponseModel> CreateClassAsync(ClassRequestModel classRequestModel);
        Task<IEnumerable<ClassResponseModel>> GetMyClassAsync();
        Task<string> DeleteClassAsync(int classId);
        Task<ClassResponseModel> UpdateClassAsync(ClassRequestModel classRequestModel);


    }
}
