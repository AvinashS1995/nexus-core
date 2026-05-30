using System;
using System.Data;
using System.Net;
using NexusCore.Model;

namespace NexusCore.Repositories.Interfaces
{
	public interface ILoginRepository
	{
        Task<DataSet> Login(UserLogin userLogin);
        Task SaveLoginLog(LoginLogModel model);
        Task UpdateRefreshToken(string empNo, string refreshToken, string refreshTokenExpiry, string sessionId, string loginIP, string deviceInfo);


    }
}

