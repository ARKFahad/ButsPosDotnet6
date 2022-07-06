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
    public class JournalEntryRequestModel : BaseRequestModel<JournalEntry>
    {
        public JournalEntryRequestModel()
        {
            OrderBy = "CreatedOn";
            IsIncludeParents = true;
        }
        public DateTime? fromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public string? BranchId {get; set;}

        public string? FiscalYearId { get; set; }
        public override Expression<Func<JournalEntry, bool>> GetExpression()
        {

            ExpressionObject = x => !x.Deleted;
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = ExpressionObject.And(x =>
                     x.Vnumber.ToLower().Contains(Keyword)
                     || x.Branch.Name.ToLower().Contains(Keyword));
            }
            if (fromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.CreatedOn.Date >= fromDate.Value.Date && x.CreatedOn.Date <= ToDate.Value.Date);
            if (!string.IsNullOrWhiteSpace(BranchId))
            {
                ExpressionObject = ExpressionObject.And(x => x.BranchId == BranchId);
            }
            if (!string.IsNullOrWhiteSpace(FiscalYearId))
            {
                ExpressionObject = ExpressionObject.And(x => x.FiscalYearId == FiscalYearId);
            }

            return ExpressionObject;
        }

        public override IQueryable<JournalEntry> IncludeParents(IQueryable<JournalEntry> queryable)
        {
            return queryable.Include(x => x.FiscalYear).Include(x => x.Branch)
                .Include(x => x.JournalEntryLines).ThenInclude(x => x.Account);
        }
    }
}
