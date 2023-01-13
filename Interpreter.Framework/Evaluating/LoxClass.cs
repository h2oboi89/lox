using System.Diagnostics.CodeAnalysis;

namespace Interpreter.Framework.Evaluating;
internal class LoxClass : LoxCallable
{
    public string Name { get; init; }
    private readonly LoxClass? superClass;
    private readonly Dictionary<string, LoxFunction> methods;
    private const string INIT = "init";
    public const string SUPER = "super";

    public LoxClass(string name, LoxClass? superClass, Dictionary<string, LoxFunction> methods)
    {
        Name = name;
        this.superClass = superClass;
        this.methods = methods;
    }

    public static bool IsInitializer(string name) => name == INIT;

    public bool TryGetMethod(string name, [MaybeNullWhen(false)] out LoxFunction function)
    {
        if (methods.TryGetValue(name, out function)) return true;

        if (superClass != null) return superClass.TryGetMethod(name, out function);

        function = null;
        return false;
    }

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
