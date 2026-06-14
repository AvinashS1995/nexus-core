using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NexusCore.Controllers;
using NexusCore.Services;
using NexusCore.Services.Interfaces;
using static NexusCore.Model.FileModel;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NexusCore.Controllers
{
    [Route("api/common")]
    public class CommonController : BaseController
    {
        private readonly ICommonService commonService;
        private readonly ILogger<CommonController> _logger;
        public CommonController(IConfiguration _config, ICommonService _commonService, ILogger<CommonController> logger) : base(_config)
        {
            _logger = logger;
            commonService = _commonService;
        }

        [HttpPost("UploadFile")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFile([FromForm] INP_UploadFile model)
        {
            var commonResponse = await commonService.UploadFile(model.File, _logedInEmpNo);
            return commonResponseClass.ReturnResponce(commonResponse.IsSucess, commonResponse);
        }

        [HttpPost("DeleteFile")]
        public async Task<IActionResult> DeleteFile(INP_BulkDeleteFileModel deleteFileModel)
        {
            var commonResponse = await commonService.DeleteFile(deleteFileModel);
            return commonResponseClass.ReturnResponce(commonResponse.IsSucess, commonResponse);
        }
    }
}

