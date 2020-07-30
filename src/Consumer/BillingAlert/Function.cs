using Amazon.Lambda.Core;
using Amazon.Lambda.KinesisEvents;
using BillingAlert.Infrastructure.Messaging;
using BillingAlert.Infrastructure.Persistence;
using BillingAlert.Infrastructure.Persistence.Models;
using BillingAlert.Models;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace BillingAlert
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

            _logger.Information("Initializing Lambda function...");
        }

        public async Task Execute(KinesisEvent kinesisEvent)
        {
            _logger.Information($"Processing {kinesisEvent.Records.Count} records.");

            var tollAmountMessages = GetRecordContents<TollAmountMessage>(kinesisEvent.Records);
            var customerIds = tollAmountMessages.Select(t => t.CustomerId);

            try
            {
                var billingAlerts = (await _billingAlertStore.Get(customerIds))?.ToList();

                if (billingAlerts == null)
                    billingAlerts = new List<BillingAlertItem>();

                var alertsToPublish = new List<AlertMessage>();

                foreach (var billingAlert in billingAlerts)
                {
                    var tollAmountMessage = tollAmountMessages.FirstOrDefault(t => t.CustomerId == billingAlert.CustomerId);
                    billingAlert.TotalBillAmount += tollAmountMessage.TollAmount;
                    billingAlert.BillAmountLastUpdated = DateTime.UtcNow;

                    if (ShouldAlert(billingAlert))
                        alertsToPublish.Add(new AlertMessage
                        {
                            CustomerId = billingAlert.CustomerId,
                            TotalBillAmount = billingAlert.TotalBillAmount,
                            AlertAmountThreshold = billingAlert.AlertAmountThreshold,
                            Message = DefaultAlertMessage(billingAlert)
                        });
                }

                await _messagePublisher.Publish(alertsToPublish);

                billingAlerts.ForEach(b => { b.IsAlerted = b.IsAlerted || alertsToPublish.Any(a => a.CustomerId == b.CustomerId); });
                await _billingAlertStore.Put(billingAlerts);
            }
            catch(Exception ex)
            {
                _logger.Error(ex, ex.Message);
            }
        }

        private string DefaultAlertMessage(BillingAlertItem billingAlert) => 
            $"Your toll amount for {DateTime.Now.Month}/{DateTime.Now.Year} is ${billingAlert.TotalBillAmount} and has exceeded the threshold value of {billingAlert.AlertAmountThreshold}";

        private bool ShouldAlert(BillingAlertItem billingAlert) => !billingAlert.IsAlerted && billingAlert.TotalBillAmount >= billingAlert.AlertAmountThreshold;

        private List<T> GetRecordContents<T>(IList<KinesisEvent.KinesisEventRecord> records)
        {
            var contents = new List<T>();
            var jsonSerilaizerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            foreach (var record in records)
            {
                using (var reader = new StreamReader(record.Kinesis.Data, Encoding.ASCII))
                {
                    var jsonString = reader.ReadToEnd();
                    contents.Add(JsonSerializer.Deserialize<T>(jsonString, jsonSerilaizerOptions));
                }
            }

            return contents;
        }
    }
}
