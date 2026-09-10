using System;
using System.Collections.Generic;
using System.Text;

namespace SmartTutor.BusinessLogic.Models
{
    public class ApiResult<T>
    {
        private ApiResult() { }

        private ApiResult(bool succeeded, T result, IEnumerable<string> errors, string message = null)
        {
            Succeeded = succeeded;
            Result = result;
            Errors = errors;
            Message = message;
        }

        public bool Succeeded { get; set; }

        public T Result { get; set; }

        public IEnumerable<string> Errors { get; set; }

        public string Message { get; set; }

        public static ApiResult<T> Success(T result, string message = null)
        {
            return new ApiResult<T>(true, result, new List<string>(), message);
        }

        public static ApiResult<T> Failure(IEnumerable<string> errors, string message = null)
        {
            return new ApiResult<T>(false, default, errors, message);
        }

        public static ApiResult<T> Failure(string message)
        {
            return new ApiResult<T>(false, default, new List<string> { message }, message);
        }

    }
}
