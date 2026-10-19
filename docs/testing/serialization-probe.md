# بازاجرای بررسی مستقل سریال‌سازی

این ابزار ممیزی خارج checkout از assembly و تنظیم Newtonsoft MVC استفاده می‌کند؛ هیچ WebApplication، DbContext، سرویس DNS یا action اجرا نمی‌شود. ابتدا ۶ assertion قرارداد، سپس extraction metadata و بازتولید شکست formatter تاریخ پیش‌فرض انجام می‌شود. fixture مصنوعی است و Golden-master Android نیست.

دو فایل زیر را در پوشهٔ موقت خارج checkout قرار دهید و اجرا کنید:

```bash
export DOTNET_ROOT=/workspace/.dotnet
export PATH="$DOTNET_ROOT:$PATH"
export DOTNET_CLI_HOME=/workspace/.dotnet-home
export NUGET_PACKAGES=/workspace/.nuget/packages
cd /workspace/.setup/audit-probe
dotnet run --project Probe.csproj
```

## Probe.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings></PropertyGroup><ItemGroup><FrameworkReference Include="Microsoft.AspNetCore.App"/><ProjectReference Include="/workspace/RazdanehCMS/CMS/Project.Web.AndroidAppsProject/Project.Web.AndroidAppsProject.csproj"/></ItemGroup></Project>
```

## Program.cs

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Project.Application.DTOs;
using Project.Application.DTOs.Server;
using Project.Application.DTOs.ServerLog;
using Project.Application.Responses;
using Project.Domain.Enums;
using Project.Web.AndroidAppsProject.Controllers;
var services = new ServiceCollection();
services.AddLogging();
services.AddControllers().AddApplicationPart(typeof(ApiController).Assembly).AddNewtonsoftJson(o=>o.SerializerSettings.ReferenceLoopHandling=ReferenceLoopHandling.Ignore);
using var provider=services.BuildServiceProvider();
var settings=provider.GetRequiredService<IOptions<MvcNewtonsoftJsonOptions>>().Value.SerializerSettings;
var response=new Response<GeneralServerDTO>(new GeneralServerDTO{ ServerDTO=new ServerDTO{Id=42,UpdatedAt=new DateTime(2026,10,18,0,0,0,DateTimeKind.Utc),Config="{\"outbounds\":[]}",ZoneId=null}, AppSettingDTO=null, ServerAdDTO=null});
var json=JsonConvert.SerializeObject(response,settings); var node=JObject.Parse(json);var count=0;
void Check(bool value,string name){if(!value)throw new Exception(name); Console.WriteLine("PASS "+name);count++;}
Check((int)node["status"]==1,"numeric response status");
Check(node["data"]["serverDTO"]["config"].Type==JTokenType.String,"configuration stays string");
Check(node["data"]["serverAdDTO"].Type==JTokenType.Null,"null fields retained");
Check(node["data"]["serverDTO"]["zoneId"].Type==JTokenType.Null,"nullable zone id");
var log=JObject.Parse(JsonConvert.SerializeObject(new AddServerLogDTO{ConnectionStatus=ConnectionStatus.Failed},settings));
Check((int)log["connectionStatus"]==1,"numeric connection enum");
Check(new Response<string>(ResponseStatus.BadRequest).ToJsonResult().StatusCode==400,"error status mapping");
File.WriteAllText("serialization-example.json",json);
var actions=provider.GetRequiredService<IActionDescriptorCollectionProvider>().ActionDescriptors.Items.OfType<ControllerActionDescriptor>();
var results=actions.Select(a=>new{a.ControllerName,a.ActionName,Area=(a.RouteValues.TryGetValue("area", out var area) ? area : null),Route=a.AttributeRouteInfo?.Template,Methods=a.ActionConstraints?.OfType<HttpMethodActionConstraint>().SelectMany(c=>c.HttpMethods).ToArray()??Array.Empty<string>(),ReturnType=a.MethodInfo.ReturnType.ToString(),Parameters=a.Parameters.Select(p=>new{p.Name,Type=p.ParameterType.ToString(),Source=p.BindingInfo?.BindingSource?.Id}),File=a.ControllerTypeInfo.FullName});
File.WriteAllText("actions.json",System.Text.Json.JsonSerializer.Serialize(results,new System.Text.Json.JsonSerializerOptions{WriteIndented=true}));
Console.WriteLine($"{count} serialization checks; {results.Count()} action descriptors; no app host started");

var dtoTypes=typeof(ServerDTO).Assembly.GetTypes().Where(t=>t.IsClass && t.IsPublic && !t.ContainsGenericParameters && t.Namespace?.StartsWith("Project.Application.DTOs")==true);
var schemas=dtoTypes.Select(t=>new{Type=t.FullName,Properties=((Newtonsoft.Json.Serialization.JsonObjectContract)settings.ContractResolver.ResolveContract(t)).Properties.Where(p=>!p.Ignored).Select(p=>new{p.PropertyName,ClrType=p.PropertyType.ToString(),Nullable=(!p.PropertyType.IsValueType || Nullable.GetUnderlyingType(p.PropertyType)!=null)})});
File.WriteAllText("schemas.json",System.Text.Json.JsonSerializer.Serialize(schemas,new System.Text.Json.JsonSerializerOptions{WriteIndented=true}));
bool observed=false;
try {JsonConvert.SerializeObject(new ServerDTO(),settings);} catch(JsonSerializationException){observed=true;}
Check(observed,"default UpdatedAt formatter failure reproduced");

```

انتظار: شش PASS قرارداد و یک PASS بازتولید نقص، نه هفت تست کامل برنامه. شمارش شش پیش از assertion آخر چاپ می‌شود. serialization-example.json نمونهٔ مصنوعی است؛ outbounds خالی به معنی اعتبار Config واقعی نیست. metadata مستقل از HTTP routing کامل و startup است؛ آزمون host در feature branch لازم است.
