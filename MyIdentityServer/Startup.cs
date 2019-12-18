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
            //��ǰ��������
            var CurrentAssemblyName = Assembly.GetExecutingAssembly().FullName;
            var AppName = Configuration["AppName"]?? "My-App";
            //redis �����ַ���
            var redisConnStr = Configuration.GetConnectionString("RedisConnection"); 
            var redis = ConnectionMultiplexer.Connect(redisConnStr);//����Redis ����
            //Session����ʱ��
            var SessionExpired = Configuration.GetValue<int?>("Session:Expired") ??180;
            var SessionName = Configuration.GetValue<string>("Session:Name") ?? "MySession";

            var basePath = Directory.GetCurrentDirectory();//Path.GetDirectoryName(typeof(Program).Assembly.Location);//��ȡӦ�ó�������Ŀ¼�����ԣ����ܹ���Ŀ¼Ӱ�죬������ô˷�����ȡ·����
            //֤��·��
            var CerDirPath = Path.Combine(Directory.GetCurrentDirectory(), Configuration["Certificates:CerPath"]);
            var CerFilePath = Path.Combine(CerDirPath, Configuration["Certificates:CerFileName"]);
            var DataProtection = Configuration["DataProtection:DirPath"] ?? "";
            Console.WriteLine(DataProtection);
            //��Ⱥ
            var Cluster = Configuration.GetValue<bool>("Cluster");
            /*
             * %LocalAppData%\ASP.NET\DataProtection-Keys
             * %LocalAppData% = C:\Users\��¼�˻�\AppData\Local
             * %AppData% = C:\Users\��¼�˻�\AppData\Roaming
             * https://docs.microsoft.com/zh-cn/aspnet/core/security/data-protection/configuration/overview?view=aspnetcore-3.0
             * �Զ��� ���ݱ�����Կ��������FrameWork ��MachineKey��
             * Ӧ��֮�乲���ܱ����ĸ���SetApplicationName
             */
            if (Cluster)
            {
                //var LOCALAPPDATA = Environment.GetEnvironmentVariable("LocalAppData");
                //Console.WriteLine(LOCALAPPDATA);
                //var FileDir = new DirectoryInfo(DataProtection.Replace("%LocalAppData%", LOCALAPPDATA));
                //Console.WriteLine(FileDir);
                #region ��ȺģʽDataProtection

                #region �����ļ���Կ����

                //services.AddDataProtection()
                //.SetApplicationName(AppName)
                //.AddKeyManagementOptions(options =>
                //{
                //    options.XmlRepository = new XmlRepository(Configuration);
                //});

                #endregion

                #endregion

                #region Redis��Կ����&Session�ֲ�ʽ

                // DataProtection persist in redis
                services.AddDataProtection()
                    .SetApplicationName(AppName)
                    .PersistKeysToStackExchangeRedis(() => redis.GetDatabase(1), "DataProtection-Keys");

                //���Redis�������ڷֲ�ʽSession
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnStr;
                    options.InstanceName = SessionName;//CurrentAssemblyName;
                });

                //���Session
                services.AddSession(options =>
                {
                    options.Cookie.Name = SessionName;
                    options.IdleTimeout = TimeSpan.FromMinutes(SessionExpired);//����session�Ĺ���ʱ��
                    options.Cookie.HttpOnly = true;//���������������ͨ��js��ø�cookie��ֵ
                    options.Cookie.IsEssential = true;
                });

                #endregion
            }

            InMemoryConfiguration.Configuration = this.Configuration;

            var DbContextConnStr = Configuration.GetConnectionString("DefaultConnection");
            //��������
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            //DbContext����
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(DbContextConnStr, b => { b.MigrationsAssembly(migrationsAssembly); });
            });
            //AspNet.Identity����
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
            .AddClaimsPrincipalFactory<CustomUserClaimsFactory<IdentityRole>>()//���ƾ֤֧��IdentityRole��Claim
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            services.AddControllersWithViews();
            services.AddRazorPages();
            //����Razor·�ɹ�Լ
            //services.AddRazorPages(o => o.Conventions.AddAreaFolderRouteModelConvention("Identity", "/Account/", model => { 
            //    foreach (var selector in model.Selectors) 
            //    { 
            //        var attributeRouteModel = selector.AttributeRouteModel; 
            //        attributeRouteModel.Order = -1; 
            //        attributeRouteModel.Template = attributeRouteModel.Template.Remove(0, "Identity".Length); 
            //    } 
            //}));

            ////��֤���������������ʱ��֤��
            //services.AddAuthentication();
            ////��Ȩ������Actionʱ��֤���ư�����Claims��Ȩ�ޣ�
            //services.AddAuthorization();

            //��֤������
            //http://localhost:5000/.well-known/openid-configuration
            services.AddIdentityServer(options =>
            {
                //scope����Ȩ��Χ������������� scope �ڣ����ɵ�access_token�����ܷ��ʱ�����
                //lifetime���������ڣ������ڵ�access_token����Ч���ʡ�
                //client ID(client_id)����ͬ�Ŀͻ��� ID�����ɲ�ͬ��Ӧ��access_token��
                //issuer name(iss)��������������������ơ�����������������
                //RSA ����֤�飨���䣩����ͬ�ļ���֤�飬���ɲ�ͬ��Ӧ��access_token��
                options.IssuerUri = "https://172.20.60.181:8013"; //slb ��ַ
                options.UserInteraction = new IdentityServer4.Configuration.UserInteractionOptions
                {
                    LoginUrl = "/Account/Login",//���ر�����¼��ַ  
                    LogoutUrl = "/Account/Logout",//���ر����˳���ַ 
                    //ConsentUrl = "/Consent",//���ر���������Ȩͬ��ҳ���ַ
                    //ErrorUrl = "/Account/Error", //���ر�������ҳ���ַ
                    LoginReturnUrlParameter = "ReturnUrl",//���ر������ô��ݸ���¼ҳ��ķ���URL���������ơ�Ĭ��ΪreturnUrl 
                    LogoutIdParameter = "logoutId", //���ر������ô��ݸ�ע��ҳ���ע����ϢID���������ơ�ȱʡΪlogoutId 
                    ConsentReturnUrlParameter = "ReturnUrl", //���ر������ô��ݸ�ͬ��ҳ��ķ���URL���������ơ�Ĭ��ΪreturnUrl
                    ErrorIdParameter = "errorId", //���ر������ô��ݸ�����ҳ��Ĵ�����ϢID���������ơ�ȱʡΪerrorId
                    CustomRedirectReturnUrlParameter = "ReturnUrl", //���ر������ô���Ȩ�˵㴫�ݸ��Զ����ض���ķ���URL���������ơ�Ĭ��ΪreturnUrl
                    CookieMessageThreshold = 5, //���ر��������������Cookie�Ĵ�С�����ƣ�����Cookies���������ƣ���Ч�ı�֤��������򿪶��ѡ���һ��������Cookies���ƾͻ������ǰ��Cookiesֵ
                };
            })
            //��������֤��
            //.AddDeveloperSigningCredential(filename: "tempkey.rsa")
            //�Զ���֤��OpenSSL-Win64 ����֤��
            .AddSigningCredential(new X509Certificate2(CerFilePath, Configuration["Certificates:Password"]))
            #region ����չ�����ݿ��пɶ�̬����AddConfigurationStore��

            //.AddInMemoryIdentityResources(InMemoryConfiguration.GetIdentityResources())
            //.AddInMemoryApiResources(InMemoryConfiguration.GetApiResources())
            //.AddInMemoryClients(InMemoryConfiguration.GetClients())

            #endregion
            // this adds the config data from DB (clients, resources, CORS)
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = builder => builder.UseSqlServer(DbContextConnStr, opts =>
                {
                    //MigrationsAssembly���򼯱�������һ��
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
                    //MigrationsAssembly���򼯱�������һ��
                    //dotnet ef migrations add InitPersistedGrant -c PersistedGrantDbContext -o Data/Migrations/IdentityServer/PersistedGrant
                    opts.MigrationsAssembly(migrationsAssembly);//"MyIdentityServer"
                });
                options.DefaultSchema = "";
                // this enables automatic token cleanup. this is optional.
                options.EnableTokenCleanup = true;
                options.TokenCleanupInterval = 30; // interval in seconds, short for testing
            })
            //.AddProfileService<MyProfileServices>()//�Զ��� �û�Ȩ��ҳ��Ϣ ������Ч ����replace�� Services�з���
            //.AddResourceOwnerValidator<ResourceOwnerPasswordExt>()//�Զ�����Դ����������ģʽ��֤
            //.AddExtensionGrantValidator<MyGrantTypeValidator>()//�Զ���GrantType
            //.AddCustomTokenRequestValidator<MyCustomTokenRequestValidatorExt>()//�Զ�������������֤
            .AddAspNetIdentity<ApplicationUser>()
            //.AddClientStore<MyClientStore>()//�Զ���ͻ��˿�;//����֧��Asp.Net.IdentityUser�˻�
            // this is something you will want in production to reduce load on and requests to the DB
            //.AddConfigurationStoreCache();
            ;

            ////�Զ��� �ͻ�����Դ��Կ��֤
            //services.AddTransient<IClientSecretValidator, ClientSecretValidatorExt>();
            ////�Զ��� Api��Դ��Կ��֤
            //services.AddTransient<IApiSecretValidator, MyApiSecretValidatorExt>();

            services.AddTransient<IProfileService, MyProfileServices>();//�Զ��� �û�Ȩ��ҳ��Ϣ 
            services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordExt>();//�Զ�����Դ����������ģʽ��֤

            //��ֹCSRF����
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

            //ǿ��ʹ��Https
            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            //ʹ��·��
            app.UseRouting();
            //ʹ��Session
            app.UseSession();

            //��֤��ʽ
            //app.UseAuthentication();// UseAuthentication not needed -- UseIdentityServer add this
            app.UseIdentityServer();
            //��Ȩ
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
        /// ��ʼ������
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
