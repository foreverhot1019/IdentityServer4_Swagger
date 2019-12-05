using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using MyIdentityServer.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyIdentityServer.IdentityServer;
using IdentityServer4.Validation;
using System.Reflection;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using MyIdentityServer.Models;
using IdentityServer4.Services;
using IdentityServer4.AspNetIdentity;

namespace MyIdentityServer
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
            InMemoryConfiguration.Configuration = this.Configuration;

            var DbContextConnStr = Configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddDbContext<ApplicationDbContext>(options => {
                options.UseSqlServer(DbContextConnStr, b => { b.MigrationsAssembly(migrationsAssembly); });
            });

            services.AddIdentity<ApplicationUser, IdentityRole>(IdentityOpts =>
            {
                // Password settings.
                IdentityOpts.Password.RequireDigit = true;
                IdentityOpts.Password.RequireLowercase = true;
                IdentityOpts.Password.RequireNonAlphanumeric = true;
                IdentityOpts.Password.RequireUppercase = true;
                IdentityOpts.Password.RequiredLength = 6;
                IdentityOpts.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                IdentityOpts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                IdentityOpts.Lockout.MaxFailedAccessAttempts = 5;
                IdentityOpts.Lockout.AllowedForNewUsers = true;

                // User settings.
                IdentityOpts.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                IdentityOpts.User.RequireUniqueEmail = false;
            })
            .AddClaimsPrincipalFactory<CustomUserClaimsFactory<IdentityRole>>()//添加凭证支持IdentityRole到Claim
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            //services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomUserClaimsFactory<IdentityRole>>();

            services.AddControllersWithViews();
            services.AddRazorPages();
            //services.AddRazorPages(o => o.Conventions.AddAreaFolderRouteModelConvention("Identity", "/Account/", model => { 
            //    foreach (var selector in model.Selectors) 
            //    { 
            //        var attributeRouteModel = selector.AttributeRouteModel; 
            //        attributeRouteModel.Order = -1; 
            //        attributeRouteModel.Template = attributeRouteModel.Template.Remove(0, "Identity".Length); 
            //    } 
            //}));

            ////认证（请求带过来令牌时验证）
            //services.AddAuthentication();
            ////授权（访问Action时验证令牌包含的Claims的权限）
            //services.AddAuthorization();


            //认证服务器
            //http://localhost:5000/.well-known/openid-configuration
            services.AddIdentityServer(options =>
            {
                options.UserInteraction = new IdentityServer4.Configuration.UserInteractionOptions
                {
                    LoginUrl = "/Account/Login",//【必备】登录地址  
                    LogoutUrl = "/Account/Logout",//【必备】退出地址 
                    //ConsentUrl = "/Consent",//【必备】允许授权同意页面地址
                    //ErrorUrl = "/Account/Error", //【必备】错误页面地址
                    LoginReturnUrlParameter = "ReturnUrl",//【必备】设置传递给登录页面的返回URL参数的名称。默认为returnUrl 
                    LogoutIdParameter = "logoutId", //【必备】设置传递给注销页面的注销消息ID参数的名称。缺省为logoutId 
                    ConsentReturnUrlParameter = "ReturnUrl", //【必备】设置传递给同意页面的返回URL参数的名称。默认为returnUrl
                    ErrorIdParameter = "errorId", //【必备】设置传递给错误页面的错误消息ID参数的名称。缺省为errorId
                    CustomRedirectReturnUrlParameter = "ReturnUrl", //【必备】设置从授权端点传递给自定义重定向的返回URL参数的名称。默认为returnUrl
                    CookieMessageThreshold = 5, //【必备】由于浏览器对Cookie的大小有限制，设置Cookies数量的限制，有效的保证了浏览器打开多个选项卡，一旦超出了Cookies限制就会清除以前的Cookies值
                };
            })
            .AddDeveloperSigningCredential(filename: "tempkey.rsa")//开发环境证书
            //自定义证书
            //.AddSigningCredential(new X509Certificate2(Path.Combine(basePath,
            //    Configuration["Certificates:CerPath"]),
            //    Configuration["Certificates:Password"]))
            #region 已扩展到数据库中可动态管理（AddConfigurationStore）

            .AddInMemoryIdentityResources(InMemoryConfiguration.GetIdentityResources())
            .AddInMemoryApiResources(InMemoryConfiguration.GetApiResources())
            .AddInMemoryClients(InMemoryConfiguration.GetClients())

            #endregion
            //// this adds the config data from DB (clients, resources, CORS)
            //.AddConfigurationStore(options =>
            //{
            //    options.ConfigureDbContext = builder => builder.UseSqlServer(DbContextConnStr, opts =>
            //    {
            //        //MigrationsAssembly程序集必须设置一致
            //        //dotnet ef migrations add InitConfigration -c ConfigurationDbContext -o Data/Migrations/IdentityServer/Configuration
            //        opts.MigrationsAssembly(migrationsAssembly);//"MyIdentityServer"
            //    });
            //    options.DefaultSchema = "";
            //})
            //// this adds the operational data from DB (codes, tokens, consents)
            //.AddOperationalStore(options =>
            //{
            //    options.ConfigureDbContext = builder => builder.UseSqlServer(DbContextConnStr, opts =>
            //    {
            //        //MigrationsAssembly程序集必须设置一致
            //        //dotnet ef migrations add InitPersistedGrant -c PersistedGrantDbContext -o Data/Migrations/IdentityServer/PersistedGrant
            //        opts.MigrationsAssembly(migrationsAssembly);//"MyIdentityServer"
            //    });
            //    options.DefaultSchema = "";
            //    // this enables automatic token cleanup. this is optional.
            //    options.EnableTokenCleanup = true;
            //    options.TokenCleanupInterval = 30; // interval in seconds, short for testing
            //})
            //.AddProfileService<MyProfileServices>()//自定义 用户权限页信息 这样无效 必须replace掉 Services中服务
            //.AddResourceOwnerValidator<ResourceOwnerPasswordExt>()//自定义资源所有者密码模式认证
            //.AddExtensionGrantValidator<MyGrantTypeValidator>()//自定义GrantType
            //.AddCustomTokenRequestValidator<MyCustomTokenRequestValidatorExt>()//自定义令牌请求认证
            .AddAspNetIdentity<ApplicationUser>()
            //.AddClientStore<MyClientStore>()//自定义客户端库;//增加支持Asp.Net.IdentityUser账户
            // this is something you will want in production to reduce load on and requests to the DB
            //.AddConfigurationStoreCache();
            ;

            ////自定义 客户端资源密钥验证
            //services.AddTransient<IClientSecretValidator, ClientSecretValidatorExt>();
            ////自定义 Api资源密钥验证
            //services.AddTransient<IApiSecretValidator, MyApiSecretValidatorExt>();

            services.AddTransient<IProfileService, MyProfileServices>();
            services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordExt>();

            //防止CSRF攻击
            services.AddAntiforgery(opts => {
                opts.HeaderName = "MichaelAntiforgery";
                //opts.SuppressXFrameOptionsHeader
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            // this will do the initial DB population
            //InitializeDatabase(app);

            //强制使用Https
            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            //认证方式
            //app.UseAuthentication();// UseAuthentication not needed -- UseIdentityServer add this
            app.UseIdentityServer();
            //授权
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <param name="app"></param>
        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                if (!context.Clients.Any())
                {
                    foreach (var client in InMemoryConfiguration.GetClients())
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in InMemoryConfiguration.GetIdentityResources())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in InMemoryConfiguration.GetApiResources())
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}
