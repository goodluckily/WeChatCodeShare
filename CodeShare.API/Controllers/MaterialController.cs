using CodeShare.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CodeShare.API.Controllers
{
    /// <summary>
    /// 素材
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class MaterialController : BaseController
    {
        public MaterialController()
        {

        }

        [HttpGet("getMaterialList")]
        public IActionResult GetMaterialList() 
        {
            var token = GetToken();

            //尚未认证 无权限获取
            var materList = BasicAPI.PostString("https://api.weixin.qq.com/cgi-bin/material/batchget_material?access_token={0}", token, "{\"type\":\"image\",\"offset\":0,\"count\":20}");
            return Ok("");
        }


        [HttpGet("updateImg")]
        public IActionResult UpdateImg(string file)
        {
            file = @"C:\Users\cy\Pictures\IQIYISnapShot\1.jpg";

            var token = GetToken();
            string fileName = file;
            string url = string.Format("https://api.weixin.qq.com/cgi-bin/media/uploadimg?access_token={0}", token);
            FileStream fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Read);
            byte[] fileByte = new byte[fs.Length];
            fs.Read(fileByte, 0, fileByte.Length);
            fs.Close();

            fileName = fs.Name;

            // 设置参数
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            CookieContainer cookieContainer = new CookieContainer();
            request.CookieContainer = cookieContainer;
            request.AllowAutoRedirect = true;
            request.Method = "POST";
            string boundary = DateTime.Now.Ticks.ToString("X");
            request.ContentType = string.Format("multipart/form-data;charset=utf-8;boundary={0}",boundary);
            byte[] itemBoundaryBytes = Encoding.UTF8.GetBytes(boundary);
            byte[] endBoundaryBytes = Encoding.UTF8.GetBytes(boundary);
            //请求头部信息
            StringBuilder sbHeader = new StringBuilder("Content-Disposition:form-data;name=media;filename=" + fileName + ";Content-Type:application/octet-stream");
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(sbHeader.ToString());
            Stream postStream = request.GetRequestStream();
            postStream.Write(itemBoundaryBytes, 0, itemBoundaryBytes.Length);
            postStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
            postStream.Write(fileByte, 0, fileByte.Length);
            postStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
            postStream.Close();

            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            Stream instream = response.GetResponseStream();
            StreamReader sr = new StreamReader(instream, Encoding.UTF8);
            string content = sr.ReadToEnd();

            //{"errcode":48001,"errmsg":"api unauthorized rid: 61b15c0d-67b7e759-286e893c"}   // 指没有权限 错误

            return Ok("");
        }
    }
}
