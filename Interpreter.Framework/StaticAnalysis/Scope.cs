using Interpreter.Framework.AST;
using Interpreter.Framework.Evaluating;

namespace Interpreter.Framework.StaticAnalysis;
internal class Scope
{
    private readonly AstInterpreter interpreter;
    private ScopeLevel? scope = null;
    private readonly Stack<FunctionType> currentFunction = new();
    private readonly Stack<ClassType> currentClass = new();

    public enum FunctionType
    {
        None,
        Function,
        Initializer,
        Method,
    }

    public enum ClassType
    {
        None,
        Class,
    }

    public Scope(AstInterpreter interpreter)
    {
        this.interpreter = interpreter;
        currentFunction.Push(FunctionType.None);
        currentClass.Push(ClassType.None);
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

    public void EnterClass()
    {
        currentClass.Push(ClassType.Class);
        EnterBlock();
        Initialize(LoxInstance.THIS);
    }

    public void ExitClass()
    {
        ExitBlock();
        currentClass.Pop();
    }

    public bool Declare(string name) => scope?.Declare(name) ?? true;

    public void Define(string name) => scope?.Define(name);

    public bool Initialize(string name)
    {
        if (!Declare(name)) return false;
        Define(name);
        return true;
    }

    public bool InFunction => currentFunction.Peek() != FunctionType.None;

    public bool InInitializer => currentFunction.Peek() == FunctionType.Initializer;

    public bool InClass => currentClass.Peek() != ClassType.None;

    public bool IsDeclared(string name) => scope?.IsDeclared(name) ?? false;

    public bool IsDefined(string name) => scope?.IsDefined(name) ?? false;

    public void ResolveLocal(Expression expression, string name) => scope?.ResolveLocal(interpreter, expression, name, 0);

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

        public void ResolveLocal(AstInterpreter interpreter, Expression expression, string name, int distance)
        {
            if (IsDeclared(name))
            {
                interpreter.Resolve(expression, distance);
            }
            else
            {
                Previous?.ResolveLocal(interpreter, expression, name, distance + 1);
            }
        }
    }
}
