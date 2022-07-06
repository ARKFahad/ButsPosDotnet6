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
    public class SupplierLedgerRequestModel : BaseRequestModel<SupplierLedgerReportLine>
    {
        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
        public string? SupplierId { get; set; }
        public override Expression<Func<SupplierLedgerReportLine, bool>> GetExpression()
        {

            if (!string.IsNullOrEmpty(Keyword))
            {
                ExpressionObject = x => x.SupplierName.Contains(Keyword);
            }
            if (!string.IsNullOrWhiteSpace(SupplierId))
            {
                ExpressionObject = ExpressionObject.And(x => x.SupplierId == SupplierId);
            }
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.Date.Date >= FromDate.Value.Date && x.Date.Date <= ToDate.Value.Date);
           

            return ExpressionObject;
        }
    }
}
