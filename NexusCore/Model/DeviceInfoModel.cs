using System;
namespace NexusCore.Model
{
    public class DeviceInfoModel
    {
        public string DeviceType { get; set; }     // Web / Mobile
        public string DeviceInfo { get; set; }     // Chrome on Windows
        public string UserAgent { get; set; }
        public string Browser { get; set; }
        public string OS { get; set; }
        public string GeoLocation { get; set; }    // Pune, Maharashtra, India
        public string ISP { get; set; }
    }


    public class IpApiResponse
    {
        public string status { get; set; }
        public string country { get; set; }
        public string regionName { get; set; }
        public string city { get; set; }
        public string timezone { get; set; }
        public string isp { get; set; }
    }

}

