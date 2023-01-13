using Interpreter.Framework.AST;

namespace Interpreter.Framework.Evaluating;
internal class LoxFunction : LoxCallable
{
    private readonly FunctionStatement declaration;
    private readonly Environment closure;
    private readonly bool IsInitializer;

    public LoxFunction(FunctionStatement declaration, Environment closure, bool isInitializer)
    {
        this.declaration = declaration;
        this.closure = closure;
        IsInitializer = isInitializer;
    }

    public LoxFunction Bind(LoxInstance loxInstance)
    {
        var environment = new Environment(closure);
        environment.Define(LoxInstance.THIS, loxInstance);
        return new LoxFunction(declaration, environment, IsInitializer);
    }

    public override int Arity => declaration.Parameters.Count;

    public override object? Call(AstInterpreter interpreter, IEnumerable<object?> arguments)
    {
        var environment = new Environment(closure);
        var args = arguments.ToList();

        for (var i = 0; i < declaration.Parameters.Count; i++)
        {
            environment.Define(declaration.Parameters[i].Lexeme, args[i]);
        }

        try
        {
            interpreter.ExecuteBlock(declaration.Body, environment);
        }
        catch (Return returnValue)
        {
            if (IsInitializer) return closure.GetAt(0, LoxInstance.THIS);

            return returnValue.Value;
        }

        if (IsInitializer) return closure.GetAt(0, LoxInstance.THIS);

        return null;
    }

    public override string ToString() => $"<function {declaration.Name.Lexeme}>";
}
