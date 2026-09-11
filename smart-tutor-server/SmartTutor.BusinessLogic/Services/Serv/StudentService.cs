using SmartTutor.BusinessLogic.Services.Impl;
using SmartTutor.DataAccess.Repositories.Impl;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartTutor.BusinessLogic.Services.Serv
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;

        public StudentService(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }


    }
}
