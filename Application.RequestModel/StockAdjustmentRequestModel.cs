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
    public class StockAdjustmentRequestModel : BaseRequestModel<StockAdjustment>
    {
        public StockAdjustmentRequestModel()
        {
            IsIncludeParents = true;
        }
        public DateTime? fromDate { get; set; }

        public DateTime? ToDate { get; set; }
        public override Expression<Func<StockAdjustment, bool>> GetExpression()
        {
            ExpressionObject = x => !x.Deleted;
            if (fromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.CreatedOn.Date >= fromDate.Value.Date && x.CreatedOn.Date <= ToDate.Value.Date);
            return ExpressionObject;

        }
        public override IQueryable<StockAdjustment> IncludeParents(IQueryable<StockAdjustment> queryable)
        {
            return queryable.Include(x => x.Branch).Include(x=>x.Supplier).Include(x => x.Product).ThenInclude(x=>x.Category);
        }
    }
}
