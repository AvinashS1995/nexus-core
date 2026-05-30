using System;
using System.Data;
using System.Net;
using NexusCore.Common;
using NexusCore.Model;
using NexusCore.Repositories;
using NexusCore.Repositories.Interfaces;

namespace NexusCore.Repositories
{
    public class LoginRepository : BaseRepository<UserLogin>, ILoginRepository
    {
        public LoginRepository(DbContext mySqlDatabase) : base(mySqlDatabase)
        {
        }

        public async Task<DataSet> Login(UserLogin userLogin)
        {
            using (var command = CreateCommand())
            {
                command.CommandText = "SPUserLogin";
                command.CommandType = CommandType.StoredProcedure;
                userLogin.Password = EncryptDecrypt.EncryptString(userLogin.Password);
                AddParameter(command, "EmpNo", userLogin.EmpNo);
                AddParameter(command, "Password", userLogin.Password);
                AddParameter(command, "FrontendVersion", userLogin.FrontendVersion);

                return await ExecuteDataSetAsync(command);

           
            }
        }

        public async Task UpdateRefreshToken(string empNo, string refreshToken, string refreshTokenExpiry, string sessionId, string loginIP, string deviceInfo)
        {
            using var command = CreateCommand();
            command.CommandText = "SPUpdateRefreshToken";
            command.CommandType = CommandType.StoredProcedure;

            AddParameter(command, "EmpNo", empNo);
            AddParameter(command, "RefreshToken", refreshToken);
            AddParameter(command, "RefreshTokenExpiry", refreshTokenExpiry);
            AddParameter(command, "SessionID", sessionId);
            AddParameter(command, "LoginIP", loginIP);
            AddParameter(command, "DeviceInfo", deviceInfo);

            await ExecuteNonQueryAsync(command);
        }


        public async Task SaveLoginLog(LoginLogModel model)
        {
            using (var command = CreateCommand())
            {
                command.CommandText = "SPSaveLoginLog";
                command.CommandType = CommandType.StoredProcedure;

                AddParameter(command, "EmpNo", model.EmpNo);
                AddParameter(command, "LoginEmpID", model.LoginEmpID);
                AddParameter(command, "CityID", model.CityID);
                AddParameter(command, "IsSuccess", model.IsSuccess ? 1 : 0);
                AddParameter(command, "ResponseCode", model.ResponseCode);
                AddParameter(command, "RemainingAttempt", model.RemainingAttempt);
                AddParameter(command, "Unlocktime", model.Unlocktime);
                AddParameter(command, "IsMasterLogin", model.IsMasterLogin ? 1 : 0);
                AddParameter(command, "SessionID", model.SessionID);
                AddParameter(command, "RefreshToken", model.RefreshToken);
                AddParameter(command, "RefreshTokenExpiry", model.RefreshTokenExpiry);
                AddParameter(command, "LoginIP", model.LoginIP);
                AddParameter(command, "DeviceInfo", model.DeviceInfo);
                AddParameter(command, "UserAgent", model.UserAgent);
                AddParameter(command, "Browser", model.Browser);
                AddParameter(command, "OS", model.OS);
                AddParameter(command, "GeoLocation", model.GeoLocation);
                AddParameter(command, "ISP", model.ISP);
                AddParameter(command, "CreatedBy", model.CreatedBy);

                await ExecuteNonQueryAsync(command);
            }
        }

    }
}

