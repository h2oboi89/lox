using Interpreter.Framework.Scanning;

namespace Interpreter.Framework.Evaluating;
internal class LoxInstance
{
    private readonly LoxClass loxClass;
    private readonly Dictionary<string, object?> fields = new();
    public const string THIS = "this";

    public LoxInstance(LoxClass loxClass)
    {
        this.loxClass = loxClass;
    }

    public object? Get(Token name)
    {
        if (fields.TryGetValue(name.Lexeme, out var property)) return property;

        if (loxClass.TryGetMethod(name.Lexeme, out var method)) return method.Bind(this);

        throw new LoxRuntimeError(name, $"Undefined property '{name.Lexeme}'.");
    }

    public void Set(Token name, object? value) => fields[name.Lexeme] = value;

    public override string ToString() => $"{loxClass.Name} instance";
}
