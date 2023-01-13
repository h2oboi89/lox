using Interpreter.Framework.Scanning;

namespace Interpreter.Framework.AST;
public abstract record class Expression
{
    public interface IVisitor<T>
    {
        T VisitAssignmentExpression(AssignmentExpression expression);
        T VisitBinaryExpression(BinaryExpression expression);
        T VisitCallExpression(CallExpression expression);
        T VisitGetExpression(GetExpression expression);
        T VisitGroupingExpression(GroupingExpression expression);
        T VisitLiteralExpression(LiteralExpression expression);
        T VisitLogicalExpression(LogicalExpression expression);
        T VisitSetExpression(SetExpression expression);
        T VisitThisExpression(ThisExpression expression);
        T VisitUnaryExpression(UnaryExpression expression);
        T VisitVariableExpression(VariableExpression expression);
    }

    public abstract T Accept<T>(IVisitor<T> visitor);
}

public record class AssignmentExpression(Token Name, Expression Value) : Expression
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitAssignmentExpression(this);
}

public record class BinaryExpression(Expression Left, Token Operator, Expression Right) : Expression
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitBinaryExpression(this);
}

public record class CallExpression(Expression Callee, Token Paren, List<Expression> Arguments) : Expression
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitCallExpression(this);
}

public record class GetExpression(Expression LoxObject, Token Name) : Expression
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitGetExpression(this);
}

public record class GroupingExpression(Expression Expression) : Expression
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitGroupingExpression(this);
}

public record class LiteralExpression(object? Value) : Expression
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitLiteralExpression(this);
}

public record class LogicalExpression(Expression Left, Token Operator, Expression Right) : Expression
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitLogicalExpression(this);
}

public record class SetExpression(Expression LoxObject, Token Name, Expression Value) : Expression
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitSetExpression(this);
}

public record class ThisExpression(Token Keyword) : Expression
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitThisExpression(this);
}

public record class UnaryExpression(Token Operator, Expression Right) : Expression
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitUnaryExpression(this);
}

public record class VariableExpression(Token Name) : Expression
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitVariableExpression(this);
}
