using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.RequestModel
{
   public class PayReceiveModel
    {
        public string? Id { get; set; }
        public string? Vnumber { get; set; }
        public int VoucherType { get; set; }
        public string? VoucherDate { get; set; }
        public string? BranchId { get; set; }
        public string? PaymentMethodId { get; set; }
        public string? CheckNo { get; set; }
        public string? AccountId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }
}
