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
    public class CustomerLedgerRequestModel : BaseRequestModel<CustomerLedgerReportLine>
    {
        public DateTime? fromDate { get; set; }

        public DateTime? ToDate { get; set; }
        public string? CustomerId { get; set; }
        public override Expression<Func<CustomerLedgerReportLine, bool>> GetExpression()
        {

            if (!string.IsNullOrEmpty(Keyword))
            {
                ExpressionObject = x => x.CustomerName.Contains(Keyword);
            }
            if (!string.IsNullOrWhiteSpace(CustomerId))
            {
                ExpressionObject = ExpressionObject.And(x => x.CustomerId == CustomerId);
            }
            if (fromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.Date.Date >= fromDate.Value.Date && x.Date.Date <= ToDate.Value.Date);
           

            return ExpressionObject;
        }
    }
}
