namespace Interpreter.Framework.Scanning
{
    public class Token
    {
        private const int DEFAULT_LINE = -1;

        public readonly TokenType Type;
        public readonly string Lexeme;
        public readonly object? Literal;
        public readonly int Line;

        public Token(TokenType type, string lexeme, object? literal = null, int line = DEFAULT_LINE)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            Line = line;
        }

        public override string ToString() => $"{Type} {Lexeme} {Literal}";
    }
}
