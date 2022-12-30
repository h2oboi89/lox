using Interpreter.Framework.AST;
using Interpreter.Framework.Scanning;

namespace Interpreter.Framework.Parsing;
public static class Parser
{
    public static (IEnumerable<Statement>, IEnumerable<ParseError> parseErrors) Parse(IEnumerable<Token> tokens) =>
        new TokenParser(tokens).Parse();
}

public class ParseError : Exception
{
    public readonly Token Token;

    public ParseError(Token token, string message) : base(message) { Token = token; }
}
