using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
   public class PurchaseItemViewModel
    {
        public PurchaseItemViewModel(PurchaseItem purchaseItem )
        {
            Id = purchaseItem.Id;
            PurchaseNo = purchaseItem.Purchase.PurchaseNo;
            PurchaseId = purchaseItem.PurchaseId;
            ProductId = purchaseItem.ProductId;
            ProductName = purchaseItem.Product?.Name;
            Option = purchaseItem.Product?.Name + " " + purchaseItem.Product?.PackSize + " " + purchaseItem.Product?.Category.Name;
            PurchasePrice = purchaseItem.PurchasePrice;
            ExpireDate = purchaseItem.ExpireDate;
            BatchNo = purchaseItem.BatchNo;
            PurchaseQuantity = purchaseItem.PurchaseQuantity;
            ReturnQuantity = purchaseItem.ReturnQuantity;
            BonusQuantity = purchaseItem.BonusQuantity;
            Amount = purchaseItem.Amount;
            CreatedOn = purchaseItem.CreatedOn;
            
        }
        public string Id { get; set; }
        public string PurchaseNo { get; set; }
        public string PurchaseId { get; set; }
        public string ProductId { get; set; }
        public string? ProductName { get; set; }
        public string Option { get; set; }
        public decimal PurchasePrice { get; set; }
        public DateTime? ExpireDate { get; set; }
        public string BatchNo { get; set; }
        public int PurchaseQuantity { get; set; }
        public int ReturnQuantity { get; set; }
        public int BonusQuantity { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedOn { get; set; }

    }
}
