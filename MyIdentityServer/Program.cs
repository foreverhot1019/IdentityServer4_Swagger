using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MyIdentityServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var env = CreateHostBuilder(args);
            var host = env.Build();
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //string EnvName = "MichaelAuthServ";
                    //var webHostbuilder = webBuilder.UseEnvironment(EnvName);
                    var ConfigBuilder = new ConfigurationBuilder()
                      .SetBasePath(Directory.GetCurrentDirectory())
                      .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                      //.AddJsonFile($"appsettings.{EnvName}.json", optional: true)
                      .AddEnvironmentVariables();
                    /*
                     * 用户敏感数据设置（数据库连接/外部App的client_secrets）
                     * 项目->右击->管理用户机密
                     * https://docs.microsoft.com/zh-cn/aspnet/core/security/app-secrets?view=aspnetcore-3.0&tabs=windows
                     */
                    ConfigBuilder.AddUserSecrets<Startup>(); 
                    //自定义配置文件
                    var configuration = ConfigBuilder.Build();

                    int Port_ssl = configuration.GetValue<int>("WebHost:Port_ssl");
                    int Port = configuration.GetValue<int>("WebHost:Port");
                    //必须是https 不然IdentityServer4 会报User is not Authorized
                    webBuilder.UseKestrel(opts =>
                    {
                        var basePath = Directory.GetCurrentDirectory();//Path.GetDirectoryName(typeof(Program).Assembly.Location);//获取应用程序所在目录（绝对，不受工作目录影响，建议采用此方法获取路径）
                        //证书路径
                        var CerDirPath = Path.Combine(Directory.GetCurrentDirectory(), configuration["Certificates:CerPath"]);
                        var CerFilePath = Path.Combine(CerDirPath, configuration["Certificates:CerFileName"]);
                        //OpenSSL-Win64 生成证书
                        var X509Certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(CerFilePath, configuration["Certificates:Password"]);
                        opts.ListenAnyIP(Port_ssl, opts =>
                        {
                            opts.UseHttps(X509Certificate);
                        });
                        opts.ListenAnyIP(Port);
                    }).UseStartup<Startup>().UseUrls($"http://localhost:{Port},https://localhost:{Port_ssl}");

                    //webBuilder.UseContentRoot(Directory.GetCurrentDirectory());
                    //webBuilder.UseIISIntegration();
                    //webBuilder.UseStartup<Startup>().UseUrls($"http://localhost:{Port},https://localhost:{Port_ssl}");
                });
    }
}
