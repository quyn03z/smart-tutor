using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartTutor.BusinessLogic.Models;
using SmartTutor.BusinessLogic.Services.Impl;
using SmartTutor.DataAccess.Repositories.Repo;
using static SmartTutor.BusinessLogic.Models.UserModels;

namespace SmartTutor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> CreateUserAsync(CreateUserModel createUserModel)
        {
            if (!ModelState.IsValid)
                return ValidationError();
            return Ok(ApiResult<CreateUserResponseModel>
                .Success(await _userService.CreateUserAsync(createUserModel)));
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginUserModel loginUserModel)
        {
            if (!ModelState.IsValid)
                return ValidationError();
            return Ok(ApiResult<LoginResponseModel>
                .Success(await _userService.LoginAsync(loginUserModel)));
        }

    }
}
