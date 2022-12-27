using Interpreter.Framework.Scanning;

namespace Interpreter.Framework.AST;
public abstract record class Expression
{
    public interface Visitor<T>
    {
        T VisitBinaryExpression(Binary expression);
        T VisitGroupingExpression(Grouping expression);
        T VisitLiteralExpression(Literal expression);
        T VisitUnaryExpression(Unary expression);
    }

    public abstract T Accept<T>(Visitor<T> visitor);

    public record class Binary(Expression Left, Token Operator, Expression Right) : Expression
    {
        public override T Accept<T>(Visitor<T> visitor) => visitor.VisitBinaryExpression(this);
    }

    public record class Grouping(Expression Expression) : Expression
    {
        public override T Accept<T>(Visitor<T> visitor) => visitor.VisitGroupingExpression(this);
    }

    public record class Literal(Object Value) : Expression
    {
        public override T Accept<T>(Visitor<T> visitor) => visitor.VisitLiteralExpression(this);
    }

    public record class Unary(Token Operator, Expression Right) : Expression
    {
        public override T Accept<T>(Visitor<T> visitor) => visitor.VisitUnaryExpression(this);
    }
}
