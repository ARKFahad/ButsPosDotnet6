using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Exceptions
{
    public class NotFoundResultException : Exception
    {
        public string ErrorCode { get; private set; }

        public NotFoundResultException(string message)
          : base(message)
        {

        }
        public NotFoundResultException(string message, Exception innerException, string errorCode = null)
         : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        public NotFoundResultException(string message, string errorCode)
          : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
