using Interpreter.Framework.AST;
using Interpreter.Framework.Scanning;

namespace Interpreter.Framework.Evaluating;
public class AstInterpreter : Expression.IVisitor<object>
{
    public (string value, LoxRuntimeError? runtimeError) Execute(Expression expression)
    {
        try
        {
            var value = Evaluate(expression);

            return (Stringify(value), null);
        }
        catch (LoxRuntimeError e)
        {
            return (string.Empty, e);
        }
    }

    public object VisitBinaryExpression(Expression.Binary expression)
    {
        var left = Evaluate(expression.Left);
        var right = Evaluate(expression.Right);

        switch (expression.Operator.Type)
        {

            case TokenType.MINUS:
                CheckNumberOperands(expression.Operator, left, right);
                return (double)left - (double)right;
            case TokenType.PLUS:
                if (left is double lDbl && right is double rDbl) return lDbl + rDbl;
                if (left is string lStr && right is string rStr) return lStr + rStr;
                throw new LoxRuntimeError(expression.Operator, "Operands must be two numbers or two strings.");
            case TokenType.SLASH:
                CheckNumberOperands(expression.Operator, left, right);
                return (double)left / (double)right;
            case TokenType.STAR:
                CheckNumberOperands(expression.Operator, left, right);
                return (double)left * (double)right;
            case TokenType.GREATER:
                CheckNumberOperands(expression.Operator, left, right);
                return (double)left > (double)right;
            case TokenType.GREATER_EQUAL:
                CheckNumberOperands(expression.Operator, left, right);
                return (double)left >= (double)right;
            case TokenType.LESS:
                CheckNumberOperands(expression.Operator, left, right);
                return (double)left < (double)right;
            case TokenType.LESS_EQUAL:
                CheckNumberOperands(expression.Operator, left, right);
                return (double)left <= (double)right;
            case TokenType.BANG_EQUAL:
                return !IsEqual(left, right);
            case TokenType.EQUAL_EQUAL:
                return IsEqual(left, right);
            default:
                throw new Exception($"unexpected token: '{expression.Operator}");
        }
    }

    public object VisitGroupingExpression(Expression.Grouping expression) => Evaluate(expression.Expression);

    public object VisitLiteralExpression(Expression.Literal expression) => expression.Value;

    public object VisitUnaryExpression(Expression.Unary expression)
    {
        var right = Evaluate(expression.Right);

        switch (expression.Operator.Type)
        {
            case TokenType.MINUS:
                CheckNumberOperands(expression.Operator, right);
                return -(double)right;
            case TokenType.BANG:
                return !IsTruthy(right);
            default:
                throw new Exception($"unexpected token: '{expression.Operator}");
        }
    }

    private object Evaluate(Expression expression) => expression.Accept(this);

    private static bool IsTruthy(object obj) => obj switch
    {
        null => false,
        bool boolean => boolean,
        _ => true
    };

    private static bool IsEqual(object a, object b)
    {
        if (a == null && b == null) return true;
        if (a == null) return false;

        return a.Equals(b);
    }

    private static void CheckNumberOperands(Token operation, params object[] operands)
    {
        foreach (var operand in operands)
        {
            if (operand is not double)
            {
                var message = "Operand must be a number.";

                if (operands.Length > 1)
                {
                    message = "Operands must be numbers.";
                }

                throw new LoxRuntimeError(operation, message);
            }
        }
    }

    private static string Stringify(object? value)
    {
        if (value is double dbl) return dbl.ToString();

        return value?.ToString() ?? "nil";
    }
}
