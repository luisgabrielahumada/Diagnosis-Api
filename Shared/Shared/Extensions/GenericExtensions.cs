using Microsoft.AspNetCore.Http;
using Shared.MapperModel;
using Shared.Response;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Shared.Extensions
{
    public static class GenericExtensions
    {
        public static bool IsNull(this object item)
        {
            return item == null;
        }
        public static T IsNull<T>(this T item, T result)
        {
            return item.IsNull() ? result : item;
        }
        public static bool IsNotNull(this object item)
        {
            return item != null;
        }

        public static bool IsIn<T>(this T item, IEnumerable<T> list)
        {
            return list.Contains(item);
        }
        public static bool IsIn<T>(this T item, params T[] list)
        {
            return item.IsIn(list.ToList());
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> item)
        {
            return item == null || !item.Any();
        }

        public static R Switch<T, R>(this T item, params KeyValuePair<T, R>[] list)
        {
            return Switch(item, default(R), list);
        }
        public static R Switch<T, R>(this T item, R defaultReturn, params KeyValuePair<T, R>[] list)
        {
            if (list == null || !list.Any(x => x.Key.Equals(item))) return defaultReturn;
            return list.FirstOrDefault(x => x.Key.Equals(item)).Value;
        }

        public static string GetExpressionField(this Expression expression)
        {
            // Reference type
            if (expression is MemberExpression memberExpression)
            {
                if (memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
                {
                    return GetExpressionField(memberExpression.Expression)
                           + "."
                           + memberExpression.Member.Name;
                }

                return memberExpression.Member.Name;
            }

            // Value Type
            if (expression is UnaryExpression unaryExpression)
            {
                if (unaryExpression.NodeType != ExpressionType.Convert)
                    throw new Exception(string.Format("Cannot interpret member from {0}", expression));

                return GetExpressionField(unaryExpression.Operand);
            }

            // Anonymous type
            if (expression is NewExpression newType)
            {
                return newType.Members.First().Name;
            }

            throw new Exception(string.Format("Could not determine member from {0}", expression));
        }
        public static string[] GetExpressionFields(this Expression expression)
        {
            var ret = new List<string>();

            // Lambda type
            if (expression is LambdaExpression lambdaExpression)
            {
                if (lambdaExpression.Body is NewArrayExpression arrayBody)
                {
                    foreach (var exp in arrayBody.Expressions)
                    {
                        ret.Add(GetExpressionField(exp));
                    }
                }

                if (lambdaExpression.Body is NewExpression expressionBody)
                {
                    foreach (var exp in expressionBody.Arguments)
                    {
                        ret.Add(GetExpressionField(exp));
                    }
                }
            }

            return ret.ToArray();
        }

        public static string CustomSerialization<T>(this T reference, Func<string, string, string> operation)
        {
            return CustomSerialization(reference, operation, new string[] { });
        }
        public static string CustomSerialization<T>(this T reference, Func<string, string, string> operation, Expression<Func<T, object>> expression)
        {
            return CustomSerialization(reference, operation, expression.GetExpressionFields());
        }
        public static string CustomSerialization<T>(this T reference, Func<string, string, string> operation, params string[] props)
        {
            var content = string.Empty;
            foreach (var property in reference.GetType().GetRuntimeProperties())
            {
                if (props.Length > 0 && !props.Contains(property.Name))
                    continue;

                var value = property.GetValue(reference);
                if (value is string valueString)
                    content += operation(property.Name, valueString);
            }
            return content;
        }

        public static bool HasDuplicatedValuesBubbleSort<T>(this IList<T> target)
        {
            for (var j = 0; j <= target.Count - 2; j++)
            {
                for (var i = 0; i <= target.Count - 2; i++)
                {
                    var prev = target[i];
                    var next = target[i + 1];
                    if (prev.Equals(next))
                    {
                        return true;
                    }
                    var temp = next;
                    target[i + 1] = prev;
                    target[i] = temp;
                }
            }

            return false;
        }

        public static void MapNetworkOperation<T, X>(this ServiceResponse<T> target, ServiceResponse<X> source)
        {
            target.Map(source, x => new
            {
                x.NetworkOperationStatus,
                x.NetworkOperationRequestBody,
                x.NetworkOperationRequestMethod,
                x.NetworkOperationRequestUri,
                x.NetworkOperationResponseBody,
                x.NetworkOperationResponseContentType,
                x.NetworkOperationResponseRawBody
            });
        }

        public static T DeepClone<T>(this T obj)
        {
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            var json = JsonSerializer.Serialize(obj, options);
            return JsonSerializer.Deserialize<T>(json, options)!;
        }
    }
}
