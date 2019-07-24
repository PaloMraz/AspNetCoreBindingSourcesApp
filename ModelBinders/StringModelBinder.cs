using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreBindingSourcesApp.ModelBinders
{
    public class StringModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            string modelName = bindingContext.ModelName;
            if (string.IsNullOrWhiteSpace(modelName))
            {
                return Task.CompletedTask;
            }

            ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            string value = valueProviderResult.FirstValue + $" (sufix from {nameof(StringModelBinder)})";
            bindingContext.Result = ModelBindingResult.Success(value);

            return Task.CompletedTask;
        }


        public class StringModelBinderProvider : IModelBinderProvider
        {
            public IModelBinder GetBinder(ModelBinderProviderContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                if (context.Metadata.ModelType == typeof(string))
                {
                    return new StringModelBinder();
                }

                return null;
            }
        }
    }
}
