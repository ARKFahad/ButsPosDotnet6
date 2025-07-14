using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.RequestModel
{
   public class SaleAddRequestModel
    {
        public SaleAddRequestModel()
        {
            CartItems = new List<SaleItemRequestModel>();
        }
        public string? CustomerId { get; set; }
        public string? PaymentMethodId { get; set; }
        public decimal Total { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountInPercentage { get; set; }
        public decimal NetTotal { get; set; }
        public decimal ReceivedAmount { get; set; }
        public decimal ReturnAmount { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal DueAmount { get; set; }
        public List<SaleItemRequestModel> CartItems { get; set; }
    }
}
