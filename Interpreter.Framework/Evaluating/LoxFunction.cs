using Interpreter.Framework.AST;

namespace Interpreter.Framework.Evaluating;
internal class LoxFunction : LoxCallable
{
    private readonly FunctionStatement declaration;
    private readonly Environment closure;

    public LoxFunction(FunctionStatement declaration, Environment closure)
    {
        this.declaration = declaration;
        this.closure = closure;
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
            return returnValue.Value;
        }

        return null;
    }

    public override string ToString() => $"<function {declaration.Name.Lexeme}>";
}
