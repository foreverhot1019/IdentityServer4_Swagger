using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MyAuthMVC.Data;

namespace MyAuthMVC
{
    public static class PrincipalValidator
    {
        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            if (context == null) throw new System.ArgumentNullException(nameof(context));

            var userId = context.Principal.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                context.RejectPrincipal();
                return;
            }

            //// Get an instance using DI
            //var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            //var user = await dbContext.Users.FindAsync(userId);
            //if (user == null)
            //{
            //    context.RejectPrincipal();
            //    return;
            //}
            //else
            //{
            //    var SecurityStamp = context.Principal.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Jti || claim.Type == "AspNet.Identity.SecurityStamp")?.Value;
            //    if (user.SecurityStamp != SecurityStamp)
            //    {
            //        context.RejectPrincipal();
            //        return;
            //    }
            //}
        }


        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Task JWTValidateAsync(TokenValidatedContext context)
        {
            if (context == null) throw new System.ArgumentNullException(nameof(context));

            var userId = context.Principal.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.NameId || claim.Type == ClaimTypes.NameIdentifier || claim.Type == "Id")?.Value;
            if (userId == null)
            {
                context.NoResult();
                //返回 400 验证错误
                context.Response.StatusCode = 400;
                context.Response.ContentType = "text/plain";
                context.Response.WriteAsync("Authenrize Failed-No User KeyId").Wait();
            }

            //// Get an instance using DI
            //var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            //var user = dbContext.Users.Find(userId);
            //if (user == null)
            //{
            //    context.NoResult();
            //    //返回 400 验证错误
            //    context.Response.StatusCode = 400;
            //    context.Response.ContentType = "text/plain";
            //    context.Response.WriteAsync("Authenrize Failed-No User Find").Wait();
            //}
            //else
            //{
            //    var SecurityStamp = context.Principal.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Jti || claim.Type == "AspNet.Identity.SecurityStamp")?.Value;
            //    if(user.SecurityStamp != SecurityStamp)
            //    {
            //        context.NoResult();
            //        //返回 400 验证错误
            //        context.Response.StatusCode = 400;
            //        context.Response.ContentType = "text/plain";
            //        context.Response.WriteAsync("Authenrize Failed-User Refreshed").Wait();
            //    }
            //}
            return Task.CompletedTask;
        }
    }

}
