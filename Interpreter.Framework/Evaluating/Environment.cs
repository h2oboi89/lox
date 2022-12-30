﻿using Interpreter.Framework.Scanning;

namespace Interpreter.Framework.Evaluating;
internal class Environment
{
    private readonly Environment? enclosing = null;

    private readonly Dictionary<string, object?> values = new();

    public Environment(Environment? enclosing = null) { this.enclosing = enclosing; }

    public void Define(string name, object? value) { values[name] = value; }

    public object? Get(Token name)
    {
        if (values.TryGetValue(name.Lexeme, out object? value)) return value;

        if (enclosing != null) return enclosing.Get(name);

        throw UndefinedVariableError(name);
    }

    public void Assign(Token name, object? value)
    {
        if (values.TryGetValue(name.Lexeme, out object? _))
        {
            values[name.Lexeme] = value;
            return;
        }

        if (enclosing != null)
        {
            enclosing.Assign(name, value);
            return;
        }

        throw UndefinedVariableError(name);
    }

    private static LoxRuntimeError UndefinedVariableError(Token name) => new(name, $"Undefined variable '{name.Lexeme}'.");
}
