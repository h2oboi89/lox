﻿namespace Interpreter.Framework;
public class InterpreterErrorEventArgs : InterpreterOutEventArgs
{
    public enum ErrorType
    {
        None,
        ScanError,
        ParseError,
        ScopeError,
        RuntimeError,
    }

    public readonly ErrorType Error;

    public InterpreterErrorEventArgs(ErrorType error, string content) : base(content) { Error = error; }
}
