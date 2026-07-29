using CWM.Adapters.MathEngine.Ast;

namespace CWM.Adapters.MathEngine;

/// <summary>
/// Recursive-descent parser respecting standard operator precedence (* and / bind tighter
/// than + and -), left-associative within the same precedence level. This is the textbook-
/// correct behavior for an arithmetic expression -- the assignment's "type and order can be
/// different" hint reads as "expressions vary in shape," not "ignore precedence."
///
/// Grammar:
///   Expression := Term (('+' | '-') Term)*
///   Term       := Factor (('*' | '/') Factor)*
///   Factor     := ('-' | '+') Factor | Primary
///   Primary    := Number | '(' Expression ')'
/// </summary>
internal sealed class Parser
{
    private readonly List<Token> _tokens;
    private int _position;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
        _position = 0;
    }

    public ExpressionNode ParseExpression()
    {
        var node = ParseExpressionInternal();
        Expect(TokenType.End);
        return node;
    }

    private ExpressionNode ParseExpressionInternal()
    {
        var left = ParseTerm();

        while (Current.Type is TokenType.Plus or TokenType.Minus)
        {
            var op = Current.Type == TokenType.Plus ? '+' : '-';
            Advance();
            var right = ParseTerm();
            left = new BinaryOperationNode(left, op, right);
        }

        return left;
    }

    private ExpressionNode ParseTerm()
    {
        var left = ParseFactor();

        while (Current.Type is TokenType.Star or TokenType.Slash)
        {
            var op = Current.Type == TokenType.Star ? '*' : '/';
            Advance();
            var right = ParseFactor();
            left = new BinaryOperationNode(left, op, right);
        }

        return left;
    }

    private ExpressionNode ParseFactor()
    {
        if (Current.Type == TokenType.Minus)
        {
            Advance();
            return new UnaryMinusNode(ParseFactor());
        }

        if (Current.Type == TokenType.Plus)
        {
            Advance();
            return ParseFactor();
        }

        return ParsePrimary();
    }

    private ExpressionNode ParsePrimary()
    {
        if (Current.Type == TokenType.Number)
        {
            var value = Current.NumberValue;
            Advance();
            return new NumberNode(value);
        }

        if (Current.Type == TokenType.LeftParen)
        {
            Advance();
            var node = ParseExpressionInternal();
            Expect(TokenType.RightParen);
            Advance();
            return node;
        }

        throw new MathEngineException($"Unexpected token '{Current.Text}' while parsing expression.");
    }

    private Token Current => _tokens[_position];

    private void Advance() => _position++;

    private void Expect(TokenType type)
    {
        if (Current.Type != type)
        {
            throw new MathEngineException($"Expected '{type}' but found '{Current.Text}'.");
        }
    }
}
