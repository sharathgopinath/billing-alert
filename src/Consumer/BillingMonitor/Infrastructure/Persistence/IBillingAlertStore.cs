using System.Collections.Generic;
using System.Threading.Tasks;

namespace BillingMonitor.Infrastructure.Persistence
{
    public interface IBillingAlertStore
    {
        Task<IList<BillingAlert>> Get(IEnumerable<int> userIds);
        Task Put(IEnumerable<BillingAlert> billingAlerts);
    }
}
