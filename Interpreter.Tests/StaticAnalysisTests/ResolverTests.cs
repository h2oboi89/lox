using Interpreter.Framework.Evaluating;
using Interpreter.Framework.Parsing;
using Interpreter.Framework.Scanning;
using Interpreter.Framework.StaticAnalysis;

namespace Interpreter.Tests.StaticAnalysisTests;

[TestFixture]
internal static class ResolverTests
{
    [Test]
    public static void Variable_InitCannotSelfReference()
    {
        var input = "{ var a = a; }";

        var expected = "Can't read local variable in its own initializer.";

        AssertInputGeneratesError(input, expected);
    }

    [Test]
    public static void Variable_CannotBeDeclaredTwice()
    {
        var input = "{ var a; var a; }";

        var expected = "There is already a variable with this name in this scope.";

        AssertInputGeneratesError(input, expected);
    }

    [Test]
    public static void Return_CannotBeTopLevel()
    {
        var input = "return 3;";

        var expected = "Can't return from top-level code.";

        AssertInputGeneratesError(input, expected);
    }

    #region Helper Methods
    private static void AssertInputGeneratesError(string input, string expected)
    {
        var errors = ProcessInput(input);

        Assert.That(errors, Has.Count.EqualTo(1));
        Assert.That(errors[0].Message, Is.EqualTo(expected));
    }

    private static List<ScopeError> ProcessInput(string input)
    {
        var (tokens, scanErrors) = Scanner.ScanTokens(input);

        Assert.That(scanErrors, Is.Empty);

        var (statements, parseErrors) = Parser.Parse(tokens);

        Assert.That(parseErrors, Is.Empty);

        var scopeErrors = Resolver.Resolve(new AstInterpreter(), statements);

        return scopeErrors.ToList();
    }
    #endregion
}
