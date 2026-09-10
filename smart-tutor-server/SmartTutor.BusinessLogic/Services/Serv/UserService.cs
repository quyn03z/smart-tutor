using BCrypt.Net;
using SmartTutor.BusinessLogic.Exceptions;
using SmartTutor.BusinessLogic.Models;
using SmartTutor.BusinessLogic.Services.Impl;
using SmartTutor.DataAccess.Repositories.Impl;
using SmartTutor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static SmartTutor.BusinessLogic.Models.UserModels;

namespace SmartTutor.BusinessLogic.Services.Serv
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
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
    }
}
