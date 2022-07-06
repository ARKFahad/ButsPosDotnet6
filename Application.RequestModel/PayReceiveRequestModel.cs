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
    public class PayReceiveRequestModel : BaseRequestModel<PayReceive>
    {
        public PayReceiveRequestModel()
        {
            IsIncludeParents = true;
        }
        public DateTime? fromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public string? BranchId { get; set; }
        public override Expression<Func<PayReceive, bool>> GetExpression()
        {
            ExpressionObject = x => !x.Deleted;
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = ExpressionObject.And(x=>x.Vnumber.Contains(Keyword) || x.Description.Contains(Keyword));
            }
            if (fromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.CreatedOn.Date >= fromDate.Value.Date && x.CreatedOn.Date <= ToDate.Value.Date);
            
            if (!string.IsNullOrWhiteSpace(BranchId))
            {
                ExpressionObject = ExpressionObject.And(x => x.BranchId == BranchId);
            }
           
            return ExpressionObject;
        }

        public override IQueryable<PayReceive> IncludeParents(IQueryable<PayReceive> queryable)
        {
            return queryable.Include(x => x.Account).Include(x => x.PaymentMethod).Include(x=>x.Branch);
        }
    }
}
