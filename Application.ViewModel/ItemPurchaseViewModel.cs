using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class ItemPurchaseViewModel
    {
        public string? ProductName { get; set; }
        public string? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public int PurchaseQuantity { get; set; }
        public decimal BonusQuantity { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal NetAmount { get; set; }
    }
}
