using System;
using System.Collections.Generic;
using System.Linq;
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
                    var expression = CreateTraverseExpression(
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

    private static Expression CreateTraverseExpression(Expression rootExpression, string expressionAsString)
    {
        var type = rootExpression.Type;
        var expression = rootExpression;

        foreach (var memberName in expressionAsString.Split('.'))
        {
            var binding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var memberInfo = type.GetMember(
                memberName,
                MemberTypes.Property|MemberTypes.Field,
                binding
            ).FirstOrDefault();

            if (memberInfo == null)
            {
                throw new ArgumentException($"Can't find field or property named {memberName} from expression {expressionAsString}");
            }

            expression = Expression.MakeMemberAccess(expression, memberInfo);
            type = expression.Type;
        }

        return expression;
    }
}