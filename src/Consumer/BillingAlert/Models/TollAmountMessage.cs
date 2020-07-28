using System;

namespace BillingAlert.Models
{
    [Serializable]
    public class TollAmountMessage
    {
        public int CustomerId { get; set; }
        public decimal TollAmount { get; set; }
    }
}
