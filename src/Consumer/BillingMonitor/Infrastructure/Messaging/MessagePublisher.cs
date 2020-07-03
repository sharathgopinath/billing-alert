using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BillingMonitor.Infrastructure.Messaging
{
    public class MessagePublisher : IMessagePublisher
    {
        private readonly IAmazonSimpleNotificationService _client;
        private readonly ILogger _logger;
        private readonly SnsSettings _settings;
        public MessagePublisher(SnsSettings settings, ILogger logger)
        {
            _client = new AmazonSimpleNotificationServiceClient(new AmazonSimpleNotificationServiceConfig
            {
                ServiceURL = settings.ServiceUrl
            });
            _logger = logger;
            _settings = settings;
        }

        public async Task Publish<T>(IEnumerable<T> messages) where T : class
        {
            async Task Publish(T message)
            {
                var response = await _client.PublishAsync(new PublishRequest
                {
                    TopicArn = _settings.TopicArn,
                    Message = JsonConvert.SerializeObject(message, new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    }),
                    MessageAttributes = new Dictionary<string, MessageAttributeValue>
                    {
                        { "EventTime", new MessageAttributeValue{ DataType = "String", StringValue = DateTime.UtcNow.ToString() } }
                    }
                }, new CancellationTokenSource(10000).Token);
                _logger.Information($"Published {message} with HTTP status {response.HttpStatusCode}.");
            }

            var snsResponses = new List<Task>();
            foreach(var message in messages)
            {
                snsResponses.Add(Publish(message));
            }

            await Task.WhenAll(snsResponses);
        }
    }
}
