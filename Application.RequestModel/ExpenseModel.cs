using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.RequestModel
{
    public class ExpenseModel
    {
        public string? Id { get; set; }
        public string? EntryDate { get; set; }
        public string? BranchId { get; set; }
        public string? ExpenseTypeId { get; set; }
        public string? PaymentMethodId { get; set; }
        public decimal Amount { get; set; }
        public string? Remark { get; set; }
       

       
    }
}
