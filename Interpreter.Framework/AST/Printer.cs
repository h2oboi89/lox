using System.Text;

namespace Interpreter.Framework.AST;
public class Printer : Expression.IVisitor<string>
{
    public string Print(Expression expression) => expression.Accept(this);

    public string VisitBinaryExpression(Expression.Binary expression) =>
        Parenthesize(expression.Operator.Lexeme, expression.Left, expression.Right);

    public string VisitGroupingExpression(Expression.Grouping expression) =>
        Parenthesize("group", expression.Expression);

    public string VisitLiteralExpression(Expression.Literal expression) =>
        Parenthesize(expression.Value?.ToString() ?? "nil");

    public string VisitUnaryExpression(Expression.Unary expression) =>
        Parenthesize(expression.Operator.Lexeme, expression.Right);

    private string Parenthesize(string name, params Expression[] expressions)
    {
        var sb = new StringBuilder();

        sb.Append("( ").Append(name);

        foreach(var expression in expressions)
        {
            sb.Append(' ');
            sb.Append(expression.Accept(this));
        }

        sb.Append(" )");

        return sb.ToString();
    }
}
