using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Consumer.Tests.Integration.Infrastructure
{
    public static class TestConfiguration
    {
        public static IConfiguration Config { get; }
        static TestConfiguration()
        {
            var environment = Environment.GetEnvironmentVariable("TestEnvironment");
            Config = new ConfigurationBuilder()
                .SetBasePath($"{Directory.GetCurrentDirectory()}/Integration")
                .AddJsonFile("appSettings.json")
                .AddJsonFile($"appSettings.{environment}.json", true).Build();
        }        
    }
}
