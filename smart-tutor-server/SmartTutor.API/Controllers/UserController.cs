using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartTutor.BusinessLogic.Models;
using SmartTutor.BusinessLogic.Services.Impl;
using static SmartTutor.BusinessLogic.Models.UserModels;

namespace SmartTutor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePasswordAsync(ChangePassWordModel changePassWordModel)
        {
            if (!ModelState.IsValid)
                return ValidationError();
            return Ok(ApiResult<string>.Success(await _userService.ChangePassWordAsync(changePassWordModel)));
        }

        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfileAsync(UpdateProfileRequestModel updateProfileRequestModel)
        {
            if (!ModelState.IsValid)
                return ValidationError();
            return Ok(ApiResult<UserResponseProfile>.Success(await _userService.UpdateProfileAsync(updateProfileRequestModel)));
        }

    }
}
