using Interpreter.Framework.AST;
using Interpreter.Framework.Evaluating;

namespace Interpreter.Framework.StaticAnalysis;
internal class Scope
{
    private readonly AstInterpreter interpreter;
    private ScopeLevel? scope = null;
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

    public void EnterBlock() => scope = new ScopeLevel(scope);

    public void ExitBlock() => scope = scope?.Previous;

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

    public bool Declare(string name) => scope?.Declare(name) ?? true;

    public void Define(string name) => scope?.Define(name);

    public bool InFunction => currentFunction.Peek() != FunctionType.None;

    public bool IsDeclared(string name) => scope?.IsDeclared(name) ?? false;

    public bool IsDefined(string name) => scope?.IsDefined(name) ?? false;

    public void ResolveLocal(Expression expression, string name)
    {
        var distance = 0;
        var scope = this.scope;

        while (scope != null)
        {
            if (scope.IsDeclared(name))
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

        public readonly ScopeLevel? Previous;

        public ScopeLevel(ScopeLevel? previous) { Previous = previous; }

        public bool Declare(string name)
        {
            if (IsDeclared(name)) return false;

            values.Add(name, false);

            return true;
        }

        public void Define(string name) => values[name] = true;

        public bool IsDeclared(string name) => values.ContainsKey(name);

        public bool IsDefined(string name) => IsDeclared(name) && values[name] == true;
    }
}
