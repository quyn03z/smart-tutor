using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SmartTutor.BusinessLogic.Models
{
    public class UserModels
    {
        public class CreateUserResponseModel
        {
            public int Id { get; set; }
        }

        public class CreateUserModel
        {
            [Required(ErrorMessage = "Tên nhập là bắt buộc.")]
            public string FullName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email là bắt buộc.")]
            [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
            [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
            [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$", ErrorMessage = "Mật khẩu phải có chữ hoa, chữ thường và số.")]
            public string Password { get; set; } = string.Empty;

            [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public class LoginUserModel
        {
            [Required(ErrorMessage = "Email đăng nhập là bắt buộc.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
            public string Password { get; set; } = string.Empty;
        }

        public class LoginResponseModel
        {
            public string Token { get; set; } = string.Empty;
            public string RefreshToken { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public List<string> Permissions { get; set; } = new List<string>();
        }

        public class ChangePassWordModel
        {
            [Required(ErrorMessage = "Mật khẩu cũ là bắt buộc.")]
            public string OldPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
            [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$", ErrorMessage = "Mật khẩu phải có chữ hoa, chữ thường và số.")]
            public string NewPassword { get; set; } = string.Empty;

            [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
            public string ConfirmNewPassword { get; set; } = string.Empty;
        }

        public class UpdateProfileRequestModel
        {
            public string? FullName { get; set; }
            public string? Phone { get; set; }
            public string? BankCode { get; set; }
            public string? BankAccountNumber { get; set; }
            public string? BankAccountName { get; set; }
        }

        public class UserResponseProfile
        {
            public string? FullName { get; set; } 
            public string Email { get; set; } = string.Empty;
            public string? Phone { get; set; }
            public string? BankCode { get; set; }
            public string? BankAccountNumber { get; set; }
            public string? BankAccountName { get; set; }
        }

        public class EmailRequest
        {
            [Required(ErrorMessage = "Email là bắt buộc.")]
            [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
            public string Email { get; set; } = string.Empty;
        }

        public class ForgotPassWordModel
        {
            public string ResetToken { get; set; } = string.Empty;
            public DateTime ExpiredAt { get; set; }
        }

        public class ResetPassWordRequestModel
        {
            [Required(ErrorMessage = "Email là bắt buộc.")]
            [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Reset token là bắt buộc.")]
            public string ResetToken { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
            [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$", ErrorMessage = "Mật khẩu phải có chữ hoa, chữ thường và số.")]
            public string NewPassword { get; set; } = string.Empty;

            [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
            public string ConfirmNewPassword { get; set; } = string.Empty;
        }

        public class TokenRequestModel
        {
            [Required(ErrorMessage = "Access Token là bắt buộc.")]
            public string AccessToken { get; set; } = string.Empty;

            [Required(ErrorMessage = "Refresh Token là bắt buộc.")]
            public string RefreshToken { get; set; } = string.Empty;
        }
    }
}
