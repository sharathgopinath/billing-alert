using System;

namespace BillingMonitor.Infrastructure.Persistence
{
    public class BillingAlert
    {
        public int UserId { get; set; }
        public decimal AlertAmountThreshold { get; set; }
        public decimal TotalBillAmount { get; set; }
        public DateTime BillAmountLastUpdated { get; set; }
        public bool IsAlerted { get; set; }
    }
}