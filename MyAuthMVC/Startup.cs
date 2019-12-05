using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using MyAuthMVC.Data;

namespace MyAuthMVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();// MVC
            //.AddNewtonsoftJson(options =>//ʹ��Microsoft.AspNetCore.Mvc.NewtonsoftJson
            //{
            //    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            //    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc; // ����ʱ��Ϊ UTC
            //}); 
            services.AddRazorPages();// RazorPage
            //services.AddControllers(); // WebAPI

            services.AddDbContext<ApplicationDbContext>(options => {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // ensure not change any return Claims from Authorization Server
            IdentityModelEventSource.ShowPII = true;//��ʾIdentityServer�������

            //services.AddAuthentication(options =>
            //{
            //    options.DefaultScheme = "Cookies";
            //    options.DefaultChallengeScheme = "oidc"; // oidc => open ID connect
            //})
            //.AddCookie("Cookies")
            //.AddOpenIdConnect("oidc", options =>
            //{
            //    options.SignInScheme = "Cookies";
            //    options.Authority = $"http://{Configuration["IdentitySrvAuth:IP"]}:{Configuration["IdentitySrvAuth:Port"]}";
            //    options.RequireHttpsMetadata = false; // please use https in production env
            //    options.ClientId = "GrantCode";
            //    options.ClientSecret = "CodeSecret";
            //    ////options.ResponseType = "id_token token"; // allow to return access token
            //    options.ResponseType = "code";
            //    options.SaveTokens = true;
            //    options.GetClaimsFromUserInfoEndpoint = true;
            //    options.Scope.Add("clientservice");
            //});

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            })
            .AddCookie("Cookies")
            .AddOpenIdConnect("oidc", options =>
            {
                options.Authority = $"https://{Configuration["IdentitySrvAuth:IP"]}:{Configuration["IdentitySrvAuth:Port_ssl"]}";
                options.RequireHttpsMetadata = true;

                options.ClientId = "GrantCode";
                options.ClientSecret = "CodeSecret";
                options.ResponseType = "code";

                //options.ClientId = "cas.mvc.client.implicit";
                ////options.ClientSecret = "";
                //options.ResponseType = "id_token token";

                options.SaveTokens = true;
                //���ô�UserInfoEndpoint��ȡclaims��Ϣ
                options.GetClaimsFromUserInfoEndpoint = true;

                ////��ȡ��ӳ�䵽claims
                //options.ClaimActions.MapJsonKey("sub", "sub");
                //options.ClaimActions.MapJsonKey("preferred_username", "preferred_username");
                //options.ClaimActions.MapJsonKey("avatar", "avatar");
                //options.ClaimActions.MapCustomJson("role", job => job["role"].ToString());

                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("productservice");
                /*ˢ��token����Ҫ����˿���allowoffline_access
                 * http://localhost:5000/.well-known/openid-configuration
                 * https://localhost:44365/connect/token
                 * grant_type:refresh_token
                 * client_Id:cas.mvc.client.implicit
                 * client_secret:clientsecret
                 * refresh_token:***
                 */
                options.Scope.Add("offline_access");

                //options.Events.OnTokenValidated =async tokenValidContext => {
                //    var OUser = tokenValidContext.HttpContext.User;
                //};
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            //ǿ��ʹ��Https
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
