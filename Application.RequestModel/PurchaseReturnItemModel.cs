using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.RequestModel
{
    public class PurchaseReturnItemModel
    {
        public string? Id { get; set; }
        public string? PurchaseId { get; set; }
        public string? ProductId { get; set; }
        public string? SupplierId { get; set; }
        public int Quantity { get; set; }
        public int ReturnQty { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal Amount { get; set; }

    }
}
