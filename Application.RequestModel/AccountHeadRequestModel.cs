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
    
    public class AccountHeadRequestModel : BaseRequestModel<AccountHead>
    {
        public AccountHeadRequestModel()
        {
            IsIncludeParents = true;
        }
        public override Expression<Func<AccountHead, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword))
               ExpressionObject = x => x.Name.Contains(Keyword) || x.Code.Contains(Keyword);

            return ExpressionObject;
        }
        public override IQueryable<AccountHead> IncludeParents(IQueryable<AccountHead> queryable)
        {
            return queryable.Include(x=>x.AccountType).Include(x=>x.Parent);
        }
    }
}
