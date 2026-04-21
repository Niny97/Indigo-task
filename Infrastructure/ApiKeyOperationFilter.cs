using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Indigo_task.Infrastructure;

public sealed class ApiKeyOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= [];
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Api-Key",
            In = ParameterLocation.Header,
            Required = true,
            Schema = new OpenApiSchema { Type = JsonSchemaType.String }
        });
    }
}
