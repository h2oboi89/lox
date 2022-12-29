using Interpreter.Framework.AST;
using Interpreter.Framework.Evaluating;
using Interpreter.Framework.Parsing;
using Interpreter.Framework.Scanning;
using static Interpreter.Framework.InterpreterErrorEventArgs;

namespace Interpreter.Framework;

public static class Interpreter
{
    private static readonly Printer printer = new();

    private static readonly AstInterpreter interpreter = new();

    public static void Run(string? source)
    {
        if (string.IsNullOrEmpty(source))
        {
            return;
        }

        var (tokens, scanErrors) = Scanner.ScanTokens(source);

        if (scanErrors.Any())
        {
            foreach (var error in scanErrors)
            {
                Report(ErrorType.ScanError, line: error.Line, where: string.Empty, message: error.Message);
            }
            return;
        }

        RaiseDebug("Tokens:");
        foreach (var token in tokens)
        {
            RaiseDebug($"- {token}");
        }

        var (expression, parseErrors) = Parser.Parse(tokens);

        if (parseErrors.Any())
        {
            foreach (var error in parseErrors)
            {
                var token = error.Token;
                var line = token.Line;
                var where = token.Type == TokenType.EOF ? " at end" : $" at '{token.Lexeme}'";

                Report(ErrorType.ParseError, line: line, where: where, message: error.Message);
            }
            return;
        }

        if (expression == null) return;

        RaiseDebug($"AST: {printer.Print(expression)}");

        var (value, runtimeError) = interpreter.Execute(expression);

        if (runtimeError != null)
        {
            var errorMessage = $"{runtimeError.Message}{Environment.NewLine}[line {runtimeError.Token.Line}]";

            RaiseError(ErrorType.RuntimeError, errorMessage);
            return;
        }

        RaiseOut(value);
        return;
    }

    private static void Report(ErrorType error, int line, string where, string message) =>
        RaiseError(error, $"[line {line}] Error{where}: {message}");

    public static event EventHandler<InterpreterOutEventArgs>? Debug;
    private static void RaiseDebug(string message) => Debug?.Invoke(typeof(Interpreter), new InterpreterOutEventArgs(message));

    public static event EventHandler<InterpreterOutEventArgs>? Out;
    private static void RaiseOut(string message) => Out?.Invoke(typeof(Interpreter), new InterpreterOutEventArgs(message));

    public static event EventHandler<InterpreterErrorEventArgs>? Error;
    private static void RaiseError(ErrorType error, string message) => Error?.Invoke(typeof(Interpreter), new InterpreterErrorEventArgs(error, message));
}
