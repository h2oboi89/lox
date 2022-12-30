using Interpreter.Framework.AST;
using Interpreter.Framework.Scanning;

namespace Interpreter.Framework.Parsing;
internal class TokenParser
{
    private readonly Token[] tokens;
    private int current = 0;

    private List<ParseError> Errors { get; } = new();

    public TokenParser(IEnumerable<Token> tokens)
    {
        this.tokens = tokens.ToArray();
    }

    #region Grammar
    public (Expression? expression, IEnumerable<ParseError> errors) Parse()
    {
        Expression? expression = null;

        try
        {
            expression = Expression();
        }
        catch (ParsingException) { }

        return (expression, Errors);
    }

    private Expression Expression() => Equality();

    private Expression Equality()
    {
        var expression = Comparison();

        while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
        {
            var op = Previous();
            var right = Comparison();
            expression = new BinaryExpression(expression, op, right);
        }

        return expression;
    }

    private Expression Comparison()
    {
        var expression = Term();

        while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
        {
            var op = Previous();
            var right = Term();
            expression = new BinaryExpression(expression, op, right);
        }

        return expression;
    }

    private Expression Term()
    {
        var expression = Factor();

        while (Match(TokenType.MINUS, TokenType.PLUS))
        {
            var op = Previous();
            var right = Factor();
            expression = new BinaryExpression(expression, op, right);
        }

        return expression;
    }

    private Expression Factor()
    {
        var expression = Unary();

        while (Match(TokenType.SLASH, TokenType.STAR))
        {
            var op = Previous();
            var right = Unary();
            expression = new BinaryExpression(expression, op, right);
        }

        return expression;
    }

    private Expression Unary()
    {
        if (Match(TokenType.BANG, TokenType.MINUS))
        {
            var op = Previous();
            var right = Unary();
            return new UnaryExpression(op, right);
        }

        return Primary();
    }

    private Expression Primary()
    {
        if (Match(TokenType.FALSE)) return new LiteralExpression(false);
        if (Match(TokenType.TRUE)) return new LiteralExpression(true);
        if (Match(TokenType.NIL)) return new LiteralExpression(null);

        if (Match(TokenType.NUMBER, TokenType.STRING)) return new LiteralExpression(Previous().Literal);

        if (Match(TokenType.LEFT_PAREN))
        {
            var expression = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
            return new GroupingExpression(expression);
        }

        throw Error(Peek(), "Expect expression.");
    }
    #endregion

    #region Helper Methods
    private bool Match(params TokenType[] tokenTypes)
    {
        foreach (var tokenType in tokenTypes)
        {
            if (Check(tokenType))
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    private bool Check(TokenType tokenType)
    {
        if (IsAtEnd()) return false;

        return Peek().Type == tokenType;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) current++;

        return Previous();
    }

    private bool IsAtEnd() => tokens[current].Type == TokenType.EOF;

    private Token Peek() => tokens[current];

    private Token Previous() => tokens[current - 1];

    private Token Consume(TokenType expected, string errorMessage)
    {
        if (Check(expected)) return Advance();

        throw Error(Peek(), errorMessage);
    }

    private ParsingException Error(Token token, string errorMessage)
    {
        Errors.Add(new ParseError(token, errorMessage));
        return new ParsingException();
    }

    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd())
        {
            if (Previous().Type == TokenType.SEMICOLON) return;

            switch (Peek().Type)
            {
                case TokenType.CLASS:
                case TokenType.FUN:
                case TokenType.VAR:
                case TokenType.FOR:
                case TokenType.IF:
                case TokenType.WHILE:
                case TokenType.PRINT:
                case TokenType.RETURN:
                    return;
            }

            Advance();
        }
    }
    #endregion
}
