using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using SmartTutor.BusinessLogic.Exceptions;
using SmartTutor.BusinessLogic.Helpers;
using SmartTutor.BusinessLogic.Models;
using SmartTutor.BusinessLogic.Services.Impl;
using SmartTutor.DataAccess.Claims;
using SmartTutor.DataAccess.Repositories.Impl;
using SmartTutor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static SmartTutor.BusinessLogic.Models.UserModels;

namespace SmartTutor.BusinessLogic.Services.Serv
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IConfiguration _configuration;
        private readonly IClaimService _claimService;

        public UserService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IConfiguration configuration,
            IClaimService claimService)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _configuration = configuration;
            _claimService = claimService;
        }

        public async Task<CreateUserResponseModel> CreateUserAsync(CreateUserModel createUserModel)
        {
            if (await _userRepository.IsEmailExistAsync(createUserModel.Email))
                throw new BadRequestException("Email đã tồn tại trong hệ thống!");

            var user = new User
            {
                Email = createUserModel.Email,
                FullName = createUserModel.FullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserModel.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            return new CreateUserResponseModel
            {
                Id = user.Id
            };
        }

        public async Task<LoginResponseModel> LoginAsync(LoginUserModel loginUserModel)
        {
            var user = await _userRepository.GetUserByEmailAsync(loginUserModel.Email);
            if (user == null) 
                throw new BadRequestException("Email đăng nhập không chính xác.");

            if (!BCrypt.Net.BCrypt.Verify(loginUserModel.Password, user.PasswordHash))
            {
                throw new BadRequestException("Mật khẩu nhập không chính xác.");
            }

            // 1. Sinh Access Token (JWT)
            var accessToken = JwtHelper.GenerateToken(user, _configuration);

            // 2. Sinh Refresh Token và lưu vào Database
            var refreshTokenString = JwtHelper.GenerateRefreshToken();
            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenString,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };
            await _refreshTokenRepository.AddAsync(refreshToken);

            return new LoginResponseModel
            {
                Role = user.Role?.RoleName ?? "User",
                Token = accessToken,
                RefreshToken = refreshTokenString
            };
        }

        public async Task LogoutAsync()
        {
            var userId = _claimService.GetUserId();
            if (userId == null)
                throw new UnauthorizedException("Người dùng chưa xác thực.");

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
                throw new NotFoundException("Không tìm thấy người dùng trong hệ thống.");

            // Thu hồi tất cả Refresh Token đang hoạt động của người dùng
            await _refreshTokenRepository.RevokeTokensByUserIdAsync(userId.Value);
        }
    }
}
