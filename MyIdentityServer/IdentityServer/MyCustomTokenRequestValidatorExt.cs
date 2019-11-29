using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyIdentityServer.IdentityServer
{
    /// <summary>
    /// 自定义令牌验证
    /// 在Startup.cs中配置服务AddCustomTokenRequestValidator
    /// </summary>
    public class MyCustomTokenRequestValidatorExt : ICustomTokenRequestValidator
    {
        public Task ValidateAsync(CustomTokenRequestValidationContext context)
        {
            context.Result.ValidatedRequest.Client.AlwaysSendClientClaims = true;
            context.Result.ValidatedRequest.ClientClaims.Add(new Claim("testtoken", "testbody"));
            return Task.FromResult(0);
        }
    }
}
