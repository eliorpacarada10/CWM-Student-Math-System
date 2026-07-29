namespace CWM.Application.Exceptions;

/// <summary>
/// The IArithmeticEvaluator port's failure contract. CWM.Adapters.MathEngine has its own
/// internal MathEngineException (kept independent so MathEngine has zero dependency on
/// Application), and translates it into this type at the port boundary -- Application never
/// needs to know MathEngineException exists.
/// </summary>
public sealed class ArithmeticEvaluationException : Exception
{
    public string Expression { get; }

    public ArithmeticEvaluationException(string expression, string reason, Exception innerException)
        : base($"Could not evaluate '{expression}': {reason}", innerException)
    {
        Expression = expression;
    }
}
