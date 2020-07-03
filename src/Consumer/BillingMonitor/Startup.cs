using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using Serilog.Formatting.Json;
using BillingMonitor.Infrastructure.Messaging;

namespace BillingMonitor
{
    public class Startup
    {
        public static bool IsInitialized { get; private set; }
        private static ServiceProvider _serviceProvider;

        public static ServiceProvider Initialize(IConfiguration configuration=null)
        {
            if (IsInitialized)
                return _serviceProvider;

            var logger = CreateLogger();
            configuration = configuration ?? CreateConfiguration();
            _serviceProvider = ConfigureDependencies(logger, configuration);

            IsInitialized = true;
            return _serviceProvider;
        }

        private static IConfiguration CreateConfiguration()
        {
            var environment = Environment.GetEnvironmentVariable("Environment");
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json")
                .AddJsonFile($"appSettings.{environment}.json", true);

            return configuration.Build();
        }

        private static ILogger CreateLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "BillingMonitor")
                .WriteTo.Console(new JsonFormatter())
                .CreateLogger();

            return Log.Logger;
        }

        private static ServiceProvider ConfigureDependencies(ILogger logger, IConfiguration configuration)
        {
            var services = new ServiceCollection();
            services.AddSingleton(logger);
            services.AddSingleton(s =>
            {
                return new SnsSettings
                {
                    TopicArn = configuration.GetSection("SnsSettings:TopicArn").Value,
                    ServiceUrl = configuration.GetSection("SnsSettings:ServiceUrl").Value
                };
            });
            services.AddSingleton<IMessagePublisher, MessagePublisher>();

            return services.BuildServiceProvider();
        }
    }
}
