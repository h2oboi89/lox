using Interpreter.Framework.Scanning;

namespace Interpreter.Framework.Parsing;

public class ParseError : Exception
{
    public readonly Token Token;

    public ParseError(Token token, string message) : base(message) { Token = token; }
}
