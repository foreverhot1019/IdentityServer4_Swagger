using AutoMapper;
using IdentityModel;
using IdentityServer4.AspNetIdentity;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyIdentityServer.IdentityServer
{
    /// <summary>
    /// 自定义用户登录校验
    /// </summary>
    public class ResourceOwnerPasswordExt : IResourceOwnerPasswordValidator// ResourceOwnerPasswordValidator<IdentityUser>
    {
        private RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private IEventService _events;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ResourceOwnerPasswordValidator<IdentityUser>> _logger;

        public ResourceOwnerPasswordExt(
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEventService events,
            ILogger<ResourceOwnerPasswordValidator<IdentityUser>> logger)
        //:base(userManager, signInManager, events, logger)
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
            var OUser = await _userManager.FindByNameAsync(context.UserName);
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

    /// <summary>
    /// 访问用户信息
    /// </summary>
    public class ProfileService : IProfileService
    {
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            try
            {
                //depending on the scope accessing the user data.
                var claims = context.Subject.Claims.ToList();

                //set issued claims to return
                context.IssuedClaims = claims.ToList();
            }
            catch (Exception ex)
            {
                //log your error
            }
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;
        }
    }

}
