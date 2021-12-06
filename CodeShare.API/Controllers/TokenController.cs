using CodeShare.API.AutoFacExtension;
using CodeShare.IService;
using CodeShare.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeShare.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TokenController : BaseController
    {
        private readonly ILogger<TokenController> _logger;

        public TokenController(ILogger<TokenController> logger)
        {
            _logger = logger;
        }

        [HttpGet("GetTokenAsync")]
        public IActionResult GetTokenAsync() 
        {
            var token = GetToken();
            _logger.Log(LogLevel.Information, token);
            return Ok(token);
        }
    }
}
