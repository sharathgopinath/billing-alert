namespace BillingMonitor.Models
{
    public class AlertMessage
    {
        public int CustomerId { get; set; }
        public decimal AlertAmountThreshold { get; set; }
        public decimal TotalBillAmount { get; set; }
        public string Message { get; set; }
    }
}
