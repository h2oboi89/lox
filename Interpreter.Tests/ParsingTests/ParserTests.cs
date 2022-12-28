using Interpreter.Framework.AST;
using Interpreter.Framework.Parsing;
using Interpreter.Framework.Scanning;

namespace Interpreter.Tests.ParsingTests;
[TestFixture]
internal static class ParserTests
{
    private static readonly Printer printer = new();

    [Test]
    public static void CanParse_PrimaryExpressions()
    {
        var inputs = new List<(string, string)>
        {
            ( "123.456", "( 123.456 )" ),
            ( "\"foo the bar\"", "( foo the bar )"),
            ( "true" , "( True )" ),
            ( "false", "( False )" ),
            ( "nil", "( nil )" ),
            ( "( 0 )", "( group ( 0 ) )" ),
        };

        AssertInputsGenerateProperExpressions(inputs);
    }

    [Test]
    public static void CanParse_UnaryExpression()
    {
        var inputs = new List<(string, string)>
        {
            ( "!false", "( ! ( False ) )"),
            ( "! true", "( ! ( True ) )"),
            ( "-1", "( - ( 1 ) )"),
        };

        AssertInputsGenerateProperExpressions(inputs);
    }

    [Test]
    public static void CanParse_FactorExpressions()
    {
        var inputs = new List<(string, string)>
        {
            ( "1 * 2", "( * ( 1 ) ( 2 ) )" ),
            ( "3 / 0", "( / ( 3 ) ( 0 ) )" ),
        };

        AssertInputsGenerateProperExpressions(inputs);
    }

    [Test]
    public static void CanParse_TermExpressions()
    {
        var inputs = new List<(string, string)>
        {
            ( "1 + 2", "( + ( 1 ) ( 2 ) )" ),
            ( "3 - 0", "( - ( 3 ) ( 0 ) )" ),
        };

        AssertInputsGenerateProperExpressions(inputs);
    }

    [Test]
    public static void CanParse_ComparisonExpressions()
    {
        var inputs = new List<(string, string)>
        {
            ( "1 > 2", "( > ( 1 ) ( 2 ) )" ),
            ( "3 >= 0", "( >= ( 3 ) ( 0 ) )" ),
            ( "1 < 2", "( < ( 1 ) ( 2 ) )" ),
            ( "3 <= 0", "( <= ( 3 ) ( 0 ) )" ),
        };

        AssertInputsGenerateProperExpressions(inputs);
    }

    [Test]
    public static void CanParse_EqualityExpressions()
    {
        var inputs = new List<(string, string)>
        {
            ( "1 == 2", "( == ( 1 ) ( 2 ) )" ),
            ( "3 != 0", "( != ( 3 ) ( 0 ) )" ),
        };

        AssertInputsGenerateProperExpressions(inputs);
    }

    [Test]
    public static void CanHandle_LackOfExpression()
    {
        var tokens = GivenThatInputWasScannedWithoutErrors("+1");

        var (expression, parseErrors) = Parser.Parse(tokens);

        var errors = parseErrors.ToList();

        Assert.That(expression, Is.Null);

        Assert.That(errors, Has.Count.EqualTo(1));
        Assert.That(errors[0].Token.Line, Is.EqualTo(1));
        Assert.That(errors[0].Token.Lexeme, Is.EqualTo("+"));
        Assert.That(errors[0].Message, Is.EqualTo("Expect expression."));
    }

    [Test]
    public static void CanHandle_MissingCloseParentheses()
    {
        var tokens = GivenThatInputWasScannedWithoutErrors("( 1");

        var (expression, parseErrors) = Parser.Parse(tokens);

        var errors = parseErrors.ToList();

        Assert.That(expression, Is.Null);

        Assert.That(errors, Has.Count.EqualTo(1));
        Assert.That(errors[0].Token.Line, Is.EqualTo(1));
        Assert.That(errors[0].Token.Type, Is.EqualTo(TokenType.EOF));
        Assert.That(errors[0].Message, Is.EqualTo("Expect ')' after expression."));
    }

    #region Helper Methods
    private static IEnumerable<Token> GivenThatInputWasScannedWithoutErrors(string input)
    {
        var (tokens, scanErrors) = Scanner.ScanTokens(input);

        Assert.That(scanErrors, Is.Empty);

        return tokens;
    }

    private static void AssertInputsGenerateProperExpressions(List<(string input, string expected)> inputs)
    {
        foreach (var (input, expected) in inputs)
        {
            var tokens = GivenThatInputWasScannedWithoutErrors(input);

            var (expression, parseErrors) = Parser.Parse(tokens);

            Assert.That(parseErrors, Is.Empty);
            Assert.That(expression, Is.Not.Null);

            Assert.That(printer.Print(expression), Is.EqualTo(expected));
        }
    }
    #endregion
}
