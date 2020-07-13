using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Serilog;

namespace BillingMonitor.Infrastructure.Persistence
{
    public class BillingAlertStore : IBillingAlertStore
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly ILogger _logger;
        private readonly BillingAlertStoreSettings _settings;

        private static readonly string IdAttribute = nameof(BillingAlert.CustomerId).ToLower();

        public BillingAlertStore(IAmazonDynamoDB dynamoDbClient
            , BillingAlertStoreSettings settings
            , ILogger logger)
        {
            _dynamoDbClient = dynamoDbClient;
            _settings = settings;
            _logger = logger;
        }

        public async Task<IList<BillingAlert>> Get(IEnumerable<int> userIds)
        {

            var response = await _dynamoDbClient.BatchGetItemAsync(new BatchGetItemRequest(new Dictionary<string, KeysAndAttributes>
            {
                {_settings.TableName, new KeysAndAttributes{
                    Keys = userIds.Select(u => new Dictionary<string, AttributeValue>{ {IdAttribute, new AttributeValue { N = u.ToString() } } }).ToList(),
                    AttributesToGet = new List<string>
                    {
                        nameof(BillingAlert.CustomerId).ToLower(),
                        nameof(BillingAlert.AlertAmountThreshold).ToLower(),
                        nameof(BillingAlert.TotalBillAmount).ToLower(),
                        nameof(BillingAlert.BillAmountLastUpdated).ToLower(),
                        nameof(BillingAlert.IsAlerted).ToLower(),
                    }
                } }
            }));

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception($"Failed to get items from {nameof(BillingAlertStore)}");

            var items = response.Responses.FirstOrDefault(r => r.Key == _settings.TableName).Value;
            return items.Select(i => Map(i)).ToList();
        }

        public async Task Put(IEnumerable<BillingAlert> billingAlerts)
        {
            async Task PutItem(BillingAlert billingAlert)
            {
                try
                {
                    var item = Map(billingAlert);
                    var response = await _dynamoDbClient.PutItemAsync(new PutItemRequest
                    {
                        Item = item,
                        TableName = _settings.TableName
                    });
                    _logger.Information($"Put item for {nameof(BillingAlert)} with HttpStatus: {response.HttpStatusCode}.");
                }
                catch(Exception ex)
                {
                    _logger.Error(ex, $"Failed to update {nameof(BillingAlert)}.");
                }
            }

            var putItemResponses = new List<Task>();
            foreach(var billingAlert in billingAlerts)
            {
                putItemResponses.Add(PutItem(billingAlert));
            }

            await Task.WhenAll(putItemResponses);
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

        private BillingAlert Map(Dictionary<string, AttributeValue> item)
        {
            return new BillingAlert
            {
                CustomerId = int.Parse(item[IdAttribute].N),
                AlertAmountThreshold = decimal.Parse(item[nameof(BillingAlert.AlertAmountThreshold).ToLower()].N),
                TotalBillAmount = decimal.Parse(item[nameof(BillingAlert.TotalBillAmount).ToLower()].N),
                BillAmountLastUpdated = DateTime.Parse(item[nameof(BillingAlert.BillAmountLastUpdated).ToLower()].S),
                IsAlerted = item[nameof(BillingAlert.IsAlerted).ToLower()].BOOL
            };
        }
    }
}
