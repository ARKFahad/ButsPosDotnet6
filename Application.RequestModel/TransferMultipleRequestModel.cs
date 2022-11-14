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
        public override Expression<Func<TransferMultiple, bool>> GetExpression()
        {
            ExpressionObject = x => !x.Deleted;
            if (!string.IsNullOrWhiteSpace(Keyword)){
                ExpressionObject =ExpressionObject.And(x => x.TransferNo.Contains(Keyword));
            }
            return ExpressionObject;
        }

        public override IQueryable<TransferMultiple> IncludeParents(IQueryable<TransferMultiple> queryable)
        {
            return queryable.Include(x => x.Destination).Include(x => x.Source).Include(x => x.Supplier)
                .Include(x=>x.TransferDetails).ThenInclude(x=>x.Product).ThenInclude(x=>x.Category);
        }
    }
}
