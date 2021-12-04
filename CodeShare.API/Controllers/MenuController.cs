using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// 创建菜单--自定义
        /// </summary>
        /// <returns></returns>
        [HttpPost("createMenu")]
        public IActionResult CreateMenu() 
        {
            return Ok("");
        }
    }
}
