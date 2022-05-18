using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Optima.Models.Enums;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Optima.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        private readonly ILog _logger;
        public BaseController()
        {
            _logger = LogManager.GetLogger(typeof(BaseController));
        }
        protected IActionResult ReturnResponse(dynamic model)
        {
            if (model.Status == RequestExecution.Successful)
            {
                return Ok(model);
            }

            return BadRequest(model);
        }

        protected Guid UserId
        {
            get { return Guid.Parse(User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value); }
            //get { return Guid.Parse(CurrentUser.Identities.FirstOrDefault(c => c.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value); }
        }

        protected DateTime CurrentDateTime
        {
            get
            {
                return DateTime.UtcNow;
            }
        }

        protected IActionResult HandleError(Exception ex, string customErrorMessage = null)
        {
            _logger.Error(ex.Message, ex);


            BaseResponse<string> rsp = new BaseResponse<string>();
            rsp.Status = RequestExecution.Error;
#if DEBUG
            rsp.Errors = new List<string>() { $"Error: {(ex?.InnerException?.Message ?? ex.Message)} --> {ex?.StackTrace}" };
            return Ok(rsp);
#else
             rsp.Errors = new List<string>() { "An error occurred while processing your request!"};
             return Ok(rsp);
#endif
        }
    }   
}