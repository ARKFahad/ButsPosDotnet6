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
    public class UserRequestModel : BaseRequestModel<User>
    {
        public UserRequestModel()
        {
            IsIncludeParents = true;
        }
        public override Expression<Func<User, bool>> GetExpression()
        {
            ExpressionObject = x => !x.Deleted;
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = ExpressionObject.And(x => x.Username.Contains(Keyword)
            || x.Role.Name.Contains(Keyword));
            return ExpressionObject;
            
        }
        public override IQueryable<User> IncludeParents(IQueryable<User> queryable)
        {
            return queryable.Include(x=>x.Role);
        }
    }
}
