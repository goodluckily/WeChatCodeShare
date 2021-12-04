using CodeShare.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeShare.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly ILogger<TokenController> _logger;
        private readonly ITokenService _tokenService;

        public TokenController(ILogger<TokenController> logger,ITokenService tokenService)
        {
            _logger = logger;
            _tokenService = tokenService;
        }


        [HttpGet("GetToken")]
        public IActionResult GetToken() 
        {
            var sadfas = _tokenService.GetToken(Guid.NewGuid());
            _logger.Log(LogLevel.Information, "GetToken");
            return Ok("111");
        }
    }
}
