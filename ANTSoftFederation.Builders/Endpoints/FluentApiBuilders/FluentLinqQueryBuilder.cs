
using System.Linq.Expressions;

namespace SorteFederation.Builders.Endpoints.FluentApiBuilders;

public class FluentLinqQueryBuilder<T> where T : new()
{
    private T _entity;
    private Dictionary<string, object> _propertyValues;
    private List<Expression<Func<T, bool>>> _whereClauses;
    private List<(LambdaExpression, bool)> _orderByClauses;

    public FluentLinqQueryBuilder()
    {
        _entity = new T();
        _propertyValues = new Dictionary<string, object>();
        _whereClauses = new List<Expression<Func<T, bool>>>();
        _orderByClauses = new List<(LambdaExpression, bool)>();
    }

    public FluentLinqQueryBuilder(T objectItem)
    {
        _entity = objectItem;
        _propertyValues = new Dictionary<string, object>();
        _whereClauses = new List<Expression<Func<T, bool>>>();
        _orderByClauses = new List<(LambdaExpression, bool)>();
    }

    public FluentLinqQueryBuilder<T> SetProperty(string propertyName, object value)
    {
        _propertyValues[propertyName] = value;
        return this;
    }

    public FluentLinqQueryBuilder<T> Where(Expression<Func<T, bool>> predicate)
    {
        _whereClauses.Add(predicate);
        return this;
    }

    public FluentLinqQueryBuilder<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        _orderByClauses.Add((keySelector, true));
        return this;
    }

    public FluentLinqQueryBuilder<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        _orderByClauses.Add((keySelector, false));
        return this;
    }

    public T Build()
    {
        ApplyProperties();
        return _entity;
    }

    private void ApplyProperties()
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (_propertyValues.TryGetValue(property.Name, out var value))
            {
                if (value != null && property.CanWrite)
                {
                    var targetType = property.PropertyType;
                    var convertedValue = Convert.ChangeType(value, targetType);
                    property.SetValue(_entity, convertedValue);
                }
            }
        }
    }

    public IEnumerable<T> ApplyFiltersAndSorting(IEnumerable<T> source)
    { 
        foreach (var whereClause in _whereClauses)
        {
            source = source.Where(whereClause.Compile());
        }
         
        IOrderedEnumerable<T> orderedSource = null;
        foreach (var (orderByClause, isOrderBy) in _orderByClauses)
        {
            var lambda = (LambdaExpression)orderByClause;
            var keySelector = lambda.Compile();

            if (isOrderBy)
            {
                orderedSource = orderedSource == null
                    ? source.OrderBy((Func<T, object>)keySelector)
                    : orderedSource.ThenBy((Func<T, object>)keySelector);
            }
            else
            {
                orderedSource = orderedSource == null
                    ? source.OrderByDescending((Func<T, object>)keySelector)
                    : orderedSource.ThenByDescending((Func<T, object>)keySelector);
            }
        }

        return orderedSource ?? source;
    }

    public FluentLinqQueryBuilder<T> Update(T objectItem)
    {
        _entity = objectItem;
        return this;
    }
}
