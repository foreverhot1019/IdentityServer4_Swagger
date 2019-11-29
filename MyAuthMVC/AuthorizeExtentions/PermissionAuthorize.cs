using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MyAuthMVC.Data;

namespace MyAuthMVC
{
    public interface IPermissionAuthorizeData : IAuthorizeData
    {
        string Groups { get; set; }
        string Permissions { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class PermissionAuthorizeAttribute : Attribute, IPermissionAuthorizeData
    {
        public string Policy { get; set; }
        public string Roles { get; set; }
        public string AuthenticationSchemes { get; set; }

        public string Groups { get; set; }

        public string Permissions { get; set; }
    }

    public class PermissionAuthorizationAppModelProvider : AuthorizationApplicationModelProvider//IApplicationModelProvider
    {
        public PermissionAuthorizationAppModelProvider(IAuthorizationPolicyProvider policyProvider)
            : base(policyProvider)
        {
        }
        //public int Order { get; }

        public new static AuthorizeFilter GetFilter(IAuthorizationPolicyProvider policyProvider, IEnumerable<IAuthorizeData> authData)
        {
            // The default policy provider will make the same policy for given input, so make it only once.
            // This will always execute synchronously.
            if (policyProvider.GetType() == typeof(DefaultAuthorizationPolicyProvider))
            {
                var policy = CombineAsync(policyProvider, authData).GetAwaiter().GetResult();
                return new AuthorizeFilter(policy);
            }
            else
            {
                return new AuthorizeFilter(policyProvider, authData);
            }
        }
        private static async Task<AuthorizationPolicy> CombineAsync(IAuthorizationPolicyProvider policyProvider, IEnumerable<IAuthorizeData> authorizeData)
        {
            if (policyProvider == null)
            {
                throw new ArgumentNullException(nameof(policyProvider));
            }
            if (authorizeData == null)
            {
                throw new ArgumentNullException(nameof(authorizeData));
            }
            var policyBuilder = new AuthorizationPolicyBuilder();
            var any = false;
            foreach (var authorizeDatum in authorizeData)
            {
                any = true;
                var useDefaultPolicy = true;
                if (!string.IsNullOrWhiteSpace(authorizeDatum.Policy))
                {
                    var policy = await policyProvider.GetPolicyAsync(authorizeDatum.Policy);
                    if (policy == null)
                    {
                        //throw new InvalidOperationException(Resources.FormatException_AuthorizationPolicyNotFound(authorizeDatum.Policy));
                        throw new InvalidOperationException(nameof(authorizeDatum.Policy));
                    }
                    policyBuilder.Combine(policy);
                    useDefaultPolicy = false;
                }
                var rolesSplit = authorizeDatum.Roles?.Split(',');
                if (rolesSplit != null && rolesSplit.Any())
                {
                    var trimmedRolesSplit = rolesSplit.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.Trim());
                    policyBuilder.RequireRole(trimmedRolesSplit);
                    useDefaultPolicy = false;
                }
                if (authorizeDatum is IPermissionAuthorizeData permissionAuthorizeDatum)
                {
                    var groupsSplit = permissionAuthorizeDatum.Groups?.Split(',');
                    if (groupsSplit != null && groupsSplit.Any())
                    {
                        var trimmedGroupsSplit = groupsSplit.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.Trim());
                        policyBuilder.RequireClaim("Group", trimmedGroupsSplit); // TODO: 注意硬编码
                        useDefaultPolicy = false;
                    }
                    var permissionsSplit = permissionAuthorizeDatum.Permissions?.Split(',');
                    if (permissionsSplit != null && permissionsSplit.Any())
                    {
                        var trimmedPermissionsSplit = permissionsSplit.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.Trim());
                        policyBuilder.RequireClaim("Permission", trimmedPermissionsSplit);// TODO: 注意硬编码
                        useDefaultPolicy = false;
                    }
                }
                var authTypesSplit = authorizeDatum.AuthenticationSchemes?.Split(',');
                if (authTypesSplit != null && authTypesSplit.Any())
                {
                    foreach (var authType in authTypesSplit)
                    {
                        if (!string.IsNullOrWhiteSpace(authType))
                        {
                            policyBuilder.AuthenticationSchemes.Add(authType.Trim());
                        }
                    }
                }
                if (useDefaultPolicy)
                {
                    policyBuilder.Combine(await policyProvider.GetDefaultPolicyAsync());
                }
            }
            return any ? policyBuilder.Build() : null;
        }

        //public void OnProvidersExecuted(ApplicationModelProviderContext context)
        //{
        //    //
        //}

        //public void OnProvidersExecuting(ApplicationModelProviderContext context)
        //{
        //    //
        //}
    }

    #region 自定义ClaimType 认证

    public class ClaimRequirementAttribute : TypeFilterAttribute
    {
        public ClaimRequirementAttribute(string claimType, string claimValue)
            : base(typeof(ClaimRequirementFilter))
        {
            Arguments = new object[] { new Claim(claimType, claimValue) };
        }
    }

    public class ClaimRequirementFilter : IAuthorizationFilter
    {
        readonly Claim _claim;

        public ClaimRequirementFilter(Claim claim)
        {
            _claim = claim;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var hasClaim = context.HttpContext.User.Claims.Any(c => c.Type == _claim.Type && c.Value == _claim.Value);
            if (!hasClaim)
            {
                context.Result = new ForbidResult();
            }
        }
    }

    #endregion

}
