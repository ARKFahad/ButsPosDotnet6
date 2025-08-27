using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel
{
    public class StockAdjustmentBatchViewModel
    {
        public StockAdjustmentBatchViewModel(StockAdjustmentBatch stockAdjustmentBatch)
        {
            Id = stockAdjustmentBatch.Id;
            Code = stockAdjustmentBatch.Code;
            BranchId = stockAdjustmentBatch.BranchId;
            BranchName = stockAdjustmentBatch.Branch?.Name;
            Remark = stockAdjustmentBatch.Remark;
            CreatedOn = stockAdjustmentBatch.CreatedOn;
            UpdatedOn = stockAdjustmentBatch.UpdatedOn;
            CreatedBy = stockAdjustmentBatch.CreatedBy;
            UpdatedBy = stockAdjustmentBatch.UpdatedBy;
            StockAdjustmentBatchItems = new List<StockAdjustmentBatchItemViewModel>();
            if (stockAdjustmentBatch.StockAdjustmentBatchItems.Any())
            {
                foreach (var item in stockAdjustmentBatch.StockAdjustmentBatchItems)
                {
                    StockAdjustmentBatchItems.Add(new StockAdjustmentBatchItemViewModel(item));
                }
            }
        }

        public string Id { get; set; }
        public string Code { get; set; }
        public string BranchId { get; set; }
        public string? BranchName { get; set; }
        public string Remark { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public IList<StockAdjustmentBatchItemViewModel> StockAdjustmentBatchItems { get; set; }
    }
}
