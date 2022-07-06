using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
   public class PurchaseListViewModel
    {
        public PurchaseListViewModel(Purchase purchase)
        {
            Id = purchase.Id;
            PurchaseNo = purchase.PurchaseNo;
            SupplierId = purchase.SupplierId;
            SupplierName = purchase.Supplier?.Name;
            ReferenceNo = purchase.ReferenceNo;
            PurchaseDate = purchase.PurchaseDate;
            BranchId = purchase.BranchId;
            BranchName = purchase.Branch?.Name;
            TotalValue = purchase.TotalValue;
            TotalVat = purchase.TotalVat;
            Subtotal = purchase.Subtotal;
            Discount = purchase.Discount;
            AdditionalCost = purchase.AdditionalCost;
            NetAmount = purchase.NetAmount;
            PurchaseType = purchase.PurchaseType;
            CreatedOn = purchase.CreatedOn;
            UpdatedOn = purchase.UpdatedOn;
            CreatedBy = purchase.CreatedBy;
            UpdatedBy = purchase.UpdatedBy;
        }

        public string Id { get; set; }
        public string PurchaseNo { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string ReferenceNo { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public decimal TotalValue { get; set; }
        public decimal TotalVat { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal AdditionalCost { get; set; }
        public decimal NetAmount { get; set; }
        public int PurchaseType { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public IList<PurchaseItemViewModel> PurchaseItems { get; set; }
    }
}
