using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.RequestModel
{
    public class StockAdjustmentBatchModel
    {
        public StockAdjustmentBatchModel()
        {
            StockAdjustmentBatchItems = new List<StockAdjustmentBatchItemModel>();
        }

        public string? Id { get; set; }
        public string? Code { get; set; }
        public string? BranchId { get; set; }
        public string? Remark { get; set; }

        public IList<StockAdjustmentBatchItemModel> StockAdjustmentBatchItems { get; set; }
    }
}
