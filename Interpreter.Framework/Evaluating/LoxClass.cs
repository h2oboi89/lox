using System.Diagnostics.CodeAnalysis;

namespace Interpreter.Framework.Evaluating;
internal class LoxClass : LoxCallable
{
    public string Name { get; init; }
    private readonly Dictionary<string, LoxFunction> methods;
    private const string INIT = "init";

    public LoxClass(string name, Dictionary<string, LoxFunction> methods)
    {
        Name = name;
        this.methods = methods;
    }

    public static bool IsInitializer(string name) => name == INIT;

    public bool TryGetMethod(string name, [MaybeNullWhen(false)] out LoxFunction function) =>
        methods.TryGetValue(name, out function);

    public override object? Call(AstInterpreter interpreter, IEnumerable<object?> arguments)
    {
        var instance = new LoxInstance(this);
        if (TryGetMethod(INIT, out var initializer))
        {
            initializer.Bind(instance).Call(interpreter, arguments);
        }
        return instance;
    }

    public override int Arity
    {
        get => TryGetMethod(INIT, out var initializer) ? initializer.Arity : 0;
    }

    public override string ToString() => Name;
}
