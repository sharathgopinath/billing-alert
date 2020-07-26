using Xunit;
using FluentAssertions;
using BillingAlert.Models;
using System.Collections.Generic;
using Consumer.Tests.Integration.Infrastructure;
using System.Threading.Tasks;
using System;
using System.Linq;
using BillingAlert.Infrastructure.Persistence.Models;
using Amazon.Lambda.KinesisEvents;
using static Amazon.Lambda.KinesisEvents.KinesisEvent;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Text;

namespace Consumer.Tests.Integration
{
    public class FunctionTest
    {
        private readonly TestContext _testContext;
        public FunctionTest()
        {
            _testContext = new TestContext();
            var billingAlerts = GetSeedData();
        }

        private IEnumerable<BillingAlertItem> GetSeedData()
        {
            return new List<BillingAlertItem>()
            {
                new BillingAlertItem{ CustomerId=1, AlertAmountThreshold=10.0m, BillAmountLastUpdated = DateTime.UtcNow, IsAlerted = false, TotalBillAmount = 0.0m }
                , new BillingAlertItem{ CustomerId=2, AlertAmountThreshold=20.0m, BillAmountLastUpdated = DateTime.UtcNow, IsAlerted = false, TotalBillAmount = 0.0m }
                , new BillingAlertItem{ CustomerId=3, AlertAmountThreshold=30.0m, BillAmountLastUpdated = DateTime.UtcNow, IsAlerted = false, TotalBillAmount = 0.0m }
                , new BillingAlertItem{ CustomerId=4, AlertAmountThreshold=40.0m, BillAmountLastUpdated = DateTime.UtcNow, IsAlerted = false, TotalBillAmount = 0.0m }
                , new BillingAlertItem{ CustomerId=5, AlertAmountThreshold=50.0m, BillAmountLastUpdated = DateTime.UtcNow, IsAlerted = false, TotalBillAmount = 0.0m }
                , new BillingAlertItem{ CustomerId=6, AlertAmountThreshold=10.0m, BillAmountLastUpdated = DateTime.UtcNow, IsAlerted = false, TotalBillAmount = 0.0m }
                , new BillingAlertItem{ CustomerId=7, AlertAmountThreshold=20.0m, BillAmountLastUpdated = DateTime.UtcNow, IsAlerted = false, TotalBillAmount = 0.0m }
                , new BillingAlertItem{ CustomerId=8, AlertAmountThreshold=30.0m, BillAmountLastUpdated = DateTime.UtcNow, IsAlerted = false, TotalBillAmount = 0.0m }
            };
        }

        [Fact]
        public async Task WhenTollAmountMessageExceedsThreshold_PublishMessage()
        {
            await _testContext.DynamoDb.RefreshState();
            await _testContext.DynamoDb.SeedData(GetSeedData());

            var messages = new List<TollAmountMessage>()
            {
                new TollAmountMessage
                {
                    CustomerId = 1,
                    TollAmount = 11.2m
                }
            };

            await _testContext.Sut.Execute(GetKinesisEvent(messages));

            var result = await _testContext.SnsSqs.DequeueMessageAsync<AlertMessage>();
            result.CustomerId.Should().Be(1);
        }

        [Fact]
        public async Task WhenAlerted_UpdateBillingAlert()
        {
            await _testContext.DynamoDb.RefreshState();
            await _testContext.DynamoDb.SeedData(GetSeedData());

            var messages = new List<TollAmountMessage>()
            {
                new TollAmountMessage
                {
                    CustomerId = 1,
                    TollAmount = 11.2m
                },
                new TollAmountMessage
                {
                    CustomerId = 2,
                    TollAmount = 20.0m
                },
                new TollAmountMessage
                {
                    CustomerId = 3,
                    TollAmount = 5.2m
                }
            };

            await _testContext.Sut.Execute(GetKinesisEvent(messages));

            var updatedBillingAlerts = await _testContext.DynamoDb.Get(messages.Select(m => m.CustomerId));
            updatedBillingAlerts.SingleOrDefault(u => u.CustomerId == messages[0].CustomerId).Should().NotBeNull();
            updatedBillingAlerts.SingleOrDefault(u => u.CustomerId == messages[1].CustomerId).Should().NotBeNull();
            updatedBillingAlerts.SingleOrDefault(u => u.CustomerId == messages[2].CustomerId).Should().NotBeNull();
        }

        private KinesisEvent GetKinesisEvent(List<TollAmountMessage> tollAmountMessages)
        {
            KinesisEvent kinesisEvent = null;
            foreach (var message in tollAmountMessages)
            {
                var jsonStringMessage = JsonSerializer.Serialize(message);
                kinesisEvent = new KinesisEvent
                {
                    Records = new List<KinesisEventRecord>
                    {
                        new KinesisEventRecord{Kinesis = new KinesisEvent.Record{
                            Data = new MemoryStream(Encoding.ASCII.GetBytes(jsonStringMessage))}
                        }
                    }
                };
            }

            return kinesisEvent;
        }
    }
}
