using Amazon.Lambda.KinesisEvents;
using BillingAlert;
using System.Threading.Tasks;

namespace Consumer.Tests.Integration.Infrastructure
{
    public class Lambda
    {
        private readonly Function _function;

        public Lambda()
        {
            _function = new Function();
        }

        public Task Execute(KinesisEvent kinesisEvent)
        {
            return _function.Execute(kinesisEvent);
        }
    }
}
