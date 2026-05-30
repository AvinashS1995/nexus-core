using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using NexusCore.Model;
using NexusCore.Repositories.Interfaces;
using NexusCore.Services.Interfaces;
using static NexusCore.Model.CommonResponseClass;
using NexusCore.Common;
using System.Data;
using Serilog;

namespace NexusCore.Services
{
    public class LoginService : BaseService, ILoginService
    {
        private readonly ILoginRepository _loginRepository;
        private readonly IConfiguration _config;
        private readonly IDeviceInfoService _deviceInfoService;

        public LoginService(ILoginRepository loginRepository, IConfiguration config, IDeviceInfoService deviceInfoService)
        {
            _loginRepository = loginRepository;
            _config = config;
            _deviceInfoService = deviceInfoService;
        }

        public async Task<CommonResponse> Login(UserLogin userLogin, IPAddress remoteIp, string ipAddress)
        {
            CommonResponse response = new CommonResponse();
            LoginEmployeeViewDataModel employeeViewDataModel = new LoginEmployeeViewDataModel();

            DataSet ds = await _loginRepository.Login(userLogin);

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            {
                response.IsSucess = false;
                response.StatusCode = NexusCore.Common.StatusCodes.SomethingWentWrong;
                return response;
            }

            // 2️⃣ Table 1 → Login Result
            LoginResultModel loginResult =
                ConvertToList<LoginResultModel>(ds.Tables[0]).FirstOrDefault();

            if (loginResult == null)
            {
                response.IsSucess = false;
                response.StatusCode = NexusCore.Common.StatusCodes.SomethingWentWrong;
                return response;
            }


            // 🔥 ALWAYS TRUST ResponseCode FROM SP
            if (loginResult.ResponseCode != (int)NexusCore.Common.StatusCodes.LoginSuccessfully)
            {
                response.IsSucess = false;
                response.StatusCode = (NexusCore.Common.StatusCodes)loginResult.ResponseCode;

                // OPTIONAL: attach extra info for UI
                response.Data = new
                {
                    remainingAttempt = loginResult.RemainingAttempt,
                    unlockTime = loginResult.Unlocktime
                };

                return response;
            }

            // 3️⃣ Table 2 → Employee Details (ONLY when 2001)
            if (ds.Tables.Count < 2 || ds.Tables[1].Rows.Count == 0)
            {
                response.IsSucess = false;
                response.StatusCode = NexusCore.Common.StatusCodes.SomethingWentWrong;
                return response;
            }

            LoginEmployeeViewDataModel employee =
                ConvertToList<LoginEmployeeViewDataModel>(ds.Tables[1]).FirstOrDefault();

            if (employee == null)
            {
                response.IsSucess = false;
                response.StatusCode = NexusCore.Common.StatusCodes.SomethingWentWrong;
                return response;
            }

            // 4️⃣ Generate JWT
            TokenData tokenData = GenerateTokenData(employee);

            // 5️⃣ Generate Refresh Token + Session
            string refreshToken = Convert.ToBase64String(
                System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));

            string refreshTokenExpiry =
                DateTime.UtcNow.AddDays(7).ToString("yyyyMMddHHmmss");

            string sessionId = Guid.NewGuid().ToString();

            var deviceInfo = await _deviceInfoService.GetDeviceInfo(ipAddress);


            // 6️⃣ UPDATE REFRESH TOKEN IN DB
            await _loginRepository.UpdateRefreshToken(
                employee.EmpNo,
                refreshToken,
                refreshTokenExpiry,
                sessionId,
                ipAddress,
                deviceInfo.DeviceInfo
            );

            // 7️⃣ SAVE LOGIN LOG
            await _loginRepository.SaveLoginLog(new LoginLogModel
            {
                EmpNo = userLogin.EmpNo,
                LoginEmpID = loginResult.LoginEmpID,
                CityID = loginResult.CityID,
                IsSuccess = true,
                ResponseCode = (int)NexusCore.Common.StatusCodes.LoginSuccessfully,
                RemainingAttempt = loginResult.RemainingAttempt,
                Unlocktime = loginResult.Unlocktime,
                IsMasterLogin = loginResult.IsMasterLogin,
                SessionID = sessionId,
                RefreshToken = refreshToken,
                RefreshTokenExpiry = refreshTokenExpiry,
                LoginIP = ipAddress,
                DeviceInfo = deviceInfo.DeviceInfo,
                UserAgent = deviceInfo.UserAgent,
                Browser = deviceInfo.Browser,
                OS = deviceInfo.OS,
                GeoLocation = deviceInfo.GeoLocation,
                ISP =  deviceInfo.ISP,
                CreatedBy = userLogin.EmpNo
            });

            // 8️⃣ SUCCESS RESPONSE
            response.IsSucess = true;
            response.StatusCode = NexusCore.Common.StatusCodes.LoginSuccessfully;
            response.Data = new
            {
                token = tokenData.AccessToken,
                expiresIn = tokenData.ExpiresIn,

                refreshToken = refreshToken,
                refreshTokenExpiry = refreshTokenExpiry,

                loginEmpID = loginResult.LoginEmpID,
                cityID = loginResult.CityID,
                isMasterLogin = loginResult.IsMasterLogin,
                remainingAttempt = loginResult.RemainingAttempt
            };

            return response;

        }

        private TokenData GenerateTokenData(LoginEmployeeViewDataModel user)
        {
            // Config (already verified OK)
            string jwtKey = _config["Jwt:Key"];
            string issuer = _config["Jwt:Issuer"];
            string audience = _config["Jwt:Audience"];
            int expiryMinutes = Convert.ToInt32(_config["Jwt:TokenExpiryMinutes"]);

            // 🔐 NULL-SAFE CLAIMS (THIS FIXES THE 500 ERROR)
            var claims = new[]
            {
                new Claim("EmpNo", user.EmpNo ?? string.Empty),
                new Claim("FirstName", user.FirstName ?? string.Empty),
                new Claim("MiddleName", user.MiddleName ?? string.Empty),
                new Claim("LastName", user.LastName ?? string.Empty),
                new Claim("Mobile", user.Mobile ?? string.Empty),
                new Claim("Email", user.Email ?? string.Empty),
                new Claim("Pincode", user.Pincode ?? string.Empty),
                new Claim("Gender", user.Gender ?? string.Empty),
                new Claim("Division", user.Division ?? string.Empty),
                new Claim("RoleID", user.RoleID.ToString()),
                new Claim("Role", user.Role ?? string.Empty),
                new Claim("DesignationID", user.DesignationID.ToString()),
                new Claim("Designation", user.Designation ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role ?? string.Empty)
            };

                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey)
                );

                var creds = new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256
                );

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                    signingCredentials: creds
                );

                return new TokenData
                {
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                    ExpiresIn = token.ValidTo
                };
        }

        





    }
}

