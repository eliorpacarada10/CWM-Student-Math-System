namespace CWM.Adapters.MathEngine.Ast;

internal abstract class ExpressionNode
{
}

internal sealed class NumberNode : ExpressionNode
{
    public decimal Value { get; }

    public NumberNode(decimal value)
    {
        Value = value;
    }
}

internal sealed class UnaryMinusNode : ExpressionNode
{
    public ExpressionNode Operand { get; }

    public UnaryMinusNode(ExpressionNode operand)
    {
        Operand = operand;
    }
}

internal sealed class BinaryOperationNode : ExpressionNode
{
    public ExpressionNode Left { get; }
    public char Operator { get; }
    public ExpressionNode Right { get; }

    public BinaryOperationNode(ExpressionNode left, char @operator, ExpressionNode right)
    {
        Left = left;
        Operator = @operator;
        Right = right;
    }
}
