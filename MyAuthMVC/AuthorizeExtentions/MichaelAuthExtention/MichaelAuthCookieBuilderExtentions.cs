using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication1;

namespace MyAuthMVC
{
    public static class MichaelAuthCookieBuilderExtentions
    {
        /// <summary>
        /// DI注册服务代理
        /// </summary>
        public static IServiceProvider ServiceInstance { get; set; }

        #region 注册MichaelAuth - Cookie 认证

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authenticationBuilder"></param>
        /// <returns></returns>
        public static AuthenticationBuilder AddMichaelAuthCookie(this AuthenticationBuilder authenticationBuilder)
        {
            var CookieAuthOpts = new CookieAuthenticationOptions
            {
                Cookie = new CookieBuilder { Name = "MichaelAuth.NetCoreApp" },
                SlidingExpiration = true,
                LoginPath = "/Account/Login",
                ReturnUrlParameter = "RetMichael",
                ExpireTimeSpan = TimeSpan.FromSeconds(30),//票据过期时间
                ////Events设置到具体的类处理
                //EventsType = typeof(CustomCookieAuthenticationEvents);
                ////自定义MachineKey或者其他密钥
                //DataProtectionProvider = DataProtectionProvider.Create(new DirectoryInfo(@"C:\temp-keys\"));
                Events = new CookieAuthenticationEvents //认证事件
                {
                    //服务端的票据-逻辑验证
                    OnValidatePrincipal = PrincipalValidator.ValidateAsync,
                }
            };
            AddMichaelAuthCookie(authenticationBuilder, CookieAuthOpts);
            return authenticationBuilder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authenticationBuilder"></param>
        /// <param name="CookieAuthOpts"></param>
        /// <returns></returns>
        public static AuthenticationBuilder AddMichaelAuthCookie(this AuthenticationBuilder authenticationBuilder, CookieAuthenticationOptions CookieAuthOpts)
        {
            authenticationBuilder.AddCookie("MichaelAuth", CookieOpts =>
            {
                if (CookieAuthOpts.AccessDeniedPath != null)
                    CookieOpts.AccessDeniedPath = CookieAuthOpts.AccessDeniedPath;//
                if (!string.IsNullOrEmpty(CookieAuthOpts.ClaimsIssuer))
                    CookieOpts.ClaimsIssuer = CookieAuthOpts.ClaimsIssuer;//
                if (CookieAuthOpts.Cookie != null)
                    CookieOpts.Cookie = CookieAuthOpts.Cookie;//
                //CookieOpts.Cookie.Name = CookieAuthOpts.Cookie.Name;
                if (CookieAuthOpts.CookieManager != null)
                    CookieOpts.CookieManager = CookieAuthOpts.CookieManager;
                if (CookieAuthOpts.ExpireTimeSpan != null)
                    CookieOpts.ExpireTimeSpan = CookieAuthOpts.ExpireTimeSpan;//票据过期时间
                //Events设置到具体的类处理
                if (CookieAuthOpts.EventsType != null)
                    CookieOpts.EventsType = CookieAuthOpts.EventsType;
                //自定义MachineKey或者其他密钥
                if (CookieAuthOpts.DataProtectionProvider != null)
                    CookieOpts.DataProtectionProvider = CookieAuthOpts.DataProtectionProvider;
                if (CookieAuthOpts.Events != null)
                    CookieOpts.Events = CookieAuthOpts.Events;//认证事件
                if (!string.IsNullOrEmpty(CookieAuthOpts.ForwardAuthenticate))
                    CookieOpts.ForwardAuthenticate = CookieAuthOpts.ForwardAuthenticate;
                if (!string.IsNullOrEmpty(CookieAuthOpts.ForwardAuthenticate))
                    CookieOpts.ForwardChallenge = CookieAuthOpts.ForwardChallenge;
                if (!string.IsNullOrEmpty(CookieAuthOpts.ForwardAuthenticate))
                    CookieOpts.ForwardDefault = CookieAuthOpts.ForwardDefault;
                if (!string.IsNullOrEmpty(CookieAuthOpts.ForwardAuthenticate))
                    CookieOpts.ForwardDefaultSelector = CookieAuthOpts.ForwardDefaultSelector;
                if (!string.IsNullOrEmpty(CookieAuthOpts.ForwardForbid))
                    CookieOpts.ForwardForbid = CookieAuthOpts.ForwardForbid;
                if (!string.IsNullOrEmpty(CookieAuthOpts.ForwardSignIn))
                    CookieOpts.ForwardSignIn = CookieAuthOpts.ForwardSignIn;
                if (!string.IsNullOrEmpty(CookieAuthOpts.ForwardSignOut))
                    CookieOpts.ForwardSignOut = CookieAuthOpts.ForwardSignOut;
                if (!string.IsNullOrEmpty(CookieAuthOpts.LoginPath))
                    CookieOpts.LoginPath = CookieAuthOpts.LoginPath;//
                if (!string.IsNullOrEmpty(CookieAuthOpts.LogoutPath))
                    CookieOpts.LogoutPath = CookieAuthOpts.LogoutPath;//
                if (!string.IsNullOrEmpty(CookieAuthOpts.ReturnUrlParameter))
                    CookieOpts.ReturnUrlParameter = CookieAuthOpts.ReturnUrlParameter;//
                if (CookieAuthOpts.SessionStore != null)
                    CookieOpts.SessionStore = CookieAuthOpts.SessionStore;//将token-claims 保存在Session中,前端储存Session地址
                CookieOpts.SlidingExpiration = CookieAuthOpts.SlidingExpiration;//
                if (CookieAuthOpts.TicketDataFormat != null)
                    CookieOpts.TicketDataFormat = CookieAuthOpts.TicketDataFormat;//

            });
            return authenticationBuilder;
        }

        #endregion

        #region 注册 JsonWebToken 认证

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authenticationBuilder"></param>
        /// <returns></returns>
        public static AuthenticationBuilder AddJWTBeareAuth(this AuthenticationBuilder authenticationBuilder, IConfiguration Configuration)
        {
            var _JWTAuthOpts = new JwtBearerOptions
            {
                Audience = Configuration["JWT:Audience"] ?? "",
                TokenValidationParameters = new TokenValidationParameters
                {
                    ////System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames
                    ////Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames
                    //NameClaimType = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.UniqueName,//JwtClaimTypes.Name
                    //RoleClaimType = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Sub,//JwtClaimTypes.Role,

                    ValidIssuer = Configuration["JWT:Issuer"] ?? "",//验证 证书所有者
                    ValidAudience = Configuration["JWT:Audience"] ?? "",//验证 读取者
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:SecurityKey"] ?? "")),//颁发证书者-安全密钥

                    /***********************************TokenValidationParameters的参数默认值***********************************/
                    // RequireSignedTokens = true,
                    // SaveSigninToken = false,
                    // ValidateActor = false,
                    // 将下面两个参数设置为false，可以不验证Issuer和Audience，但是不建议这样做。
                    // ValidateAudience = true,
                    // ValidateIssuer = true, 
                    // ValidateIssuerSigningKey = false,
                    // 是否要求Token的Claims中必须包含Expires
                    // RequireExpirationTime = true,
                    // 允许的服务器时间偏移量
                    // ClockSkew = TimeSpan.FromSeconds(300),
                    // 是否验证Token有效期，使用当前时间与Token的Claims中的NotBefore和Expires对比
                    // ValidateLifetime = true
                },
                ////Events设置到具体的类处理
                //EventsType = typeof(CustomJwtUserValidationEvent);
                Events = new JwtBearerEvents()
                {
                    OnTokenValidated = PrincipalValidator.JWTValidateAsync,
                    //收到验证消息（url-access_token）
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Query["access_token"];
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = c =>
                    {
                        c.NoResult();
                        c.Response.StatusCode = 500;
                        c.Response.ContentType = "text/plain";
                        c.Response.WriteAsync(c.Exception.ToString()).Wait();
                        return Task.CompletedTask;
                    },
                    OnChallenge = c =>
                    {
                        c.HandleResponse();
                        return Task.CompletedTask;
                    }
                }
            };
            AddJWTBeareAuth(authenticationBuilder, _JWTAuthOpts);
            return authenticationBuilder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authenticationBuilder"></param>
        /// <param name="_JWTAuthOpts"></param>
        /// <returns></returns>
        public static AuthenticationBuilder AddJWTBeareAuth(this AuthenticationBuilder authenticationBuilder, JwtBearerOptions _JWTAuthOpts)
        {
            authenticationBuilder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, JWTAuthOpts =>
            {
                if (!string.IsNullOrEmpty(_JWTAuthOpts.Audience))
                    JWTAuthOpts.Audience = _JWTAuthOpts.Audience;
                if (!string.IsNullOrEmpty(_JWTAuthOpts.Authority))
                    JWTAuthOpts.Authority = _JWTAuthOpts.Authority;
                if (_JWTAuthOpts.BackchannelHttpHandler != null)
                    JWTAuthOpts.BackchannelHttpHandler = _JWTAuthOpts.BackchannelHttpHandler;
                if (_JWTAuthOpts.BackchannelTimeout != null)
                    JWTAuthOpts.BackchannelTimeout = _JWTAuthOpts.BackchannelTimeout;
                if (!string.IsNullOrEmpty(_JWTAuthOpts.Challenge))
                    JWTAuthOpts.Challenge = _JWTAuthOpts.Challenge;
                if (!string.IsNullOrEmpty(_JWTAuthOpts.ClaimsIssuer))
                    JWTAuthOpts.ClaimsIssuer = _JWTAuthOpts.ClaimsIssuer;
                if (_JWTAuthOpts.Configuration != null)
                    JWTAuthOpts.Configuration = _JWTAuthOpts.Configuration;
                if (_JWTAuthOpts.ConfigurationManager != null)
                    JWTAuthOpts.ConfigurationManager = _JWTAuthOpts.ConfigurationManager;
                if (_JWTAuthOpts.Events != null)
                    JWTAuthOpts.Events = _JWTAuthOpts.Events;
                if (_JWTAuthOpts.EventsType != null)
                    JWTAuthOpts.EventsType = _JWTAuthOpts.EventsType;
                if (!string.IsNullOrEmpty(_JWTAuthOpts.ForwardAuthenticate))
                    JWTAuthOpts.ForwardAuthenticate = _JWTAuthOpts.ForwardAuthenticate;
                if (!string.IsNullOrEmpty(_JWTAuthOpts.ForwardChallenge))
                    JWTAuthOpts.ForwardChallenge = _JWTAuthOpts.ForwardChallenge;
                if (!string.IsNullOrEmpty(_JWTAuthOpts.ForwardDefault))
                    JWTAuthOpts.ForwardDefault = _JWTAuthOpts.ForwardDefault;
                if (_JWTAuthOpts.ForwardDefaultSelector != null)
                    JWTAuthOpts.ForwardDefaultSelector = _JWTAuthOpts.ForwardDefaultSelector;
                if (!string.IsNullOrEmpty(_JWTAuthOpts.ForwardForbid))
                    JWTAuthOpts.ForwardForbid = _JWTAuthOpts.ForwardForbid;
                if (!string.IsNullOrEmpty(_JWTAuthOpts.ForwardSignIn))
                    JWTAuthOpts.ForwardSignIn = _JWTAuthOpts.ForwardSignIn;
                if (!string.IsNullOrEmpty(_JWTAuthOpts.ForwardSignOut))
                    JWTAuthOpts.ForwardSignOut = _JWTAuthOpts.ForwardSignOut;
                JWTAuthOpts.IncludeErrorDetails = _JWTAuthOpts.IncludeErrorDetails;
                if (!string.IsNullOrEmpty(_JWTAuthOpts.MetadataAddress))
                    JWTAuthOpts.MetadataAddress = _JWTAuthOpts.MetadataAddress;
                JWTAuthOpts.RefreshOnIssuerKeyNotFound = _JWTAuthOpts.RefreshOnIssuerKeyNotFound;
                JWTAuthOpts.RequireHttpsMetadata = _JWTAuthOpts.RequireHttpsMetadata;
                JWTAuthOpts.SaveToken = _JWTAuthOpts.SaveToken;
                if (_JWTAuthOpts.TokenValidationParameters != null)
                    JWTAuthOpts.TokenValidationParameters = _JWTAuthOpts.TokenValidationParameters;
            });

            return authenticationBuilder;
        }

        #endregion
    }
}
