using CodeShare.Common;
using CodeShare.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace CodeShare.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WeixinController : BaseController
    {
        /// <summary>
        ///  检查签名是否正确:
        /// </summary>
        /// <param name="signature">微信加密签名</param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="nonce">随机数</param>
        /// <param name="echostr">随机字符串</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index([FromQuery] PostUrlParameters param)
        {
            /// <summary>
            /// 是接受微信的事件推送，比如别人往你公众号发了一条消息，
            /// 这个时候，微信会往你配置的服务器url上推送一条消息，
            /// 里面包含了别人发送的内容。
            /// </summary>
            //验证token
            var token = WeChatAppSetting.Token;
            if (string.IsNullOrEmpty(token)) return Content("请先设置Token！");
            var isWeiXin = BasicAPI.Check(param.signature, param.timestamp, param.nonce, token);
            if (isWeiXin)
            {
                return Content(param.echostr); //返回随机字符串则表示验证通过
            }
            return Content("不是微信消息请求"); //返回随机字符串则表示验证通过
        }


        /// <summary>
        /// 用于处理微信公众号发送的实质性消息
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> IndexAsync([FromQuery]PostUrlParameters param)
        {
            try
            {
                #region 验证消息真伪

                var token = WeChatAppSetting.Token;
                var isWeiXin = BasicAPI.Check(param.signature, param.timestamp, param.nonce, token);
                if (!isWeiXin)
                {
                    return Content("不是微信消息请求");
                }
                
                #endregion

                StreamReader stream = new StreamReader(Request.Body, Encoding.UTF8);
                string body = await stream.ReadToEndAsync();

                #region 解析
                var msgXml = XElement.Parse(body);
                var wxMessage = new WxMessage();
                wxMessage.ToUserName = msgXml.Element("ToUserName").Value;
                wxMessage.FromUserName = msgXml.Element("FromUserName").Value;
                wxMessage.CreateTime = Int64.Parse(msgXml.Element("CreateTime").Value);
                wxMessage.MsgType = msgXml.Element("MsgType").Value;
                wxMessage.MsgId = Int64.Parse(msgXml.Element("MsgId")?.Value);
                wxMessage.MediaId = msgXml.Element("MediaId")?.Value;
                wxMessage.Format = msgXml.Element("Format")?.Value;
                wxMessage.Recognition = msgXml.Element("Recognition")?.Value; 
                #endregion

                var sentContent = string.Empty;

                //回复
                switch (wxMessage.MsgType.ToLower())
                {
                    case "text":
                        //ToUserName 和 FromUserName 调换就好
                        sentContent = returnTextMessage(wxMessage, "请百度搜索一下,text");
                        break;
                    case "image"://图片消息
                        sentContent = "";
                        break;
                    case "voice"://语音消息
                        sentContent = returnTextMessage(wxMessage, "请百度搜索一下,voice");
                        break;
                    case "video"://视频消息

                        break;
                    case "shortvideo"://小视频

                        break;
                    case "location"://地理位置

                        break;
                    case "link"://链接消息

                        break;
                }

                return Content(sentContent);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }
        /// <summary>
        /// 文本消息
        /// </summary>
        /// <returns></returns>
        private string returnTextMessage(WxMessage wxMessage, string content) 
        {
            return @$"<xml>
                            <ToUserName><![CDATA[{wxMessage.FromUserName}]]></ToUserName>
                            <FromUserName><![CDATA[{wxMessage.ToUserName}]]></FromUserName>
                            <CreateTime>{DateTime.Now.Ticks}</CreateTime>
                            <MsgType><![CDATA[{wxMessage.MsgType}]]></MsgType>
                            <Content><![CDATA[{content}]]></Content>
                   </xml>";
        }

        /// <summary>
        /// 语音消息
        /// </summary>
        /// <returns></returns>
        private string returnVoiceMessage(WxMessage wxMessage, string content)
        {
            return @$"<xml>
                            <ToUserName><![CDATA[{wxMessage.FromUserName}]]></ToUserName>
                            <FromUserName><![CDATA[{wxMessage.ToUserName}]]></FromUserName>
                            <CreateTime>{DateTime.Now.Ticks}</CreateTime>
                            <MsgType><![CDATA[{wxMessage.MsgType}]]></MsgType>
                            <Content><![CDATA[{content}]]></Content>
                   </xml>";
        }
    }

    public class WxMessage
    {
        /// <summary>
        /// 本公众帐号
        /// </summary>
        public string ToUserName { get; set; }
        /// <summary>
        /// 用户帐号
        /// </summary>
        public string FromUserName { get; set; }
        /// <summary>
        /// 发送时间戳
        /// </summary>
        public long CreateTime { get; set; }
        /// <summary>
        /// 发送的文本内容 
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 消息的类型
        /// </summary>
        public string MsgType { get; set; }

        /// <summary>
        /// 消息id，64位整型
        /// </summary>
        public long MsgId { get; set; }

        /// <summary>
        /// 语音格式：amr
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// 语音识别结果，UTF8编码
        /// </summary>
        public string Recognition { get; set; }

        /// <summary>
        /// 消息媒体id，可以调用获取临时素材接口拉取数据
        /// </summary>
        public string MediaId { get; set; }
    }

    /// <summary>
    /// 微信服务器 POST XML 消息请求的网址中的参数
    /// </summary>
    public class PostUrlParameters
    {
        /// <summary>
        /// 签名串
        /// </summary>
        public string signature { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public string openid { get; set; }

        /// <summary>
        /// 随机数
        /// </summary>
        public string nonce { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public string timestamp { get; set; }

        public string echostr { get; set; }
    }
}
