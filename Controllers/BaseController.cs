using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Optima.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected IActionResult ReturnResponse(dynamic model)
        {
            if (model.Status == RequestExecution.Successful)
            {
                return Ok(model);
            }

            return BadRequest(model);
        }
    }   
}