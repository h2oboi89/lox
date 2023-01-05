using Interpreter.Framework.Scanning;

namespace Interpreter.Framework.AST;
public abstract record class Statement
{
    public interface IVisitor<T>
    {
        T VisitBlockStatement(BlockStatement statement);
        T VisitExpressionStatement(ExpressionStatement statement);
        T VisitIfStatement(IfStatement statement);
        T VisitPrintStatement(PrintStatement statement);
        T VisitVariableStatement(VariableStatement statement);
    }

    public abstract T Accept<T>(IVisitor<T> visitor);
}

public record class BlockStatement(List<Statement> Statements) : Statement
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitBlockStatement(this);
}

public record class ExpressionStatement(Expression Expression) : Statement
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitExpressionStatement(this);
}

public record class IfStatement(Expression Condition, Statement ThenBranch, Statement? ElseBranch) : Statement
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitIfStatement(this);
}

public record class PrintStatement(Expression Expression) : Statement
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitPrintStatement(this);
}

public record class VariableStatement(Token Name, Expression Initializer) : Statement
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitVariableStatement(this);
}
