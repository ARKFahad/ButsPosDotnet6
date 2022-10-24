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
    public class SaleItemListRequestModel : BaseRequestModel<SaleItem>
    {
        public SaleItemListRequestModel()
        {
            IsIncludeParents = true;
        }
        public DateTime? fromDate { get; set; }

        public DateTime? ToDate { get; set; }
        public string? BranchId { get; set; }
        public string? SupplierId { get; set; }
        public string? ProductId { get; set; }
        public override Expression<Func<SaleItem, bool>> GetExpression()
        {
            ExpressionObject = x => !x.Deleted;
            if (!string.IsNullOrWhiteSpace(Keyword)) 
                ExpressionObject = ExpressionObject.And(x => x.Sale.SaleNo.Contains(Keyword) || x.Product.Name.Contains(Keyword));

            if (!string.IsNullOrWhiteSpace(BranchId))
            {
                ExpressionObject=ExpressionObject.And(x => x.BranchId == BranchId);
            }
            if (!string.IsNullOrWhiteSpace(SupplierId))
            {
                ExpressionObject = ExpressionObject.And(x => x.SupplierId == SupplierId);
            }
            if (!string.IsNullOrWhiteSpace(ProductId))
            {
                ExpressionObject = ExpressionObject.And(x => x.ProductId == ProductId);
            }
            if (fromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.CreatedOn.Date >= fromDate.Value.Date && x.CreatedOn.Date <= ToDate.Value.Date);

            return ExpressionObject;

        }
        public override IQueryable<SaleItem> IncludeParents(IQueryable<SaleItem> queryable)
        {
            return queryable.Include(x=>x.Sale).Include(x => x.Product).ThenInclude(x => x.Category);
        }
    }
}
