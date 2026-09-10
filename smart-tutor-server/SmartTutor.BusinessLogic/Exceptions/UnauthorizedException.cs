using System;
using System.Collections.Generic;
using System.Text;

namespace SmartTutor.BusinessLogic.Exceptions
{
    [Serializable]
    public class UnauthorizedException(string message) : Exception(message);
}
