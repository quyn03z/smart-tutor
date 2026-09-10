using System;
using System.Collections.Generic;
using System.Text;

namespace SmartTutor.BusinessLogic.Exceptions
{
    [Serializable]
    public class BadRequestException(string message) : Exception(message);
}
