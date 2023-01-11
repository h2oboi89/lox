using Interpreter.Framework.AST;
using Interpreter.Framework.Evaluating;

namespace Interpreter.Framework.StaticAnalysis;
internal class Scope
{
    private readonly AstInterpreter interpreter;
    private readonly LinkedList<ScopeLevel> scopes = new();
    private readonly Stack<FunctionType> currentFunction = new();

    public enum FunctionType
    {
        None,
        Function,
    }

    public Scope(AstInterpreter interpreter)
    {
        this.interpreter = interpreter;
        currentFunction.Push(FunctionType.None);
    }

    public void EnterBlock() => scopes.AddLast(new ScopeLevel());

    public void ExitBlock() => scopes.RemoveLast();

    public void EnterFunction(FunctionType type)
    {
        currentFunction.Push(type);
        EnterBlock();
    }

    public void ExitFunction()
    {
        ExitBlock();
        currentFunction.Pop();
    }

    public bool Declare(string name) => scopes?.Last?.Value.Declare(name) ?? true;

    public void Define(string name) => scopes?.Last?.Value.Define(name);

    public bool InFunction => currentFunction.Peek() != FunctionType.None;

    public bool IsDeclared(string name) => scopes?.Last?.Value.IsDeclared(name) ?? false;

    public bool IsDefined(string name) => scopes?.Last?.Value.IsDefined(name) ?? false;

    public void ResolveLocal(Expression expression, string name)
    {
        var distance = 0;
        var scope = scopes.Last;

        while (scope != null)
        {
            if (scope.Value.IsDeclared(name))
            {
                interpreter.Resolve(expression, distance);
                return;
            }

            scope = scope.Previous;
            distance++;
        }
    }

    class ScopeLevel
    {
        private readonly Dictionary<string, bool> values = new();

        public bool Declare(string name)
        {
            if (values.ContainsKey(name))
            {
                return false;
            }

            values.Add(name, false);

            return true;
        }

        public void Define(string name)
        {
            values[name] = true;
        }

        public bool IsDeclared(string name)
        {
            return values.ContainsKey(name);
        }

        public bool IsDefined(string name)
        {
            return IsDeclared(name) && values[name] == true;
        }
    }
}
