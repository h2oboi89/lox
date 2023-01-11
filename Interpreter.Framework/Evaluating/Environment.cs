using Interpreter.Framework.Scanning;

namespace Interpreter.Framework.Evaluating;
internal class Environment
{
    private readonly Environment? enclosing = null;

    private readonly Dictionary<string, object?> values = new();

    public Environment(Environment? enclosing = null) { this.enclosing = enclosing; }

    public void Clear() => values.Clear();

    public void Define(string name, object? value) { values[name] = value; }

    public object? Get(Token name)
    {
        if (values.TryGetValue(name.Lexeme, out object? value)) return value;

        if (enclosing != null) return enclosing.Get(name);

        throw UndefinedVariableError(name);
    }

    public object? GetAt(int distance, Token name) => 
        Ancestor(distance).Get(name);

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

    public void AssignAt(int distance, Token name, object? value) =>
        Ancestor(distance).Assign(name, value);

    private static LoxRuntimeError UndefinedVariableError(Token name) => new(name, $"Undefined variable '{name.Lexeme}'.");

    private Environment Ancestor(int distance)
    {
        var environment = this;

        for (var i = 0; i < distance; i++)
        {
            environment = environment.enclosing;
        }

        return environment;
    }
}
