using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace aqua_api.Shared.Host.WebApi.Filters
{
    /// <summary>
    /// Parameter filter to handle [FromForm] IFormFile parameters
    /// This prevents Swashbuckle from trying to generate parameters for file uploads
    /// by providing a valid schema that won't cause errors
    /// </summary>
    public class FileUploadParameterFilter : IParameterFilter
    {
        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            var paramInfo = context.ParameterInfo;
            if (paramInfo == null)
                return;

            // Handle [FromForm] parameters - they will be part of the request body
            var fromFormAttr = paramInfo.GetCustomAttribute<FromFormAttribute>();
            if (fromFormAttr != null)
            {
                // For IFormFile, provide a valid schema to prevent Swashbuckle errors
                // The OperationFilter will handle it as part of the request body later
                if (paramInfo.ParameterType == typeof(IFormFile) || 
                    paramInfo.ParameterType == typeof(IFormFileCollection))
                {
                    // Provide a valid schema to prevent parameter generation errors
                    parameter.Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary",
                        Description = "File upload"
                    };
                    // Set to form to indicate it's form data (will be moved to body by OperationFilter)
                    parameter.In = ParameterLocation.Query;
                    parameter.Required = !paramInfo.IsOptional && !paramInfo.HasDefaultValue;
                }
            }
        }
    }

    /// <summary>
    /// Operation filter to handle IFormFile parameters and [FromForm] parameters as multipart/form-data
    /// Note: IFormFile should NOT have [FromForm] attribute - it's automatically bound from form data
    /// </summary>
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Get all parameters that should be in the form body:
            // 1. IFormFile parameters (without [FromForm] - they're automatically form-bound)
            // 2. Parameters with [FromForm] attribute
            var allParameters = context.MethodInfo.GetParameters().ToList();
            
            var fileParameters = allParameters
                .Where(p => p.ParameterType == typeof(IFormFile) || 
                           p.ParameterType == typeof(IFormFileCollection) ||
                           (p.ParameterType.IsGenericType && 
                            p.ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                            p.ParameterType.GetGenericArguments().Length > 0 &&
                            p.ParameterType.GetGenericArguments()[0] == typeof(IFormFile)))
                .ToList();

            var formParameters = allParameters
                .Where(p => p.GetCustomAttribute<FromFormAttribute>() != null)
                .ToList();

            // Combine file parameters and [FromForm] parameters
            var allFormParameters = fileParameters.Union(formParameters).Distinct().ToList();

            if (!allFormParameters.Any())
                return;

            // Create multipart/form-data request body
            var properties = new Dictionary<string, OpenApiSchema>();
            var required = new HashSet<string>();

            // Add file parameters
            foreach (var fileParam in fileParameters)
            {
                var paramName = fileParam.Name ?? "file";
                properties[paramName] = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary",
                    Description = "File to upload"
                };
                
                if (!fileParam.IsOptional && !fileParam.HasDefaultValue)
                {
                    required.Add(paramName);
                }
            }

            // Add non-file [FromForm] parameters
            foreach (var param in formParameters.Except(fileParameters))
            {
                var paramName = param.Name ?? "param";
                properties[paramName] = context.SchemaGenerator.GenerateSchema(
                    param.ParameterType,
                    context.SchemaRepository);
                
                if (!param.IsOptional && !param.HasDefaultValue)
                {
                    required.Add(paramName);
                }
            }

            // Remove file parameters and [FromForm] parameters from parameters list (they're now in the request body)
            var paramNamesToRemove = allFormParameters.Select(p => p.Name).Where(n => n != null).ToHashSet();

            if (operation.Parameters != null)
            {
                operation.Parameters = operation.Parameters
                    .Where(p => p.Name == null || !paramNamesToRemove.Contains(p.Name))
                    .ToList();
            }

            // Set the request body for multipart/form-data
            operation.RequestBody = new OpenApiRequestBody
            {
                Required = allFormParameters.Any(p => !p.IsOptional && !p.HasDefaultValue),
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = properties,
                            Required = required
                        }
                    }
                }
            };
        }
    }
}
