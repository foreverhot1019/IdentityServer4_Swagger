using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyAuthMVC
{
    public class MyAuthorizeFilter : AuthorizeFilter
    {
        public MyAuthorizeFilter(AuthorizationPolicy policy)
            : base(policy)
        {
        }
        public MyAuthorizeFilter(string policy)
            : base(policy)
        {
        }
        public override Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var dataTokens = context.RouteData.DataTokens;//获取 Area等数据
            var AuthSchemes = Policy.AuthenticationSchemes;
            var Claims_Identity = context.HttpContext.User.Identity as System.Security.Claims.ClaimsIdentity;
            if(Claims_Identity.IsAuthenticated)
            {
                var Arr = Policy.Requirements.ToList();
                //.Where(x=>x.GetType() == typeof())...AsEnumerable<System.Security.Claims.Claim>();
                //Claims_Identity.HasClaim()
            }
            //context.HttpContext.Authentication.HttpContext.Authentication.
            return base.OnAuthorizationAsync(context);
        }
    }

    public class MyIAuthorizeFilter : IAuthorizationFilter
    {
        /// <summary>
        /// AllowAnonymous
        /// 控制器或者Action 上标记AllowAnonymous就会自动添加AllowAnonymousFilter
        /// </summary>
        private readonly Type OAllowAnonymousFilter = typeof(AllowAnonymousFilter);

        private readonly List<(string ControllerName, string ActionName)> ArrIngoreAuthorizeLink = new List<(string ControllerName, string ActionName)> {
            (ControllerName:"Home",ActionName:"Error"),
            (ControllerName:"Account",ActionName:"Login"),
        };

        public MyIAuthorizeFilter()
        {

        }

        /// <summary>
        /// 授权
        /// </summary>
        /// <param name="context"></param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var routeData = context.RouteData;
            var dataTokens = routeData.DataTokens;//获取 Area等数据
            var ActionDes = context.ActionDescriptor;
            //判断Action-AllowAnonymous
            var IngoreAuthorize = ActionDes.FilterDescriptors.Any(x => x.GetType() == OAllowAnonymousFilter);
            if (IngoreAuthorize)
            {
                return;
            }
            string fullActionName = ActionDes.DisplayName;//通过ActionContext类的ActionDescriptor属性，也可以获取Action的名称
                                                          //不过这样获取到的是Action的完全限定名："AspNetCoreFilterContext.Controllers.HomeController.Index (AspNetCoreFilterContext)"，可以看到其中还包含Controller的类名、命名空间和程序集名称

            var ArrPrpperty = ActionDes.GetType().GetProperties();
            //var ControllerDescriptor = ArrPrpperty.Where(x => x.Name == "ControllerTypeInfo").FirstOrDefault().GetValue(ActionDes);
            //var ActionAttribute = ArrPrpperty.Where(x => x.IsDefined(typeof(AllowAnonymousAttribute), true));
            //获取当前 请求的 Controller名称
            string controllerName = routeData.Values["Controller"].ToString();//通过ActionContext类的RouteData属性获取Controller的名称：Home
            //var controllerName = ActionDes.ControllerName;
            //var controllerName = ActionDes.GetType().GetProperty("ControllerName").GetValue(ActionDes);
            //获取当前 请求的 Controller中的Action名称
            string actionName = routeData.Values["Action"].ToString();//通过ActionContext类的RouteData属性获取Action的名称：Index
            //var actionName = ActionDes.ActionName;
            //var actionName = ActionDes.GetType().GetProperty("ActionName").GetValue(ActionDes);

            //过滤 特殊 链接
            if (!ArrIngoreAuthorizeLink.Any(x => x.ControllerName == controllerName && x.ActionName == actionName))
            {
                var User = context.HttpContext?.User;
                if (User != null)
                {
                    //处理 未
                    var Claims_Identity = User.Identity as System.Security.Claims.ClaimsIdentity;
                    if (!Claims_Identity.IsAuthenticated)
                    {
                        //context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                        context.HttpContext.Response.Redirect("/Account/Login");
                        return;
                    }
                    else
                    {
                        //验证Claims/SecurityStamp 等
                    }
                }
                else
                {
                    //context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                    context.HttpContext.Response.Redirect("/Account/Login");
                    return;
                }
            }
        }

    }

    /// <summary>
    /// 自定义控制器公约
    /// </summary>
    public class AddAuthorizeFiltersControllerConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (controller.ControllerName.Contains("Api"))
            {
                controller.Filters.Add(new AuthorizeFilter("apipolicy"));
            }
            else
            {
                controller.Filters.Add(new AuthorizeFilter("defaultpolicy"));
            }
        }
    }

}