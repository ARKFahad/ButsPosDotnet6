using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.ReportModels
{
   public class PLModel
    {
        [Key]
        public Guid Id { get; set; }
        public string? BranchId { get; set; }
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public decimal COGS { get; set; }
        public decimal Expens { get; set; }
        public decimal PLWithoutExpense { get; set; }
        public decimal PL { get; set; }
    }
}
