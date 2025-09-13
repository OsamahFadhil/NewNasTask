using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Insight.Invoicing.API.Swagger;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasFileParameters = context.MethodInfo.GetParameters()
            .Any(p => p.ParameterType == typeof(IFormFile) || p.ParameterType == typeof(IFormFile[]));

        if (!hasFileParameters) return;

        operation.RequestBody = new OpenApiRequestBody
        {
            Content =
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>()
                    }
                }
            }
        };

        var formSchema = operation.RequestBody.Content["multipart/form-data"].Schema;

        foreach (var parameter in context.MethodInfo.GetParameters())
        {
            if (parameter.GetCustomAttribute<Microsoft.AspNetCore.Mvc.FromFormAttribute>() != null)
            {
                if (parameter.ParameterType == typeof(IFormFile))
                {
                    formSchema.Properties[parameter.Name!] = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    };
                }
                else if (parameter.ParameterType == typeof(IFormFile[]))
                {
                    formSchema.Properties[parameter.Name!] = new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary"
                        }
                    };
                }
                else
                {
                    formSchema.Properties[parameter.Name!] = new OpenApiSchema
                    {
                        Type = GetOpenApiType(parameter.ParameterType)
                    };
                }
            }
        }

        operation.Parameters?.Clear();
    }

    private static string GetOpenApiType(Type type)
    {
        if (type == typeof(int) || type == typeof(int?)) return "integer";
        if (type == typeof(decimal) || type == typeof(decimal?) || type == typeof(double) || type == typeof(double?)) return "number";
        if (type == typeof(bool) || type == typeof(bool?)) return "boolean";
        if (type == typeof(DateTime) || type == typeof(DateTime?)) return "string";
        return "string";
    }
}
