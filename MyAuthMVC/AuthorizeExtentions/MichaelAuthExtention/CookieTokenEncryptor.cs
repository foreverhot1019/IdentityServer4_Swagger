using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyAuthMVC.AuthorizeExtentions.MichaelAuthExtention
{
    public class CookieTokenEncryptor
    {
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="cookieSchema"></param>
        /// <param name="CookieName"></param>
        /// <returns></returns>
        public static async Task Encrypt(HttpContext httpContext, IEnumerable<Claim> claims, string cookieSchema = "Identity.Application")
        {
            // Get the encrypted cookie value
            var CookieOpt = httpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<CookieAuthenticationOptions>>();
            var CookieOptVal = CookieOpt.CurrentValue;
            var claimsIdentity = new ClaimsIdentity(claims, cookieSchema);
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(new List<ClaimsIdentity> {
                claimsIdentity
            });
            await httpContext.SignInAsync(cookieSchema, claimsPrincipal, new AuthenticationProperties
            {
                // 持久保存-true:写入Cookie过期时间
                IsPersistent = true,
                ExpiresUtc = DateTime.Now.AddMinutes(1)//票据-过期时间
            });
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="httpContext">Http内容</param>
        /// <param name="cookieSchema">原加密的架构</param>
        /// <param name="CookieName">Cookie名称</param>
        /// <returns></returns>
        public static AuthenticationTicket Decrypt(HttpContext httpContext, string cookieSchema = "Identity.Application", string cookieName = ".AspNetCore.Identity.Application")
        {
            // Get the encrypted cookie value
            var CookieOpt = httpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<CookieAuthenticationOptions>>();
            var CookieOptVal = CookieOpt?.CurrentValue;
            var cookieManager = CookieOptVal?.CookieManager ?? new ChunkingCookieManager();
            var cookie = cookieManager.GetRequestCookie(httpContext, cookieName);
            if (!string.IsNullOrWhiteSpace(cookie))
            {
                ////自定义MachineKey或者其他密钥 数据保护提供者
                //var provider = DataProtectionProvider.Create(new DirectoryInfo(@"C:\temp-keys\"));
                var provider = CookieOptVal.DataProtectionProvider;

                var dataProtector = provider.CreateProtector("Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware", cookieSchema, "v2");

                ////Get the decrypted cookie as plain text
                //UTF8Encoding specialUtf8Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
                //byte[] protectedBytes = Base64UrlTextEncoder.Decode(cookie);
                //byte[] plainBytes = dataProtector.Unprotect(protectedBytes);
                //string plainText = specialUtf8Encoding.GetString(plainBytes);

                //Get teh decrypted cookies as a Authentication Ticket
                TicketDataFormat ticketDataFormat = new TicketDataFormat(dataProtector);
                AuthenticationTicket ticket = ticketDataFormat.Unprotect(cookie);
                return ticket;
            }
            else
                return null;
        }
    }
}
