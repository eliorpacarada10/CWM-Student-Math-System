using CWM.Adapters.MathEngine;
using CWM.Application.Exceptions;
using Xunit;

namespace CWM.Tests.UnitTests.MathEngine;

/// <summary>
/// Tests only the public ArithmeticEvaluatorAdapter -- Tokenizer/Parser/Evaluator/AST are
/// internal implementation details, exactly as intended (this project's only public surface
/// is the adapter that implements Application's port).
/// </summary>
public class ArithmeticEvaluatorAdapterTests
{
    private readonly ArithmeticEvaluatorAdapter _evaluator = new();

    [Theory]
    [InlineData("2+2", 4)]
    [InlineData("2+3*4", 14)] // precedence: * before +
    [InlineData("6*2+3-4", 11)]
    [InlineData("2+3/6-4", -1.5)]
    [InlineData("(2+3)*4", 20)] // parentheses override precedence
    [InlineData("-5+3", -2)] // unary minus
    [InlineData("-(2+3)", -5)]
    public void Evaluate_respects_standard_operator_precedence(string expression, decimal expected)
    {
        Assert.Equal(expected, _evaluator.Evaluate(expression));
    }

    [Fact]
    public void Evaluate_throws_ArithmeticEvaluationException_on_division_by_zero()
    {
        Assert.Throws<ArithmeticEvaluationException>(() => _evaluator.Evaluate("1/0"));
    }

    [Fact]
    public void Evaluate_throws_ArithmeticEvaluationException_on_malformed_expression()
    {
        Assert.Throws<ArithmeticEvaluationException>(() => _evaluator.Evaluate("2+*4"));
    }

    [Fact]
    public void Evaluate_throws_ArithmeticEvaluationException_on_unbalanced_parentheses()
    {
        Assert.Throws<ArithmeticEvaluationException>(() => _evaluator.Evaluate("(2+3"));
    }
}
