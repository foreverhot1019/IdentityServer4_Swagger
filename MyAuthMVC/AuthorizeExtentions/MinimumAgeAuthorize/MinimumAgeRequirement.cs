using Microsoft.AspNetCore.Authorization;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace MyAuthMVC
{
    public class MinimumAgeRequirement : IAuthorizationRequirement
    {
        public int MinimumAge { get; set; }

        public MinimumAgeRequirement(int age)
        {
            this.MinimumAge = age;
        }
    }

    public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MinimumAgeRequirement requirement)
        {
            var user = context.User;
            if (!user.HasClaim(c => c.Type == ClaimTypes.DateOfBirth && c.Issuer == "https://michael.com.cn"))
            {
                return Task.FromResult(0);
            }
            //var ArrIdentity = user?.Identities;
            //var userIsAnonymous = user?.Identity == null || !ArrIdentity.Any(i => i.IsAuthenticated);
            //if (!userIsAnonymous)
            //{
            //    context.Succeed(requirement);
            //}

            var dateOfBirth = Convert.ToDateTime(context.User.FindFirst(
                c => c.Type == ClaimTypes.DateOfBirth && c.Issuer == "https://michael.com.cn").Value);

            int calculatedAge = DateTime.Today.Year - dateOfBirth.Year;
            if (dateOfBirth > DateTime.Today.AddYears(-calculatedAge))
            {
                calculatedAge--;
            }

            if (calculatedAge <= requirement.MinimumAge)
            {
                context.Succeed(requirement);//授权成功
            }
            return Task.FromResult(0);
        }
    }
}