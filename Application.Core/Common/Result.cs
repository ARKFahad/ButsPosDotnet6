using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Core.Common
{
    public class Result : IResult
    {
        public Result()
        {
        }

        public bool Succeeded { get; set; }
        public string Message { get; set; }
      
    }

    public class Result<T> : Result, IResult<T>
    {
        public Result()
        {
        }

        public T Data { get; set; }
        public string Error { get; set; }

       
        public static Result<T> Fail(string error, string message)
        {
            return new Result<T> { Succeeded = false, Error = error, Message = message };
        }

        public static Task<Result<T>> FailAsync(string error, string message)
        {
            return Task.FromResult(Fail(error, message));
        }

        public static Result<T> Success(T data, string message)
        {
            return new Result<T> { Succeeded = true, Data = data, Message = message };
        }

        public static Task<Result<T>> SuccessAsync(T data, string message)
        {
            return Task.FromResult(Success(data, message));
        }
        
    }
}