using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class ItemProfitLossViewModel
    {
     
        public string? ProductName { get; set; }
        public string? SupplierName { get; set; }
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cogs { get; set; }
        public int ReturnQuantity { get; set; }

    }
}
