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
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using static SmartTutor.BusinessLogic.Models.UserModels;

namespace SmartTutor.BusinessLogic.Services.Serv
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IResetPasswordTokenRepository _resetPasswordTokenRepository;
        private readonly IConfiguration _configuration;
        private readonly IClaimService _claimService;

        public UserService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IResetPasswordTokenRepository resetPasswordTokenRepository,
            IConfiguration configuration,
            IClaimService claimService)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _resetPasswordTokenRepository = resetPasswordTokenRepository;
            _configuration = configuration;
            _claimService = claimService;
        }

        public async Task<string> ChangePassWordAsync(ChangePassWordModel changePassWordModel)
        {
            var userId = _claimService.GetUserId();
            if (userId == null)
                throw new UnauthorizedException("Người dùng chưa xác thực.");
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("Không tìm thấy người dùng.");

            if (!BCrypt.Net.BCrypt.Verify(changePassWordModel.OldPassword, user.PasswordHash))
                throw new BadRequestException("Mật khẩu cũ không chính xác.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePassWordModel.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            return "Đổi mật khẩu thành công.";
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

        public async Task<ForgotPassWordModel> ForgotPasswordAsync(EmailRequest email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email.Email);
            if (user == null)
                throw new BadRequestException("Email không tồn tại trong hệ thống.");

            // Vô hiệu hóa các reset token cũ chưa sử dụng của user
            await _resetPasswordTokenRepository.InvalidateTokensByUserIdAsync(user.Id);

            // Sinh reset token ngẫu nhiên và an toàn
            var resetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var resetTokenExpiry = DateTime.UtcNow.AddMinutes(15);

            var resetPasswordTokenEntity = new ResetPasswordToken
            {
                UserId = user.Id,
                ResetToken = resetToken,
                ExpiredAt = resetTokenExpiry,
                isUsed = false,
                CreateAt = DateTime.UtcNow
            };

            await _resetPasswordTokenRepository.AddAsync(resetPasswordTokenEntity);

            return new ForgotPassWordModel
            {
                ResetToken = resetToken,
                ExpiredAt = resetTokenExpiry
            };
        }

        public async Task<string> ResetPasswordAsync(ResetPassWordRequestModel request)
        {
            var tokenEntity = await _resetPasswordTokenRepository.GetValidTokenAsync(request.ResetToken);
            if (tokenEntity == null || tokenEntity.User?.Email != request.Email)
            {
                throw new BadRequestException("Mã đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");
            }

            var user = tokenEntity.User;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            // Đánh dấu token đã sử dụng
            tokenEntity.isUsed = true;
            await _resetPasswordTokenRepository.UpdateAsync(tokenEntity);

            // Thu hồi tất cả Refresh Token (đăng xuất khỏi các thiết bị)
            await _refreshTokenRepository.RevokeTokensByUserIdAsync(user.Id);

            return "Đặt lại mật khẩu thành công.";
        }

        public async Task<UserResponseProfile> GetUserByIdAsync()
        {
            var userId = _claimService.GetUserId();
            if (userId == null)
                throw new UnauthorizedException("Người dùng chưa xác thực.");
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("Không tìm thấy người dùng.");

            return new UserResponseProfile
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Phone = user.Phone,
                BankCode = user.BankCode,
                BankAccountNumber = user.BankAccountNumber,
                BankAccountName = user.BankAccountName,
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

        public async Task<LoginResponseModel> RefreshTokenAsync(TokenRequestModel tokenRequestModel)
        {
            var principal = JwtHelper.GetPrincipalFromExpiredToken(tokenRequestModel.AccessToken, _configuration);
            if (principal == null)
            {
                throw new BadRequestException("Access Token không hợp lệ.");
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new BadRequestException("Token không chứa thông tin người dùng hợp lệ.");
            }

            var storedRefreshToken = await _refreshTokenRepository.GetByTokenAsync(tokenRequestModel.RefreshToken);
            if (storedRefreshToken == null || storedRefreshToken.UserId != userId || storedRefreshToken.IsRevoked)
            {
                throw new BadRequestException("Refresh Token không hợp lệ hoặc đã bị thu hồi.");
            }

            if (storedRefreshToken.ExpiresAt < DateTime.UtcNow)
            {
                throw new BadRequestException("Refresh Token đã hết hạn. Vui lòng đăng nhập lại.");
            }

            var user = storedRefreshToken.User;
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy thông tin người dùng.");
            }

            // Thu hồi refresh token cũ (Token rotation)
            storedRefreshToken.IsRevoked = true;
            storedRefreshToken.RevokedAt = DateTime.UtcNow;
            await _refreshTokenRepository.UpdateAsync(storedRefreshToken);

            // Tạo cặp token mới
            var newAccessToken = JwtHelper.GenerateToken(user, _configuration);
            var newRefreshTokenString = JwtHelper.GenerateRefreshToken();

            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = newRefreshTokenString,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };
            await _refreshTokenRepository.AddAsync(newRefreshTokenEntity);

            return new LoginResponseModel
            {
                Token = newAccessToken,
                RefreshToken = newRefreshTokenString,
                Role = user.Role?.RoleName ?? "User"
            };
        }

        public async Task LogoutAsync()
        {
            var userId = _claimService.GetUserId();
            if (userId == null)
                throw new UnauthorizedException("Người dùng chưa xác thực.");

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("Không tìm thấy người dùng trong hệ thống.");

            // Thu hồi tất cả Refresh Token đang hoạt động của người dùng
            await _refreshTokenRepository.RevokeTokensByUserIdAsync(userId.Value);
        }

        public async Task<UserResponseProfile> UpdateProfileAsync(UpdateProfileRequestModel updateProfileRequestModel)
        {
            var userId = _claimService.GetUserId();
            if (userId == null)
                throw new UnauthorizedException("Người dùng chưa xác thực.");
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("Không tìm thấy người dùng.");

            user.FullName = updateProfileRequestModel.FullName;
            user.Phone = updateProfileRequestModel.Phone ?? string.Empty;
            user.BankCode = updateProfileRequestModel.BankCode ?? string.Empty;
            user.BankAccountNumber = updateProfileRequestModel.BankAccountNumber ?? string.Empty;
            user.BankAccountName = updateProfileRequestModel.BankAccountName ?? string.Empty;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            return new UserResponseProfile
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Phone = user.Phone,
                BankCode = user.BankCode,
                BankAccountNumber = user.BankAccountNumber,
                BankAccountName = user.BankAccountName,
            };
        }
    }
}
