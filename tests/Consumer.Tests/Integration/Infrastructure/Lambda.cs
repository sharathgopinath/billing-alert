using BillingAlert;
using BillingAlert.Models;
using System.Collections.Generic;
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

        public Task Execute(IEnumerable<TollAmountMessage> messages)
        {
            return _function.Execute(messages);
        }
    }
}
