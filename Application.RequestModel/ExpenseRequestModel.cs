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
    public class ExpenseRequestModel : BaseRequestModel<Expense>
    {
        public ExpenseRequestModel()
        {
            IsIncludeParents = true;
        }
        public string? ExpenseTypeId { get; set; }
        public string? BranchId { get; set; }
        public DateTime? fromDate { get; set; }

        public DateTime? ToDate { get; set; }
       
        public override Expression<Func<Expense, bool>> GetExpression()
        {
            ExpressionObject = x => !x.Deleted;
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = ExpressionObject.And(x => x.ExpenseType.Name.Contains(Keyword));
            if (!string.IsNullOrWhiteSpace(ExpenseTypeId))
            {
                ExpressionObject = ExpressionObject.And(x => x.ExpenseTypeId == ExpenseTypeId);
            }
           
            if (fromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.CreatedOn.Date >= fromDate.Value.Date && x.CreatedOn.Date <= ToDate.Value.Date);
            if (!string.IsNullOrWhiteSpace(BranchId))
            {
                ExpressionObject = ExpressionObject.And(x => x.BranchId == BranchId);
            }
            return ExpressionObject;
        }

        public override IQueryable<Expense> IncludeParents(IQueryable<Expense> queryable)
        {
            return queryable.Include(x => x.ExpenseType).Include(x=>x.Branch);
        }
    }
}
