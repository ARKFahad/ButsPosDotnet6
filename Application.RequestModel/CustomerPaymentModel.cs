using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.RequestModel
{
   public class CustomerPaymentModel
    {
        public string? Id { get; set; }
        public string? BillNo { get; set; }
        public string? BranchId { get; set; }
        public string? CustomerId { get; set; }
        public string? PaymentDate { get; set; }
        public string? PaymentMethodId { get; set; }
        public decimal PaymentAmount { get; set; }
        public string? Remark { get; set; }
        public decimal CustomerPreviousBalance { get; set; }
        public decimal CustomerDueAmount { get; set; }
    }
}
