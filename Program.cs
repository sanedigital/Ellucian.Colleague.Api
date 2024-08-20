// Copyright 2023-2024 Ellucian Company L.P. and its affiliates.
// This application entry point is based on ASP.NET Core new project templates and is included
// as a starting point for app host configuration.
// This file may need updated according to the specific scenario of the application being upgraded.
// For more information on ASP.NET Core hosting, see https://docs.microsoft.com/aspnet/core/fundamentals/host/web-host
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Ellucian.Colleague.Api
{
    /// <summary>
    /// The main entry point for Ellucian Colleague API
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            //Log.Logger = new LoggerConfiguration()
            //.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            //.WriteTo.Console()
            //.CreateBootstrapLogger();

            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates the host 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((hostContext, loggerConfig) =>
                {
                    loggerConfig.ReadFrom.Configuration(hostContext.Configuration);
                    loggerConfig.MinimumLevel.ControlledBy(Bootstrapper.LoggingLevelSwitch);
                    // Uncomment the lines below to add correlation id and/or client ip to logs
                    //loggerConfig.Enrich.WithClientIp("X-Forward-Id");
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
