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
    public class SaleRequestModel : BaseRequestModel<Sale>
    {
        public SaleRequestModel()
        {
            IsIncludeParents = true;
        }
        public DateTime? fromDate { get; set; }

        public DateTime? ToDate { get; set; }
        public string? BranchId { get; set; }
        public string? CustomerId { get; set; }
        public override Expression<Func<Sale, bool>> GetExpression()
        {

            ExpressionObject = x => !x.Deleted;
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = ExpressionObject.And(x =>
                     x.SaleNo.ToLower().Contains(Keyword)
                     || x.Branch.Name.ToLower().Contains(Keyword)
                     || x.Customer.Name.ToLower().Contains(Keyword)
                     || x.Branch.Name.ToLower().Contains(Keyword)
                     || x.Customer.Name.ToLower().Contains(Keyword)
                     || x.SaleItems.Any(x=>x.Product.Name.StartsWith(Keyword)));
            }
            if(fromDate.HasValue&& ToDate.HasValue)
            ExpressionObject = ExpressionObject.And(x => x.CreatedOn.Date >= fromDate.Value.Date && x.CreatedOn.Date <= ToDate.Value.Date);
            if (!string.IsNullOrWhiteSpace(BranchId))
            {
                //var EXPR2 = ExpressionObject;
                //EXPR2 = x => x.BranchId == BranchId;
                //var body = Expression.And(ExpressionObject.Body, EXPR2.Body);
                //var lambda = Expression.Lambda<Func<Sale, bool>>(body, ExpressionObject.Parameters[0]);
                // ExpressionObject = lambda;
                //return ExpressionObject;
                ExpressionObject = ExpressionObject.And(x => x.BranchId == BranchId);
               
                
               
            }
            if (!string.IsNullOrWhiteSpace(CustomerId))
            {
                ExpressionObject = ExpressionObject.And(x => x.CustomerId == CustomerId);
            }

            return ExpressionObject;
        }

        public override IQueryable<Sale> IncludeParents(IQueryable<Sale> queryable)
        {
            return queryable.Include(x => x.Customer).Include(x => x.Branch)
                .Include(x => x.SaleItems).ThenInclude(x => x.Product).ThenInclude(x => x.Category);
        }
    }
}
