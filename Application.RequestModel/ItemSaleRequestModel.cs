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
    public class ItemSaleRequestModel : BaseRequestModel<SaleItem>
    {
        public ItemSaleRequestModel()
        {
            IsIncludeParents = true;
        }
        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
        public string? BranchId { get; set; }
        public string? SupplierId { get; set; }
        public override Expression<Func<SaleItem, bool>> GetExpression()
        {
            ExpressionObject = x => !x.Deleted;
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = ExpressionObject.And(x => x.Product.Name.Contains(Keyword));
            }
            if (!string.IsNullOrWhiteSpace(SupplierId)) 
                ExpressionObject=ExpressionObject.And(x => x.SupplierId ==SupplierId);
            if (!string.IsNullOrWhiteSpace(BranchId))
                ExpressionObject = ExpressionObject.And(x => x.Sale.BranchId == BranchId);
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.CreatedOn.Date >= FromDate.Value.Date && x.CreatedOn.Date <= ToDate.Value.Date);
            return ExpressionObject;
        }
        public override IQueryable<SaleItem> IncludeParents(IQueryable<SaleItem> queryable)
        {
            return queryable.Include(x=>x.Sale).Include(x => x.Product).ThenInclude(x => x.Category);
        }
    }
}
