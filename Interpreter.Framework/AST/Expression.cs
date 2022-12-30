using Interpreter.Framework.Scanning;

namespace Interpreter.Framework.AST;
public abstract record class Expression
{
    public interface IVisitor<T>
    {
        T VisitBinaryExpression(BinaryExpression expression);
        T VisitGroupingExpression(GroupingExpression expression);
        T VisitLiteralExpression(LiteralExpression expression);
        T VisitUnaryExpression(UnaryExpression expression);
    }

    public abstract T Accept<T>(IVisitor<T> visitor);
}

public record class BinaryExpression(Expression Left, Token Operator, Expression Right) : Expression
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitBinaryExpression(this);
}

public record class GroupingExpression(Expression Expression) : Expression
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitGroupingExpression(this);
}

public record class LiteralExpression(object? Value) : Expression
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitLiteralExpression(this);
}

public record class UnaryExpression(Token Operator, Expression Right) : Expression
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitUnaryExpression(this);
}
