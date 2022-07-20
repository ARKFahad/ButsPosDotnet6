using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class StockAdjustmentViewModel
    {
        public StockAdjustmentViewModel(StockAdjustment stockAdjustment)
        {
            Code = stockAdjustment.Code;
            Product = stockAdjustment.Product != null ? new ProductViewModel(stockAdjustment.Product) : null;
            SupplierId = stockAdjustment.SupplierId;
            BranchId = stockAdjustment.BranchId;
            SupplierName = stockAdjustment.Supplier?.Name;
            BranchName = stockAdjustment.Branch?.Name;
            Quantity = stockAdjustment.Quantity;
            PurchaseRate = stockAdjustment.PurchaseRate;

        }
        public string Code { get; set; }
        public ProductViewModel Product { get; set; }
        public string SupplierId { get; set; }
        public string BranchId { get; set; }
        public string SupplierName { get; set; }
        public string BranchName { get; set; }
        public int Quantity { get; set; }
        public decimal PurchaseRate { get; set; }
    }
}
