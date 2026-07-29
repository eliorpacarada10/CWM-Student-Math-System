using CWM.Application.Exceptions;
using CWM.Application.Ports.Driven;

namespace CWM.Adapters.MathEngine;

/// <summary>
/// The one public type in this project. Everything else (Tokenizer, Parser, Evaluator, the
/// AST, MathEngineException) is internal -- this adapter is the entire surface Application
/// (via DI) ever touches.
/// </summary>
public sealed class ArithmeticEvaluatorAdapter : IArithmeticEvaluator
{
    public decimal Evaluate(string expression)
    {
        try
        {
            var tokens = Tokenizer.Tokenize(expression);
            var parser = new Parser(tokens);
            var ast = parser.ParseExpression();
            return Evaluator.Evaluate(ast);
        }
        catch (MathEngineException ex)
        {
            // Translate our own internal exception into the port's contract exception --
            // Application should never need to know MathEngineException exists.
            throw new ArithmeticEvaluationException(expression, ex.Message, ex);
        }
    }
}
