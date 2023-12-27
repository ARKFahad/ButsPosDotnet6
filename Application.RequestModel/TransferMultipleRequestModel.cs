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
    public class TransferMultipleRequestModel : BaseRequestModel<TransferMultiple>
    {
        public TransferMultipleRequestModel()
        {
            IsIncludeParents = true;
        }
        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
        public string? SourceBranchId { get; set; }
        public string? DestinationBranchId { get; set; }
        public string? SupplierId { get; set; }
        public override Expression<Func<TransferMultiple, bool>> GetExpression()
        {
            ExpressionObject = x => !x.Deleted;
            if (!string.IsNullOrWhiteSpace(Keyword)){
                ExpressionObject =ExpressionObject.And(x => x.TransferNo.Contains(Keyword));
            }
            if (!string.IsNullOrWhiteSpace(SourceBranchId))
            {
                ExpressionObject = ExpressionObject.And(x => x.SourceId == SourceBranchId);
            }
            if (!string.IsNullOrWhiteSpace(DestinationBranchId))
            {
                ExpressionObject = ExpressionObject.And(x => x.DestinationId == DestinationBranchId);
            }
            if (!string.IsNullOrWhiteSpace(SupplierId))
            {
                ExpressionObject = ExpressionObject.And(x => x.SupplierId == SupplierId);
            }
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.CreatedOn.Date >= FromDate.Value.Date && x.CreatedOn.Date <= ToDate.Value.Date);

            return ExpressionObject;
        }

        public override IQueryable<TransferMultiple> IncludeParents(IQueryable<TransferMultiple> queryable)
        {
            return queryable.Include(x => x.Destination).Include(x => x.Source).Include(x => x.Supplier)
                .Include(x=>x.TransferDetails).ThenInclude(x=>x.Product).ThenInclude(x=>x.Category);
        }
    }
}
