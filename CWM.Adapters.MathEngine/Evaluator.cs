using CWM.Adapters.MathEngine.Ast;

namespace CWM.Adapters.MathEngine;

internal static class Evaluator
{
    public static decimal Evaluate(ExpressionNode node)
    {
        switch (node)
        {
            case NumberNode number:
                return number.Value;

            case UnaryMinusNode unary:
                return -Evaluate(unary.Operand);

            case BinaryOperationNode binary:
                var left = Evaluate(binary.Left);
                var right = Evaluate(binary.Right);

                return binary.Operator switch
                {
                    '+' => left + right,
                    '-' => left - right,
                    '*' => left * right,
                    '/' => right == 0
                        ? throw new MathEngineException("Division by zero.")
                        : left / right,
                    _ => throw new MathEngineException($"Unknown operator '{binary.Operator}'.")
                };

            default:
                throw new MathEngineException("Unknown expression node.");
        }
    }
}
