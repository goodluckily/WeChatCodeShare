using CodeShare.API.AutoFacExtension;
using CodeShare.Common;
using CodeShare.IService;
using CodeShare.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CodeShare.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TokenController : BaseController
    {
        

        private readonly ILogger<TokenController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public TokenController(ILogger<TokenController> logger,IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }


        /// <summary>
        /// 获取 Token
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetTokenAsync")]
        public IActionResult GetTokenAsync() 
        {
            var token = GetToken();
            _logger.Log(LogLevel.Information, token);
            return Ok(token);
        }

        /// <summary>
        /// 获取 微信服务器Ip地址
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetWeiChatIp")]
        public async Task<IActionResult> GetWeiChatIpAsync()
        {
            var token = GetToken();
            var client = _httpClientFactory.CreateClient("wechatClient");
            var reponse = await client.GetAsync($"cgi-bin/get_api_domain_ip?access_token={token}");
            var result = await reponse.Content.ReadAsStringAsync();
            return Ok(result);
        }


        /// <summary>
        /// 获取 微信订阅模版列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetPubTemplateTitleList")]
        public async Task<IActionResult> GetPubTemplateTitleList()
        {
            var token = GetToken();
            var client = _httpClientFactory.CreateClient("wechatClient");
            var reponse = await client.GetAsync($"wxaapi/newtmpl/getpubtemplatetitles?access_token={token}");
            var result = await reponse.Content.ReadAsStringAsync();
            return Ok(result);
        }
    }
}
