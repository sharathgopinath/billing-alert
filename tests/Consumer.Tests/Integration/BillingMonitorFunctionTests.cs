using Xunit;
using FluentAssertions;
using BillingMonitor.Models;
using System.Collections.Generic;
using Consumer.Tests.Integration.Infrastructure;
using System.Threading.Tasks;
using System;
using BillingMonitor.Infrastructure.Persistence;

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

        private IEnumerable<BillingAlert> GetSeedData()
        {
            return new List<BillingAlert>()
            {
                new BillingAlert{ CustomerId=1, AlertAmountThreshold=10.0m, BillAmountLastUpdated = DateTime.UtcNow, IsAlerted = false, TotalBillAmount = 0.0m }
                , new BillingAlert{ CustomerId=2, AlertAmountThreshold=20.0m, BillAmountLastUpdated = DateTime.UtcNow, IsAlerted = false, TotalBillAmount = 0.0m }
                , new BillingAlert{ CustomerId=3, AlertAmountThreshold=30.0m, BillAmountLastUpdated = DateTime.UtcNow, IsAlerted = false, TotalBillAmount = 0.0m }
                , new BillingAlert{ CustomerId=4, AlertAmountThreshold=40.0m, BillAmountLastUpdated = DateTime.UtcNow, IsAlerted = false, TotalBillAmount = 0.0m }
                , new BillingAlert{ CustomerId=5, AlertAmountThreshold=50.0m, BillAmountLastUpdated = DateTime.UtcNow, IsAlerted = false, TotalBillAmount = 0.0m }
                , new BillingAlert{ CustomerId=6, AlertAmountThreshold=10.0m, BillAmountLastUpdated = DateTime.UtcNow, IsAlerted = false, TotalBillAmount = 0.0m }
                , new BillingAlert{ CustomerId=7, AlertAmountThreshold=20.0m, BillAmountLastUpdated = DateTime.UtcNow, IsAlerted = false, TotalBillAmount = 0.0m }
                , new BillingAlert{ CustomerId=8, AlertAmountThreshold=30.0m, BillAmountLastUpdated = DateTime.UtcNow, IsAlerted = false, TotalBillAmount = 0.0m }
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

            await _testContext.Sut.Execute(messages);

            var result = await _testContext.SnsSqs.DequeueMessageAsync<AlertMessage>();
            result.CustomerId.Should().Be(1);
        }
    }
}
