using System;
using System.Text.Json;
using NexusCore.Model;
using NexusCore.Services;
using NexusCore.Services.Interfaces;

namespace NexusPlatform.Services
{
	public class DeviceInfoService : BaseService, IDeviceInfoService
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly HttpClient _http;

        public DeviceInfoService(
            IHttpContextAccessor httpContext,
            HttpClient http)
        {
            _httpContext = httpContext;
            _http = http;
        }

        public async Task<DeviceInfoModel> GetDeviceInfo(string ip)
        {
            var request = _httpContext.HttpContext.Request;
            string userAgent = request.Headers["User-Agent"].ToString();

            var device = new DeviceInfoModel
            {
                UserAgent = userAgent,
                DeviceType = GetDeviceType(userAgent),
                Browser = GetBrowser(userAgent),
                OS = GetOS(userAgent),
                DeviceInfo = $"{GetBrowser(userAgent)} on {GetOS(userAgent)}"
            };

            // 🌍 GEO LOCATION
            try
            {
                var geoJson = await _http.GetStringAsync($"http://ip-api.com/json/{ip}");
                var geo = JsonSerializer.Deserialize<IpApiResponse>(geoJson);

                if (geo?.status == "success")
                {
                    device.GeoLocation =
                        $"{geo.city}, {geo.regionName}, {geo.country}";
                    device.ISP = geo.isp;
                }
            }
            catch
            {
                device.GeoLocation = "Unknown";
            }

            return device;
        }

        // ---------------- HELPER METHODS ----------------

        private string GetDeviceType(string ua)
        {
            if (ua.Contains("Mobile")) return "Mobile";
            return "Web";
        }

        private string GetBrowser(string ua)
        {
            if (ua.Contains("Edg")) return "Edge";
            if (ua.Contains("Chrome")) return "Chrome";
            if (ua.Contains("Firefox")) return "Firefox";
            if (ua.Contains("Safari")) return "Safari";
            return "Unknown";
        }

        private string GetOS(string ua)
        {
            if (ua.Contains("Windows")) return "Windows";
            if (ua.Contains("Android")) return "Android";
            if (ua.Contains("iPhone") || ua.Contains("iOS")) return "iOS";
            if (ua.Contains("Mac")) return "macOS";
            return "Unknown";
        }

    }
}

