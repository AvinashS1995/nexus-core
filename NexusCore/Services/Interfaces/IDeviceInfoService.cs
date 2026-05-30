using System;
using NexusCore.Model;

namespace NexusCore.Services.Interfaces
{
	public interface IDeviceInfoService
	{
        Task<DeviceInfoModel> GetDeviceInfo(string ip);
    }
}

