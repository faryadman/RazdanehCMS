using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Project.Application.DTOs.Server;

namespace Project.LegacyApi.Tests;

public static class ContractMetadata
{
    public static JToken Actions(IServiceProvider services)
    {
        var actions = services.GetRequiredService<IActionDescriptorCollectionProvider>().ActionDescriptors.Items
            .OfType<ControllerActionDescriptor>()
            .Select(action => new
            {
                Controller = action.ControllerTypeInfo.FullName,
                Action = action.ActionName,
                Area = action.RouteValues.TryGetValue("area", out var area) ? area : null,
                Route = action.AttributeRouteInfo?.Template,
                Methods = action.ActionConstraints?.OfType<HttpMethodActionConstraint>()
                    .SelectMany(constraint => constraint.HttpMethods).Order(StringComparer.Ordinal).ToArray() ?? [],
                Parameters = action.Parameters.Select(parameter => new
                {
                    parameter.Name, Type = parameter.ParameterType.ToString(),
                    Source = parameter.BindingInfo?.BindingSource?.Id
                }),
                Attributes = action.MethodInfo.GetCustomAttributes(false).Concat(action.ControllerTypeInfo.GetCustomAttributes(false))
                    .Select(attribute => attribute.GetType().FullName)
                    .Where(name => name?.StartsWith("Microsoft.AspNetCore.", StringComparison.Ordinal) == true)
                    .Order(StringComparer.Ordinal)
            })
            .OrderBy(action => action.Controller, StringComparer.Ordinal)
            .ThenBy(action => action.Action, StringComparer.Ordinal)
            .ThenBy(action => string.Join(",", action.Methods), StringComparer.Ordinal);
        return JToken.FromObject(actions);
    }

    public static JToken Schemas(IServiceProvider services)
    {
        var settings = services.GetRequiredService<IOptions<MvcNewtonsoftJsonOptions>>().Value.SerializerSettings;
        var types = typeof(ServerDTO).Assembly.GetTypes()
            .Where(type => type.IsPublic && type.IsClass && !type.ContainsGenericParameters &&
                type.Namespace?.StartsWith("Project.Application.DTOs", StringComparison.Ordinal) == true)
            .OrderBy(type => type.FullName, StringComparer.Ordinal);
        return JToken.FromObject(types.Select(type => new
        {
            Type = type.FullName,
            Properties = ((JsonObjectContract)settings.ContractResolver!.ResolveContract(type)).Properties
                .Where(property => !property.Ignored)
                .Select(property => new
                {
                    property.PropertyName, Type = property.PropertyType!.ToString(),
                    Nullable = !property.PropertyType.IsValueType || Nullable.GetUnderlyingType(property.PropertyType) != null
                })
        }));
    }

    public static JToken Options(IServiceProvider services)
    {
        var mvc = services.GetRequiredService<IOptions<MvcOptions>>().Value;
        var json = services.GetRequiredService<IOptions<MvcNewtonsoftJsonOptions>>().Value.SerializerSettings;
        var razor = services.GetRequiredService<IOptions<RazorViewEngineOptions>>().Value;
        return JToken.FromObject(new
        {
            mvc.EnableEndpointRouting,
            Filters = mvc.Filters.Select(filter => filter is TypeFilterAttribute attribute ? attribute.ImplementationType.FullName : filter.GetType().FullName),
            razor.ViewLocationFormats,
            ContractResolver = json.ContractResolver!.GetType().FullName,
            json.ReferenceLoopHandling, json.NullValueHandling, json.DefaultValueHandling,
            json.DateFormatHandling, json.DateTimeZoneHandling,
            Converters = json.Converters.Select(converter => converter.GetType().FullName)
        });
    }
}
