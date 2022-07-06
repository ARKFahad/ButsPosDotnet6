using System.Collections.Generic;

namespace Application.Core.Common { 

    public interface IResult
    {
        bool Succeeded { get; set; }
        string Message { get; set; }
        
       
    }

    public interface IResult<out T> : IResult
    {
        T Data { get; }
        string Error { get; set; }
    }
}