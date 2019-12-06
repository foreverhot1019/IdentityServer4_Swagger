using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyAuthApi
{
    public class SwaggerExtention
    {
    }

    /// <summary>
    /// Swagger 扩展Authorize
    /// </summary>
    public class HttpHeaderOperation : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
            {
                operation.Parameters = new List<OpenApiParameter>();
            }
            var filterPipeline = context.ApiDescription.ActionDescriptor.FilterDescriptors;
            var isAuthorized = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is AuthorizeFilter);
            var allowAnonymous = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is IAllowAnonymousFilter);
            //context.ApiDescription.TryGetMethodInfo(out System.Reflection.MethodInfo MInfo);
            //var isAuthorized = MInfo.GetCustomAttributes(typeof(AuthorizeAttribute),false);

            var actionAttrs = context.ApiDescription.CustomAttributes();
            var isAuthorizedAct = actionAttrs.Any(a => a.GetType() == typeof(AuthorizeAttribute));

            if (isAuthorizedAct == false) //提供action都没有权限特性标记，检查控制器有没有
            {
                var controllerAttrs = context.ApiDescription.CustomAttributes();

                isAuthorizedAct = controllerAttrs.Any(a => a.GetType() == typeof(AuthorizeAttribute));
            }

            var isAllowAnonymousAct = actionAttrs.Any(a => a.GetType() == typeof(AllowAnonymousAttribute));

            if ((isAuthorized && !allowAnonymous) || (isAuthorizedAct && !isAllowAnonymousAct))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "Authorization",  //添加Authorization头部参数
                    In = ParameterLocation.Header,
                    Required = true,
                    Style = ParameterStyle.Simple
                });
                operation.Security = new List<OpenApiSecurityRequirement> {
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Name="Bearer",
                                Reference = new OpenApiReference()
                                {
                                    Id = "Bearer",
                                    Type = ReferenceType.SecurityScheme
                                }
                            }, new List<string> { "swagger_api" } }
                    }
                };
            }
        }
    }

    /// <summary>
    /// IdentityServer4
    /// </summary>
    public class AuthResponsesOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            //获取是否添加登录特性
            var authAttributes = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
             .Union(context.MethodInfo.GetCustomAttributes(true))
             .OfType<AuthorizeAttribute>().Any();

            if (authAttributes)
            {
                operation.Responses.Add("401", new OpenApiResponse { Description = "暂无访问权限" });
                operation.Responses.Add("403", new OpenApiResponse { Description = "禁止访问" });
                operation.Security = new List<OpenApiSecurityRequirement> {
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Name="oauth2",
                                Reference = new OpenApiReference()
                                {
                                    Id = "oauth2",
                                    Type = ReferenceType.SecurityScheme
                                }
                            }, new List<string> { "swagger_api" } }
                    }
                };
            };
            //operation.Security =  new List<IDictionary<string, IEnumerable<string>>>
            //{
            //    new Dictionary<string, IEnumerable<string>> {{"oauth2", new[] { "swagger_api" } }}
            //};
        }
    }

    public class SetVersionInPathDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var updatedPaths = new OpenApiPaths();

            foreach (var entry in swaggerDoc.Paths)
            {
                updatedPaths.Add(
                    entry.Key.Replace("v{version}", swaggerDoc.Info.Version),
                    entry.Value);
            }

            swaggerDoc.Paths = updatedPaths;
        }
    }

    public class RemoveVersionParameterOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Remove version parameter from all Operations
            var versionParameter = operation.Parameters.Single(p => p.Name == "version");
            operation.Parameters.Remove(versionParameter);
        }
    }
}
