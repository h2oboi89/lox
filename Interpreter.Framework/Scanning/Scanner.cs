namespace Interpreter.Framework.Scanning;

public static class Scanner
{
    public static (IEnumerable<Token> tokens, IEnumerable<ScanError> scanErrors) ScanTokens(string source) =>
        new SourceScanner(source).Scan();
}
