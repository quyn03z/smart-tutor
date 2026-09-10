using System;
using System.Collections.Generic;
using System.Text;

namespace SmartTutor.BusinessLogic.Exceptions
{
    public class NotFoundException(string message) : Exception(message)
    {
    }
}
