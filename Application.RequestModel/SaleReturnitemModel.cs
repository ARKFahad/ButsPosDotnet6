using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.RequestModel
{
    public class SaleReturnitemModel
    {
        public string? Id { get; set; }
        public string? SaleId { get; set; }
        public string? ProductId { get; set; }
        public string? SupplierId { get; set; }
        public int Quantity { get; set; }
        public int ReturnQty { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public decimal ItemCogs { get; set; }
        public bool Deleted { get; set; }
        

    }
}
