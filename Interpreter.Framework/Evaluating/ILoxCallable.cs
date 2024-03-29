﻿namespace Interpreter.Framework.Evaluating;

interface ILoxCallable
{
    int Arity { get; }

    object? Call(AstInterpreter interpreter, IEnumerable<object?> arguments);
}

abstract class LoxCallable : ILoxCallable
{
    public virtual int Arity => 0;

    public abstract object? Call(AstInterpreter interpreter, IEnumerable<object?> arguments);
}
