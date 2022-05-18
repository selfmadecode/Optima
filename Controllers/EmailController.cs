using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.Config;
using Optima.Services.Interface;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EmailController : BaseController
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }
        [HttpPost]
        [ProducesResponseType(typeof(BaseResponse<string>), 200)]
        public async Task<IActionResult> Send()
        {
            try
            {
               var des = new List<string>();
                des.Add("kentekz61@gmail.com");

                string[] replacements = { };
                return ReturnResponse(await _emailService.SendMail(des, replacements, "TEST MAIL", EmailTemplateUrl.Test));                
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }
    }
}
