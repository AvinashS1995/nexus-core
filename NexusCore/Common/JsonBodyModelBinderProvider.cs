using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace NexusCore.Common
{
    public class JsonBodyModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (!context.Metadata.IsComplexType)
                return null;

            var configuration = context.Services
                .GetRequiredService<IConfiguration>();

            var binderType = typeof(JsonBodyModelBinder<>)
                .MakeGenericType(context.Metadata.ModelType);

            return (IModelBinder)Activator
                .CreateInstance(binderType, configuration);
        }
    }
}

