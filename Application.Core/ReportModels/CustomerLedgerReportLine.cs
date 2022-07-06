using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.ReportModels
{
   public class CustomerLedgerReportLine
    {
        [Key]
        public Guid Id { get; set; }
        public string? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerCode { get; set; }
        public DateTime Date { get; set; }
        public string? RefNo { get; set; }
        public string? Debit { get; set; }
        public string? Credit { get; set; }
        public decimal Balance { get; set; }
    }
}
