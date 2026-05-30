using System;
using static NexusCore.Model.CommonResponseClass;
using static NexusCore.Model.FileModel;

namespace NexusCore.Services.Interfaces
{
	public interface ICommonService
	{
        Task<CommonResponse> UploadFile(IFormFile file, string LoginEmpno);
        Task<Stream> ReadObjectData(string folderName, string fileName);
        Task<CommonResponse> ListingObjectsAsync();
        Task<CommonResponse> MoveFile(string filePath, string destinationDirectory);
        Task<CommonResponse> DeleteFile(INP_BulkDeleteFileModel deleteFileModel);
    }
}

