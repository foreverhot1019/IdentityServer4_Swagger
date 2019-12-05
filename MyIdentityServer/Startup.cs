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
            .AddClaimsPrincipalFactory<CustomUserClaimsFactory<IdentityRole>>()//���ƾ֤֧��IdentityRole��Claim
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

            ////��֤���������������ʱ��֤��
            //services.AddAuthentication();
            ////��Ȩ������Actionʱ��֤���ư�����Claims��Ȩ�ޣ�
            //services.AddAuthorization();


            //��֤������
            //http://localhost:5000/.well-known/openid-configuration
            services.AddIdentityServer(options =>
            {
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
            .AddDeveloperSigningCredential(filename: "tempkey.rsa")//��������֤��
            //�Զ���֤��
            //.AddSigningCredential(new X509Certificate2(Path.Combine(basePath,
            //    Configuration["Certificates:CerPath"]),
            //    Configuration["Certificates:Password"]))
            #region ����չ�����ݿ��пɶ�̬����AddConfigurationStore��

            .AddInMemoryIdentityResources(InMemoryConfiguration.GetIdentityResources())
            .AddInMemoryApiResources(InMemoryConfiguration.GetApiResources())
            .AddInMemoryClients(InMemoryConfiguration.GetClients())

            #endregion
            //// this adds the config data from DB (clients, resources, CORS)
            //.AddConfigurationStore(options =>
            //{
            //    options.ConfigureDbContext = builder => builder.UseSqlServer(DbContextConnStr, opts =>
            //    {
            //        //MigrationsAssembly���򼯱�������һ��
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
            //        //MigrationsAssembly���򼯱�������һ��
            //        //dotnet ef migrations add InitPersistedGrant -c PersistedGrantDbContext -o Data/Migrations/IdentityServer/PersistedGrant
            //        opts.MigrationsAssembly(migrationsAssembly);//"MyIdentityServer"
            //    });
            //    options.DefaultSchema = "";
            //    // this enables automatic token cleanup. this is optional.
            //    options.EnableTokenCleanup = true;
            //    options.TokenCleanupInterval = 30; // interval in seconds, short for testing
            //})
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

            services.AddTransient<IProfileService, MyProfileServices>();
            services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordExt>();

            //��ֹCSRF����
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

            //ǿ��ʹ��Https
            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

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
