namespace Interpreter.Framework.Scanning;

public readonly record struct Token(TokenType Type, string Lexeme, int Line, object? Literal = null)
{
    public override string ToString() => $"{Line} {Type} {Lexeme} {Literal}";
}
