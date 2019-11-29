using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyIdentityServer.IdentityServer
{
    /// <summary>
    /// 自定义GrantType
    /// StartUp 注册builder.AddExtensionGrantValidator<MyGrantTypeValidator>();
    /// </summary>
    public class MyGrantTypeValidator : IExtensionGrantValidator
    {
        private readonly ITokenValidator _validator;

        public MyGrantTypeValidator(ITokenValidator validator)
        {
            _validator = validator;
        }

        public string GrantType => "MyGrantType";

        /// <summary>
        /// 校验 用户名/密码等需求
        /// 增加Claim
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            //var userName = context.Request.Raw.Get("czar_name");
            //var userPassword = context.Request.Raw.Get("czar_password");
            //var userToken = context.Request.Raw.Get("token");

            //if (string.IsNullOrEmpty(userToken))
            //{
            //    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
            //    return;
            //}

            //var result = await _validator.ValidateAccessTokenAsync(userToken);
            //if (result.IsError)
            //{
            //    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
            //    return;
            //}

            // get user's identity
            //var sub = result.Claims.FirstOrDefault(c => c.Type == "sub").Value;

            var claims = new List<Claim>() { new Claim("role", GrantType) }; // Claim 用于配置服务站点 [Authorize("anonymous")]
            context.Result = new GrantValidationResult(GrantType, GrantType, claims);
            return Task.CompletedTask;

        }
    }
}
