using Interpreter.Framework.AST;
using Interpreter.Framework.Evaluating;
using Interpreter.Framework.Scanning;

namespace Interpreter.Framework.StaticAnalysis;
public class Resolver : Expression.IVisitor<object?>, Statement.IVisitor<object?>
{
    private readonly Scope scope;
    private readonly List<ScopeError> errors = new();

    private Resolver(AstInterpreter interpreter)
    {
        scope = new Scope(interpreter);
    }

    public static IEnumerable<ScopeError> Resolve(AstInterpreter interpreter, IEnumerable<Statement> statements)
    {
        var resolver = new Resolver(interpreter);

        resolver.Resolve(statements);

        return resolver.errors;
    }

    #region Expressions
    public object? VisitAssignmentExpression(AssignmentExpression expression)
    {
        Resolve(expression.Value);
        ResolveLocal(expression, expression.Name);
        return null;
    }

    public object? VisitBinaryExpression(BinaryExpression expression)
    {
        Resolve(expression.Left);
        Resolve(expression.Right);
        return null;
    }

    public object? VisitCallExpression(CallExpression expression)
    {
        Resolve(expression.Callee);

        foreach(var argument in expression.Arguments)
        {
            Resolve(argument);
        }

        return null;
    }

    public object? VisitGroupingExpression(GroupingExpression expression)
    {
        Resolve(expression.Expression); 
        return null;
    }

    public object? VisitLiteralExpression(LiteralExpression expression)
    {
        return null;
    }

    public object? VisitLogicalExpression(LogicalExpression expression)
    {
        Resolve(expression.Left);
        Resolve(expression.Right);
        return null;
    }

    public object? VisitUnaryExpression(UnaryExpression expression)
    {
        Resolve(expression.Right);
        return null;
    }

    public object? VisitVariableExpression(VariableExpression expression)
    {
        if (
            scope.IsDeclared(expression.Name.Lexeme) && 
            !scope.IsDefined(expression.Name.Lexeme))
        {
            errors.Add(new ScopeError(expression.Name, "Can't read local variable in its own initializer."));
        }

        ResolveLocal(expression, expression.Name);
        return null;
    }
    #endregion

    #region Statements
    public object? VisitBlockStatement(BlockStatement statement)
    {
        scope.EnterBlock();
        Resolve(statement.Statements);
        scope.ExitBlock();
        return null;
    }

    public object? VisitExpressionStatement(ExpressionStatement statement)
    {
        Resolve(statement.Expression);
        return null;
    }

    public object? VisitFunctionStatement(FunctionStatement statement)
    {
        Declare(statement.Name);
        Define(statement.Name);

        ResolveFunction(statement, Scope.FunctionType.Function);
        return null;
    }

    public object? VisitIfStatement(IfStatement statement)
    {
        Resolve(statement.Condition);
        Resolve(statement.ThenBranch);
        if (statement.ElseBranch != null)
        {
            Resolve(statement.ElseBranch);
        }
        return null;
    }

    public object? VisitPrintStatement(PrintStatement statement)
    {
        Resolve(statement.Expression);
        return null;
    }

    public object? VisitReturnStatement(ReturnStatement statement)
    {
        if (!scope.InFunction)
        {
            errors.Add(new ScopeError(statement.Keyword, "Can't return from top-level code."));
        }

        Resolve(statement.Value);
        return null;
    }

    public object? VisitVariableStatement(VariableStatement statement)
    {
        Declare(statement.Name);
        Resolve(statement.Initializer);
        Define(statement.Name);
        return null;
    }

    public object? VisitWhileStatement(WhileStatement statement)
    {
        Resolve(statement.Condition);
        Resolve(statement.Body); 
        return null;
    }
    #endregion

    #region Helper Methods
    public void Resolve(IEnumerable<Statement> statements)
    {
        foreach (var statement in statements)
        {
            Resolve(statement);
        }
    }

    private void Resolve(Statement statement) => statement.Accept(this);

    private void Resolve(Expression expression) => expression.Accept(this);

    private void ResolveLocal(Expression expression, Token name) => scope.ResolveLocal(expression, name.Lexeme);

    private void ResolveFunction(FunctionStatement function, Scope.FunctionType type)
    {
        scope.EnterFunction(type);

        foreach(var token in function.Parameters)
        {
            Declare(token);
            Define(token);
        }
        Resolve(function.Body);

        scope.ExitFunction();
    }

    private void Declare(Token name)
    {
        if (!scope.Declare(name.Lexeme))
        {
            errors.Add(new ScopeError(name, "There is already a variable with this name in this scope."));
        }
    }

    private void Define(Token name)
    {
        scope.Define(name.Lexeme);
    }
    #endregion
}
