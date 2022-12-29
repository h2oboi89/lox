using Interpreter.Framework.Scanning;

namespace Interpreter.Framework.Evaluating;

public class LoxRuntimeError : Exception
{
    public readonly Token Token;

    public LoxRuntimeError(Token token, string message) : base(message) { Token = token; }
}