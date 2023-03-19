using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using BattleTech;

namespace CustomComponents;

internal static class FormulaEvaluator
{
    internal static Func<MechDef, double> CompileMechDef(string expressionAsString)
    {
        try
        {
            return CompileMechDefInternal(expressionAsString);
        }
        catch (Exception e)
        {
            Log.Main.Error?.Log($"Can't compile expression: {expressionAsString}", e);
            return null;
        }
    }

    // TODO make [[Chassis.Tonnage]] a dynamic access thing, allow property and field access, and maybe even custom access
    // TODO support float and int
    private static Func<MechDef, double> CompileMechDefInternal(string expressionAsString)
    {
        var inputParameter = Expression.Parameter(typeof(MechDef), "m");
        Expression<Func<MechDef, double>> chassisTonnageFunc = m => m.Chassis.Tonnage;
        var chassisTonnageInvoke = Expression.Convert(Expression.Invoke(chassisTonnageFunc, inputParameter), typeof(double));
        var dict = new Dictionary<string, Expression>
        {
            { "[[Chassis.Tonnage]]", chassisTonnageInvoke }
        };
        return CompileWithPlaceholders<MechDef>(expressionAsString, inputParameter, dict);
    }

    private static Func<TI, double> CompileWithPlaceholders<TI>(
        string expressionAsString,
        ParameterExpression inputParameter,
        IReadOnlyDictionary<string, Expression> expressions
    )
    {
        var tokenRegex = new Regex(@"([\+\-\*\/])");
        var tokens = tokenRegex.Split(expressionAsString.Replace(" ", ""));
        var operationTokens = new Queue<string>();
        var valueExpressions = new Queue<Expression>();
        foreach (var token in tokens)
        {
            if (token is "*" or "/" or "+" or "-")
            {
                operationTokens.Enqueue(token);
            }
            else if (expressions.TryGetValue(token, out var expression))
            {
                valueExpressions.Enqueue(expression);
            }
            else
            {
                valueExpressions.Enqueue(Expression.Constant(double.Parse(token)));
            }
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
}