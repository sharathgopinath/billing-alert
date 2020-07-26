using System.Collections.Generic;
using System.Threading.Tasks;

namespace BillingAlert.Infrastructure.Messaging
{
    public interface IMessagePublisher
    {
        Task Publish<T>(IEnumerable<T> messages) where T : class;
    }
}
