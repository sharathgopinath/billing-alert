using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using BillingMonitor.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Consumer.Tests.Integration.Infrastructure
{
    public class DynamoDb : IAsyncDisposable
    {
        private readonly AmazonDynamoDBClient _dynamoDbClient;

        public string BillingAlertStoreTableName { get; }
        public string ServiceUrl { get; }

        public DynamoDb()
        {
            ServiceUrl = TestConfiguration.Config.GetSection("Localstack:ServiceUrl").Value;
            BillingAlertStoreTableName = "billing-alert";
            _dynamoDbClient = new AmazonDynamoDBClient(new AmazonDynamoDBConfig
            {
                ServiceURL = ServiceUrl,
                Timeout = TimeSpan.FromSeconds(30)
            });
            Task.WaitAll(RefreshState());
        }

        public async Task RefreshState()
        {
            await _dynamoDbClient.DeleteTableAsync(BillingAlertStoreTableName);
            await CreateBillingAlertStoreTable();
        }

        private async Task CreateBillingAlertStoreTable()
        {
            var response = await _dynamoDbClient.CreateTableAsync(new CreateTableRequest
            {
                BillingMode = BillingMode.PAY_PER_REQUEST,
                TableName = BillingAlertStoreTableName,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition(nameof(BillingAlert.UserId).ToLower(), ScalarAttributeType.N)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement(nameof(BillingAlert.UserId).ToLower(), KeyType.HASH)
                }
            });
        }

        public async ValueTask DisposeAsync()
        {
            await _dynamoDbClient.DeleteTableAsync(BillingAlertStoreTableName);
            _dynamoDbClient?.Dispose();
        }
    }
}
