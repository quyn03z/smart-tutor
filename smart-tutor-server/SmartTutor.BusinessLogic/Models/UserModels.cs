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
            public string Email { get; set; }
            [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
            public string Password { get; set; }
        }

        public class LoginResponseModel
        {

            public string Token { get; set; }
            public string RefreshToken { get; set; }
            public string Role { get; set; }
            public List<string> Permissions { get; set; }
        }

    }
}
