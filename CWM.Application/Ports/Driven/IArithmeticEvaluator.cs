namespace CWM.Application.Ports.Driven;

/// <summary>
/// Requirement: "independent processor for mathematical operations". Application depends only
/// on this interface -- CWM.Adapters.MathEngine implements it and could be swapped for a
/// different evaluator, or lifted out as a standalone package, without Application changing.
/// Implementations must throw CWM.Application.Exceptions.ArithmeticEvaluationException (not
/// their own internal exception type) on malformed input or invalid operations.
/// </summary>
public interface IArithmeticEvaluator
{
    decimal Evaluate(string expression);
}
