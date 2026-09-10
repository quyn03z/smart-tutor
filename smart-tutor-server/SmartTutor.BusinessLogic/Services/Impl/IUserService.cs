using System;
using System.Collections.Generic;
using System.Text;

namespace SmartTutor.BusinessLogic.Services.Impl
{
    public interface IUserService
    {
        Task<LoginResponseModel> LoginAsync(LoginUserModel loginUserModel);

        Task<CreateUserResponseModel> CreateUserAsync(CreateUserModel createUserModel);

    }
}
