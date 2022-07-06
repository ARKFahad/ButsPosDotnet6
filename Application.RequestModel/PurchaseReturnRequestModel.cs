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
    public class PurchaseReturnRequestModel : BaseRequestModel<PurchaseReturn>
    {
        public PurchaseReturnRequestModel()
        {
            IsIncludeParents = true;
        }
        public DateTime? fromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public string? BranchId { get; set; }

        public string? SupplierId { get; set; }

        public override Expression<Func<PurchaseReturn, bool>> GetExpression()
        {
            ExpressionObject = x => !x.Deleted;
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = ExpressionObject.And(x =>
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
            if (!string.IsNullOrWhiteSpace(SupplierId))
            {
                ExpressionObject = ExpressionObject.And(x => x.SupplierId == SupplierId);
            }

            return ExpressionObject;
        }
        public override IQueryable<PurchaseReturn> IncludeParents(IQueryable<PurchaseReturn> queryable)
        {
            return queryable.Include(x => x.Supplier).Include(x => x.Branch)
                .Include(x => x.PurchaseReturnItems).ThenInclude(x => x.Product).ThenInclude(x => x.Category);
        }
    }
}
