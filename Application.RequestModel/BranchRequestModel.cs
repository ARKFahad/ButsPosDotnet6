using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.RequestModel
{
    public class BranchRequestModel : BaseRequestModel<Branch>
    {
        public override Expression<Func<Branch, bool>> GetExpression()
        {
            ExpressionObject = x => !x.Deleted;
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = ExpressionObject.And(x => x.Name.Contains(Keyword) || x.Address.Contains(Keyword));
            }
           
            return ExpressionObject;
        }
    }
}
