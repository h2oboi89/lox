using Interpreter.Framework.AST;

namespace Interpreter.Framework.Evaluating;
internal class LoxFunction : LoxCallable
{
    private readonly FunctionStatement declaration;

    public LoxFunction(FunctionStatement declaration) { this.declaration = declaration; }

    public override int Arity => declaration.Parameters.Count;

    public override object? Call(AstInterpreter interpreter, IEnumerable<object?> arguments)
    {
        var environment = new Environment(interpreter.Globals);
        var args = arguments.ToList();

        for(var i = 0; i < declaration.Parameters.Count; i++)
        {
            environment.Define(declaration.Parameters[i].Lexeme, args[i]);
        }

        interpreter.ExecuteBlock(declaration.Body, environment);

        return null;
    }

    public override string ToString() => $"<function {declaration.Name.Lexeme}>";
}
