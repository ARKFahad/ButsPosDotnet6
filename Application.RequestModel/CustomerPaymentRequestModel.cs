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
    public class CustomerPaymentRequestModel : BaseRequestModel<CustomerPayment>
    {
        public CustomerPaymentRequestModel()
        {
            IsIncludeParents = true;
        }
        public DateTime? fromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public string? BranchId { get; set; }
        public string? CustomerId { get; set; }
        public override Expression<Func<CustomerPayment, bool>> GetExpression()
        {
            ExpressionObject = x => !x.Deleted;
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = ExpressionObject.And(x=>x.BillNo.Contains(Keyword) || x.Customer.Name.Contains(Keyword));
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

        public override IQueryable<CustomerPayment> IncludeParents(IQueryable<CustomerPayment> queryable)
        {
            return queryable.Include(x => x.Customer).Include(x => x.PaymentMethod).Include(x=>x.Branch);
        }
    }
}
