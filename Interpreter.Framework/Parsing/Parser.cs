using Interpreter.Framework.AST;
using Interpreter.Framework.Scanning;

namespace Interpreter.Framework.Parsing;
public static class Parser
{
    public static (Expression? expression, IEnumerable<ParseError> parseErrors) Parse(IEnumerable<Token> tokens) =>
        new TokenParser(tokens).Parse();
}

public record ParseError(Token Token, string Message);

internal class ParsingException : Exception { }
