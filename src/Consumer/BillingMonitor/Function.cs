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

            var customerIds = tollAmountMessages.Select(t => t.CustomerId);

            try
            {
                var billingAlerts = (await _billingAlertStore.Get(customerIds))?.ToList();

                if (billingAlerts == null)
                    billingAlerts = new List<BillingAlert>();

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
            }
            catch(Exception ex)
            {
                _logger.Error(ex, ex.Message);
            }
        }

        private string DefaultAlertMessage(BillingAlert billingAlert) => 
            $"Your toll amount for {DateTime.Now.Month}/{DateTime.Now.Year} is ${billingAlert.TotalBillAmount} and has exceeded the threshold value of {billingAlert.AlertAmountThreshold}";

        private bool ShouldAlert(BillingAlert billingAlert) => !billingAlert.IsAlerted && billingAlert.TotalBillAmount >= billingAlert.AlertAmountThreshold;
    }
}
