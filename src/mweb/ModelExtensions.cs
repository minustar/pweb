namespace Minustar.Website;

public static class ModelExtensions
{
    public static string? DescriptionFor<TModel, TResult>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TResult>> expression)
    {
        if (!TryGetMemberFromExpression(expression, out MemberInfo? member))
            throw new InvalidOperationException("No member could be loacated.");

        var attrib = member.GetCustomAttribute<DescriptionAttribute>();
        if (attrib is not null)
            return html.Encode(attrib.Description);

        return null;
    }

    private static bool TryGetMemberFromExpression<TModel, TResult>(Expression<Func<TModel, TResult>> expression, [NotNullWhen(true)] out MemberInfo? member)
    {
        member = null;

        if (expression.Body is MemberExpression memberExpr)
        {
            member = memberExpr.Member;
            return true;
        }

        return false;
    }
}
