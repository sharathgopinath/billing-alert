using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using BillingAlert.Infrastructure.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Consumer.Tests.Integration.Infrastructure
{
    public class DynamoDb : IAsyncDisposable
    {
        private readonly AmazonDynamoDBClient _dynamoDbClient;
        private static readonly string IdAttribute = nameof(BillingAlertItem.CustomerId).ToLower();

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

        public async Task SeedData(IEnumerable<BillingAlertItem> billingAlerts)
        {
            async Task PutItem(BillingAlertItem billingAlert)
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

        public async Task<IList<BillingAlertItem>> Get(IEnumerable<int> userIds)
        {
            var response = await _dynamoDbClient.BatchGetItemAsync(new BatchGetItemRequest(new Dictionary<string, KeysAndAttributes>
            {
                {BillingAlertStoreTableName, new KeysAndAttributes{
                    Keys = userIds.Select(u => new Dictionary<string, AttributeValue>{ {IdAttribute, new AttributeValue { N = u.ToString() } } }).ToList(),
                    AttributesToGet = new List<string>
                    {
                        nameof(BillingAlertItem.CustomerId).ToLower(),
                        nameof(BillingAlertItem.AlertAmountThreshold).ToLower(),
                        nameof(BillingAlertItem.TotalBillAmount).ToLower(),
                        nameof(BillingAlertItem.BillAmountLastUpdated).ToLower(),
                        nameof(BillingAlertItem.IsAlerted).ToLower(),
                    }
                } }
            }));

            var items = response.Responses.FirstOrDefault(r => r.Key == BillingAlertStoreTableName).Value;
            return items.Select(i => Map(i)).ToList();
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
                    new AttributeDefinition(nameof(BillingAlertItem.CustomerId).ToLower(), ScalarAttributeType.N)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement(nameof(BillingAlertItem.CustomerId).ToLower(), KeyType.HASH)
                }
            });
        }

        private Dictionary<string, AttributeValue> Map(BillingAlertItem billingAlert)
        {
            return new Dictionary<string, AttributeValue>
                {
                    { IdAttribute, new AttributeValue{N = billingAlert.CustomerId.ToString()} },
                    { nameof(BillingAlertItem.AlertAmountThreshold).ToLower(), new AttributeValue{N = billingAlert.AlertAmountThreshold.ToString()} },
                    { nameof(BillingAlertItem.TotalBillAmount).ToLower(), new AttributeValue{N = billingAlert.TotalBillAmount.ToString()} },
                    { nameof(BillingAlertItem.BillAmountLastUpdated).ToLower(), new AttributeValue{S = billingAlert.BillAmountLastUpdated.ToString()} },
                    { nameof(BillingAlertItem.IsAlerted).ToLower(), new AttributeValue{BOOL = billingAlert.IsAlerted} }
                };
        }

        private BillingAlertItem Map(Dictionary<string, AttributeValue> item)
        {
            return new BillingAlertItem
            {
                CustomerId = int.Parse(item[IdAttribute].N),
                AlertAmountThreshold = decimal.Parse(item[nameof(BillingAlertItem.AlertAmountThreshold).ToLower()].N),
                TotalBillAmount = decimal.Parse(item[nameof(BillingAlertItem.TotalBillAmount).ToLower()].N),
                BillAmountLastUpdated = DateTime.Parse(item[nameof(BillingAlertItem.BillAmountLastUpdated).ToLower()].S),
                IsAlerted = item[nameof(BillingAlertItem.IsAlerted).ToLower()].BOOL
            };
        }

        public async ValueTask DisposeAsync()
        {
            await _dynamoDbClient.DeleteTableAsync(BillingAlertStoreTableName);
            _dynamoDbClient?.Dispose();
        }
    }
}
