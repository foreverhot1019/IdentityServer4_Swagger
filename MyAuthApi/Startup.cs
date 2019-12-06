using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MyAuthApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(); // WebAPI
            //services.AddControllersWithViews(); // MVC
            //services.AddRazorPages(); // RazorPage

            //IdentityModelEventSource.ShowPII = true;//显示IdentityServer具体错误
            //认证方式配置
            services.AddAuthentication(Configuration["IdentitySrvAuth:Scheme"])
            .AddIdentityServerAuthentication(opts =>//IdentityServer认证方式配置
            {
                //opts.ApiName = "productservice";//增加这个 会验证与IdentityServer4 JWT-AccessToken中的aud 是否一致
                opts.RequireHttpsMetadata = true;// for dev env
                opts.Authority = $"https://{Configuration["IdentitySrvAuth:IP"]}:{Configuration["IdentitySrvAuth:Port_ssl"]}";
                //认证过期时间验证间隔时间
                opts.JwtValidationClockSkew = TimeSpan.FromSeconds(10);

                opts.JwtBearerEvents = new JwtBearerEvents
                {
                    //验证token是否符合自定义规范
                    OnTokenValidated = async tkvldContext =>
                    {
                        if (tkvldContext.Principal.Identity.IsAuthenticated)
                        {
                            var ArrClaim = tkvldContext.Principal.Claims;
                            var exp = ArrClaim.Where(x => x.Type == "exp").FirstOrDefault()?.Value;
                            long.TryParse(exp, out long longVal);
                            var s = new DateTime(longVal);
                            var SecurityStamp = ArrClaim.Where(x => x.Type == "AspNet.Identity.SecurityStamp").FirstOrDefault()?.Value;
                            var Identifier = ArrClaim.Where(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").FirstOrDefault()?.Value;

                            //UserManager<IdentityUser> userManager = tkvldContext.HttpContext.RequestServices.GetRequiredService<UserManager<IdentityUser>>();
                            //userManager.GetUserAsync(tkvldContext.Principal);
                            //tkvldContext.Success();
                        }
                        else
                            tkvldContext.Fail("认证失败");
                    }
                };
            })
            .AddJwtBearer("MyJwtBearer", opts =>
            {
                //认证过期时间间隔
                opts.TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(10);
                opts.Events = new JwtBearerEvents
                {
                    //验证token是否符合自定义规范
                    OnTokenValidated = async tkvldContext =>
                    {
                        if (tkvldContext.Principal.Identity.IsAuthenticated)
                        {
                            var ArrClaim = tkvldContext.Principal.Claims;
                            var exp = ArrClaim.Where(x => x.Type == "exp").FirstOrDefault()?.Value;
                            long.TryParse(exp, out long longVal);
                            var s = new DateTime(longVal);
                            var SecurityStamp = ArrClaim.Where(x => x.Type == "AspNet.Identity.SecurityStamp").FirstOrDefault()?.Value;
                            var Identifier = ArrClaim.Where(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").FirstOrDefault()?.Value;

                            //UserManager<IdentityUser> userManager = tkvldContext.HttpContext.RequestServices.GetRequiredService<UserManager<IdentityUser>>();
                            //userManager.GetUserAsync(tkvldContext.Principal);
                            //tkvldContext.Success();
                        }
                        else
                            tkvldContext.Fail("认证失败");
                    }
                };
            });
            //授权配置
            services.AddAuthorization(opts =>
            {
                var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                    JwtBearerDefaults.AuthenticationScheme,
                    "MyJwtBearer");
                defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
                opts.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
            });

            //swagger配置
            services.AddSwaggerGen(SwaggerGenOpts =>
            {
                // resolve the IApiVersionDescriptionProvider service
                // note: that we have to build a temporary service provider here because one has not been created yet
                //var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
                SwaggerGenOpts.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "V1.0.0",
                    Title = "My Swagger Demo",
                    Description = "webapi适配swagger",
                    //TermsOfService = "None",
                    Contact = new OpenApiContact
                    {
                        Name = "Michael-Swagger",
                        Email = "Michael_Wang1019@feiliks.com",
                        Url = new Uri("https://blog.csdn.net/foreverhot1019")
                    }
                });

                // 为 Swagger JSON and UI设置xml文档注释路径
                var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);//获取应用程序所在目录（绝对，不受工作目录影响，建议采用此方法获取路径）
                //添加接口XML的路径
                var xmlPath = Path.Combine(basePath, "MyAuthApiSwagger.xml");
                //如果需要显示控制器注释只需将第二个参数设置为true
                SwaggerGenOpts.IncludeXmlComments(xmlPath, true);

                #region 增加 全局 http-Header-Authorization=Bearer Token 设置

                // Add security definitions-Swagger增加令牌获取
                SwaggerGenOpts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "Please enter into field the word 'Bearer' followed by a space and the JWT value",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                });
                // 增加一个全局的安全必须
                SwaggerGenOpts.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { 
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference()
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        }, 
                        Array.Empty<string>()
                    }
                });

                #endregion

                #region 增加一个 全局获取oauth2-token 设置
                // app.UseSwaggerUI 设置 client_Id&client_secret

                //Swagger增加令牌获取
                SwaggerGenOpts.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        ClientCredentials = new OpenApiOAuthFlow
                        {
                            //AuthorizationUrl code和implicit 模式-获取登录授权界面
                            TokenUrl = new Uri("https://localhost:44365/connect/token", UriKind.Absolute),//获取登录授权接口
                            Scopes = new Dictionary<string, string> { { "clientservice", "client_service" } },//指定客户端请求的api作用域。 如果为空，则客户端无法访问
                        }
                    },
                    In = ParameterLocation.Query
                });
                // 增加一个全局的安全必须
                SwaggerGenOpts.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                        },
                        new[] { "clientservice" }
                    }
                });

                #endregion

                //添加httpHeader参数 报 Required field is not provided
                //SwaggerGenOpts.OperationFilter<HttpHeaderOperation>();
                //
                //SwaggerGenOpts.OperationFilter<AuthResponsesOperationFilter>();

                #region 多个Swagger https://www.cnblogs.com/weihanli/p/ues-swagger-in-aspnetcore3_0.html

                //SwaggerGenOpts.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "API V1" });
                //SwaggerGenOpts.SwaggerDoc("v2", new OpenApiInfo { Version = "v2", Title = "API V2" });

                //SwaggerGenOpts.DocInclusionPredicate((docName, apiDesc) =>
                //{
                //    var versions = apiDesc.CustomAttributes()
                //        .OfType<ApiVersionAttribute>()
                //        .SelectMany(attr => attr.Versions);

                //    return versions.Any(v => $"v{v.ToString()}" == docName);
                //});

                //SwaggerGenOpts.OperationFilter<RemoveVersionParameterOperationFilter>();
                //SwaggerGenOpts.DocumentFilter<SetVersionInPathDocumentFilter>();

                #endregion
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //强制使用Https
            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            #region Swagger配置

            app.UseSwagger(opts =>
            {
                //opts.RouteTemplate = "swagger/{documentName}/swagger.json";
            });
            app.UseSwaggerUI(opts =>
            {
                opts.SwaggerEndpoint("/Swagger/v1/swagger.json", "ApiHelper V1");
                #region OAuth 配置

                //opts.OAuthAppName("ClientsService-OAuthAppName");//名称
                //opts.OAuthClientId("product.api.service");
                //opts.OAuthClientSecret("productsecret");
                ////opts.OAuth2RedirectUrl//跳转地址code和implicit 模式-用到
                ////opts.OAuthAdditionalQueryStringParams//额外参数
                ////opts.OAuthUseBasicAuthenticationWithAccessCodeGrant();
                ////opts.OAuthRealm//OAuth1  额外参数 增加到 AuthorizationUrl&TokenUrl

                #endregion
            });

            #endregion
        }
    }
}
