using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class ItemSaleProfitLossViewModel
    {
     
        public string? ProductName { get; set; }
        public string? SupplierName { get; set; }
        public int SaleQuantity { get; set; }
        public int ReturnQuantity { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal Cogs { get; set; }


    }
}
