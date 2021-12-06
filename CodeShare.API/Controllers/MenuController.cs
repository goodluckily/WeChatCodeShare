using CodeShare.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeShare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : BaseController
    {
        public MenuController()
        {

        }

        /// <summary>
        /// 创建菜单
        /// </summary>
        /// <returns></returns>
        [HttpPost("createMenu")]
        public IActionResult CreateMenu()
        {
            var token =  GetToken();
            string rootPath = Environment.CurrentDirectory + "\\menu.json";
            var fullJson = GetContent(rootPath);
            try
            {
                var result = BasicAPI.CreateMenu(token, fullJson);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        /// <summary>
        /// 删除菜单
        /// </summary>
        /// <returns></returns>
        [HttpDelete("deleteMenu")]
        public IActionResult DeleteMenu()
        {
            var token = GetToken();
            try
            {
                var result = BasicAPI.DeleteMenu(token);
                return Ok(result.errmsg);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }
    }
}
