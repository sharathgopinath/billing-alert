using Amazon.Lambda.Core;
using BillingMonitor.Infrastructure.Messaging;
using BillingMonitor.Infrastructure.Persistence;
using BillingMonitor.Models;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace BillingMonitor
{
    public class Function
    {
        private readonly ILogger _logger;
        private readonly ServiceProvider _serviceProvider;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IBillingAlertStore _billingAlertStore;
        
        public Function()
        {
            _serviceProvider = Startup.Initialize();
            _logger = _serviceProvider.GetRequiredService<ILogger>();
            _messagePublisher = _serviceProvider.GetRequiredService<IMessagePublisher>();
            _billingAlertStore = _serviceProvider.GetRequiredService<IBillingAlertStore>();
        }

        public async Task Execute(IEnumerable<TollAmountMessage> tollAmountMessages)
        {
            _logger.Information($"Processing {tollAmountMessages.Count()} records.");

            await _billingAlertStore.Put(new List<BillingAlert>
            {
                new BillingAlert{UserId=1, AlertAmountThreshold=10, BillAmountLastUpdated = DateTime.UtcNow},
                new BillingAlert{UserId=2, AlertAmountThreshold=10, BillAmountLastUpdated = DateTime.UtcNow},
                new BillingAlert{UserId=3, AlertAmountThreshold=10, BillAmountLastUpdated = DateTime.UtcNow},
            });

            var items = await _billingAlertStore.Get(new List<int> { 1, 2, 3 });

            await _messagePublisher.Publish(tollAmountMessages);
        }
    }
}
