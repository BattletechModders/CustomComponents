using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using BattleTech;

namespace CustomComponents;

internal static class FormulaEvaluator
{
    internal static Func<MechDef, double> CompileMechDef(string expressionAsString)
    {
        try
        {
            return Compile<MechDef>(expressionAsString);
        }
        catch (Exception e)
        {
            Log.Main.Error?.Log($"Can't compile expression: {expressionAsString}", e);
            return null;
        }
    }

    // TODO support float and int
    internal static Func<TI, double> Compile<TI>(string expressionAsString)
    {
        var inputParameter = Expression.Parameter(typeof(TI), "input");

        var propertyTraverseRegex = new Regex(@"\[\[([^\]]+)\]\]");
        var tokenRegex = new Regex(@"([\+\-\*\/])");
        var tokens = tokenRegex.Split(expressionAsString.Replace(" ", ""));
        var operationTokens = new Queue<string>();
        var valueExpressions = new Queue<Expression>();
        foreach (var token in tokens)
        {
            if (token is "*" or "/" or "+" or "-")
            {
                operationTokens.Enqueue(token);
                continue;
            }

            {
                var match = propertyTraverseRegex.Match(token);
                if (match.Success)
                {
                    var expression = CreatePropertyTraverseExpression(
                        inputParameter,
                        match.Groups[1].Captures[0].Value
                    );
                    valueExpressions.Enqueue(Expression.Convert(expression, typeof(double)));
                    continue;
                }
            }

            valueExpressions.Enqueue(Expression.Constant(double.Parse(token)));
        }

        var lastExpression = valueExpressions.Dequeue();
        foreach (var token in operationTokens)
        {
            lastExpression = token switch
            {
                "*" => Expression.Multiply(lastExpression, valueExpressions.Dequeue()),
                "/" => Expression.Divide(lastExpression, valueExpressions.Dequeue()),
                "+" => Expression.Add(lastExpression, valueExpressions.Dequeue()),
                "-" => Expression.Subtract(lastExpression, valueExpressions.Dequeue()),
                _ => throw new InvalidOperationException()
            };
        }
        return Expression.Lambda<Func<TI, double>>(lastExpression, inputParameter).Compile();
    }

    private static Expression CreatePropertyTraverseExpression(Expression rootExpression, string expressionAsString)
    {
        var type = rootExpression.Type;
        var expression = rootExpression;

        foreach (var property in expressionAsString.Split('.'))
        {
            var propertyInfo = type.GetProperty(property, BindingFlags.GetProperty|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);

            if (propertyInfo == null)
            {
                throw new ArgumentException($"Can't find property named {property} from expression {expressionAsString}");
            }

            type = propertyInfo.PropertyType;
            expression = Expression.Property(expression, propertyInfo);
        }

        return expression;
    }
}