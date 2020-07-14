using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using Serilog.Formatting.Json;
using BillingAlert.Infrastructure.Messaging;
using BillingAlert.Infrastructure.Persistence;
using Amazon.DynamoDBv2;

namespace BillingAlert
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
                .Enrich.WithProperty("Application", "BillingAlert")
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
            services.AddSingleton(s =>
            {
                return new BillingAlertStoreSettings
                {
                    TableName = configuration.GetSection("BillingAlertStore:TableName").Value,
                    ServiceUrl = configuration.GetSection("BillingAlertStore:ServiceUrl").Value
                };
            });

            services.AddSingleton<IMessagePublisher, MessagePublisher>();

            services.AddSingleton<IAmazonDynamoDB>(s =>
            {
                var settings = s.GetService<BillingAlertStoreSettings>();
                return new AmazonDynamoDBClient(new AmazonDynamoDBConfig
                {
                    ServiceURL = settings.ServiceUrl
                });
            });
            services.AddTransient<IBillingAlertStore, BillingAlertStore>();

            return services.BuildServiceProvider();
        }
    }
}
