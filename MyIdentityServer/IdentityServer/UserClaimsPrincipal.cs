using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyIdentityServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyIdentityServer.IdentityServer
{
    public class UserClaimsPrincipal : IUserClaimsPrincipalFactory<ApplicationUser>
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        public UserClaimsPrincipal(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
        {
            var claimsPrincipal = await _signInManager.ClaimsFactory.CreateAsync(user);
            //ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims);
            //ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            return await Task.FromResult(claimsPrincipal);

        }
    }

    public class CustomUserClaimsFactory<TRole> : UserClaimsPrincipalFactory<ApplicationUser, TRole>
        where TRole : class
    {
        public CustomUserClaimsFactory(UserManager<ApplicationUser> userManager, RoleManager<TRole> roleManager, IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        public async override Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
        {
            var UserCiamsPrincipal = await base.CreateAsync(user);
            var identity = UserCiamsPrincipal.Identities.First();

            //ClaimsIdentity claimsIdentity = new ClaimsIdentity(identity.Claims);
            //ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            //if (!claimsIdentity.HasClaim(x => x.Type == "Resource"))
            //{
            //    claimsIdentity.AddClaim(new Claim("Resource", user.Resource));
            //}

            return UserCiamsPrincipal;
        }
    }

    //public static IdentityBuilder AddCustomUserClaimsPrincipalFactory(this IdentityBuilder builder)
    //{
    //    var interfaceType = typeof(IUserClaimsPrincipalFactory<>);
    //    interfaceType = interfaceType.MakeGenericType(builder.UserType);

    //    var classType = typeof(CustomUserClaimsFactory<>);
    //    classType = classType.MakeGenericType(builder.RoleType);

    //    builder.Services.AddScoped(interfaceType, classType);

    //    return builder;
    //}
}
