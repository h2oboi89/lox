using Interpreter.Framework.AST;
using Interpreter.Framework.Evaluating;
using Interpreter.Framework.Parsing;
using Interpreter.Framework.Scanning;

namespace Interpreter.Tests.EvaluatingTests;
internal static class AstInterpreterTests
{
    // TODO: lower case True and False
    [Test]
    public static void BinaryExpressions_Numeric()
    {
        var inputs = new List<(string, string)>
        {
            ( "1 - 1", "0" ),
            ( "2 + 3", "5" ),
            ( "10 / 4", "2.5" ),
            ( "6 * 7", "42"),
            ( "3.14 > 2.72", "True" ),
            ( "1 >= 1", "True" ),
            ( "5 < 2", "False" ),
            ( "5 <= 6", "True" ),
            ( "1 != 1", "False" ),
            ( "1 == 1", "True" ),
            ( "1 / 3", "0.3333333333333333")
        };

        AssertInputsEvaluateToOutputs(inputs);
    }

    [Test]
    public static void BinaryExpressions_NonNumericEquality()
    {
        var inputs = new List<(string, string)>
        {
            ( "nil != nil", "False" ),
            ( "nil == nil", "True" ),
            ( "nil == 1", "False" ),
            ( "nil != 1", "True" ),
            ( "1 != nil", "True" ),
            ( "1 == nil", "False" ),
            ( """ "foo" == "bar" """, "False" ),
            ( """ "foo" != "bar" """, "True" ),
        };

        AssertInputsEvaluateToOutputs(inputs);
    }

    [Test]
    public static void BinaryExpressions_StringConcatenation()
    {
        var inputs = new List<(string, string)>
        {
            ( """ "foo" + "bar" """, "foobar" ),
        };

        AssertInputsEvaluateToOutputs(inputs);
    }

    [Test]
    public static void BinaryExpressions_InvalidPlus()
    {
        var inputs = new List<string> {
            """ 1 + "foo" """,
            """" "foo" + 1 """"
        };

        foreach (var input in inputs)
        {
            var interpreter = new AstInterpreter();

            var expression = GivenThatInputWasScannedAndParsedWithoutErrors(input);

            var (value, runtimeError) = interpreter.Execute(expression);

            Assert.That(runtimeError, Is.Not.Null);
            Assert.That(value, Is.Empty);

            Assert.That(runtimeError.Message, Is.EqualTo("Operands must be two numbers or two strings."));
        }
    }

    [Test]
    public static void BinaryExpressions_InvalidNumeric()
    {
        var inputs = new List<string>
        {
            "2 - true", "true - 2",
            "3 * true", "true * 3",
            "4 / true", "true / 4",

            "5 > false", "false > 5",
            "6 >= false", "false >= 6",
            "7 < false", "false < 5",
            "8 <= false", "false <= 8",
        };

        foreach (var input in inputs)
        {
            var interpreter = new AstInterpreter();

            var expression = GivenThatInputWasScannedAndParsedWithoutErrors(input);

            var (value, runtimeError) = interpreter.Execute(expression);

            Assert.That(runtimeError, Is.Not.Null);
            Assert.That(value, Is.Empty);

            Assert.That(runtimeError.Message, Is.EqualTo("Operands must be numbers."));
        }
    }

    [Test]
    public static void GroupingExpressions()
    {
        var inputs = new List<(string, string)>
        {
            ( "6 / 3 - 1", "1" ),
            ( "(6 / 3) - 1", "1" ),
            ( "6 / (3 - 1)", "3" ),
            ( "( nil )", "nil" ),
        };

        AssertInputsEvaluateToOutputs(inputs);
    }

    [Test]
    public static void UnaryExpressions()
    {
        var inputs = new List<(string, string)>
        {
            ( "5 + -1", "4" ),
            ( "!true", "False" ),
            ( "!false", "True" ),
            ( "!nil", "True" ),
            ( "!0", "False" ),
            ( "!1", "False" ),
            ( """ !"foo" """, "False")
        };

        AssertInputsEvaluateToOutputs(inputs);
    }

    [Test]
    public static void UnaryExpressions_InvalidNumeric()
    {
        var inputs = new List<string>
        {
            "-true"
        };

        foreach (var input in inputs)
        {
            var interpreter = new AstInterpreter();

            var expression = GivenThatInputWasScannedAndParsedWithoutErrors(input);

            var (value, runtimeError) = interpreter.Execute(expression);

            Assert.That(runtimeError, Is.Not.Null);
            Assert.That(value, Is.Empty);

            Assert.That(runtimeError.Message, Is.EqualTo("Operand must be a number."));
        }
    }

    #region Helper Methods
    private static Expression GivenThatInputWasScannedAndParsedWithoutErrors(string input)
    {
        var (tokens, scanErrors) = Scanner.ScanTokens(input);

        Assert.That(scanErrors, Is.Empty);

        var (expression, parseErrors) = Parser.Parse(tokens);

        Assert.That(parseErrors, Is.Empty);
        Assert.That(expression, Is.Not.Null);

        return expression;
    }

    private static void AssertInputsEvaluateToOutputs(List<(string input, string expected)> inputs)
    {
        foreach (var (input, expected) in inputs)
        {
            var interpreter = new AstInterpreter();

            var expression = GivenThatInputWasScannedAndParsedWithoutErrors(input);

            var (value, runtimeError) = interpreter.Execute(expression);

            Assert.Multiple(() =>
            {
                Assert.That(runtimeError, Is.Null);
                Assert.That(value, Is.EqualTo(expected));
            });
        }
    }
    #endregion
}
