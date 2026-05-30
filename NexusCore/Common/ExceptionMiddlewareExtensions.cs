using System;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;

namespace NexusCore.Common
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    // ✅ HTTP STATUS (ASP.NET)
                    context.Response.StatusCode =
                        Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError;

                    context.Response.ContentType = "application/json";

                    var errorFeature =
                        context.Features.Get<IExceptionHandlerFeature>();

                    if (errorFeature != null)
                    {
                        // ✅ BUSINESS STATUS (YOUR ENUM)
                        var businessCode = StatusCodes.SomethingWentWrong;

                        var response = new
                        {
                            StatusCode = (int)businessCode,
                            Message = ErrorMessages.GetMessage(businessCode),
                            Error = errorFeature.Error.Message
                        };

                        await context.Response.WriteAsync(
                            JsonConvert.SerializeObject(response));
                    }
                });
            });
        }
    }
}

