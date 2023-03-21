using CustomComponents;

namespace CustomComponentsTests;

public class FormulaEvaluatorTests
{
    [TestCase("2", 2)]
    [TestCase("2 + 5", 7)]
    [TestCase("2 + 5 * 10", 70)]
    [TestCase("2 + 5 * 10 / 2", 35)]
    [TestCase("2 + 5 * 10 / 2 - 15", 20)]
    [TestCase("[[PropertyA]]", 1)]
    [TestCase("[[PropertyA]] * 2", 2)]
    [TestCase("3 * [[PropertyA]]", 3)]
    [TestCase("3 * [[PropertyA]] * 2", 6)]
    [TestCase("3 * [[Property1.PropertyB]] * 2", 60)]
    public void Compile(string expressionAsString, double result)
    {
        var func = FormulaEvaluator.Compile<ClassTestData>(expressionAsString);
        Assert.That(func(new()), Is.EqualTo(result));
    }

    internal class ClassTestData
    {
        internal int PropertyA => 1;

        internal MyNestedClass Property1 { get; } = new();

        internal class MyNestedClass
        {
            internal int PropertyB => 10;
        }
    }
}