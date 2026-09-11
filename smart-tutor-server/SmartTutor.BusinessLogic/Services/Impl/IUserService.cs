using SmartTutor.BusinessLogic.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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

        Task<UserResponseProfile> GetUserByIdAsync();

        Task<ForgotPassWordModel> ForgotPasswordAsync(EmailRequest email);

        Task<string> ResetPasswordAsync(ResetPassWordRequestModel request);

        Task<LoginResponseModel> RefreshTokenAsync(TokenRequestModel tokenRequestModel);
    }
}
