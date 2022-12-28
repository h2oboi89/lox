using Interpreter.Framework.AST;
using Interpreter.Framework.Parsing;
using Interpreter.Framework.Scanning;

namespace Interpreter.Framework;

public static class Interpreter
{
    private static readonly Printer printer = new Printer();

    public static void Run(string source)
    {
        var (tokens, scanErrors) = Scanner.ScanTokens(source);

        if (scanErrors.Any())
        {
            foreach (var error in scanErrors)
            {
                Report(error.Line, string.Empty, error.Message);
            }
            return;
        }

        // TODO: if debug
        foreach (var token in tokens)
        {
            Out?.Invoke(typeof(Interpreter), new InterpreterEventArgs(token.ToString()));
        }

        var (expression, parseErrors) = Parser.Parse(tokens);

        if (parseErrors.Any())
        {
            foreach (var error in parseErrors)
            {
                var token = error.Token;
                var line = token.Line;
                var where = token.Type == TokenType.EOF ? " at end" : $" at '{token.Lexeme}'";

                Report(line, where, error.Message);
            }
            return;
        }

        // TODO if debug
        if (expression != null)
        {
            Out?.Invoke(typeof(Interpreter), new InterpreterEventArgs(printer.Print(expression)));
        }
    }

    private static void Report(int line, string where, string message)
    {
        Error?.Invoke(typeof(Interpreter), new InterpreterEventArgs($"[line {line}] Error{where}: {message}"));
    }

    public static event EventHandler<InterpreterEventArgs>? Out;

    public static event EventHandler<InterpreterEventArgs>? Error;
}
