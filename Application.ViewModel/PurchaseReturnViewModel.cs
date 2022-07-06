using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class PurchaseReturnViewModel
    {
        public PurchaseReturnViewModel(PurchaseReturn purchaseReturn)
        {
            Id = purchaseReturn.Id;
            ReferenceNo = purchaseReturn.ReferenceNo;
            SupplierId = purchaseReturn.SupplierId;
            SupplierName = purchaseReturn.Supplier?.Name;
            PurchaseReturnDate = purchaseReturn.CreatedOn;
            BranchId = purchaseReturn.BranchId;
            BranchName = purchaseReturn.Branch?.Name;
            TotalValue = purchaseReturn.TotalValue;
            TotalItem = purchaseReturn.TotalItem;
            CreatedOn = purchaseReturn.CreatedOn;
            UpdatedOn = purchaseReturn.UpdatedOn;
            CreatedBy = purchaseReturn.CreatedBy;
            UpdatedBy = purchaseReturn.UpdatedBy;
            PurchaseReturnItems = new List<PurchaseReturnItemViewModel>();
            if (purchaseReturn.PurchaseReturnItems.Any())
            {
                foreach (var item in purchaseReturn.PurchaseReturnItems)
                {
                    PurchaseReturnItems.Add(new PurchaseReturnItemViewModel(item));
                }
            }
        }

        public string Id { get; set; }
        public string ReferenceNo { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public DateTime PurchaseReturnDate { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public decimal TotalValue { get; set; }
        public int TotalItem { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public IList<PurchaseReturnItemViewModel> PurchaseReturnItems { get; set; }
    }
}
