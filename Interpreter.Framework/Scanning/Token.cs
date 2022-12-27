﻿namespace Interpreter.Framework.Scanning;

public class Token
{
    public readonly TokenType Type;
    public readonly string Lexeme;
    public readonly object? Literal;
    public readonly int Line;

    public Token(TokenType type, string lexeme, int line, object? literal = null)
    {
        Type = type;
        Lexeme = lexeme;
        Literal = literal;
        Line = line;
    }

    public override string ToString() => $"{Line} {Type} {Lexeme} {Literal}";
}
