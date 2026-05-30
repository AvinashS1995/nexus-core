using System;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using NexusCore.Model;

namespace NexusCore.Common
{
    public class JsonBodyModelBinder<T> : IModelBinder
    {
        private readonly IConfiguration _configuration;

        public JsonBodyModelBinder(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType != typeof(T))
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }

            try
            {
                string requestBody;

                bool isEncrypted = _configuration
                    .GetValue<bool>("RequestResponseSecurity:IsEncrypted");

                using var reader = new StreamReader(
                    bindingContext.HttpContext.Request.Body);

                var rawJson = await reader.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(rawJson))
                {
                    bindingContext.Result = ModelBindingResult.Failed();
                    return;
                }

                if (isEncrypted)
                {
                    var requestData =
                        JsonConvert.DeserializeObject<RequestDataModel>(rawJson);

                    requestBody =
                        EncryptDecrypt.DecryptString(requestData.data);
                }
                else
                {
                    requestBody = rawJson;
                }

                var model = JsonConvert.DeserializeObject<T>(
                    requestBody,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.None
                    });

                bindingContext.Result =
                    ModelBindingResult.Success(model);
            }
            catch (Exception ex)
            {
                bindingContext.ModelState
                    .AddModelError("JsonBodyModelBinder", ex.Message);

                bindingContext.Result =
                    ModelBindingResult.Failed();
            }
        }
    }
}

   


