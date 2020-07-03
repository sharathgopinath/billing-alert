using Xunit;
using FluentAssertions;
using BillingMonitor.Models;
using System.Collections.Generic;
using Consumer.Tests.Integration.Infrastructure;
using System.Threading.Tasks;

namespace Consumer.Tests.Integration
{
    public class FunctionTest
    {
        private readonly TestContext _testContext;
        public FunctionTest()
        {
            _testContext = new TestContext();
        }

        [Fact]
        public async Task WhenTollAmountMessageArrives_PublishMessage()
        {
            var messages = new List<TollAmountMessage>()
            {
                new TollAmountMessage
                {
                    CustomerId = 123,
                    TollAmount = 11.2m
                }
            };

            await _testContext.Sut.Execute(messages);

            var result = await _testContext.SnsSqs.DequeueMessageAsync<TollAmountMessage>();
            result.CustomerId.Should().Be(123);
            result.TollAmount.Should().Be(11.2m);
        }
    }
}
