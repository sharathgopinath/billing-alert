using System;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Serilog;
using System.Threading;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Polly;
using Amazon.SQS.Model;
using Amazon.SimpleNotificationService.Model;

namespace Consumer.Tests.Integration.Infrastructure
{
    public class SnsSqs : IAsyncDisposable
    {
        public string ServiceUrl { get; private set; }
        public string TopicArn { get; private set; }
        private string QueueUrl;

        private readonly AmazonSimpleNotificationServiceClient _snsClient;
        private readonly AmazonSQSClient _sqsClient;

        public SnsSqs()
        {
            ServiceUrl = TestConfiguration.Config.GetSection("Localstack:ServiceUrl").Value;

            _snsClient = new AmazonSimpleNotificationServiceClient(new AmazonSimpleNotificationServiceConfig { ServiceURL = ServiceUrl });
            _sqsClient = new AmazonSQSClient(new AmazonSQSConfig { ServiceURL = ServiceUrl });

            WaitTillReady();
            CreateTopicsAndQueues();
        }

        public async ValueTask DisposeAsync()
        {
            await _snsClient.DeleteTopicAsync(TopicArn);
            await _sqsClient.DeleteQueueAsync(QueueUrl);

            _snsClient.Dispose();
            _sqsClient.Dispose();
        }

        public async Task<T> DequeueMessageAsync<T>()
        {
            var sub = await _snsClient.ListSubscriptionsAsync();
            var message = await Policy
                .HandleResult<ReceiveMessageResponse>(r => r.Messages.Count == 0)
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromMilliseconds(100)
                    , TimeSpan.FromMilliseconds(200)
                    , TimeSpan.FromMilliseconds(500)
                    , TimeSpan.FromMilliseconds(1000)
                })
                .ExecuteAsync(async () => await _sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest { QueueUrl = QueueUrl }));
            
            var messageBody = message?.Messages?.FirstOrDefault()?.Body;

            return JsonConvert.DeserializeObject<T>(messageBody ?? "{}");
        }

        private void CreateTopicsAndQueues()
        {
            var topicAndQueueName = $"toll-amount-threshold-breached-{Guid.NewGuid().ToString().Substring(0,8)}";
            
            var createTopicResponse = _snsClient.CreateTopicAsync(new CreateTopicRequest { Name = topicAndQueueName }).Result;
            var createQueueResponse = _sqsClient.CreateQueueAsync(new CreateQueueRequest { QueueName = topicAndQueueName }).Result;

            TopicArn = createTopicResponse.TopicArn;
            QueueUrl = createQueueResponse.QueueUrl;

            var subscriptionArn = _snsClient.SubscribeAsync(new SubscribeRequest { TopicArn = createTopicResponse.TopicArn, Endpoint = createQueueResponse.QueueUrl, Protocol = "sqs" }).Result;
            var r = _snsClient.SetSubscriptionAttributesAsync(subscriptionArn.SubscriptionArn, "RawMessageDelivery", "true").Result;
        }

        private void WaitTillReady()
        {
            for(var i=0; i<20; i++)
            {
                try
                {
                    Log.Logger.Information("Waiting for SNS SQS...");
                    var cancellationToken = new CancellationTokenSource(2000).Token;
                    var snsResponse = _snsClient.ListTopicsAsync(cancellationToken).Result;
                    var sqsResponse = _sqsClient.ListQueuesAsync("", cancellationToken).Result;

                    if (snsResponse.HttpStatusCode == HttpStatusCode.OK && sqsResponse.HttpStatusCode == HttpStatusCode.OK)
                        return;
                }
                catch (Exception ex)
                {
                    Log.Logger.Information($"Failed to connect, attempt - {i} - {ex.Message}");
                }
            }

            throw new Exception("SNS/SQS initialization failed");
        }
    }
}
