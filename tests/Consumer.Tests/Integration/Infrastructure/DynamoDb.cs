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
        private static readonly string IdAttribute = nameof(BillingAlert.CustomerId).ToLower();

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
        }

        public async Task SeedData(IEnumerable<BillingAlert> billingAlerts)
        {
            async Task PutItem(BillingAlert billingAlert)
            {
                var item = Map(billingAlert);
                var response = await _dynamoDbClient.PutItemAsync(new PutItemRequest
                {
                    Item = item,
                    TableName = BillingAlertStoreTableName
                });
            }

            var putItemResponses = new List<Task>();
            foreach (var billingAlert in billingAlerts)
            {
                putItemResponses.Add(PutItem(billingAlert));
            }

            await Task.WhenAll(putItemResponses);
        }

        public async Task RefreshState()
        {
            try
            {
                var table = await _dynamoDbClient.DescribeTableAsync(BillingAlertStoreTableName);
                await _dynamoDbClient.DeleteTableAsync(BillingAlertStoreTableName);
                await CreateBillingAlertStoreTable();
            }
            catch (ResourceNotFoundException)
            {
                await CreateBillingAlertStoreTable();
            }
        }

        private async Task CreateBillingAlertStoreTable()
        {
            var response = await _dynamoDbClient.CreateTableAsync(new CreateTableRequest
            {
                BillingMode = BillingMode.PAY_PER_REQUEST,
                TableName = BillingAlertStoreTableName,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition(nameof(BillingAlert.CustomerId).ToLower(), ScalarAttributeType.N)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement(nameof(BillingAlert.CustomerId).ToLower(), KeyType.HASH)
                }
            });
        }

        private Dictionary<string, AttributeValue> Map(BillingAlert billingAlert)
        {
            return new Dictionary<string, AttributeValue>
                {
                    { IdAttribute, new AttributeValue{N = billingAlert.CustomerId.ToString()} },
                    { nameof(BillingAlert.AlertAmountThreshold).ToLower(), new AttributeValue{N = billingAlert.AlertAmountThreshold.ToString()} },
                    { nameof(BillingAlert.TotalBillAmount).ToLower(), new AttributeValue{N = billingAlert.TotalBillAmount.ToString()} },
                    { nameof(BillingAlert.BillAmountLastUpdated).ToLower(), new AttributeValue{S = billingAlert.BillAmountLastUpdated.ToString()} },
                    { nameof(BillingAlert.IsAlerted).ToLower(), new AttributeValue{BOOL = billingAlert.IsAlerted} }
                };
        }

        public async ValueTask DisposeAsync()
        {
            await _dynamoDbClient.DeleteTableAsync(BillingAlertStoreTableName);
            _dynamoDbClient?.Dispose();
        }
    }
}
