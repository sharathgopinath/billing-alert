using Amazon.Lambda.Core;
using BillingMonitor.Infrastructure.Messaging;
using BillingMonitor.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
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
        
        public Function()
        {
            _serviceProvider = Startup.Initialize();
            _logger = _serviceProvider.GetRequiredService<ILogger>();
            _messagePublisher = _serviceProvider.GetRequiredService<IMessagePublisher>();
        }

        public async Task Execute(IEnumerable<TollAmountMessage> tollAmountMessages)
        {
            _logger.Information($"Processing {tollAmountMessages.Count()} records.");
            await _messagePublisher.Publish(tollAmountMessages);
        }
    }
}
