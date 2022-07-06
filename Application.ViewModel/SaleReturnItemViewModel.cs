using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class SaleReturnItemViewModel
    {
        public SaleReturnItemViewModel(SaleReturnItem saleReturnItem)
        {
            Id = saleReturnItem.Id;
            SaleId = saleReturnItem.SaleReturnId;
            ProductId = saleReturnItem.ProductId;
            ProductName = saleReturnItem.Product?.Name;
            Price = saleReturnItem.Rate;
            Quantity = saleReturnItem.ReturnQuantity;
            Amount = saleReturnItem.Amount;
           
        }
        public string Id { get; set; }
        public string SaleId { get; set; }
        public string ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
       
        public decimal Amount { get; set; }

    }
}
