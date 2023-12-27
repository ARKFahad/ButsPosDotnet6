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
    public class PurchaseItemListRequestModel : BaseRequestModel<PurchaseItem>
    {
        public PurchaseItemListRequestModel()
        {
            IsIncludeParents = true;
        }
        public DateTime? fromDate { get; set; }

        public DateTime? ToDate { get; set; }
        public string? BranchId { get; set; }
        public string? SupplierId { get; set; }
        public string? ProductId { get; set; }
        public override Expression<Func<PurchaseItem, bool>> GetExpression()
        {
            ExpressionObject = x => !x.Deleted;
            if (!string.IsNullOrWhiteSpace(Keyword))
                ExpressionObject = ExpressionObject.And(x => x.Purchase.PurchaseNo.Contains(Keyword) || x.Product.Name.Contains(Keyword));

            if (!string.IsNullOrWhiteSpace(BranchId))
            {
                ExpressionObject = ExpressionObject.And(x => x.BranchId == BranchId);
            }
            if (!string.IsNullOrWhiteSpace(SupplierId))
            {
                ExpressionObject = ExpressionObject.And(x => x.Purchase.SupplierId == SupplierId);
            }
            if (!string.IsNullOrWhiteSpace(ProductId))
            {
                ExpressionObject = ExpressionObject.And(x => x.ProductId == ProductId);
            }
            if (fromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.CreatedOn.Date >= fromDate.Value.Date && x.CreatedOn.Date <= ToDate.Value.Date);

            return ExpressionObject;
        }
        public override IQueryable<PurchaseItem> IncludeParents(IQueryable<PurchaseItem> queryable)
        {
            return queryable.Include(x => x.Purchase).Include(x => x.Product).ThenInclude(x => x.Category);
        }
    }
}
