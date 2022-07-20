using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.RequestModel
{
    public class StockAdjustmentModel
    {
        public string ProductId { get; set; }
        public string SupplierId { get; set; }
        public string BranchId { get; set; }
        public int Quantity { get; set; }
        public decimal PurchaseRate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
