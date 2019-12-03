using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MyAuthApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //string EnvName = "MichaelApi";
                    //webBuilder.UseEnvironment(EnvName);
                    //自定义配置文件
                    IConfigurationRoot configuration = new ConfigurationBuilder()
                      .SetBasePath(Directory.GetCurrentDirectory())
                      .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                      //.AddJsonFile($"appsettings.{EnvName}.json", optional: true)
                      .Build();

                    int Port_ssl = configuration.GetValue<int>("WebHost:Port_ssl");
                    int Port = configuration.GetValue<int>("WebHost:Port");

                    webBuilder.UseKestrel(opts=> {
                        opts.ListenAnyIP(Port_ssl, opts => {
                            opts.UseHttps();
                        });
                        opts.ListenAnyIP(Port);
                    }).UseStartup<Startup>().UseUrls($"https://localhost:{Port_ssl},http://localhost:{Port}");
                });
    }
}
