using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FakeFacebook.Commom
{
    public class SwaggerFileUploadFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var formDataSchema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>(),
                Required = new HashSet<string>()
            };

            foreach (var param in context.MethodInfo.GetParameters())
            {
                var paramName = param.Name;

                if (param.ParameterType == typeof(IFormFile))
                {
                    formDataSchema.Properties[paramName] = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    };
                }
                else if (param.ParameterType == typeof(List<IFormFile>))
                {
                    formDataSchema.Properties[paramName] = new OpenApiSchema
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
                    // Assume simple types (string, int, etc.)
                    formDataSchema.Properties[paramName] = new OpenApiSchema
                    {
                        Type = MapSwaggerType(param.ParameterType)
                    };
                }

                formDataSchema.Required.Add(paramName);
            }

            operation.RequestBody = new OpenApiRequestBody
            {
                Content = {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = formDataSchema
                }
            }
            };
        }

        private string MapSwaggerType(Type type)
        {
            if (type == typeof(int) || type == typeof(int?)) return "integer";
            if (type == typeof(long) || type == typeof(long?)) return "integer";
            if (type == typeof(bool) || type == typeof(bool?)) return "boolean";
            if (type == typeof(DateTime) || type == typeof(DateTime?)) return "string"; // ISO date
            return "string"; // default fallback
        }
    }


}
