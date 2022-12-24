using Interpreter.Framework.Scanning;

namespace Interpreter.Framework
{
    public static class Interpreter
    {
        public static void Run(string source)
        {
            var (tokens, errors) = Scanner.ScanTokens(source);

            if (errors.Any()) 
            {
                foreach(var error in errors)
                {
                    Report(error.Line, string.Empty, error.Message);
                }
                return;
            }
            
            foreach(var token in tokens)
            {
                Out?.Invoke(typeof(Interpreter), new InterpreterEventArgs(token.ToString()));
            }
        }

        private static void Report(int line, string where, string message)
        {
            Error?.Invoke(typeof(Interpreter), new InterpreterEventArgs($"[line {line}] Error{where}: {message}"));
        }

        public static event EventHandler<InterpreterEventArgs>? Out;

        public static event EventHandler<InterpreterEventArgs>? Error;
    }
}
