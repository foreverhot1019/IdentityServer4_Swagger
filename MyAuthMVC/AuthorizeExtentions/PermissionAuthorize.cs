using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MyAuthMVC.Data;
using Newtonsoft.Json;

namespace MyAuthMVC
{
    //参考：https://www.tuicool.com/articles/QBjQVbU
    public interface IPermissionAuthorizeData : IAuthorizeData
    {
        string Groups { get; set; }
        string Permissions { get; set; }
    }

    static class PermissionClaimTypes
    {
        public static string Group = "Groups";
        public static string Permission = "Permissions";
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class PermissionAuthorizeAttribute : Attribute, IPermissionAuthorizeData
    {
        #region .Net-IAuthorizeData自带

        public string Policy { get; set; }
        public string Roles { get; set; }
        public string AuthenticationSchemes { get; set; }

        #endregion

        public string Groups { get; set; }

        public string Permissions { get; set; }
    }

    public class PermissionAuthorizeData : IPermissionAuthorizeData
    {
        public string Policy { get; set; }
        public string Roles { get; set; }
        public string AuthenticationSchemes { get; set; }
        public string Groups { get; set; }
        public string Permissions { get; set; }
    }

    public class PermissionAuthorizationRequirement : AuthorizationHandler<PermissionAuthorizationRequirement>, IAuthorizationRequirement
    {
        public PermissionAuthorizeData AuthorizeData { get; }

        public PermissionAuthorizationRequirement(PermissionAuthorizeData authorizeData)
        {
            AuthorizeData = authorizeData;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionAuthorizationRequirement requirement)
        {
            if (context.User == null)
            {
                return Task.CompletedTask;
            }

            // 以半角逗号分隔的权限满足"需要"的其中之一即可，角色和分组也类似。
            // 分组、角色和权限三者在此也是 Or 的关系，所以是在尽力去找任一匹配。
            var found = false;
            if (requirement.AuthorizeData.Permissions != null)
            {
                var permissionsClaim = context.User.Claims.FirstOrDefault(c => string.Equals(c.Type, PermissionClaimTypes.Permission, StringComparison.OrdinalIgnoreCase));
                if (permissionsClaim?.Value != null && permissionsClaim.Value.Length > 0)
                {
                    var permissionsClaimSplit = SafeSplit(permissionsClaim.Value);
                    var permissionsDataSplit = SafeSplit(requirement.AuthorizeData.Permissions);
                    found = permissionsDataSplit.Intersect(permissionsClaimSplit).Any();
                }
            }

            if (!found && requirement.AuthorizeData.Roles != null)
            {
                var rolesClaim = context.User.Claims.FirstOrDefault(c => string.Equals(c.Type, ClaimTypes.Role, StringComparison.OrdinalIgnoreCase));
                if (rolesClaim?.Value != null && rolesClaim.Value.Length > 0)
                {
                    var rolesClaimSplit = SafeSplit(rolesClaim.Value);
                    var rolesDataSplit = SafeSplit(requirement.AuthorizeData.Roles);
                    found = rolesDataSplit.Intersect(rolesClaimSplit).Any();
                }
            }

            if (!found && requirement.AuthorizeData.Groups != null)
            {
                var groupsClaim = context.User.Claims.FirstOrDefault(c => string.Equals(c.Type, PermissionClaimTypes.Group, StringComparison.OrdinalIgnoreCase));
                if (groupsClaim?.Value != null && groupsClaim.Value.Length > 0)
                {
                    var groupsClaimSplit = SafeSplit(groupsClaim.Value);
                    var groupsDataSplit = SafeSplit(requirement.AuthorizeData.Groups);
                    found = groupsDataSplit.Intersect(groupsClaimSplit).Any();
                }
            }

            if (found)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        private IEnumerable<string> SafeSplit(string source)
        {
            return source.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.Trim()).Where(m => !string.IsNullOrWhiteSpace(m));
        }
    }

    public class PermissionAuthorizationPolicyProvider : IAuthorizationPolicyProvider
    {
        const string PolicyPrefix = "Permission:";

        public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

        public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            // ASP.NET Core only uses one authorization policy provider, so if the custom implementation
            // doesn't handle all policies (including default policies, etc.) it should fall back to an
            // alternate provider.
            //
            // In this sample, a default authorization policy provider (constructed with options from the 
            // dependency injection container) is used if this custom provider isn't able to handle a given
            // policy name.
            //
            // If a custom policy provider is able to handle all expected policy names then, of course, this
            // fallback pattern is unnecessary.
            FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => FallbackPolicyProvider.GetDefaultPolicyAsync();

        // For ASP.NET Core 3.0
        public Task<AuthorizationPolicy> GetFallbackPolicyAsync() => FallbackPolicyProvider.GetFallbackPolicyAsync();

        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(PolicyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var policyValue = policyName.Substring(PolicyPrefix.Length);
                var authorizeData = JsonConvert.DeserializeObject<PermissionAuthorizeData>(policyValue);
                var policy = new AuthorizationPolicyBuilder();
                policy.AddRequirements(new PermissionAuthorizationRequirement(authorizeData));
                return Task.FromResult(policy.Build());
            }

            // If the policy name doesn't match the format expected by this policy provider,
            // try the fallback provider. If no fallback provider is used, this would return 
            // Task.FromResult<AuthorizationPolicy>(null) instead.
            return FallbackPolicyProvider.GetPolicyAsync(policyName);
        }
    }

    //替换服务
    //services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

    #region 实现And 授权

    #region .NetCore 2.2 实现方式

    /// <summary>
    /// 实现And 授权
    /// </summary>
    //参考：http://blog.tubumu.com/2018/11/28/aspnetcore-mvc-extend-authorization/
    //public class PermissionAuthorizationAppModelProvider : AuthorizationApplicationModelProvider
    //{
    //    private IAuthorizationPolicyProvider _policyProvider;

    //    public PermissionAuthorizationAppModelProvider(IAuthorizationPolicyProvider policyProvider)
    //        : base(policyProvider)
    //    {
    //        _policyProvider = policyProvider;
    //    }

    //    /// <summary>
    //    /// 获取过滤器
    //    /// </summary>
    //    /// <param name="policyProvider"></param>
    //    /// <param name="authData"></param>
    //    /// <returns></returns>
    //    public new static AuthorizeFilter GetFilter(IAuthorizationPolicyProvider policyProvider, IEnumerable<IAuthorizeData> authData)
    //    {
    //        // The default policy provider will make the same policy for given input, so make it only once.
    //        // This will always execute synchronously.
    //        if (policyProvider.GetType() == typeof(DefaultAuthorizationPolicyProvider))
    //        {
    //            var policy = CombineAsync(policyProvider, authData).GetAwaiter().GetResult();
    //            return new AuthorizeFilter(policy);
    //        }
    //        else
    //        {
    //            return new AuthorizeFilter(policyProvider, authData);
    //        }
    //    }

    //    /// <summary>
    //    /// 编译策略过滤器
    //    /// </summary>
    //    /// <param name="policyProvider"></param>
    //    /// <param name="authorizeData"></param>
    //    /// <returns></returns>
    //    private static async Task<AuthorizationPolicy> CombineAsync(IAuthorizationPolicyProvider policyProvider, IEnumerable<IAuthorizeData> authorizeData)
    //    {
    //        if (policyProvider == null)
    //        {
    //            throw new ArgumentNullException(nameof(policyProvider));
    //        }
    //        if (authorizeData == null)
    //        {
    //            throw new ArgumentNullException(nameof(authorizeData));
    //        }
    //        var policyBuilder = new AuthorizationPolicyBuilder();
    //        var any = false;
    //        foreach (var authorizeDatum in authorizeData)
    //        {
    //            any = true;
    //            var useDefaultPolicy = true;
    //            if (!string.IsNullOrWhiteSpace(authorizeDatum.Policy))
    //            {
    //                var policy = await policyProvider.GetPolicyAsync(authorizeDatum.Policy);
    //                if (policy == null)
    //                {
    //                    //throw new InvalidOperationException(Resources.FormatException_AuthorizationPolicyNotFound(authorizeDatum.Policy));
    //                    throw new InvalidOperationException(nameof(authorizeDatum.Policy));
    //                }
    //                policyBuilder.Combine(policy);
    //                useDefaultPolicy = false;
    //            }
    //            var rolesSplit = authorizeDatum.Roles?.Split(',');
    //            if (rolesSplit != null && rolesSplit.Any())
    //            {
    //                var trimmedRolesSplit = rolesSplit.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.Trim());
    //                policyBuilder.RequireRole(trimmedRolesSplit);
    //                useDefaultPolicy = false;
    //            }
    //            if (authorizeDatum is IPermissionAuthorizeData permissionAuthorizeDatum)
    //            {
    //                var groupsSplit = permissionAuthorizeDatum.Groups?.Split(',');
    //                if (groupsSplit != null && groupsSplit.Any())
    //                {
    //                    var trimmedGroupsSplit = groupsSplit.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.Trim());
    //                    policyBuilder.RequireClaim("Group", trimmedGroupsSplit); // TODO: 注意硬编码
    //                    useDefaultPolicy = false;
    //                }
    //                var permissionsSplit = permissionAuthorizeDatum.Permissions?.Split(',');
    //                if (permissionsSplit != null && permissionsSplit.Any())
    //                {
    //                    var trimmedPermissionsSplit = permissionsSplit.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.Trim());
    //                    policyBuilder.RequireClaim("Permission", trimmedPermissionsSplit);// TODO: 注意硬编码
    //                    useDefaultPolicy = false;
    //                }
    //            }
    //            var authTypesSplit = authorizeDatum.AuthenticationSchemes?.Split(',');
    //            if (authTypesSplit != null && authTypesSplit.Any())
    //            {
    //                foreach (var authType in authTypesSplit)
    //                {
    //                    if (!string.IsNullOrWhiteSpace(authType))
    //                    {
    //                        policyBuilder.AuthenticationSchemes.Add(authType.Trim());
    //                    }
    //                }
    //            }
    //            if (useDefaultPolicy)
    //            {
    //                policyBuilder.Combine(await policyProvider.GetDefaultPolicyAsync());
    //            }
    //        }
    //        return any ? policyBuilder.Build() : null;
    //    }
    //}
    #endregion

    /// <summary>
    /// 实现And 授权
    /// </summary>
    //参考：http://blog.tubumu.com/2018/11/28/aspnetcore-mvc-extend-authorization/
    public class PermissionAuthorizationAppModelProvider : IApplicationModelProvider
    {
        private IAuthorizationPolicyProvider _policyProvider;

        public PermissionAuthorizationAppModelProvider(IAuthorizationPolicyProvider policyProvider)
            //: base(policyProvider)
        {
            _policyProvider = policyProvider;
        }
        public PermissionAuthorizationAppModelProvider()
        {
        }

        public int Order { get; }

        #region AuthorizationApplicationModelProvider原生方法

        /// <summary>
        /// 获取过滤器
        /// </summary>
        /// <param name="policyProvider"></param>
        /// <param name="authData"></param>
        /// <returns></returns>
        private static AuthorizeFilter GetFilter(IAuthorizationPolicyProvider policyProvider, IEnumerable<IAuthorizeData> authData)
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

        /// <summary>
        /// 编译策略过滤器
        /// </summary>
        /// <param name="policyProvider"></param>
        /// <param name="authorizeData"></param>
        /// <returns></returns>
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

        #endregion

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
            foreach (var controllerModel in context.Result.Controllers)
            {
                var controllerModelAuthData = controllerModel.Attributes.OfType<IAuthorizeData>().ToArray();
                if (controllerModelAuthData.Length > 0)
                {
                    controllerModel.Filters.Add(GetFilter(_policyProvider, controllerModelAuthData));
                }
                foreach (var attribute in controllerModel.Attributes.OfType<IAllowAnonymous>())
                {
                    controllerModel.Filters.Add(new AllowAnonymousFilter());
                }
                foreach (var actionModel in controllerModel.Actions)
                {
                    var actionModelAuthData = actionModel.Attributes.OfType<IAuthorizeData>().ToArray();
                    if (actionModelAuthData.Length > 0)
                    {
                        actionModel.Filters.Add(GetFilter(_policyProvider, actionModelAuthData));
                    }
                    foreach (var attribute in actionModel.Attributes.OfType<IAllowAnonymous>())
                    {
                        actionModel.Filters.Add(new AllowAnonymousFilter());
                    }
                }
            }
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            //
        }
    }

    #endregion

}
