# Bug: Custom model binder not invoked for AJAX POST request with JSON payload

Note: The following description uses code from sample ASP.NET Core 2.1 MVC application that is located here:
https://github.com/PaloMraz/AspNetCoreBindingSourcesApp

## Description

When POSTing data to an action method with application/json payload, custom `IModelBinder` **is not** used. Whe POSTing to an action method with `application/x-www-form-urlencoded` payload, custom `IModelBinder` **is** used. IMHO, the custom model binder should be used in both cases.

The viewmodel - Models/PersonViewModel.cs:
```
public class PersonViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }

    public override string ToString()
    {
        return $"Id={Id}, Name={Name}";
    }
}
```

The controller - Controllers/PersonController.cs:
```
public class PersonController : Controller
{
    public IActionResult Index()
    {
        return this.View();
    }


    [HttpPost]
    public IActionResult PostAsJson([FromBody] PersonViewModel model)
    {            
        return this.Ok($"{model} (posted to {nameof(PostAsJson)})");
    }


    [HttpPost]
    public IActionResult PostAsForm(PersonViewModel model)
    {
        return this.Ok($"{model} (posted to {nameof(PostAsForm)})");
    }
}
```

The jQuery code posting the data - Views/Person/Index.cshtml"
```
$(document).ready(function () {
    var resultSpan = $("#result");
    $("#submit_as_json").click(function () {
        var data = JSON.stringify({ Id: 1, Name: "Fred" });
        postData(data, '@Url.Action("PostAsJson")', 'application/json');
    });
    $("#submit_as_form").click(function () {
        var data = "Id=1&Name=Fred";
        postData(data, '@Url.Action("PostAsForm")');
    });

    function postData(data, url, contentType) {
        var settings = {
            type: "POST",
            url: url,
            data: data,
            success: function (data) {
                resultSpan.text("Success: " + data);
            },
            error: function (error) {
                resultSpan.text("Failure: " + error);
            }
        };
        if (contentType) {
            settings.contentType = contentType;
        }
        $.ajax(settings);
    }
});
```

The model binder - ModelBinders/StringModelBinder.cs:
```
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
```

Model binder registration - Startup.cs:
```
public void ConfigureServices(IServiceCollection services)
{
    services.Configure<CookiePolicyOptions>(options =>
    {
        // This lambda determines whether user consent for non-essential cookies is needed for a given request.
        options.CheckConsentNeeded = context => true;
        options.MinimumSameSitePolicy = SameSiteMode.None;
    });


    services
        .AddMvc(options =>
        {
            options.ModelBinderProviders.Insert(0, new StringModelBinder.StringModelBinderProvider());
        })
        .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
}
```

## Environment

### Visual Studio
```
Microsoft Visual Studio Community 2019
Version 16.1.6
VisualStudio.16.Release/16.1.6+29102.190
Microsoft .NET Framework
Version 4.7.03056
```
### dotnet --info
```
.NET Core SDK (reflecting any global.json):
 Version:   3.0.100-preview7-012821
 Commit:    6348f1068a

Runtime Environment:
 OS Name:     Windows
 OS Version:  10.0.17134
 OS Platform: Windows
 RID:         win10-x64
 Base Path:   C:\Program Files\dotnet\sdk\3.0.100-preview7-012821\

Host (useful for support):
  Version: 3.0.0-preview7-27912-14
  Commit:  4da6ee6450

.NET Core SDKs installed:
  2.0.2 [C:\Program Files\dotnet\sdk]
  2.0.3 [C:\Program Files\dotnet\sdk]
  2.1.2 [C:\Program Files\dotnet\sdk]
  2.1.4 [C:\Program Files\dotnet\sdk]
  2.1.100 [C:\Program Files\dotnet\sdk]
  2.1.200-preview-007474 [C:\Program Files\dotnet\sdk]
  2.1.200 [C:\Program Files\dotnet\sdk]
  2.1.201 [C:\Program Files\dotnet\sdk]
  2.1.202 [C:\Program Files\dotnet\sdk]
  2.1.302 [C:\Program Files\dotnet\sdk]
  2.1.403 [C:\Program Files\dotnet\sdk]
  2.1.500 [C:\Program Files\dotnet\sdk]
  2.1.502 [C:\Program Files\dotnet\sdk]
  2.1.507 [C:\Program Files\dotnet\sdk]
  2.1.602 [C:\Program Files\dotnet\sdk]
  2.1.700 [C:\Program Files\dotnet\sdk]
  2.1.701 [C:\Program Files\dotnet\sdk]
  2.1.800-preview-009696 [C:\Program Files\dotnet\sdk]
  2.2.301 [C:\Program Files\dotnet\sdk]
  3.0.100-preview4-011223 [C:\Program Files\dotnet\sdk]
  3.0.100-preview7-012821 [C:\Program Files\dotnet\sdk]

.NET Core runtimes installed:
  Microsoft.AspNetCore.All 2.1.2 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.All]
  Microsoft.AspNetCore.All 2.1.5 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.All]
  Microsoft.AspNetCore.All 2.1.6 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.All]
  Microsoft.AspNetCore.All 2.1.9 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.All]
  Microsoft.AspNetCore.All 2.1.11 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.All]
  Microsoft.AspNetCore.All 2.1.12 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.All]
  Microsoft.AspNetCore.All 2.2.6 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.All]
  Microsoft.AspNetCore.App 2.1.2 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
  Microsoft.AspNetCore.App 2.1.5 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
  Microsoft.AspNetCore.App 2.1.6 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
  Microsoft.AspNetCore.App 2.1.9 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
  Microsoft.AspNetCore.App 2.1.11 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
  Microsoft.AspNetCore.App 2.1.12 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
  Microsoft.AspNetCore.App 2.2.6 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
  Microsoft.AspNetCore.App 3.0.0-preview7.19365.7 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
  Microsoft.NETCore.App 2.0.0 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
  Microsoft.NETCore.App 2.0.3 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
  Microsoft.NETCore.App 2.0.5 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
  Microsoft.NETCore.App 2.0.6 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
  Microsoft.NETCore.App 2.0.7 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
  Microsoft.NETCore.App 2.0.9 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
  Microsoft.NETCore.App 2.1.2 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
  Microsoft.NETCore.App 2.1.5 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
  Microsoft.NETCore.App 2.1.6 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
  Microsoft.NETCore.App 2.1.9 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
  Microsoft.NETCore.App 2.1.11 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
  Microsoft.NETCore.App 2.1.12 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
  Microsoft.NETCore.App 2.2.6 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
  Microsoft.NETCore.App 3.0.0-preview7-27912-14 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
  Microsoft.WindowsDesktop.App 3.0.0-preview7-27912-14 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]
```