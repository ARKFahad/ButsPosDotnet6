using Application.Core.Common;
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
    public class StockRequestModel : BaseRequestModel<StockModel>
    {
        public string? ProductId { get; set; }
        public string? BranchId { get; set; }
        public List<string> SupplierIds { get; set; }
        public List<string> CategoryIds { get; set; }
        public StockRequestModel()
        {
            SupplierIds = new List<string>();
            CategoryIds = new List<string>();
        }
        public override Expression<Func<StockModel, bool>> GetExpression()
        {
            if (!string.IsNullOrEmpty(Keyword)) {
                ExpressionObject = x => x.ProductName.ToUpper().StartsWith(Keyword.ToUpper());
            }
            if (!string.IsNullOrWhiteSpace(BranchId))
            {
                //var EXPR2 = ExpressionObject;
                //EXPR2 = x => x.BranchId == BranchId;
                //var body = Expression.And(ExpressionObject.Body, EXPR2.Body);
                //var lambda = Expression.Lambda<Func<Sale, bool>>(body, ExpressionObject.Parameters[0]);
                // ExpressionObject = lambda;
                //return ExpressionObject;
                ExpressionObject = ExpressionObject.And(x => x.BranchId == BranchId);

            }
            if (SupplierIds.Any())
            {
                ExpressionObject = ExpressionObject.And(x => SupplierIds.Contains(x.SupplierId));
            }
            if (CategoryIds.Any())
            {
                ExpressionObject = ExpressionObject.And(x => CategoryIds.Contains(x.CategoryId));
            }
            if (!string.IsNullOrWhiteSpace(ProductId))
            {
                ExpressionObject = ExpressionObject.And(x => x.ProductId == ProductId);
            }
            return ExpressionObject;
        }
       
    }
}
