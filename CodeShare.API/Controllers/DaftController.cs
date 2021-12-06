using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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

            return Ok();
        }

    }
}
