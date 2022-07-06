using Application.Core.Common;
using Application.Core.ReportModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.RequestModel
{
    public class PLRequestModel : BaseRequestModel<PLModel>
    {
        public DateTime? fromDate { get; set; }

        public DateTime? ToDate { get; set; }
        public string? BranchId { get; set; }
        public override Expression<Func<PLModel, bool>> GetExpression()
        {

            if (!string.IsNullOrEmpty(Keyword))
            {
                ExpressionObject = x => x.BranchId.Contains(Keyword);
            }
            if (!string.IsNullOrWhiteSpace(BranchId))
            {
                ExpressionObject = ExpressionObject.And(x => x.BranchId == BranchId);
            }
            if (fromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.Date.Date >= fromDate.Value.Date && x.Date.Date <= ToDate.Value.Date);
           

            return ExpressionObject;
        }
    }
}
