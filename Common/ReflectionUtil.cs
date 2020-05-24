using System;
using System.Linq.Expressions;

namespace Common
{
    public static class ReflectionUtil
    {
        public static string GetPropertyName<T>(Expression<Func<T>> expression)
        {
            MemberExpression body = (MemberExpression)expression.Body;
            return body.Member.Name;
        }
    }
}
