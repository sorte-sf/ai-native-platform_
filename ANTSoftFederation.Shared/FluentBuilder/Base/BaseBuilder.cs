namespace SorteFederation.Shared.FluentBuilder.Base;

public abstract class BaseBuilder<T> where T : class, new()
{
    protected T _entity;

    protected BaseBuilder()
    {
        _entity = new T();
    }

    protected BaseBuilder(T entity)
    {
        _entity = entity;
    }

    public T Build()
    {
        return _entity;
    }
}
