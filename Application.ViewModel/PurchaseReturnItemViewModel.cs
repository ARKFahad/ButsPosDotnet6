using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class PurchaseReturnItemViewModel
    {
        public PurchaseReturnItemViewModel(PurchaseReturnItem purchaseReturnItem)
        {
            Id = purchaseReturnItem.Id;
            PurchaseId = purchaseReturnItem.PurchaseReturnId;
            ProductId = purchaseReturnItem.ProductId;
            ProductName = purchaseReturnItem.Product?.Name;
            Price = purchaseReturnItem.Rate;
            Quantity = purchaseReturnItem.ReturnQuantity;
            Amount = purchaseReturnItem.Amount;
            Reason = purchaseReturnItem.Reason;

        }
        public string Id { get; set; }
        public string PurchaseId { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public decimal Amount { get; set; }
        public string Reason { get; set; }

    }
}
