using System;
using System.Collections.Generic;
using System.Text;
using static SmartTutor.BusinessLogic.Models.UserModels;

namespace SmartTutor.BusinessLogic.Services.Impl
{
    public interface IUserService
    {
        Task<LoginResponseModel> LoginAsync(LoginUserModel loginUserModel);

        Task<CreateUserResponseModel> CreateUserAsync(CreateUserModel createUserModel);

        Task LogoutAsync();

        Task<string> ChangePassWordAsync(ChangePassWordModel changePassWordModel);

        Task<UserResponseProfile> UpdateProfileAsync(UpdateProfileRequestModel updateProfileRequestModel);
    }
}
