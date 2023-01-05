using Interpreter.Framework.AST;
using Interpreter.Framework.Parsing;
using Interpreter.Framework.Scanning;

namespace Interpreter.Tests.ParsingTests;
[TestFixture]
internal static class ParserTests
{
    private static readonly Printer printer = new();

    #region Expressions (covers ExpressionStatement)
    [Test]
    public static void LiteralExpression_False()
    {
        var input = "false;";

        var expected = """
        ( expression
            ( false )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void LiteralExpression_True()
    {
        var input = "true;";

        var expected = """
        ( expression
            ( true )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void LiteralExpression_Nil()
    {
        var input = "nil;";

        var expected = """
        ( expression
            ( nil )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void LiteralExpression_Number()
    {
        var input = "1.23;";

        var expected = """
        ( expression
            ( 1.23 )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void LiteralExpression_String()
    {
        var input = "\"1.23\";";

        var expected = """
        ( expression
            ( "1.23" )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void LiteralExpression_Identifier()
    {
        var input = "foo;";

        var expected = """
        ( expression
            ( foo )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void GroupExpression()
    {
        var input = "( 1.23 );";

        var expected = """
        ( expression
            ( group
                ( 1.23 )
            )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void InvalidGroupExpression()
    {
        var input = "( 1.23";

        var expected = "Expect ')' after expression.";

        AssertInputGeneratesError(input, expected);
    }

    [Test]
    public static void InvalidExpression()
    {
        var input = ";";

        var expected = "Expect expression.";

        AssertInputGeneratesError(input, expected);
    }


    [Test]
    public static void UnaryExpression_Bang()
    {
        var input = "!True;";

        var expected = """
        ( expression
            ( !
                ( True )
            )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void UnaryExpression_Minus()
    {
        var input = "-1;";

        var expected = """
        ( expression
            ( -
                ( 1 )
            )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }

    #region Binary Expressions
    [Test]
    public static void FactorExpression_Slash()
    {
        var input = "1 / 3;";

        var expected = """
        ( expression
            ( /
                ( 1 )
                ( 3 )
            )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void FactorExpression_Star()
    {
        var input = "2 * 5;";

        var expected = """
        ( expression
            ( *
                ( 2 )
                ( 5 )
            )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void TermExpression_Plus()
    {
        var input = "4 + 6;";

        var expected = """
        ( expression
            ( +
                ( 4 )
                ( 6 )
            )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void TermExpression_Minus()
    {
        var input = "9 - 8;";

        var expected = """
        ( expression
            ( -
                ( 9 )
                ( 8 )
            )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }


    [Test]
    public static void ComparisonExpression_Greater()
    {
        var input = "1 > 2;";

        var expected = """
        ( expression
            ( >
                ( 1 )
                ( 2 )
            )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void ComparisonExpression_GreaterEqual()
    {
        var input = "3 >= 4;";

        var expected = """
        ( expression
            ( >=
                ( 3 )
                ( 4 )
            )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void ComparisonExpression_Less()
    {
        var input = "5 < 6;";

        var expected = """
        ( expression
            ( <
                ( 5 )
                ( 6 )
            )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void ComparisonExpression_LessEqual()
    {
        var input = "7 <= 8;";

        var expected = """
        ( expression
            ( <=
                ( 7 )
                ( 8 )
            )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }


    [Test]
    public static void EqualityExpression_BangEqual()
    {
        var input = "9 != 10;";

        var expected = """
        ( expression
            ( !=
                ( 9 )
                ( 10 )
            )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }


    [Test]
    public static void EquailtyExpression_EqualEqual()
    {
        var input = "11 == 12;";

        var expected = """
        ( expression
            ( ==
                ( 11 )
                ( 12 )
            )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }
    #endregion

    [Test]
    public static void AssignmentExpression()
    {
        var input = "a = 3;";

        var expected = """
        ( expression
            ( a =
                ( 3 )
            )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void AssignmentExpression_Invalid()
    {
        var input = "3 = 1;";

        var expected = "Invalid assignment target.";

        AssertInputGeneratesError(input, expected);
    }

    #region Logical Expressions

    [Test]
    public static void LogicalExpression_Or()
    {
        var input = "print \"hi\" or 2;";

        var expected = """
        ( print
            ( or
                ( "hi" )
                ( 2 )
            )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void LogicalExpression_And()
    {
        var input = "print nil and 2;";

        var expected = """
        ( print
            ( and
                ( nil )
                ( 2 )
            )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }
    #endregion
    #endregion

    #region Statements
    [Test]
    public static void BlockStatement()
    {
        var input = """
        { 
            a = 1; 
            b = 2; 
            a + b; 
        }
        """;

        var expected = """
        ( block
            ( expression
                ( a =
                    ( 1 )
                )
            )
            ( expression
                ( b =
                    ( 2 )
                )
            )
            ( expression
                ( +
                    ( a )
                    ( b )
                )
            )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }


    [Test]
    public static void BlockStatement_WithError()
    {
        var input = """
        { 
            a = 3; 
            b = 4; 
            5 = 6;
            print a + b;
        }
        """;

        var expectedError = "Invalid assignment target.";

        var expectedStatements = """
        ( block
            ( expression
                ( a =
                    ( 3 )
                )
            )
            ( expression
                ( b =
                    ( 4 )
                )
            )
            ( print
                ( +
                    ( a )
                    ( b )
                )
            )
        )
        """;

        var (statements, errors) = ProcessInput(input);

        Assert.That(errors, Has.Count.EqualTo(1));
        Assert.That(errors[0], Is.EqualTo(expectedError));

        Assert.That(printer.Print(statements), Is.EqualTo(expectedStatements));
    }


    [Test]
    public static void IfStatement_MissingLeftParen()
    {
        var input = "if 3;";

        var expected = "Expect '(' after 'if'.";

        AssertInputGeneratesError(input, expected);
    }

    [Test]
    public static void IfStatement_MissingRightParen()
    {
        var input = "if ( true;";

        var expected = "Expect ')' after if condition.";

        AssertInputGeneratesError(input, expected);
    }


    [Test]
    public static void IfStatement_NoElse()
    {
        var input = "if ( true ) print 1;";

        var expected = """
        ( if
            ( condition
                ( true )
            )
            ( then
                ( print
                    ( 1 )
                )
            )
            ( else )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void IfStatement_WithElse()
    {
        var input = """
        if ( true ) 
            print 1; 
        else 
            print 2;
        """;

        var expected = """
        ( if
            ( condition
                ( true )
            )
            ( then
                ( print
                    ( 1 )
                )
            )
            ( else
                ( print
                    ( 2 )
                )
            )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void PrintStatement()
    {
        var input = "print 3;";

        var expected = """
        ( print
            ( 3 )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void VariableDeclaration_NoInitializer()
    {
        var input = "var a;";

        var expected = """
        ( var a =
            ( nil )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void VariableDeclaration_WithInitializer()
    {
        var input = "var a = 3;";

        var expected = """
        ( var a =
            ( 3 )
        )
        """;

        AssertThatInputGeneratesProperTree(input, expected);
    }
    #endregion

    // TODO: validate synchronize

    #region Helper Methods
    // TODO: include line #s?
    private static (List<Statement> statements, List<string> error) ProcessInput(string input)
    {
        var (tokens, scanErrors) = Scanner.ScanTokens(input);

        Assert.That(scanErrors, Is.Empty);

        var (statements, parseErrors) = Parser.Parse(tokens);

        return (statements.ToList(), parseErrors.Select(e => e.Message).ToList());
    }

    private static void AssertThatInputGeneratesProperTree(string input, string expected)
    {
        var (statements, parseErrors) = ProcessInput(input);

        Assert.That(parseErrors, Is.Empty);

        Assert.That(printer.Print(statements), Is.EqualTo(expected));
    }

    private static void AssertInputGeneratesError(string input, string expected)
    {
        var (statements, parseErrors) = ProcessInput(input);

        var errors = parseErrors.ToList();
        Assert.That(errors, Has.Count.EqualTo(1));
        Assert.That(errors[0], Is.EqualTo(expected));

        Assert.That(statements, Is.Empty);
    }
    #endregion
}
