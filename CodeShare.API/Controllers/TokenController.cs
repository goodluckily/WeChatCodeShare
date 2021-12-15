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

        /// <summary>
        ///  检查签名是否正确:
        /// </summary>
        /// <param name="signature">微信加密签名</param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="nonce">随机数</param>
        /// <param name="echostr">随机字符串</param>
        /// <returns></returns>
        [HttpGet("TokenValidation")]
        public ActionResult TokenValidation(string signature, string timestamp, string nonce, string echostr)
        {
            /// <summary>
            /// 是接受微信的事件推送，比如别人往你公众号发了一条消息，
            /// 这个时候，微信会往你配置的服务器url上推送一条消息，
            /// 里面包含了别人发送的内容。
            /// </summary>
            
            //验证token
            var token = WeChatAppSetting.Token;
            if (string.IsNullOrEmpty(token)) return Content("请先设置Token！");
            var isWeiXin = BasicAPI.Check(signature, timestamp, nonce, token);
            if (isWeiXin)
            {
                return Content(echostr); //返回随机字符串则表示验证通过
            }
            return Content("不是微信消息请求"); //返回随机字符串则表示验证通过
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
