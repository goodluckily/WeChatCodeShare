using CodeShare.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeShare.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DaftController : BaseController
    {
        public DaftController()
        {

        }


        /// <summary>
        /// 新建草稿
        /// </summary>
        /// <returns></returns>
        [HttpPost("createDaft")]
        public IActionResult CreateDaft()
        {
            var token = GetToken();
            var builder = new StringBuilder();
            builder
                .Append("{")
                .Append('"' + "title" + '"' + ":").Append("标题").Append(",")
                .Append('"' + "author" + '"' + ":").Append("作者").Append(",")
                .Append('"' + "digest" + '"' + ":").Append("图文消息的摘要，仅有单图文消息才有摘要，多图文此处为空。如果本字段为没有填写，则默认抓取正文前54个字。").Append(",")
                .Append('"' + "content" + '"' + ":").Append("图文消息的具体内容，支持HTML标签，必须少于2万字符，小于1M，且此处会去除JS,涉及图片url必须来源 上传图文消息内的图片获取URL 接口获取。外部图片url将被过滤").Append(",")
                .Append('"' + "content_source_url" + '"' + ":").Append("https://www.cnblogs.com/goodluckily/").Append(",")//图文消息的原文地址，即点击“阅读原文”后的URL
                .Append('"' + "thumb_media_id" + '"' + ":").Append("ud-ONaDDSgEsucCzqwk2WrzinHQFqcQ6LgG9ZtDRmzJYEjRv1Vxn5F4mRpkiHRL6").Append(",")//图文消息的封面图片素材id（必须是永久MediaID）
                .Append('"' + "show_cover_pic" + '"' + ":").Append(0).Append(",")//是否显示封面，0为false，即不显示，1为true，即显示(默认)
                .Append('"' + "need_open_comment" + '"' + ":").Append(0).Append(",")//Uint32 是否打开评论，0不打开(默认)，1打开
                .Append('"' + "only_fans_can_comment" + '"' + ":").Append(0)//Uint32 是否粉丝才可评论，0所有人可评论(默认)，1粉丝才可评论
                .Append("}");
            var aaa = BasicAPI.CreateDaft(token, builder);

            //{{"errcode":48001,"errmsg":"api unauthorized rid: 61b15daa-63695829-705b4ac9"}}

            return Ok();
        }

    }
}
