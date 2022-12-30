namespace Interpreter.Framework.AST;
public abstract record class Statement
{
    public interface IVisitor<T>
    {
        T VisitExpressionStatement(ExpressionStatement statement);
        T VisitPrintStatement(PrintStatement statement);
    }

    public abstract T Accept<T>(IVisitor<T> visitor);
}

public record class ExpressionStatement(Expression Expression) : Statement
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitExpressionStatement(this);
}

public record class PrintStatement(Expression Expression) : Statement
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitPrintStatement(this);
}
