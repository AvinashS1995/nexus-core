using System;
namespace NexusCore.Repositories.Interfaces
{
    public interface ICommonRepository
    {
        Task<long> UploadFile(string UploadFileName, string UploadFilePath,string UploadFileType,long UploadFileSize, string LoginEmpno);
    }
}

