using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: generate_ast <output directory>");
            Environment.Exit(64);
        }

        var outputDir = args[0];

        DefineAst(outputDir,
            "Expression",
            new string[]
            {
                "Assignment : Token Name, Expression Value",
                "Binary     : Expression Left, Token Operator, Expression Right",
                "Grouping   : Expression Expression",
                "Literal    : object? Value",
                "Unary      : Token Operator, Expression Right",
                "Variable   : Token Name",
            }
        );

        DefineAst(outputDir,
            "Statement",
            new string[]
            {
                "Block      : List<Statement> Statements",
                "Expression : Expression Expression",
                "If         : Expression Condition, Statement ThenBranch, Statement? ElseBranch",
                "Print      : Expression Expression",
                "Variable   : Token Name, Expression Initializer",
            }
        );
    }

    private const string INDENT = "    ";
    private static int indentLevel = 0;
    private static string Indent() => string.Join(string.Empty, Enumerable.Repeat(INDENT, indentLevel));

    private const string VISITOR = "IVisitor";

    private static void DefineAst(string outputDir, string baseName, IEnumerable<string> types)
    {
        using var writer = new StreamWriter(Path.Combine(outputDir, $"{baseName}.cs"));

        writer.WriteLine($"using Interpreter.Framework.Scanning;");
        writer.WriteLine();
        writer.WriteLine("namespace Interpreter.Framework.AST;");

        writer.WriteLine($"public abstract record class {baseName}");
        writer.WriteLine("{");
        indentLevel++;

        writer.WriteLine(DefineVisitor(baseName, types));
        writer.WriteLine();

        writer.WriteLine($"{Indent()}public abstract T Accept<T>({VISITOR}<T> visitor);");

        indentLevel--;
        writer.WriteLine("}");

        foreach (var type in types)
        {
            var parts = type.Split(':');
            var className = $"{parts[0].Trim()}{baseName}";
            var fields = parts[1].Trim();

            writer.WriteLine();
            writer.WriteLine(DefineType(baseName, className, fields));
        }
    }

    private static string DefineVisitor(string baseName, IEnumerable<string> types)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"{Indent()}public interface {VISITOR}<T>");
        sb.AppendLine($"{Indent()}{{");
        indentLevel++;

        foreach (var type in types)
        {
            var typeName = $"{type.Split(':')[0].Trim()}{baseName}";

            sb.AppendLine($"{Indent()}T Visit{typeName}({typeName} {baseName.ToLower()});");
        }

        indentLevel--;
        sb.Append($"{Indent()}}}");

        return sb.ToString();
    }

    private static string DefineType(string baseName, string className, string fields)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"{Indent()}public record class {className}({fields}) : {baseName}");
        sb.AppendLine($"{Indent()}{{");
        indentLevel++;

        sb.AppendLine($"{Indent()}public override T Accept<T>({VISITOR}<T> visitor) => visitor.Visit{className}(this);");

        indentLevel--;
        sb.Append($"{Indent()}}}");

        return sb.ToString();
    }
}