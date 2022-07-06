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
    public class ProductRequestModel : BaseRequestModel<Product>
    {
        public List<string> SupplierIds { get; set; }
        public List<string> CategoryIds { get; set; }
        public ProductRequestModel()
        {
            IsIncludeParents = true;
            SupplierIds = new List<string>();
            CategoryIds = new List<string>();
        }
        public override Expression<Func<Product, bool>> GetExpression()
        {
            ExpressionObject = x => !x.Deleted;
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = ExpressionObject.And(x => x.Name.ToLower().StartsWith(Keyword.ToLower()));
            if (SupplierIds.Any())
            {
                ExpressionObject = ExpressionObject.And(x => SupplierIds.Contains(x.SupplierId));
            }
            if (CategoryIds.Any())
            {
                ExpressionObject = ExpressionObject.And(x => CategoryIds.Contains(x.CategoryId));
            }
            return ExpressionObject;
        }

        public override IQueryable<Product> IncludeParents(IQueryable<Product> queryable)
        {
            return queryable.Include(x=>x.Category).Include(x=>x.GenericName)
                .Include(x=>x.Supplier).Include(x=>x.MeasurementUnit).Include(x=>x.CountryOfOrigin);
        }
    }
}
