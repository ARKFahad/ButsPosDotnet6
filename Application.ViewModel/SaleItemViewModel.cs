using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class SaleItemViewModel
    {
        public SaleItemViewModel(SaleItem saleItem)
        {
            Id = saleItem.Id;
            SaleId = saleItem.SaleId;
            ProductId = saleItem.ProductId;
            ProductName = saleItem?.Product?.Name;
            Option = saleItem?.Product?.Name + " " + saleItem?.Product?.PackSize + " " + saleItem?.Product?.Category?.Name;
            Price = saleItem.Price;
            Quantity = saleItem.Quantity;  
            Amount = saleItem.Amount;
            ItemCogs = saleItem.CogsPerItem;
        }
        public string Id { get; set; }
        public string SaleId { get; set; }
        public string ProductId { get; set; }
        public string? ProductName { get; set; }
        public string Option { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal ItemCogs { get; set; }
        public decimal Amount { get; set; }
        
      
       
    }
}
