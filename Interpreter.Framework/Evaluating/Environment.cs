﻿using Interpreter.Framework.Scanning;
using System.Diagnostics.CodeAnalysis;

namespace Interpreter.Framework.Evaluating;
internal class Environment
{
    public readonly Environment? Enclosing = null;

    private readonly Dictionary<string, object?> values = new();

    public Environment(Environment? enclosing = null) { Enclosing = enclosing; }

    public void Clear() => values.Clear();

    public void Define(string name, object? value) { values[name] = value; }

    public object? Get(Token name)
    {
        if (values.TryGetValue(name.Lexeme, out object? value)) return value;

        if (Enclosing != null) return Enclosing.Get(name);

        throw UndefinedVariableError(name);
    }

    public object? GetAt(int distance, string name) =>
        Ancestor(distance).values[name];

    public void Assign(Token name, object? value)
    {
        if (values.TryGetValue(name.Lexeme, out object? _))
        {
            values[name.Lexeme] = value;
            return;
        }

        if (Enclosing != null)
        {
            Enclosing.Assign(name, value);
            return;
        }

        throw UndefinedVariableError(name);
    }

    public void AssignAt(int distance, Token name, object? value) =>
        Ancestor(distance).Assign(name, value);

    private static LoxRuntimeError UndefinedVariableError(Token name) => new(name, $"Undefined variable '{name.Lexeme}'.");


    private Environment Ancestor(int distance)
    {
        //NOTE: resolver ensures we avoid null reference issues
        var environment = this;

        for (var i = 0; i < distance; i++)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            environment = environment.Enclosing;
#pragma warning restore CS8602
        }

#pragma warning disable CS8603 // Possible null reference return.
        return environment;
#pragma warning restore CS8603
    }
}
