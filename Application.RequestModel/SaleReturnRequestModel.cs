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
    public class SaleReturnRequestModel : BaseRequestModel<SaleReturn>
    {
        public SaleReturnRequestModel()
        {
            IsIncludeParents = true;
        }
        public DateTime? fromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public string? BranchId { get; set; }

        public string? CustomerId { get; set; }

        public override Expression<Func<SaleReturn, bool>> GetExpression()
        {
            ExpressionObject = x => !x.Deleted;
             if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject =ExpressionObject.And(x =>
                    x.ReferenceNo.ToLower().Contains(Keyword)
                    || x.Branch.Name.ToLower().Contains(Keyword)
                   );  
            }
            if (fromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.CreatedOn.Date >= fromDate.Value.Date && x.CreatedOn.Date <= ToDate.Value.Date);
            if (!string.IsNullOrWhiteSpace(BranchId))
            {
                ExpressionObject = ExpressionObject.And(x => x.BranchId == BranchId);
            }
            if (!string.IsNullOrWhiteSpace(CustomerId))
            {
                ExpressionObject = ExpressionObject.And(x => x.CustomerId == CustomerId);
            }

            return ExpressionObject;
        }
        public override IQueryable<SaleReturn> IncludeParents(IQueryable<SaleReturn> queryable)
        {
            return queryable.Include(x => x.Customer).Include(x => x.Branch)
                .Include(x => x.SaleReturnItems).ThenInclude(x => x.Product).ThenInclude(x => x.Category);
        }
    }
}
