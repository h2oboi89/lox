using Interpreter.Framework.AST;
using Interpreter.Framework.Scanning;

namespace Interpreter.Framework.Parsing;
internal class TokenParser
{
    private readonly Token[] tokens;
    private int current = 0;

    private const int ARG_LIMIT = 255;

    private List<ParseError> Errors { get; } = new();

    public TokenParser(IEnumerable<Token> tokens)
    {
        this.tokens = tokens.ToArray();
    }

    #region Grammar
    public (IEnumerable<Statement> expression, IEnumerable<ParseError> parseErrors) Parse()
    {
        List<Statement> statements = new();

        while (!IsAtEnd())
        {
            var statement = Declaration();

            if (statement != null)
            {
                statements.Add(statement);
            }
        }

        return (statements, Errors);
    }

    private Statement? Declaration()
    {
        try
        {
            if (Match(TokenType.FUN)) return FunctionStatement("function");

            if (Match(TokenType.VAR)) return VariableDeclaration();

            return Statement();
        }
        catch (ParseError error)
        {
            Errors.Add(error);
            Synchronize();
            return null;
        }
    }

    private Statement FunctionStatement(string kind)
    {
        var name = Consume(TokenType.IDENTIFIER, $"Expect {kind} name.");

        ConsumeCharacterAfter(TokenType.LEFT_PAREN, '(', $"{kind} name");

        var parameters = new List<Token>();

        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                parameters.Add(Consume(TokenType.IDENTIFIER, "Expect parameter name."));
            }
            while (Match(TokenType.COMMA));
        }

        var paren = ConsumeCharacterAfter(TokenType.RIGHT_PAREN, ')', "parameters");

        if (parameters.Count > ARG_LIMIT)
        {
            Errors.Add(new ParseError(paren, $"Can't have more than {ARG_LIMIT} parameters."));
        }

        ConsumeCharacterBefore(TokenType.LEFT_BRACE, '{', $"{kind} body");

        var body = Block();

        return new FunctionStatement(name, parameters, body);
    }

    private Statement VariableDeclaration()
    {
        var name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

        Expression initializer = new LiteralExpression(null);
        if (Match(TokenType.EQUAL))
        {
            initializer = Expression();
        }

        ConsumeSemicolon("variable declaration");

        return new VariableStatement(name, initializer);
    }

    private Statement Statement()
    {
        if (Match(TokenType.FOR)) return ForStatement();

        if (Match(TokenType.IF)) return IfStatement();

        if (Match(TokenType.PRINT)) return PrintStatement();

        if (Match(TokenType.RETURN)) return ReturnStatement();

        if (Match(TokenType.WHILE)) return WhileStatement();

        if (Match(TokenType.LEFT_BRACE)) return new BlockStatement(Block());

        return ExpressionStatement();
    }

    private Statement ForStatement()
    {
        ConsumeCharacterAfter(TokenType.LEFT_PAREN, '(', "'for'");

        Statement? initializer;

        if (Match(TokenType.SEMICOLON))
        {
            initializer = null;
        }
        else if (Match(TokenType.VAR))
        {
            initializer = VariableDeclaration();
        }
        else
        {
            initializer = ExpressionStatement();
        }

        Expression? condition = null;

        if (!Check(TokenType.SEMICOLON))
        {
            condition = Expression();
        }

        ConsumeSemicolon("loop condition");

        Expression? increment = null;

        if (!Check(TokenType.RIGHT_PAREN))
        {
            increment = Expression();
        }

        ConsumeCharacterAfter(TokenType.RIGHT_PAREN, ')', "for clauses");

        var body = Statement();

        if (increment != null)
        {
            body = new BlockStatement(new List<Statement> { body, new ExpressionStatement(increment) });
        }

        if (condition == null) condition = new LiteralExpression(true);

        body = new WhileStatement(condition, body);

        if (initializer != null)
        {
            body = new BlockStatement(new List<Statement> { initializer, body });
        }

        return body;
    }

    private Statement IfStatement()
    {
        ConsumeCharacterAfter(TokenType.LEFT_PAREN, '(', "'if'");

        var condition = Expression();

        ConsumeCharacterAfter(TokenType.RIGHT_PAREN, ')', "if condition");

        var thenBranch = Statement();

        var elseBranch = Match(TokenType.ELSE) ? Statement() : null;

        return new IfStatement(condition, thenBranch, elseBranch);
    }

    private Statement PrintStatement()
    {
        var value = Expression();

        ConsumeSemicolon("value");

        return new PrintStatement(value);
    }

    private Statement ReturnStatement()
    {
        var keyword = Previous();

        Expression value = new LiteralExpression(null);

        if (!Check(TokenType.SEMICOLON))
        {
            value = Expression();
        }

        ConsumeSemicolon("return value");

        return new ReturnStatement(keyword, value);
    }

    private Statement WhileStatement()
    {
        ConsumeCharacterAfter(TokenType.LEFT_PAREN, '(', "'while'");

        var condition = Expression();

        ConsumeCharacterAfter(TokenType.RIGHT_PAREN, ')', "condition");

        var body = Statement();

        return new WhileStatement(condition, body);
    }

    private List<Statement> Block()
    {
        var statements = new List<Statement>();

        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
        {
            var statement = Declaration();

            if (statement != null)
            {
                statements.Add(statement);
            }
        }

        ConsumeCharacterAfter(TokenType.RIGHT_BRACE, '}', "block");

        return statements;
    }

    private Statement ExpressionStatement()
    {
        var expression = Expression();

        ConsumeSemicolon("expression");

        return new ExpressionStatement(expression);
    }

    private Expression Expression() => Assignment();

    private Expression Assignment()
    {
        var expression = Or();

        if (Match(TokenType.EQUAL))
        {
            var equals = Previous();
            var value = Assignment();

            if (expression is VariableExpression variableExpression)
            {
                return new AssignmentExpression(variableExpression.Name, value);
            }

            throw new ParseError(equals, "Invalid assignment target.");
        }

        return expression;
    }

    private Expression Or()
    {
        var expression = And();

        while (Match(TokenType.OR))
        {
            var op = Previous();
            var right = And();
            expression = new LogicalExpression(expression, op, right);
        }

        return expression;
    }

    private Expression And()
    {
        var expression = Equality();

        while (Match(TokenType.AND))
        {
            var op = Previous();
            var right = Equality();
            expression = new LogicalExpression(expression, op, right);
        }

        return expression;
    }

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

        return Call();
    }

    private Expression Call()
    {
        var expression = Primary();

        while (true)
        {
            if (Match(TokenType.LEFT_PAREN))
            {
                expression = FinishCall(expression);
            }
            else
            {
                break;
            }
        }

        return expression;
    }

    private Expression FinishCall(Expression callee)
    {
        var arguments = new List<Expression>();

        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                arguments.Add(Expression());
            }
            while (Match(TokenType.COMMA));
        }

        var paren = ConsumeCharacterAfter(TokenType.RIGHT_PAREN, ')', "arguments");

        if (arguments.Count > ARG_LIMIT)
        {
            Errors.Add(new ParseError(paren, $"Can't have more than {ARG_LIMIT} arguments."));
        }

        return new CallExpression(callee, paren, arguments);
    }

    private Expression Primary()
    {
        if (Match(TokenType.FALSE)) return new LiteralExpression(false);
        if (Match(TokenType.TRUE)) return new LiteralExpression(true);
        if (Match(TokenType.NIL)) return new LiteralExpression(null);

        if (Match(TokenType.NUMBER, TokenType.STRING)) return new LiteralExpression(Previous().Literal);

        if (Match(TokenType.IDENTIFIER)) return new VariableExpression(Previous());

        if (Match(TokenType.LEFT_PAREN))
        {
            var expression = Expression();
            ConsumeCharacterAfter(TokenType.RIGHT_PAREN, ')', "expression");
            return new GroupingExpression(expression);
        }

        throw new ParseError(Peek(), "Expect expression.");
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

        throw new ParseError(Peek(), errorMessage);
    }

    private Token ConsumeCharacterBefore(TokenType token, char expected, string previous) => ConsumeCharacter(token, expected, "before", previous);
    private Token ConsumeCharacterAfter(TokenType token, char expected, string previous) => ConsumeCharacter(token, expected, "after", previous);
    private Token ConsumeCharacter(TokenType token, char expected, string where, string previous) => Consume(token, $"Expect '{expected}' {where} {previous}.");

    private void ConsumeSemicolon(string previous) => ConsumeCharacterAfter(TokenType.SEMICOLON, ';', previous);

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
