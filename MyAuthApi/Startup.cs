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

            //IdentityModelEventSource.ShowPII = true;//��ʾIdentityServer�������
            //��֤��ʽ����
            services.AddAuthentication(Configuration["IdentitySrvAuth:Scheme"])
            .AddIdentityServerAuthentication(opts =>//IdentityServer��֤��ʽ����
            {
                opts.ApiName = "MichaelApi";
                //opts.ApiName = Configuration["Service:Name"]; // match with configuration in IdentityServer
                opts.RequireHttpsMetadata = false;// for dev env
                opts.Authority = $"https://{Configuration["IdentitySrvAuth:IP"]}:{Configuration["IdentitySrvAuth:Port_ssl"]}";
                //��֤����ʱ����֤���ʱ��
                opts.JwtValidationClockSkew = TimeSpan.FromSeconds(10);

                opts.JwtBearerEvents = new JwtBearerEvents
                {
                    //��֤token�Ƿ�����Զ���淶
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
                            tkvldContext.Fail("��֤ʧ��");
                    }
                };
            })
            .AddJwtBearer("MyJwtBearer", opts =>
            {
                //��֤����ʱ����
                opts.TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(10);
                opts.Events = new JwtBearerEvents
                {
                    //��֤token�Ƿ�����Զ���淶
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
                            tkvldContext.Fail("��֤ʧ��");
                    }
                };
            });
            //��Ȩ����
            services.AddAuthorization(opts =>
            {
                var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                    JwtBearerDefaults.AuthenticationScheme,
                    "MyJwtBearer");
                defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
                opts.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
            });

            //swagger����
            services.AddSwaggerGen(SwaggerGenOpts =>
            {
                // resolve the IApiVersionDescriptionProvider service
                // note: that we have to build a temporary service provider here because one has not been created yet
                //var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
                SwaggerGenOpts.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "V1.0.0",
                    Title = "My Swagger Demo",
                    Description = "webapi����swagger",
                    //TermsOfService = "None",
                    Contact = new OpenApiContact
                    {
                        Name = "Michael-Swagger",
                        Email = "Michael_Wang1019@feiliks.com",
                        Url = new Uri("https://blog.csdn.net/foreverhot1019")
                    }
                });

                // Ϊ Swagger JSON and UI����xml�ĵ�ע��·��
                var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);//��ȡӦ�ó�������Ŀ¼�����ԣ����ܹ���Ŀ¼Ӱ�죬������ô˷�����ȡ·����
                //��ӽӿ�XML��·��
                var xmlPath = Path.Combine(basePath, "MyAuthApiSwagger.xml");
                //�����Ҫ��ʾ������ע��ֻ�轫�ڶ�����������Ϊtrue
                SwaggerGenOpts.IncludeXmlComments(xmlPath, true);

                #region ȫ��Bearer Token��֤ ����

                //// Add security definitions-Swagger�������ƻ�ȡ
                //SwaggerGenOpts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                //{
                //    Description = "Please enter into field the word 'Bearer' followed by a space and the JWT value",
                //    Name = "Authorization",
                //    In = ParameterLocation.Header,
                //    Type = SecuritySchemeType.ApiKey,
                //});
                //// ����һ��ȫ�ֵİ�ȫ����
                //SwaggerGenOpts.AddSecurityRequirement(new OpenApiSecurityRequirement
                //{
                //    { new OpenApiSecurityScheme
                //    {
                //        Reference = new OpenApiReference()
                //        {
                //            Id = "Bearer",
                //            Type = ReferenceType.SecurityScheme
                //        }
                //    }, Array.Empty<string>() }
                //});

                // Add security definitions-Swagger�������ƻ�ȡ
                SwaggerGenOpts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "Please enter into field the word 'Bearer' followed by a space and the JWT value",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                });
                // ����һ��ȫ�ֵİ�ȫ����
                SwaggerGenOpts.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference()
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme
                        }
                    }, Array.Empty<string>() }
                });

                //Swagger�������ƻ�ȡ
                //SwaggerGenOpts.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                //{
                //    Flow = "implicit", // ֻ��ͨ���������ȡ���ƣ�������swagger��
                //    AuthorizationUrl = "http://localhost:5000/connect/authorize",//��ȡ��¼��Ȩ�ӿ�
                //    Scopes = new Dictionary<string, string> {
                //        { "demo_api", "Demo API - full access" }//ָ���ͻ��������api������ ���Ϊ�գ���ͻ����޷�����
                //    }
                //});                

                //Swagger�������ƻ�ȡ
                SwaggerGenOpts.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Flows = new OpenApiOAuthFlows
                    {
                        ClientCredentials = new OpenApiOAuthFlow
                        {
                            //AuthorizationUrl code��implicit ģʽ
                            AuthorizationUrl = new Uri("https://localhost:44365/connect/token"),//��ȡ��¼��Ȩ�ӿ�
                            Scopes = new Dictionary<string, string> { { "clientservice", "client_service" } },//ָ���ͻ��������api������ ���Ϊ�գ���ͻ����޷�����
                        }
                    },
                    In = ParameterLocation.Query
                });

                //���httpHeader����
                SwaggerGenOpts.OperationFilter<HttpHeaderOperation>();
                SwaggerGenOpts.OperationFilter<HttpHeaderOperation>();

                #endregion

                #region ���Swagger https://www.cnblogs.com/weihanli/p/ues-swagger-in-aspnetcore3_0.html

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
            //ǿ��ʹ��Https
            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            #region Swagger����

            app.UseSwagger(opts =>
            {
                //opts.RouteTemplate = "swagger/{documentName}/swagger.json";
            });
            app.UseSwaggerUI(opts =>
            {
                opts.SwaggerEndpoint("/Swagger/v1/swagger.json", "ApiHelper V1");
            });

            #endregion
        }
    }
}
