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
    public class TransferRequestModel : BaseRequestModel<TransferHistory>
    {
        public TransferRequestModel()
        {
            IsIncludeParents = true;
        }

        public DateTime? fromDate { get; set; }

        public DateTime? ToDate { get; set; }
        public string? ProductId { get; set; }
        public override Expression<Func<TransferHistory, bool>> GetExpression()
        {
            ExpressionObject = x => !x.Deleted;
            if (fromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.CreatedOn.Date >= fromDate.Value.Date && x.CreatedOn.Date <= ToDate.Value.Date);
          
            if (!string.IsNullOrWhiteSpace(ProductId))
            {
                ExpressionObject = ExpressionObject.And(x => x.ProductId == ProductId);
            }
            return ExpressionObject;

        }
        public override IQueryable<TransferHistory> IncludeParents(IQueryable<TransferHistory> queryable)
        {
            return queryable.Include(x => x.Source).Include(x => x.Product).ThenInclude(x=>x.Category).Include(x=>x.Destination);
        }
    }
}
