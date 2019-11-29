using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyAuthMVC
{
    /// <summary>
    /// MinimumAge授权属性
    /// </summary>
    internal class MinimumAgeAuthorizeAttribute : AuthorizeAttribute
    {
        const string POLICY_PREFIX = "MinimumAge";

        public MinimumAgeAuthorizeAttribute(int age)
        {
            MinimumAge = age;
            //this.Policy = "MinimumAge";
        }

        // Get or set the Age property by manipulating the underlying Policy property
        public int MinimumAge
        {
            get
            {
                if (int.TryParse(Policy.Substring(POLICY_PREFIX.Length), out var age))
                {
                    return age;
                }
                return default(int);
            }
            set
            {
                Policy = $"{POLICY_PREFIX}{value.ToString()}";
            }
        }
    }

    /// <summary>
    /// MinimumAge授权提供者
    /// </summary>
    internal class MinimumAgePolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly AuthorizationOptions _options;
        const string POLICY_PREFIX = "MinimumAge";
        private DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

        public MinimumAgePolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _options = options.Value;
            // ASP.NET Core only uses one authorization policy provider, so if the custom implementation
            // doesn't handle all policies it should fall back to an alternate provider.
            FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
            Task.FromResult(new AuthorizationPolicyBuilder(CookieAuthenticationDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build());

        public Task<AuthorizationPolicy> GetRequiredPolicyAsync() => Task.FromResult<AuthorizationPolicy>(null);

        // Policies are looked up by string name, so expect 'parameters' (like age)
        // to be embedded in the policy names. This is abstracted away from developers
        // by the more strongly-typed attributes derived from AuthorizeAttribute
        // (like [MinimumAgeAuthorize()] in this sample)
        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            var AuthPolicy = _options.GetPolicy(policyName);
            if (AuthPolicy == null)
            {
                if (policyName.StartsWith(POLICY_PREFIX, StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(policyName.Substring(POLICY_PREFIX.Length), out var age))
                {
                    var policy = new AuthorizationPolicyBuilder(CookieAuthenticationDefaults.AuthenticationScheme, "MichaelAuth");
                    policy.AddRequirements(new MinimumAgeRequirement(age));
                    return Task.FromResult(policy.Build());
                }
                return FallbackPolicyProvider.GetPolicyAsync(policyName);
            }
            else
            {
                return Task.FromResult<AuthorizationPolicy>(AuthPolicy);
            }
        }
    }
}
