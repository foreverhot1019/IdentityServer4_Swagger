using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
            services.AddControllers();

            //IdentityModelEventSource.ShowPII = true;//显示IdentityServer具体错误
            services.AddAuthentication(Configuration["IdentitySrvAuth:Scheme"])
            .AddIdentityServerAuthentication(opts =>
            {
                opts.ApiName = "MichaelApi";
                //opts.ApiName = Configuration["Service:Name"]; // match with configuration in IdentityServer
                opts.RequireHttpsMetadata = false;// for dev env
                opts.Authority = $"http://{Configuration["IdentitySrvAuth:IP"]}:{Configuration["IdentitySrvAuth:Port"]}";

                opts.JwtValidationClockSkew = TimeSpan.FromSeconds(10);//认证过期时间间隔
                
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
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
