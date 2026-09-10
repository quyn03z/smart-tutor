using SmartTutor.BusinessLogic.Services.Impl;
using SmartTutor.DataAccess.Repositories.Impl;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartTutor.BusinessLogic.Services.Serv
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }



    }
}
