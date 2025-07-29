using Application.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.RequestModel
{
    public class StockAdjustmentBatchRequestModel : BaseRequestModel<StockAdjustmentBatch>
    {
        public StockAdjustmentBatchRequestModel()
        {
            IsIncludeParents = true;
        }
        public DateTime? fromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public string? BranchId { get; set; }

        public override Expression<Func<StockAdjustmentBatch, bool>> GetExpression()
        {
            ExpressionObject = x => !x.Deleted;
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = ExpressionObject.And(x =>
                     x.Code.ToLower().Contains(Keyword)
                     || x.Branch.Name.ToLower().Contains(Keyword)
                   );
            }
            if (fromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.CreatedOn.Date >= fromDate.Value.Date && x.CreatedOn.Date <= ToDate.Value.Date);
            if (!string.IsNullOrWhiteSpace(BranchId))
            {
                ExpressionObject = ExpressionObject.And(x => x.BranchId == BranchId);
            }

            return ExpressionObject;
        }
        public override IQueryable<StockAdjustmentBatch> IncludeParents(IQueryable<StockAdjustmentBatch> queryable)
        {
            return queryable.Include(x => x.Branch).Include(x => x.StockAdjustmentBatchItems)
                .ThenInclude(x => x.Product).ThenInclude(x => x.Category).Include(x => x.StockAdjustmentBatchItems)
                .ThenInclude(x => x.Supplier);
        }
    }
}
