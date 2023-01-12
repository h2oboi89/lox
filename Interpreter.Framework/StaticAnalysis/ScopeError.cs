using Interpreter.Framework.Scanning;

namespace Interpreter.Framework.StaticAnalysis;
[Serializable]
public class ScopeError : Exception
{
    public readonly Token Token;

    public ScopeError(Token token, string message) : base(message) { Token = token; }
}