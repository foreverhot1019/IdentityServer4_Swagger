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
using System.Security.Cryptography.X509Certificates;
using System.IO;
using Microsoft.AspNetCore.DataProtection;
using MyIdentityServer.DataProtection;
using StackExchange.Redis;

namespace MyIdentityServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;

            //var ConfigBuilder = new ConfigurationBuilder()
            //          .SetBasePath(env.ContentRootPath)
            //          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            //          .AddEnvironmentVariables();
            //if (env.IsDevelopment())
            //{
            //    ConfigBuilder.AddUserSecrets<Startup>();
            //}

            //Configuration = ConfigBuilder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //当前程序集名称
            var CurrentAssemblyName = Assembly.GetExecutingAssembly().FullName;
            var AppName = Configuration["AppName"]?? "My-App";
            //redis 链接字符串
            var redisConnStr = Configuration.GetConnectionString("RedisConnection"); 
            var redis = ConnectionMultiplexer.Connect(redisConnStr);//建立Redis 连接
            //Session过期时间
            var SessionExpired = Configuration.GetValue<int?>("Session:Expired") ??180;
            var SessionName = Configuration.GetValue<string>("Session:Name") ?? "MySession";

            var basePath = Directory.GetCurrentDirectory();//Path.GetDirectoryName(typeof(Program).Assembly.Location);//获取应用程序所在目录（绝对，不受工作目录影响，建议采用此方法获取路径）
            //证书路径
            var CerDirPath = Path.Combine(Directory.GetCurrentDirectory(), Configuration["Certificates:CerPath"]);
            var CerFilePath = Path.Combine(CerDirPath, Configuration["Certificates:CerFileName"]);
            var DataProtection = Configuration["DataProtection:DirPath"] ?? "";
            Console.WriteLine(DataProtection);
            //集群
            var Cluster = Configuration.GetValue<bool>("Cluster");
            /*
             * %LocalAppData%\ASP.NET\DataProtection-Keys
             * %LocalAppData% = C:\Users\登录账户\AppData\Local
             * %AppData% = C:\Users\登录账户\AppData\Roaming
             * https://docs.microsoft.com/zh-cn/aspnet/core/security/data-protection/configuration/overview?view=aspnetcore-3.0
             * 自定义 数据保护密钥（类似于FrameWork 中MachineKey）
             * 应用之间共享受保护的负载SetApplicationName
             */
            if (Cluster)
            {
                //var LOCALAPPDATA = Environment.GetEnvironmentVariable("LocalAppData");
                //Console.WriteLine(LOCALAPPDATA);
                //var FileDir = new DirectoryInfo(DataProtection.Replace("%LocalAppData%", LOCALAPPDATA));
                //Console.WriteLine(FileDir);
                #region 集群模式DataProtection

                #region 本地文件密钥配置

                //services.AddDataProtection()
                //.SetApplicationName(AppName)
                //.AddKeyManagementOptions(options =>
                //{
                //    options.XmlRepository = new XmlRepository(Configuration);
                //});

                #endregion

                #endregion

                #region Redis密钥配置&Session分布式

                // DataProtection persist in redis
                services.AddDataProtection()
                    .SetApplicationName(AppName)
                    .PersistKeysToStackExchangeRedis(() => redis.GetDatabase(1), "DataProtection-Keys");

                //添加Redis缓存用于分布式Session
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnStr;
                    options.InstanceName = SessionName;//CurrentAssemblyName;
                });

                //添加Session
                services.AddSession(options =>
                {
                    options.Cookie.Name = SessionName;
                    options.IdleTimeout = TimeSpan.FromMinutes(SessionExpired);//设置session的过期时间
                    options.Cookie.HttpOnly = true;//设置在浏览器不能通过js获得该cookie的值
                    options.Cookie.IsEssential = true;
                });

                #endregion
            }

            InMemoryConfiguration.Configuration = this.Configuration;

            var DbContextConnStr = Configuration.GetConnectionString("DefaultConnection");
            //程序集名称
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            //DbContext设置
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(DbContextConnStr, b => { b.MigrationsAssembly(migrationsAssembly); });
            });
            //AspNet.Identity设置
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

            services.AddControllersWithViews();
            services.AddRazorPages();
            //增加Razor路由公约
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
                //scope（授权范围）：服务包含在 scope 内，生成的access_token，才能访问本服务。
                //lifetime（生命周期）：过期的access_token，无效访问。
                //client ID(client_id)：不同的客户端 ID，生成不同对应的access_token。
                //issuer name(iss)：翻译过来“发行者名称”，类似于主机名。
                //RSA 加密证书（补充）：不同的加密证书，生成不同对应的access_token。
                options.IssuerUri = "https://172.20.60.181:8013"; //slb 地址
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
            //开发环境证书
            //.AddDeveloperSigningCredential(filename: "tempkey.rsa")
            //自定义证书OpenSSL-Win64 生成证书
            .AddSigningCredential(new X509Certificate2(CerFilePath, Configuration["Certificates:Password"]))
            #region 已扩展到数据库中可动态管理（AddConfigurationStore）

            //.AddInMemoryIdentityResources(InMemoryConfiguration.GetIdentityResources())
            //.AddInMemoryApiResources(InMemoryConfiguration.GetApiResources())
            //.AddInMemoryClients(InMemoryConfiguration.GetClients())

            #endregion
            // this adds the config data from DB (clients, resources, CORS)
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = builder => builder.UseSqlServer(DbContextConnStr, opts =>
                {
                    //MigrationsAssembly程序集必须设置一致
                    //dotnet ef migrations add InitConfigration -c ConfigurationDbContext -o Data/Migrations/IdentityServer/Configuration
                    opts.MigrationsAssembly(migrationsAssembly);//"MyIdentityServer"
                });
                options.DefaultSchema = "";
            })
            // this adds the operational data from DB (codes, tokens, consents)
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = builder => builder.UseSqlServer(DbContextConnStr, opts =>
                {
                    //MigrationsAssembly程序集必须设置一致
                    //dotnet ef migrations add InitPersistedGrant -c PersistedGrantDbContext -o Data/Migrations/IdentityServer/PersistedGrant
                    opts.MigrationsAssembly(migrationsAssembly);//"MyIdentityServer"
                });
                options.DefaultSchema = "";
                // this enables automatic token cleanup. this is optional.
                options.EnableTokenCleanup = true;
                options.TokenCleanupInterval = 30; // interval in seconds, short for testing
            })
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

            services.AddTransient<IProfileService, MyProfileServices>();//自定义 用户权限页信息 
            services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordExt>();//自定义资源所有者密码模式认证

            //防止CSRF攻击
            services.AddAntiforgery(opts =>
            {
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
            //使用路由
            app.UseRouting();
            //使用Session
            app.UseSession();

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
