namespace Interpreter.Framework.Scanning;

class SourceScanner
{
    private readonly string source;
    private int start = 0;
    private int current = 0;
    private int line = 1;

    private static readonly Dictionary<string, TokenType> keyWords = new()
    {
        { "and",    TokenType.AND },
        { "class",  TokenType.CLASS },
        { "else",   TokenType.ELSE },
        { "false",  TokenType.FALSE },
        { "for",    TokenType.FOR },
        { "fun",    TokenType.FUN },
        { "if",     TokenType.IF },
        { "nil",    TokenType.NIL },
        { "or",     TokenType.OR },
        { "print",  TokenType.PRINT },
        { "return", TokenType.RETURN },
        { "super",  TokenType.SUPER },
        { "this",   TokenType.THIS },
        { "true",   TokenType.TRUE },
        { "var",    TokenType.VAR },
        { "while",  TokenType.WHILE },
    };

    private List<Token> Tokens { get; } = new();
    private List<ScanError> Errors { get; } = new();

    public SourceScanner(string source) { this.source = source; }

    public (IEnumerable<Token> tokens, IEnumerable<ScanError> scanErrors) Scan()
    {
        while (!IsAtEnd())
        {
            ScanToken();
        }

        Tokens.Add(new Token(TokenType.EOF, string.Empty, line, null));

        return (Tokens, Errors);
    }

    #region Tokens
    private void ScanToken()
    {
        start = current;
        var c = Advance();

        switch (c)
        {
            case '(': AddToken(TokenType.LEFT_PAREN); break;
            case ')': AddToken(TokenType.RIGHT_PAREN); break;
            case '{': AddToken(TokenType.LEFT_BRACE); break;
            case '}': AddToken(TokenType.RIGHT_BRACE); break;
            case ',': AddToken(TokenType.COMMA); break;
            case '.': AddToken(TokenType.DOT); break;
            case '-': AddToken(TokenType.MINUS); break;
            case '+': AddToken(TokenType.PLUS); break;
            case ';': AddToken(TokenType.SEMICOLON); break;
            case '*': AddToken(TokenType.STAR); break;
            case '!': AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
            case '=': AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
            case '<': AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
            case '>': AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;
            case '/': CommentOrDivision(); break;
            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace
                break;
            case '\n': line++; break;
            case '"': String(); break;
            default: Default(c); break;
        }
    }

    private void CommentOrDivision()
    {
        if (Match('/'))
        {
            while (Peek() != '\n' && !IsAtEnd()) Advance();
        }
        else
        {
            AddToken(TokenType.SLASH);
        }
    }

    private void String()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n') line++;
            Advance();
        }

        if (IsAtEnd())
        {
            Errors.Add(new(line, "Unterminated string."));
            return;
        }

        Advance(); // closing " character

        // trim the surrounding " characters
        var value = source[(start + 1)..(current - 1)];

        AddToken(TokenType.STRING, value);
    }

    private void Default(char c)
    {
        if (IsDigit(c))
        {
            Number();
        }
        else if (IsAlpha(c))
        {
            Identifier();
        }
        else
        {
            Errors.Add(new(line, $"Unexpected character: '{c}'."));
        }
    }

    private void Number()
    {
        void ConsumeNumber() { while (IsDigit(Peek())) Advance(); }

        ConsumeNumber();

        if (Peek() == '.' && IsDigit(Peek(1)))
        {
            Advance(); // consume decimal

            ConsumeNumber();
        }

        AddToken(TokenType.NUMBER, double.Parse(source[start..current]));
    }

    private void Identifier()
    {
        while (IsAlphaNumeric(Peek())) Advance();

        var text = source[start..current];

        if (keyWords.TryGetValue(text, out var keyWordTokenType))
        {
            AddToken(keyWordTokenType);
        }
        else
        {
            AddToken(TokenType.IDENTIFIER);
        }

    }
    #endregion

    #region Helper Methods
    private bool IsAtEnd(int offset = 0) => (current + offset) >= source.Length;

    private char Advance() => source[current++];

    private void AddToken(TokenType type, object? literal = null)
    {
        var text = source[start..current];
        Tokens.Add(new Token(type, text, line, literal));
    }

    private bool Match(char expected)
    {
        if (IsAtEnd()) return false;
        if (source[current] != expected) return false;

        current++;
        return true;
    }
    private char Peek(int offset = 0) => IsAtEnd(offset) ? '\0' : source[current + offset];

    private static bool IsDigit(char c) => c >= '0' && c <= '9';

    private static bool IsAlpha(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';

    private static bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);
    #endregion
}
