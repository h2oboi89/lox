namespace Interpreter.Framework.Scanning
{
    public static class Scanner
    {
        public static (IEnumerable<Token> tokens, IEnumerable<ScanError> errors) ScanTokens(string source) =>
            new SourceScanner(source).Scan();
    }

    public readonly record struct Token(TokenType Type, string Lexeme, int Line, object? Literal = null)
    {
        public override string ToString() => $"{Line} {Type} {Lexeme} {Literal}";
    }

    public record ScanError(int Line, string Message);

    public enum TokenType
    {
        // Single-character tokens.
        LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
        COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR,

        // One or two character tokens.
        BANG, BANG_EQUAL,
        EQUAL, EQUAL_EQUAL,
        GREATER, GREATER_EQUAL,
        LESS, LESS_EQUAL,

        // Literals.
        IDENTIFIER, STRING, NUMBER,

        // Keywords.
        AND, CLASS, ELSE, FALSE, FUN, FOR, IF, NIL, OR,
        PRINT, RETURN, SUPER, THIS, TRUE, VAR, WHILE,

        EOF
    }
}
