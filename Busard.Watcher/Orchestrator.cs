using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Linq;
using Busard.Core;
using Serilog.Events;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Busard.Watcher
{
    internal static class Orchestrator
    {
        #region Members and Constructor

        private const string SqlServer = "SqlServer";

        /// <summary>
        /// The global configuration object stored in 'config.global.yaml'
        /// </summary>
        private static Core.GlobalConfiguration _globalConfig = new GlobalConfiguration();

        /// <summary>
        /// Composes the services using the yaml configuration file. Sets watchers and watchers modules accordingly.
        /// </summary>
        private static void ComposeServices(HostBuilderContext context, IServiceCollection services)
        {
            //services.TryAddSingleton<NotifierService>();
            services.AddHostedService<NotifierService>();

            // ----------------- Watchers ----------------
            foreach (var module in _globalConfig.Watchers.Modules)
            {
                switch (module)
                {
                    case SqlServer:
                        foreach (var watchers in _globalConfig.Watchers.SqlServer.Watchers)
                        {
                            switch (watchers)
                            {
                                case "Issues":
                                    services.AddHostedService<Busard.SqlServer.Monitoring.IssuesWatcher>();
                                    //services.AddSingleton<IHostedService, SqlServer.Monitoring.IssuesWatcher>();
                                    break;
                                case "AlwaysOn":
                                    services.AddHostedService<Busard.SqlServer.Monitoring.AlwaysOnWatcher>();
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private static void ComposeTimedServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddTransient<Core.Monitoring.TimedWatchersConcurrentPriorityQueue>();
            services.AddHostedService<Core.Monitoring.TimedQueueService>();

            foreach (var module in _globalConfig.Watchers.Modules)
            {
                switch (module)
                {
                    case SqlServer:
                        foreach (var watchers in _globalConfig.Watchers.SqlServer.Watchers)
                        {
                            switch (watchers)
                            {
                                case "ErrorLog":
                                    services.AddHostedService<SqlServer.Monitoring.ErrorLogWatcher>();
                                    break;
                                case "PerformanceCounters":
                                    services.AddHostedService<SqlServer.Monitoring.PerformanceCountersWatcher>();
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }


        /// <summary>
        /// Adds modules specific yaml configuration files to the DI container
        /// </summary>
        //private static void _configureServices(IConfigurationBuilder configuration)
        //{
        //    if (_globalConfig.Watchers.Modules.Contains(SqlServer))
        //    {
        //        configuration.AddYamlFile("config.sqlserver.yaml", optional: false);
        //    }
        //}

        /// <summary>
        /// Initializes the <see cref="Orchestrator"/> class.
        /// </summary>
        static Orchestrator()
        {
            // using the Serilog extension for the logging hosting
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console() // TODO : get config
                .CreateLogger();

            Log.Information("Orchestrator created at: {time}", DateTimeOffset.Now);
        }

        /// <summary>
        /// It's the underlying DI container used in ASP.NET. 
        /// "The following code creates a Generic Host using non-HTTP workload. The IHostedService implementation is added to the DI container"
        /// In an HTTP workload, CreateDefaultBuilder calls ConfigureWebHostDefaults instead of CreateDefaultBuilder
        /// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                //.AddControllersAsServices()
                .ConfigureHostConfiguration((configure) =>
                {
                    configure.AddYamlFile("/conf/config.global.yaml", optional: false);
                    configure.AddYamlFile("/secrets/config.secret.yaml", optional: false);
                    configure.Build().Bind(_globalConfig);
                }
                )
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions();
                    services.Configure<Core.GlobalConfiguration>(hostContext.Configuration);
                })
                .ConfigureServices(ComposeServices)
                .ConfigureServices(ComposeTimedServices)
                //.ConfigureAppConfiguration(_configureServices)
                .UseSerilog();

        #endregion

        public static void Dispose()
        {
            //TODO - how to make sure all notifications were sent ?

            //base.Dispose();
            Log.CloseAndFlush();
        }

    }
}
