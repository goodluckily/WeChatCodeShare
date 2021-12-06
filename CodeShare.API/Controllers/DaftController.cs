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
                .Append('"' + "content_source_url" + '"' + ":").Append("").Append(",")
                .Append('"' + "thumb_media_id" + '"' + ":").Append("").Append(",")
                .Append('"' + "show_cover_pic" + '"' + ":").Append(0).Append(",")
                .Append('"' + "need_open_comment" + '"' + ":").Append(0).Append(",")
                .Append('"' + "only_fans_can_comment" + '"' + ":").Append(0)
                .Append("}");
            var aaa = BasicAPI.CreateDaft(token, builder);
            return Ok();
        }

    }
}
