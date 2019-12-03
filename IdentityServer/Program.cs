// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace IdentityServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "IdentityServer4";

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var webBuilder = WebHost.CreateDefaultBuilder(args);
            string EnvName = "Development";
            webBuilder.UseEnvironment(EnvName);
            //自定义配置文件
            IConfigurationRoot configuration = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddJsonFile($"appsettings.{EnvName}.json", optional: true).Build();

            int Port_ssl = configuration.GetValue<int>("WebHost:Port_ssl");
            int Port = configuration.GetValue<int>("WebHost:Port");

            //webBuilder.UseKestrel(opts => {
            //    opts.ListenAnyIP(Port_ssl, opts => {
            //        opts.UseHttps();
            //    });
            //    opts.ListenAnyIP(Port);
            //}).UseStartup<Startup>().UseUrls("http:localhost:{Port}");

            webBuilder.UseContentRoot(Directory.GetCurrentDirectory());
            webBuilder.UseIISIntegration();
            webBuilder.UseStartup<Startup>().UseUrls($"http://localhost:{Port}");

            return webBuilder;
        }


        //public static IHostBuilder CreateHostBuilder(string[] args) =>
        //    Host.CreateDefaultBuilder(args)
        //        .ConfigureWebHostDefaults(webBuilder =>
        //        {
        //            string EnvName = "MichaelAuthServ";
        //            webBuilder.UseEnvironment(EnvName);
        //            //自定义配置文件
        //            IConfigurationRoot configuration = new ConfigurationBuilder()
        //              .SetBasePath(Directory.GetCurrentDirectory())
        //              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        //              .AddJsonFile($"appsettings.{EnvName}.json", optional: true).Build();

        //            int Port_ssl = configuration.GetValue<int>("WebHost:Port_ssl");
        //            int Port = configuration.GetValue<int>("WebHost:Port");

        //            //webBuilder.UseKestrel(opts => {
        //            //    opts.ListenAnyIP(Port_ssl, opts => {
        //            //        opts.UseHttps();
        //            //    });
        //            //    opts.ListenAnyIP(Port);
        //            //}).UseStartup<Startup>().UseUrls("http:localhost:{Port}");

        //            webBuilder.UseContentRoot(Directory.GetCurrentDirectory());
        //            webBuilder.UseIISIntegration();
        //            webBuilder.UseStartup<Startup>().UseUrls($"http://localhost:{Port}");
        //        });
    }
}