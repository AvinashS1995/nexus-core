using System;
using System.Net;
using NexusCore.Model;
using static NexusCore.Model.CommonResponseClass;

namespace NexusCore.Services.Interfaces
{
	public interface ILoginService
	{
        Task<CommonResponse> Login(UserLogin userLogin, IPAddress remoteIp, string ipAddress);

    }
}

