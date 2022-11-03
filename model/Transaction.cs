using System;
using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models
{
    /// <summary>
    /// Always reflected
    /// </summary>
    public class Transaction
    {
        [Key]
        public string Id { get; set; }
        public string AccountFrom { get; set; }
        public string AccountTo { get; set; }

        public decimal Amount { get; set; }
        public string EntryType { get; set; }
        public string Type { get; set; }
        public DateTime Date { get; set; }
        // public bool IsReflected { get; set; }
        public string Remark { get; set; }
    }
}