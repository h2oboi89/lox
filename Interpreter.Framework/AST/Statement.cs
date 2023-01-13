using Interpreter.Framework.Scanning;

namespace Interpreter.Framework.AST;
public abstract record class Statement
{
    public interface IVisitor<T>
    {
        T VisitBlockStatement(BlockStatement statement);
        T VisitClassStatement(ClassStatement statement);
        T VisitExpressionStatement(ExpressionStatement statement);
        T VisitFunctionStatement(FunctionStatement statement);
        T VisitIfStatement(IfStatement statement);
        T VisitPrintStatement(PrintStatement statement);
        T VisitReturnStatement(ReturnStatement statement);
        T VisitVariableStatement(VariableStatement statement);
        T VisitWhileStatement(WhileStatement statement);
    }

    public abstract T Accept<T>(IVisitor<T> visitor);
}

public record class BlockStatement(List<Statement> Statements) : Statement
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitBlockStatement(this);
}

public record class ClassStatement(Token Name, VariableExpression? SuperClass, List<FunctionStatement> Methods) : Statement
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitClassStatement(this);
}

public record class ExpressionStatement(Expression Expression) : Statement
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitExpressionStatement(this);
}

public record class FunctionStatement(Token Name, List<Token> Parameters, List<Statement> Body) : Statement
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitFunctionStatement(this);
}

public record class IfStatement(Expression Condition, Statement ThenBranch, Statement? ElseBranch) : Statement
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitIfStatement(this);
}

public record class PrintStatement(Expression Expression) : Statement
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitPrintStatement(this);
}

public record class ReturnStatement(Token Keyword, Expression? Value) : Statement
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitReturnStatement(this);
}

public record class VariableStatement(Token Name, Expression? Initializer) : Statement
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitVariableStatement(this);
}

public record class WhileStatement(Expression Condition, Statement Body) : Statement
{
    public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitWhileStatement(this);
}
