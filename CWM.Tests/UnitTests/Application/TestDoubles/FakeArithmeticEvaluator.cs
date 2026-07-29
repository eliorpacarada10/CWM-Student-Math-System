using CWM.Application.Exceptions;
using CWM.Application.Ports.Driven;

namespace CWM.Tests.UnitTests.Application.TestDoubles;

/// <summary>
/// Lets a test control exactly which expressions succeed and which fail, without depending
/// on the real MathEngine adapter -- keeps GradeExamBatchUseCase tests focused purely on
/// orchestration behavior (e.g. "one bad task doesn't fail the batch").
/// </summary>
public sealed class FakeArithmeticEvaluator : IArithmeticEvaluator
{
    private readonly Dictionary<string, decimal> _results = new();
    private readonly HashSet<string> _failingExpressions = new();

    public FakeArithmeticEvaluator WithResult(string expression, decimal result)
    {
        _results[expression] = result;
        return this;
    }

    public FakeArithmeticEvaluator WithFailure(string expression)
    {
        _failingExpressions.Add(expression);
        return this;
    }

    public decimal Evaluate(string expression)
    {
        if (_failingExpressions.Contains(expression))
        {
            throw new ArithmeticEvaluationException(expression, "simulated failure", new InvalidOperationException());
        }

        return _results.TryGetValue(expression, out var result)
            ? result
            : throw new InvalidOperationException($"No stubbed result for expression '{expression}'.");
    }
}
