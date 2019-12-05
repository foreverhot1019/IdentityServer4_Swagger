using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using MyIdentityServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyIdentityServer.IdentityServer
{
    /// <summary>
    ///  Profile就是用户资料，ids 4里面定义了一个IProfileService的接口用来获取用户的一些信息
    ///  ，主要是为当前的认证上下文绑定claims。我们可以实现IProfileService从外部创建claim扩展到ids4里面。
    ///  然后返回
    /// </summary>
    public class MyProfileServices : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public MyProfileServices(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public async Task<List<Claim>> GetClaimsFromUserAsync(ApplicationUser user)
        {
            var claims = new List<Claim> {
                //new Claim(JwtClaimTypes.Subject,user.Id.ToString()),
                //new Claim(JwtClaimTypes.PreferredUserName,user.UserName)

                new Claim(JwtClaimTypes.Gender,"女")
            };
            var claimsPrincipal = await _signInManager.ClaimsFactory.CreateAsync(user);
            claims.AddRange(claimsPrincipal.Claims);

            if (!string.IsNullOrEmpty(user.Resource))
            {
                claims.Add(new Claim("resource", user.Resource));
            }
            return claims;
        }

        /// <summary>
        /// 获取用户Claims
        /// 用户请求userinfo endpoint时会触发该方法
        /// http://localhost:5003/connect/userinfo
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subjectId = context.Subject.Claims.FirstOrDefault(c => c.Type == "sub").Value;
            var user = await _userManager.FindByIdAsync(subjectId);
            context.IssuedClaims = await GetClaimsFromUserAsync(user);
        }

        /// <summary>
        /// 判断用户是否可用
        /// Identity Server会确定用户是否有效
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task IsActiveAsync(IsActiveContext context)
        {
            var subjectId = context.Subject.Claims.FirstOrDefault(c => c.Type == "sub").Value;
            var user = await _userManager.FindByIdAsync(subjectId);
            context.IsActive = user != null && (!user.LockoutEnd.HasValue || user.LockoutEnd.Value<DateTimeOffset.Now); //该用户是否已经激活，可用，否则不能接受token

            /*
             这样还应该判断用户是否已经锁定，那么应该IsActive=false
             */
        }
    }

    /// <summary>
    /// 访问用户信息
    /// </summary>
    public class ProfileService : IProfileService
    {
        protected UserManager<ApplicationUser> _userManager;

        public ProfileService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            try
            {
                //>Processing
                var user = await _userManager.GetUserAsync(context.Subject);

                var claims = await _userManager.GetClaimsAsync(user);

                context.IssuedClaims.AddRange(claims);
            }
            catch (Exception ex)
            {
                //log your error
            }
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            //>Processing
            var user = await _userManager.GetUserAsync(context.Subject);

            context.IsActive = (user != null);
        }
    }
}
