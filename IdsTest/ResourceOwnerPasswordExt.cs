using IdentityModel;
using IdentityServer4.AspNetIdentity;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using IdsTest.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdsTest
{
    /// <summary>
    /// 自定义用户登录校验
    /// </summary>
    public class ResourceOwnerPasswordExt : ResourceOwnerPasswordValidator<ApplicationUser>//IResourceOwnerPasswordValidator//
    {
        private RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private IEventService _events;
        private readonly ILogger<ResourceOwnerPasswordExt> _logger;

        public ResourceOwnerPasswordExt(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEventService events,
            ILogger<ResourceOwnerPasswordExt> logger)
        : base(userManager, signInManager, events, logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _events = events;
            _logger = logger;
        }

        /// <summary>
        /// 登录用户校验
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var Rememberme = false;
            var VfyResult = await _signInManager.PasswordSignInAsync(context.UserName, context.Password, Rememberme, true);
            var OUser = await _signInManager.UserManager.FindByNameAsync(context.UserName);
            context.Result = new GrantValidationResult("", OidcConstants.AuthenticationMethods.Password);
            //根据context.UserName和context.Password与数据库的数据做校验，判断是否合法
            if (VfyResult.Succeeded)
            {
                context.Result = new GrantValidationResult(
                    subject: context.UserName,
                    authenticationMethod: "custom",
                    claims: (await _signInManager.CreateUserPrincipalAsync(OUser)).Claims);

                //context.Result = new GrantValidationResult(
                //    subject: OUser.Id,
                //    authenticationMethod: "custom",
                //    claims: (await _signInManager.CreateUserPrincipalAsync(OUser)).Claims
                //    );
            }
            //else
            //await base.ValidateAsync(context);
            else if (VfyResult.IsLockedOut)
            {
                //验证失败
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidClient, "账号被锁定");
            }
            else if (VfyResult.RequiresTwoFactor)
            {
                //验证失败
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidClient, "账号已启用2步验证");
            }
            else if (VfyResult.IsNotAllowed)
            {
                //验证失败
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidClient, "账号不被允许");
            }
            else
            {
                //验证失败
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "账号用户名/密码错误");
            }
        }
    }
}
