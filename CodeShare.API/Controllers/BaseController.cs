using CodeShare.IService;
using CodeShare.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeShare.Model;
using CodeShare.API.AutoFacExtension;

namespace CodeShare.API.Controllers
{
    public class BaseController : ControllerBase
    {
        [CustomProperty]
        public ITokenService _tokenService { get; init; }


        [ApiExplorerSettings(IgnoreApi = true)]
        public virtual async Task<string> GetTokenAsync()
        {
            var tokenDB = await _tokenService.GetTokenByType();
            string tokenStr = string.Empty;
            var locaTime = DateTime.Now;
            if (tokenDB != null)
            {
                //开始时间比较是否过期 修改
                var thisTime = DateTime.Now;
                var dbExTime = tokenDB.EditDateTime.AddSeconds(tokenDB.Expires_In);
                if (thisTime < dbExTime) return tokenDB.Access_Token;

                //Edit
                var result = GetAccessTokenAndTime();
                tokenDB.Access_Token = result.access_token;
                tokenDB.Expires_In = result.expires_in;
                tokenDB.EditDateTime = locaTime.AddMinutes(-10);//10分钟
                var editToken = await _tokenService.UpdateAsync(tokenDB);
                return tokenDB.Access_Token;
            }
            else
            {
                var result = GetAccessTokenAndTime();
                var addToken = await _tokenService.CreateTokenAsync(new Token()
                {
                    WeiChatType = WeiChatEnum.CodeShare,
                    TokenType = TokenEnum.Token,
                    Access_Token = result.access_token,
                    Expires_In = result.expires_in,
                    EditDateTime = locaTime.AddMinutes(-10)//10分钟
                });
                tokenStr = addToken.Access_Token;
            }
            return tokenStr;
        }

        private (string access_token, double expires_in) GetAccessTokenAndTime()
        {
            var accessDynamic = BasicAPI.GetAccessToken(WeChatAppSetting.Appid, WeChatAppSetting.AppSecret);
            var access_token = accessDynamic.access_token;
            var expires_in = accessDynamic.expires_in;
            return (access_token, expires_in);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public virtual string GetJsToken()
        {
            return "";
        }
    }
}
