using System;
using NexusCore.Common;
using NexusCore.Repositories.Interfaces;
using NexusCore.Services.Interfaces;
using NexusCore.Repositories.Interfaces;
using static NexusCore.Model.CommonResponseClass;
using static NexusCore.Model.FileModel;

namespace NexusCore.Services
{
    public class CommonService : BaseService, ICommonService
    {
        protected ICommonRepository commonRepository { get; set; }
        private readonly ILogger<CommonService> _logger;
        protected IConfiguration config { get; set; }
        private FileHandler fileHandler;

        public CommonService(ICommonRepository _commonRepository, IConfiguration _configuration, ILogger<CommonService> logger)
        {
            commonRepository = _commonRepository;
            config = _configuration;
            fileHandler = new FileHandler(_configuration);
            _logger = logger;
        }

        public async Task<Stream> ReadObjectData(string folderName, string fileName)
        {
            return await fileHandler.ReadObjectData(folderName, fileName);
        }

        public async Task<CommonResponse> UploadFile(IFormFile file, string LoginEmpno)
        {
            var FileOneReturn = await fileHandler.UploadFile(file);
            string filePath = Convert.ToString(FileOneReturn.Data.filePath);
            string fileName = filePath.Substring(filePath.LastIndexOf('/'));
            if (!String.IsNullOrEmpty(fileName))
            {
                fileName = fileName.Replace(" ", "");
                fileName = fileName.Replace("-", "_");
                filePath = filePath.Substring(0, filePath.LastIndexOf('/')) + fileName;
            }

            if (!String.IsNullOrEmpty(filePath))
            {
                filePath = await Task.Run(() => fileHandler.GetPreSignedDocUrl(filePath));
            }

            var SignfilePath = filePath;

            string[] filePaths = filePath.Split('?');
            filePath = filePaths[0].Trim();

            long uploadId = await commonRepository.UploadFile(fileName,filePath,file.ContentType,file.Length, LoginEmpno);

            CommonResponse commonResponse = new CommonResponse();
            commonResponse.IsSucess = true;
            commonResponse.Data = new { filePath = filePath, SignfilePath = SignfilePath };
            commonResponse.StatusCode = Common.StatusCodes.RecordFetchedSuccessfully;
            return commonResponse;

        }

        public async Task<CommonResponse> ListingObjectsAsync()
        {
            return await fileHandler.ListingObjectsAsync();
        }

        public async Task<CommonResponse> MoveFile(string filePath, string destinationDirectory)
        {
            return await fileHandler.MoveFile(filePath, destinationDirectory);
        }

        public async Task<CommonResponse> DeleteFile(INP_BulkDeleteFileModel deleteFileModel)
        {
            return await fileHandler.DeleteFile(deleteFileModel.FileNamesWithDirectory);
        }
    }
}

