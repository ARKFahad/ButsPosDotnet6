using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Application.RequestModel
{
    public abstract class BaseRequestModel<T> where T : class
    {
        protected Expression<Func<T, bool>>
            ExpressionObject = e => true; // protected:-ai property ta baserequestmodel er bassa jara tara pabe

        public BaseRequestModel()
        {
            Page = 0;
            RowsPerPage = 5;
            this.ExpressionObject = x => true; //initializing the expressionObject true
            OrderBy = "UpdatedOn";
            
        }
        public int Page { get; set; }

        public int RowsPerPage { get; set; }

        public string? OrderBy { get; set; }

        public bool IsAscending { get; set; }

        public string? Keyword { get; set; }

        public bool IsIncludeParents { get; set; }

       

        public Func<IQueryable<T>, IOrderedQueryable<T>> OrderByFunc() //order by lambda expression generate kore dibe
        {
            string propertyName = OrderBy;
            bool ascending = IsAscending;
            var source = Expression.Parameter(typeof(IQueryable<T>), "source");
            var item = Expression.Parameter(typeof(T), "item");
            var member = Expression.Property(item, propertyName);
            var selector = Expression.Quote(Expression.Lambda(member, item));
            var body = Expression.Call(
                typeof(Queryable), ascending ? "OrderBy" : "OrderByDescending",
                new[] { item.Type, member.Type },
                source, selector);
            var expr = Expression.Lambda<Func<IQueryable<T>, IOrderedQueryable<T>>>(body, source);
            var func = expr.Compile();
            return func;
        }

        public IQueryable<T> SkipAndTake(IQueryable<T> queryable)
        {
            if (Page != -1) queryable = queryable.Skip(Page * RowsPerPage).Take(RowsPerPage);
            return queryable;
        }

        public virtual IQueryable<T> IncludeParents(IQueryable<T> queryable)
        {
            return queryable;
        }

        public abstract Expression<Func<T, bool>> GetExpression();

        public IQueryable<T> GetOrderedData(IQueryable<T> queryable)
        {
            queryable = queryable.Where(GetExpression());
            queryable = OrderByFunc()(queryable);
            return queryable;
        }

    }
}

public static class ExpressionHelper
{
    public static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second,
        Func<Expression, Expression, Expression> merge)
    {
        // build parameter map (from parameters of second to parameters of first)
        var map = first.Parameters.Select((f, i) => new { f, s = second.Parameters[i] })
            .ToDictionary(p => p.s, p => p.f);

        // replace parameters in the second lambda expression with parameters from the first
        var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);

        // apply composition of lambda expression bodies to parameters from the first expression 
        return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
    }

    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first,
        Expression<Func<T, bool>> second)
    {
        return first.Compose(second, Expression.And);
    }

    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first,
        Expression<Func<T, bool>> second)
    {
        return first.Compose(second, Expression.Or);
    }

    public static bool IdIsOk(this string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }
        //Guid guid;
        //if (!Guid.TryParse(id, out guid))
        //{
        //    return false;
        //}
        //if (guid == new Guid())
        //{
        //    return false;
        //}
        return true;
    }

}

public class ParameterRebinder : ExpressionVisitor
{
    private readonly Dictionary<ParameterExpression, ParameterExpression> map;

    public ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
    {
        this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
    }

    public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map,
        Expression exp)
    {
        return new ParameterRebinder(map).Visit(exp);
    }

    protected override Expression VisitParameter(ParameterExpression p)
    {
        ParameterExpression replacement;
        if (map.TryGetValue(p, out replacement))
        {
            p = replacement;
        }
        return base.VisitParameter(p);
    }
}

public class ReplaceExpressionVisitor
    : ExpressionVisitor
{
    private readonly Expression _oldValue;
    private readonly Expression _newValue;

    public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
    {
        _oldValue = oldValue;
        _newValue = newValue;
    }

    public override Expression Visit(Expression node)
    {
        if (node == _oldValue)
            return _newValue;
        return base.Visit(node);
    }
}

