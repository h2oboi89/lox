namespace Interpreter.Framework.Scanning
{
    public class ScanError
    {
        public readonly int Line;
        public readonly string Message;

        public ScanError(int line, string message) { Line = line; Message = message; }
    }
}
