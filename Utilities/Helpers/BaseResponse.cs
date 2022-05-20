using Optima.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Utilities.Helpers
{
    public class BaseResponse<T>
    {
        public BaseResponse()
        {
        }
        public BaseResponse(T data, string responseMessage = null)
        {
            this.Data = data;
            this.Status = RequestExecution.Successful;
            this.ResponseMessage = responseMessage;
        }

        public BaseResponse(string error, List<string> errors = null)
        {
            this.Status = RequestExecution.Failed;
            this.ResponseMessage = error;
            this.Errors = errors;
        }
        public BaseResponse(T data, string error, List<string> errors, RequestExecution status)
        {
            this.Status = status;
            this.ResponseMessage = error;
            this.Errors = errors;
            this.Data = data;
        }
        public RequestExecution Status { get; set; }
        public T Data { get; set; }
        public string ResponseMessage { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
