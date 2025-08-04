using Application.Core.Entities;

namespace Application.ViewModel
{
    public class StockAdjustmentBatchItemViewModel
    {
        public StockAdjustmentBatchItemViewModel(StockAdjustmentBatchItem stockAdjustmentBatchItem)
        {
            Id = stockAdjustmentBatchItem.Id;
            StockAdjustmentBatchId = stockAdjustmentBatchItem.StockAdjustmentBatchId;
            ProductId = stockAdjustmentBatchItem.ProductId;
            ProductName = stockAdjustmentBatchItem.Product?.Name + " " + stockAdjustmentBatchItem.Product?.PackSize + " " + stockAdjustmentBatchItem.Product?.Category.Name;
            SupplierId = stockAdjustmentBatchItem.SupplierId;
            SupplierName = stockAdjustmentBatchItem.Supplier?.Name;
            Quantity = stockAdjustmentBatchItem.Quantity;
            PurchaseRate = stockAdjustmentBatchItem.PurchaseRate;
            ExpiryDate = stockAdjustmentBatchItem.ExpiryDate;
            CreatedOn = stockAdjustmentBatchItem.CreatedOn;
            CreatedBy = stockAdjustmentBatchItem.CreatedBy;
        }
        public string Id { get; set; }
        public string StockAdjustmentBatchId { get; set; }
        public string ProductId { get; set; }
        public string? ProductName { get; set; }
        public string SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public int Quantity { get; set; }
        public decimal PurchaseRate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
    }
}
