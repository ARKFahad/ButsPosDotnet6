using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Exceptions
{
   public class UnauthorizationException : Exception
    {
        public string ErrorCode { get; private set; }

        public UnauthorizationException(string message)
        : base(message)
        {

        }

        public UnauthorizationException(string message, Exception innerException, string errorCode = null)
        : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

    }
}
