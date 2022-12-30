﻿using Interpreter.Framework.Evaluating;
using Interpreter.Framework.Parsing;
using Interpreter.Framework.Scanning;
using static Interpreter.Framework.InterpreterErrorEventArgs;

namespace Interpreter.Framework;

public static class Interpreter
{
    private static readonly AstInterpreter interpreter = new();
    private static bool initialized = false;

    private static void Initialize()
    {
        interpreter.Out += (_, e) => RaiseOut(e.Content);
        initialized= true;
    }

    public static void Run(string? source)
    {
        if (!initialized) Initialize();

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

        var (statements, parseErrors) = Parser.Parse(tokens);

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

        //RaiseDebug($"AST: {printer.Print(statements)}");

        var runtimeError = interpreter.Interpret(statements);

        if (runtimeError != null)
        {
            var errorMessage = $"{runtimeError.Message}{System.Environment.NewLine}[line {runtimeError.Token.Line}]";

            RaiseError(ErrorType.RuntimeError, errorMessage);
            return;
        }

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
