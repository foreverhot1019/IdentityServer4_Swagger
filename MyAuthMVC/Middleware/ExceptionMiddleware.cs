using MyAuthMVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MyAuthMVC.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        //private readonly ILog _logger4net;

        public ExceptionMiddleware(RequestDelegate next)//, ILog logger4net
        {
            _next = next;
            //_logger = logger;
            //_logger4net = logger4net;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                //_logger.LogError($"Something went wrong: {ex}");
                //_logger4net.Error($"{httpContext}", ex);
                ////获取DI注册服务
                //IHttpContextAccessor httpContxtAcs = httpContext.RequestServices.GetService<IHttpContextAccessor>();
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        /// <summary>
        /// 处理错误
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            return context.Response.WriteAsync(new ErrorDetails()
            {
                StatusCode = context.Response.StatusCode,
                Message = $"Internal Server Error from the custom middleware.{exception.Message}-{exception.StackTrace}"
            }.ToString());
        }
    }
}
