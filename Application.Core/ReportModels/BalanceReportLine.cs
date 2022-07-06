using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.ReportModels
{
   public class BalanceReportLine
    {
        [Key]
        public Guid Id { get; set; }
        public string? BranchId { get; set; }
        public DateTime Date { get; set; }
        public decimal CashSales { get; set; }
        public decimal CustomerDueCollection { get; set; }
        public decimal Expens { get; set; }
        public decimal SalesReturn { get; set; }
        public decimal SupplierPayment { get; set; }
        public decimal Balance { get; set; }
    }
}
