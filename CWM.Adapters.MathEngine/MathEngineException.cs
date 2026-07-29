namespace CWM.Adapters.MathEngine;

/// <summary>
/// Raised for malformed expressions or invalid operations (e.g. divide by zero). Kept
/// entirely internal to this project -- MathEngine has zero dependency on Application, so it
/// could be lifted out as a standalone package. ArithmeticEvaluatorAdapter translates this
/// into Application's ArithmeticEvaluationException at the port boundary.
/// </summary>
internal sealed class MathEngineException : Exception
{
    public MathEngineException(string message) : base(message)
    {
    }
}
