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
    public class TransferDetailListRequestModel : BaseRequestModel<TransferDetail>
    {
        public TransferDetailListRequestModel()
        {
            IsIncludeParents = true;
        }
        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
        public string? SourceBranchId { get; set; }
        public string? DestinationBranchId { get; set; }
        public string? ProductId { get; set; }
        public override Expression<Func<TransferDetail, bool>> GetExpression()
        {
            ExpressionObject = x => !x.Deleted;
            if (!string.IsNullOrWhiteSpace(Keyword))
                ExpressionObject = ExpressionObject.And(x => x.TransferMultiple.TransferNo.Contains(Keyword));

            if (!string.IsNullOrWhiteSpace(SourceBranchId))
            {
                ExpressionObject = ExpressionObject.And(x => x.TransferMultiple.SourceId == SourceBranchId);
            }
            if (!string.IsNullOrWhiteSpace(DestinationBranchId))
            {
                ExpressionObject = ExpressionObject.And(x => x.TransferMultiple.DestinationId == DestinationBranchId);
            }
            if (!string.IsNullOrWhiteSpace(ProductId))
            {
                ExpressionObject = ExpressionObject.And(x => x.ProductId == ProductId);
            }
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.CreatedOn.Date >= FromDate.Value.Date && x.CreatedOn.Date <= ToDate.Value.Date);

            return ExpressionObject;
        }
        public override IQueryable<TransferDetail> IncludeParents(IQueryable<TransferDetail> queryable)
        {
            return queryable.Include(x => x.TransferMultiple).Include(x => x.Product).ThenInclude(x=>x.Category);
        }
    }
}
