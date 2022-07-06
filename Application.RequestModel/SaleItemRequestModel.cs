using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.RequestModel
{
   public class SaleItemRequestModel
    {
        // productId: string, quantity: number, price: number,productName?:string,productPackSize?:string,productStockQuantity?:number
        public string? ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? ProductName { get; set; }
        public string? ProductPackSize { get; set; }
        public int ProductStockQuantity { get; set; }
    }
}
