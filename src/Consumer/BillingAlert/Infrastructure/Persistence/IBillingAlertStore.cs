using BillingAlert.Infrastructure.Persistence.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BillingAlert.Infrastructure.Persistence
{
    public interface IBillingAlertStore
    {
        Task<IList<BillingAlertItem>> Get(IEnumerable<int> userIds);
        Task Put(IEnumerable<BillingAlertItem> billingAlerts);
    }
}
