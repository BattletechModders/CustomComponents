using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace CustomComponents;

public class FormulaEvaluator
{
    public static FormulaEvaluator Shared = new();

    private readonly DataTable table;
    private FormulaEvaluator()
    {
        table =  new();
        table.Columns.Add("column", typeof(double));
        table.Rows.Add(1.0);
    }

    private object Compute(string expr)
    {
        var value = table.Compute(expr, null);
        return value != DBNull.Value ? value : null;
    }

    private static readonly Regex Regex = new(@"(?:\[\[([^\]]+)\]\])", RegexOptions.Singleline | RegexOptions.Compiled);

    public object Evaluate(string expression, Dictionary<string, string> variables = null)
    {
        var originalExpression = expression;
        if (variables != null)
        {
            var keys = new HashSet<string>();
            foreach (Match match in Regex.Matches(expression))
            {
                var key = match.Groups[1].Value;
                keys.Add(key);
            }

            foreach (var key in keys)
            {
                if (!variables.TryGetValue(key, out var value))
                {
                    value = "1"; // avoids division by zero issues
                }
                var placeholder = "[[" + key + "]]";
                expression = expression.Replace(placeholder, value);
            }
        }

        try
        {
            return Compute(expression);
        }
        catch (Exception)
        {
            var log = "Could not process expression=" + expression;
            if (expression != originalExpression)
            {
                log += " originalExpression=" + originalExpression;
            }
            if (variables != null)
            {
                log += " variables=" + string.Join(",", variables);
            }
            Log.Main.Error?.Log(log);
            throw;
        }
    }
}