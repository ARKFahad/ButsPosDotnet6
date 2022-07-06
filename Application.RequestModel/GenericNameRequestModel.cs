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
    public class GenericNameRequestModel : BaseRequestModel<GenericName>
    {
        public GenericNameRequestModel()
        {
            IsIncludeParents = true;
        }
        public string? CategoryId { get; set; }
        public override Expression<Func<GenericName, bool>> GetExpression()
        {
            ExpressionObject = x => !x.Deleted;
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = ExpressionObject.And(x => x.Name.Contains(Keyword));
            if (!string.IsNullOrWhiteSpace(CategoryId))
            {
                ExpressionObject = ExpressionObject.And(x => x.CategoryId==CategoryId);
            }
            return ExpressionObject;
        }

        public override IQueryable<GenericName> IncludeParents(IQueryable<GenericName> queryable)
        {
            return queryable.Include(x => x.Category);
        }
    }
}
