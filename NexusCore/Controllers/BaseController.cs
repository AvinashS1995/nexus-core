using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusCore.Model;


namespace NexusCore.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Policy = nameof(Policy.Account))]
    public class BaseController : Controller
    {
        protected IConfiguration config;
        protected CommonResponseClass commonResponseClass;
        protected string _logedInEmpNo { get { return GetLogedInEmpNo(); } }

        public BaseController(IConfiguration _config)
        {
            config = _config;
            commonResponseClass = new CommonResponseClass();
        }

        protected string ipAddress()
        {
            try
            {
                if (HttpContext == null)
                    return string.Empty;

                // For Load Balancer / Proxy
                if (HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
                {
                    return HttpContext.Request.Headers["X-Forwarded-For"]
                        .FirstOrDefault();
                }

                return HttpContext.Connection.RemoteIpAddress?.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        private string GetLogedInEmpNo()
        {
            try
            {
                if (User != null && User.Identities != null)
                {
                    var identity = User.Identities.ElementAt(1) as ClaimsIdentity;
                    if (identity != null)
                    {
                        if (identity.IsAuthenticated)
                        {
                            return identity.Claims.FirstOrDefault(p => p.Type == "EmpNo")?.Value;
                        }
                    }
                }
            }
            catch { }

            return string.Empty;
        }
    }
}

