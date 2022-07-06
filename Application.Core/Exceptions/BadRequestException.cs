using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Exceptions
{
    public class BadRequestException : Exception
    {
        public string ErrorCode { get; private set; }

        public BadRequestException(string message)
          : base(message)
        {

        }
        public BadRequestException(string message, Exception innerException, string errorCode = null)
         : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        public BadRequestException(string message, string errorCode)
          : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
