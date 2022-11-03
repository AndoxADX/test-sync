using System;
using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models
{
    /// <summary>
    public class TradeCreateViewModel
    {
        public string OutTradeId { get; set; }
        public string CoinCode { get; set; }
        public decimal TotalAmount { get; set; }
        public string Subject { get; set; }
        public string SubjectDetail { get; set; }
        public string NotifyUrl { get; set; }
        public int AccountId { get; set; }

    }
}