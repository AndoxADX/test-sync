using System;
using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models
{
    /// <summary>
    public class GiftCreateViewModel
    {
        public string OutGiftId { get; set; }
        public string CoinCode { get; set; }
        public decimal TotalAmount { get; set; }
        public string Subject { get; set; }
        public string SubjectDetail { get; set; }
        public string NotifyUrl { get; set; }
        public string UserId { get; set; }
        public int AccountId { get; set; }

    }
}