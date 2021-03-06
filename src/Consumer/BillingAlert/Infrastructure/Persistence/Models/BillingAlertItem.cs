﻿using System;

namespace BillingAlert.Infrastructure.Persistence.Models
{
    public class BillingAlertItem
    {
        public int CustomerId { get; set; }
        public decimal AlertAmountThreshold { get; set; }
        public decimal TotalBillAmount { get; set; }
        public DateTime BillAmountLastUpdated { get; set; }
        public bool IsAlerted { get; set; }
    }
}