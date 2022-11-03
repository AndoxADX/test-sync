using System;
using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models
{
    public class ReqTransaction
    {
        [Key]
        public string Id { get; set; }
        public string AccountFrom {get; set;}
        public string AccountTo {get; set;}
        public decimal Amount { get; set; }
        public string EntryType { get; set; }
        public string Type { get; set; }
        public DateTime Date { get; set; }
        public bool IsProcess { get; set; }
        public bool IsSuccess { get; set; }
        public string Remark { get; set; }
    }
}