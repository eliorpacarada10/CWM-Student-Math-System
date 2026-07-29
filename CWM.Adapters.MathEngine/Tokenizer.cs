using System.Globalization;

namespace CWM.Adapters.MathEngine;

internal enum TokenType
{
    Number,
    Plus,
    Minus,
    Star,
    Slash,
    LeftParen,
    RightParen,
    End
}

internal readonly record struct Token(TokenType Type, string Text, decimal NumberValue = 0m);

internal static class Tokenizer
{
    public static List<Token> Tokenize(string expression)
    {
        var tokens = new List<Token>();
        var i = 0;

        while (i < expression.Length)
        {
            var c = expression[i];

            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            switch (c)
            {
                case '+':
                    tokens.Add(new Token(TokenType.Plus, "+"));
                    i++;
                    continue;
                case '-':
                    tokens.Add(new Token(TokenType.Minus, "-"));
                    i++;
                    continue;
                case '*':
                    tokens.Add(new Token(TokenType.Star, "*"));
                    i++;
                    continue;
                case '/':
                    tokens.Add(new Token(TokenType.Slash, "/"));
                    i++;
                    continue;
                case '(':
                    tokens.Add(new Token(TokenType.LeftParen, "("));
                    i++;
                    continue;
                case ')':
                    tokens.Add(new Token(TokenType.RightParen, ")"));
                    i++;
                    continue;
            }

            if (char.IsDigit(c) || c == '.')
            {
                var start = i;
                while (i < expression.Length && (char.IsDigit(expression[i]) || expression[i] == '.'))
                {
                    i++;
                }

                var text = expression[start..i];
                if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
                {
                    throw new MathEngineException($"'{text}' is not a valid number.");
                }

                tokens.Add(new Token(TokenType.Number, text, value));
                continue;
            }

            throw new MathEngineException($"Unexpected character '{c}' at position {i} in expression '{expression}'.");
        }

        tokens.Add(new Token(TokenType.End, string.Empty));
        return tokens;
    }
}
