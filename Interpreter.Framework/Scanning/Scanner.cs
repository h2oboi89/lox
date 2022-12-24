namespace Interpreter.Framework.Scanning
{
    public static class Scanner
    {
        public static (IEnumerable<Token> tokens, IEnumerable<ScanError> errors) ScanTokens(string source)
        {
            var scanState = new ScanState(source);

            while (!scanState.IsAtEnd())
            {
                scanState.ScanToken();
            }

            return (scanState.Tokens, scanState.Errors);
        }
    }
}
