using Interpreter.Framework.AST;
using Interpreter.Framework.Evaluating.Globals;
using Interpreter.Framework.Scanning;

namespace Interpreter.Framework.Evaluating;
public class AstInterpreter : Expression.IVisitor<object?>, Statement.IVisitor<object?>
{
    private readonly Environment globals = new();
    private readonly Dictionary<Expression, int> locals = new();
    private Environment environment;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public AstInterpreter()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        Reset();
    }

    public void Reset()
    {
        globals.Clear();
        environment = globals;

        globals.Define("clock", new Clock());
        globals.Define("reset", new Reset());
    }

    public LoxRuntimeError? Interpret(IEnumerable<Statement> statements)
    {
        try
        {
            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        catch (LoxRuntimeError e)
        {
            return e;
        }

        return null;
    }

    public event EventHandler<InterpreterOutEventArgs>? Out;
    private void RaiseOut(string message) => Out?.Invoke(this, new InterpreterOutEventArgs(message));

    #region Expressions
    public object? VisitAssignmentExpression(AssignmentExpression expression)
    {
        var value = Evaluate(expression.Value);

        if (locals.TryGetValue(expression, out var distance))
        {
            environment.AssignAt(distance, expression.Name, value);
        }
        else
        {
            globals.Assign(expression.Name, value);
        }

        return value;
    }

    public object? VisitBinaryExpression(BinaryExpression expression)
    {
        var left = Evaluate(expression.Left);
        var right = Evaluate(expression.Right);

        static object BinaryPlus(BinaryExpression expression, object? left, object? right)
        {
            if (left is double lDbl && right is double rDbl) return lDbl + rDbl;
            if (left is string lStr && right is string rStr) return lStr + rStr;
            throw new LoxRuntimeError(expression.Operator, "Operands must be two numbers or two strings.");
        }

        return expression.Operator.Type switch
        {
            TokenType.PLUS => BinaryPlus(expression, left, right),
            TokenType.MINUS => Functor.Minus(CheckNumberOperands(expression.Operator, left, right)),
            TokenType.SLASH => Functor.Divide(CheckNumberOperands(expression.Operator, left, right)),
            TokenType.STAR => Functor.Multiply(CheckNumberOperands(expression.Operator, left, right)),
            TokenType.GREATER => Functor.Greater(CheckNumberOperands(expression.Operator, left, right)),
            TokenType.GREATER_EQUAL => Functor.GreaterEqual(CheckNumberOperands(expression.Operator, left, right)),
            TokenType.LESS => Functor.Less(CheckNumberOperands(expression.Operator, left, right)),
            TokenType.LESS_EQUAL => Functor.LessEqual(CheckNumberOperands(expression.Operator, left, right)),
            TokenType.BANG_EQUAL => !IsEqual(left, right),
            TokenType.EQUAL_EQUAL => IsEqual(left, right),
            _ => throw new Exception($"unexpected token: '{expression.Operator}"), // impossible to hit
        };
    }

    static class Functor
    {
        public static Func<(double, double), double> Minus => ((double left, double right) operands) => operands.left - operands.right;
        public static Func<(double, double), double> Divide => ((double left, double right) operands) => operands.left / operands.right;
        public static Func<(double, double), double> Multiply => ((double left, double right) operands) => operands.left * operands.right;
        public static Func<(double, double), bool> Greater => ((double left, double right) operands) => operands.left > operands.right;
        public static Func<(double, double), bool> GreaterEqual => ((double left, double right) operands) => operands.left >= operands.right;
        public static Func<(double, double), bool> Less => ((double left, double right) operands) => operands.left < operands.right;
        public static Func<(double, double), bool> LessEqual => ((double left, double right) operands) => operands.left <= operands.right;
    }

    public object? VisitCallExpression(CallExpression expression)
    {
        var callee = Evaluate(expression.Callee);

        var arguments = expression.Arguments.Select(Evaluate).ToArray();

        if (callee is ILoxCallable function)
        {
            if (arguments.Length != function.Arity)
            {
                throw new LoxRuntimeError(expression.Paren, $"Expected {function.Arity} arguments but got {arguments.Length}.");
            }

            return function.Call(this, arguments);
        }
        else
        {
            throw new LoxRuntimeError(expression.Paren, "Can only call functions and classes.");
        }
    }

    public object? VisitGroupingExpression(GroupingExpression expression) => Evaluate(expression.Expression);

    public object? VisitLiteralExpression(LiteralExpression expression) => expression.Value;

    public object? VisitLogicalExpression(LogicalExpression expression)
    {
        var left = Evaluate(expression.Left);

        if (expression.Operator.Type == TokenType.OR)
        {
            if (IsTruthy(left)) return left;
        }

        if (expression.Operator.Type == TokenType.AND)
        {
            if (!IsTruthy(left)) return left;
        }

        return Evaluate(expression.Right);
    }

    public object? VisitUnaryExpression(UnaryExpression expression)
    {
        var right = Evaluate(expression.Right);

        return expression.Operator.Type switch
        {
            TokenType.MINUS => -CheckNumberOperand(expression.Operator, right),
            TokenType.BANG => !IsTruthy(right),
            _ => throw new Exception($"unexpected token: '{expression.Operator}"), // impossible to hit
        };
    }

    public object? VisitVariableExpression(VariableExpression expression) => LookUpVariable(expression.Name, expression);
    #endregion

    #region Statements
    public object? VisitBlockStatement(BlockStatement statement)
    {
        ExecuteBlock(statement.Statements, new Environment(environment));
        return null;
    }

    public object? VisitExpressionStatement(ExpressionStatement statement)
    {
        Evaluate(statement.Expression);

        return null;
    }

    public object? VisitFunctionStatement(FunctionStatement statement)
    {
        var function = new LoxFunction(statement, environment);

        environment.Define(statement.Name.Lexeme, function);

        return null;
    }

    public object? VisitIfStatement(IfStatement statement)
    {
        if (IsTruthy(Evaluate(statement.Condition)))
        {
            Execute(statement.ThenBranch);
        }
        else if (statement.ElseBranch != null)
        {
            Execute(statement.ElseBranch);
        }

        return null;
    }

    public object? VisitPrintStatement(PrintStatement statement)
    {
        var value = Evaluate(statement.Expression);

        RaiseOut(Utilities.Stringify(value));

        return null;
    }

    public object? VisitReturnStatement(ReturnStatement statement) =>
        throw new Return(Evaluate(statement.Value));

    public object? VisitVariableStatement(VariableStatement statement)
    {
        var value = Evaluate(statement.Initializer);

        environment.Define(statement.Name.Lexeme, value);

        return null;
    }

    public object? VisitWhileStatement(WhileStatement statement)
    {
        while (IsTruthy(Evaluate(statement.Condition)))
        {
            Execute(statement.Body);
        }

        return null;
    }
    #endregion

    #region Helper Methods
    private object? Evaluate(Expression expression) => expression.Accept(this);

    private void Execute(Statement statement) => statement.Accept(this);

    internal void ExecuteBlock(IEnumerable<Statement> statements, Environment environment)
    {
        var previous = this.environment;

        try
        {
            this.environment = environment;

            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        finally
        {
            this.environment = previous;
        }
    }

    private static bool IsTruthy(object? obj) => obj switch
    {
        null => false,
        bool boolean => boolean,
        _ => true
    };

    private static bool IsEqual(object? a, object? b)
    {
        if (a == null && b == null) return true;
        if (a == null) return false;

        return a.Equals(b);
    }

    private static double CheckNumberOperand(Token operation, object? operand)
    {
        if (operand is double dbl) return dbl;

        throw new LoxRuntimeError(operation, "Operand must be a number.");
    }

    private static (double left, double right) CheckNumberOperands(Token operation, object? left, object? right)
    {
        if (left is double lDbl && right is double rDbl) return (lDbl, rDbl);

        throw new LoxRuntimeError(operation, "Operands must be numbers.");
    }

    internal void Resolve(Expression expression, int depth) => locals[expression] = depth;

    private object? LookUpVariable(Token name, Expression expression)
    {
        if (locals.TryGetValue(expression, out var distance))
        {
            return environment.GetAt(distance, name);
        }
        else
        {
            return globals.Get(name);
        }
    }
    #endregion
}
