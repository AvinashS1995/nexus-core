using System;
using Microsoft.AspNetCore.Mvc;
using NexusCore.Common;
using OfficeOpenXml;
using Org.BouncyCastle.Asn1.Ocsp;

namespace NexusCore.Model
{
	public class CommonResponseClass : Controller
    {
        public class CommonResponse
        {
            public bool IsSucess { get; set; }
            public NexusCore.Common.StatusCodes StatusCode { get; set; }
            public string Message { get; set; }
            public dynamic Data { get; set; }
            public bool IsDynamicMessage { get; set; }
        }

        public class XlsxModel : CommonResponse
        {
            public ExcelWorksheet excelWorksheet { get; set; }
        }


        public class CodeClass
        {
            public string Code { get; set; }
            public string Message { get; set; }
        }
        public string LoadJson(NexusCore.Common.StatusCodes Code)
        {
            try
            {
                return ErrorMessages.GetMessage(Code);
            }
            catch (Exception)
            {
                return "No Message Found!!";

            }
        }
        public IActionResult ReturnResponce(bool IsSuccess, CommonResponse responcebody)
        {
            IActionResult responce = null;
            if (IsSuccess)
            {
                responce = Ok(new
                {
                    StatusCode = responcebody.StatusCode,
                    message = responcebody.IsDynamicMessage ? responcebody.Message : LoadJson(responcebody.StatusCode),
                    data = responcebody.Data
                });
            }
            else
            {
                responce = BadRequest(new
                {
                    StatusCode = responcebody.StatusCode,
                    message = responcebody.IsDynamicMessage ? responcebody.Message : LoadJson(responcebody.StatusCode),
                    data = responcebody.Data
                });
            }

            return responce;
        }

        //public IActionResult ReturnResponce(ControllerBase controller,bool isSuccess, CommonResponse responseBody)
        //{
        //    if (isSuccess)
        //    {
        //        return controller.Ok(new
        //        {
        //            StatusCode = responseBody.StatusCode,
        //            message = responseBody.IsDynamicMessage
        //                ? responseBody.Message
        //                : LoadJson(responseBody.StatusCode),
        //            data = responseBody.Data
        //        });
        //    }

        //    return controller.BadRequest(new
        //    {
        //        StatusCode = responseBody.StatusCode,
        //        message = responseBody.IsDynamicMessage
        //            ? responseBody.Message
        //            : LoadJson(responseBody.StatusCode),
        //        data = responseBody.Data
        //    });
        //}

        public class FileCommonResponse : CommonResponse
        {
            public string ErrorMessage { get; set; }
            public decimal filesize { get; set; }
        }

    }
}

