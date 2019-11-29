using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyAuthMVC.Middleware
{
    public class AuthorizeMiddleware
    {
        private readonly RequestDelegate _next;
        public IAuthenticationSchemeProvider _Schemes { get; set; }
        //private readonly string _policyName;

        public AuthorizeMiddleware(RequestDelegate next)//, string policyName
        {
            _next = next;
            //_policyName = policyName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context, IAuthorizationService authorizationService, IAuthenticationSchemeProvider Schemes/* other scoped dependencies */)
        {
            _Schemes = Schemes;
            //以下代码都不是必须的，只是展示一些使用方法，你可以选择使用
            //这个例子只是修改一下response的header
            context.Response.OnStarting(state =>
            {
                var httpContext = (HttpContext)state;
                httpContext.Response.Headers.Add("Michael", "MichaelMiddleware");
                return Task.FromResult(0);
            }, context);
            //获取所有 认证架构
            var ArrScheme = await Schemes.GetAllSchemesAsync();
            #region MichaelAuth 认证授权

            if(ArrScheme.Any(x=>x.Name == "MichaelAuth"))
            {
                //调用认证
                var result = await context.AuthenticateAsync("MichaelAuth");
                if (result?.Principal != null)
                {
                    context.User = result.Principal;
                }
            }
            
            #endregion

            //处理结束转其它中间组件去处理
            //await _next.Invoke(context);//RequestDelegate-Invoke 之后不能修改response，否则报错
            await _next(context);//继续下一个中间件
            //...可继续下一步处理

            //context.Features.Set<IAuthenticationFeature>(new AuthenticationFeature
            //{
            //    OriginalPath = context.Request.Path,
            //    OriginalPathBase = context.Request.PathBase
            //});

            ////获取DI注入 服务
            //var handlers = context.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
            //foreach (var scheme in await Schemes.GetRequestHandlerSchemesAsync())
            //{
            //    var handler = await handlers.GetHandlerAsync(context, scheme.Name) as IAuthenticationRequestHandler;
            //    if (handler != null && await handler.HandleRequestAsync())
            //    {
            //        return;
            //    }
            //}

            ////处理结束转其它中间组件去处理
            //await _next(context);
        }
    }

    public static class AuthorizationApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseAuthorization(this IApplicationBuilder app)
        {
            return app.UseAuthorization(new AuthorizationOptions());
        }

        public static IApplicationBuilder UseAuthorization(this IApplicationBuilder app,
            AuthorizationOptions authorizationOptions)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (authorizationOptions == null)
            {
                throw new ArgumentNullException(nameof(authorizationOptions));
            }

            return app.UseMiddleware<AuthorizeMiddleware>(Options.Create(authorizationOptions));
        }
    }
}
