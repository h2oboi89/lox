﻿using System.Text;

namespace Interpreter.Framework.AST;
public class Printer : Expression.IVisitor<string>, Statement.IVisitor<string>
{
    public string Print(IEnumerable<Statement> statements) =>
        string.Join(Environment.NewLine, statements.Select(s => s.Accept(this)));

    #region Expressions
    public string VisitAssignmentExpression(AssignmentExpression expression) =>
        Parenthesize($"{expression.Name.Lexeme} =", expression.Value);

    public string VisitBinaryExpression(BinaryExpression expression) =>
        Parenthesize(expression.Operator.Lexeme, expression.Left, expression.Right);

    public string VisitCallExpression(CallExpression expression)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"{Indent()}( call");
        indentLevel++;

        sb.AppendLine(Parenthesize("callee", expression.Callee));

        sb.AppendLine(Parenthesize("arguments", expression.Arguments));

        indentLevel--;
        sb.Append($"{Indent()})");

        return sb.ToString();
    }

    public string VisitGetExpression(GetExpression expression) =>
        Parenthesize($"get {expression.Name.Lexeme}", expression.LoxObject);

    public string VisitGroupingExpression(GroupingExpression expression) =>
        Parenthesize("group", expression.Expression);

    public string VisitLiteralExpression(LiteralExpression expression)
    {
        var name = Utilities.Stringify(expression.Value);

        if (expression.Value is string) name = $"\"{name}\"";

        return Parenthesize(name);
    }

    public string VisitLogicalExpression(LogicalExpression expression) =>
        Parenthesize(expression.Operator.Lexeme, expression.Left, expression.Right);

    public string VisitSetExpression(SetExpression expression) =>
        Parenthesize($"set {expression.Name.Lexeme}", expression.LoxObject, expression.Value);
    
    public string VisitSuperExpression(SuperExpression expression) =>
        Parenthesize($"{expression.Keyword.Lexeme} {expression.Method.Lexeme}");

    public string VisitThisExpression(ThisExpression expression) =>
        Parenthesize(expression.Keyword.Lexeme);

    public string VisitUnaryExpression(UnaryExpression expression) =>
        Parenthesize(expression.Operator.Lexeme, expression.Right);

    public string VisitVariableExpression(VariableExpression expression) =>
        Parenthesize(expression.Name.Lexeme);
    #endregion

    #region Statements
    public string VisitBlockStatement(BlockStatement statement) =>
        Parenthesize("block", statement.Statements);

    public string VisitClassStatement(ClassStatement statement)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"{Indent()}( class {statement.Name.Lexeme}");
        indentLevel++;

        if (statement.SuperClass == null)
        {
            sb.AppendLine(Parenthesize("super"));
        }
        else
        {
            sb.AppendLine(Parenthesize($"super {statement.SuperClass.Name.Lexeme}"));
        }

        if (!statement.Methods.Any())
        {
            sb.AppendLine($"{Indent()}( methods )");
        }
        else
        {
            sb.AppendLine($"{Indent()}( methods");
            indentLevel++;

            foreach (var method in statement.Methods)
            {
                sb.AppendLine(VisitFunctionStatement(method));
            }

            indentLevel--;
            sb.AppendLine($"{Indent()})");
        }

        indentLevel--;
        sb.Append($"{Indent()})");

        return sb.ToString();
    }

    public string VisitExpressionStatement(ExpressionStatement statement) =>
        Parenthesize("expression", statement.Expression);

    public string VisitFunctionStatement(FunctionStatement statement)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"{Indent()}( function {statement.Name.Lexeme}");
        indentLevel++;

        if (!statement.Parameters.Any())
        {
            sb.AppendLine($"{Indent()}( parameters )");
        }
        else
        {
            sb.AppendLine($"{Indent()}( parameters");
            indentLevel++;

            foreach (var parameter in statement.Parameters)
            {
                sb.AppendLine($"{Indent()}( {parameter.Lexeme} )");
            }

            indentLevel--;
            sb.AppendLine($"{Indent()})");
        }

        sb.AppendLine(Parenthesize("body", statement.Body));

        indentLevel--;
        sb.Append($"{Indent()})");

        return sb.ToString();
    }

    public string VisitIfStatement(IfStatement statement)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"{Indent()}( if");
        indentLevel++;

        sb.AppendLine(Parenthesize("condition", statement.Condition));

        sb.AppendLine(Parenthesize("then", statement.ThenBranch));

        if (statement.ElseBranch != null)
        {
            sb.AppendLine(Parenthesize("else", statement.ElseBranch));
        }
        else
        {
            sb.AppendLine(Parenthesize("else"));
        }

        indentLevel--;
        sb.Append($"{Indent()})");

        return sb.ToString();
    }

    public string VisitPrintStatement(PrintStatement statement) =>
        Parenthesize("print", statement.Expression);

    public string VisitReturnStatement(ReturnStatement statement) =>
        statement.Value == null ?
            Parenthesize("return") :
            Parenthesize("return", statement.Value);

    public string VisitVariableStatement(VariableStatement statement) =>
        statement.Initializer == null ?
            Parenthesize($"var {statement.Name.Lexeme}") :
            Parenthesize($"var {statement.Name.Lexeme} =", statement.Initializer);

    public string VisitWhileStatement(WhileStatement statement)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"{Indent()}( while");
        indentLevel++;

        sb.AppendLine(Parenthesize("condition", statement.Condition));

        sb.AppendLine(Parenthesize("body", statement.Body));

        indentLevel--;
        sb.Append($"{Indent()})");

        return sb.ToString();
    }
    #endregion

    #region Helper Methods
    private string Parenthesize(string name, params Expression[] expressions) =>
        Parenthesize(name, expressions.ToList());

    private string Parenthesize(string name, IEnumerable<Expression> expressions) =>
        Parenthesize(name, expressions, (printer, node) => node.Accept(this));

    private string Parenthesize(string name, params Statement[] statements) =>
        Parenthesize(name, statements.ToList());

    private string Parenthesize(string name, IEnumerable<Statement> statements) =>
        Parenthesize(name, statements, (printer, node) => node.Accept(this));

    private string Parenthesize<T>(string name, IEnumerable<T> nodes, Func<Printer, T, string> stringify)
    {
        if (!nodes.Any())
        {
            return Parenthesize(name);
        }

        var sb = new StringBuilder();

        sb.AppendLine($"{Indent()}( {name}");

        indentLevel++;
        foreach (var node in nodes)
        {
            sb.AppendLine(stringify(this, node));
        }
        indentLevel--;

        sb.Append($"{Indent()})");

        return sb.ToString();
    }

    private string Parenthesize(string name) => $"{Indent()}( {name} )";

    private const string INDENT = "    ";
    private int indentLevel = 0;
    private string Indent() => string.Join(string.Empty, Enumerable.Repeat(INDENT, indentLevel));
    #endregion
}
