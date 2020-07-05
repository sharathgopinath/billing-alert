using BillingMonitor;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Consumer.Tests.Integration.Infrastructure
{
    public class TestContext
    {
        public Lambda Sut { get; }
        public SnsSqs SnsSqs { get; }
        public DynamoDb DynamoDb { get; }

        public TestContext()
        {
            SnsSqs = new SnsSqs();
            DynamoDb = new DynamoDb();
            
            var config = InitializeAppConfiguration();
            Startup.Initialize(config);

            Sut = new Lambda();
        }

        private IConfiguration InitializeAppConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                { "SnsSettings:ServiceUrl", SnsSqs.ServiceUrl }
                , { "SnsSettings:TopicArn", SnsSqs.TopicArn }
                , { "BillingAlertStore:TableName", DynamoDb.BillingAlertStoreTableName }
                , { "BillingAlertStore:ServiceUrl", DynamoDb.ServiceUrl }
            });

            return configurationBuilder.Build();
        }
    }
}
