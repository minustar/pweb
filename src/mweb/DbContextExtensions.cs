namespace Minustar.Website;

public static class DbContextExtensions
{
    public static PropertyBuilder<TProperty> PropertyColumnName<TEntity, TProperty>(this EntityTypeBuilder<TEntity> entityType, Expression<Func<TEntity, TProperty>> propertyExpression, string columnName)
        where TEntity : class
    {        
        var property = entityType.Property(propertyExpression).HasColumnName(columnName);
        return property;
    }
}
