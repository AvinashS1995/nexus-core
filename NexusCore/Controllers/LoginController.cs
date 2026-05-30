using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusCore.Common;
using NexusCore.Controllers;
using NexusCore.Model;
using NexusCore.Services.Interfaces;
using static NexusCore.Model.CommonResponseClass;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NexusCore.Controllers
{
    [Route("api/[controller]")]
    public class LoginController : BaseController
    {
        ILoginService loginService { get; set; }
        IConfiguration _config;
        public LoginController(IConfiguration config, ILoginService _loginService) : base(config)
        {
            loginService = _loginService;
            _config = config;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody][ModelBinder(BinderType = typeof(JsonBodyModelBinder<UserLogin>))] UserLogin userLogin)
        {
            Microsoft.AspNetCore.Http.HttpContext context = HttpContext;
            var remoteIpAddress = context.Connection.RemoteIpAddress;
            CommonResponse data = await loginService.Login(userLogin, remoteIpAddress, ipAddress());
            if (data.IsSucess)
            {
                setTokenCookie(data.Data.refreshToken);
            }
            return commonResponseClass.ReturnResponce(data.IsSucess, data);
        }

        protected void setTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // true in production
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

       


    }
}

