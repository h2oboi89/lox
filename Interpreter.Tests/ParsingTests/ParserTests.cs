using Interpreter.Framework.AST;
using Interpreter.Framework.Parsing;
using Interpreter.Framework.Scanning;
using System.Text;

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

        AssertInputGeneratesProperTree(input, expected);
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

        AssertInputGeneratesProperTree(input, expected);
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

        AssertInputGeneratesProperTree(input, expected);
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

        AssertInputGeneratesProperTree(input, expected);
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

        AssertInputGeneratesProperTree(input, expected);
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

        AssertInputGeneratesProperTree(input, expected);
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

        AssertInputGeneratesProperTree(input, expected);
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

        AssertInputGeneratesProperTree(input, expected);
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

        AssertInputGeneratesProperTree(input, expected);
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

        AssertInputGeneratesProperTree(input, expected);
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

        AssertInputGeneratesProperTree(input, expected);
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

        AssertInputGeneratesProperTree(input, expected);
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

        AssertInputGeneratesProperTree(input, expected);
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

        AssertInputGeneratesProperTree(input, expected);
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

        AssertInputGeneratesProperTree(input, expected);
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

        AssertInputGeneratesProperTree(input, expected);
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

        AssertInputGeneratesProperTree(input, expected);
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

        AssertInputGeneratesProperTree(input, expected);
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

        AssertInputGeneratesProperTree(input, expected);
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

        AssertInputGeneratesProperTree(input, expected);
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

        AssertInputGeneratesProperTree(input, expected);
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

        AssertInputGeneratesProperTree(input, expected);
    }
    #endregion

    [Test]
    public static void CallExpression_MissingCloseParen()
    {
        var input = "foo( 1;";

        var expected = "Expect ')' after arguments.";

        AssertInputGeneratesError(input, expected);
    }


    [Test]
    public static void CallExpression_TooManyArgs()
    {
        var argCount = 256;
        var input = $"foo( {string.Join(", ", Enumerable.Repeat(1, argCount))} );";

        var expectedOutput = new StringBuilder();
        expectedOutput.AppendLine("( expression");
        expectedOutput.AppendLine("    ( call");
        expectedOutput.AppendLine("        ( callee");
        expectedOutput.AppendLine("            ( foo )");
        expectedOutput.AppendLine("        )");
        expectedOutput.AppendLine("        ( arguments");
        for (var i = 0; i < argCount; i++)
        {
            expectedOutput.AppendLine("            ( 1 )");
        }
        expectedOutput.AppendLine("        )");
        expectedOutput.AppendLine("    )");
        expectedOutput.Append(")");

        var expectedError = "Can't have more than 255 arguments.";

        AssertInputGeneratesExpected(input, expectedOutput.ToString(), expectedError);
    }

    [Test]
    public static void CallExpression_NoArgs()
    {
        var input = "foo();";

        var expected = """
        ( expression
            ( call
                ( callee
                    ( foo )
                )
                ( arguments )
            )
        )
        """;

        AssertInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void CallExpression_Args()
    {
        var input = "foo( 1, 2, 3 );";

        var expected = """
        ( expression
            ( call
                ( callee
                    ( foo )
                )
                ( arguments
                    ( 1 )
                    ( 2 )
                    ( 3 )
                )
            )
        )
        """;

        AssertInputGeneratesProperTree(input, expected);
    }
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

        AssertInputGeneratesProperTree(input, expected);
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
    public static void Class_MissingClassName()
    {
        var input = "class;";

        var expected = "Expect class name.";

        AssertInputGeneratesError(input, expected);
    }

    [Test]
    public static void Class_MissingLeftBrace()
    {
        var input = "class foo;";

        var expected = "Expect '{' before class body.";

        AssertInputGeneratesError(input, expected);
    }

    [Test]
    public static void Class_MissingRightBrace()
    {
        var input = "class foo { bar() { }";

        var expected = "Expect '}' after class body.";

        AssertInputGeneratesError(input, expected);
    }

    [Test]
    public static void Class_Minimal()
    {
        var input = "class foo { }";

        var expected = """
        ( class foo
            ( methods )
        )
        """;

        AssertInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void Class_WithMethods()
    {
        var input = """
        class foo {
            init() { 
                this.baz = 1; 
            }

            bar() { 
                print this.baz;
            }
        }
        """;

        var expected = """
        ( class foo
            ( methods
                ( function init
                    ( parameters )
                    ( body
                        ( expression
                            ( set baz
                                ( this )
                                ( 1 )
                            )
                        )
                    )
                )
                ( function bar
                    ( parameters )
                    ( body
                        ( print
                            ( get baz
                                ( this )
                            )
                        )
                    )
                )
            )
        )
        """;

        AssertInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void For_MissingLeftParen()
    {
        var input = "for 1;";

        var expected = "Expect '(' after 'for'.";

        AssertInputGeneratesError(input, expected);
    }

    [Test]
    public static void For_MissingRightParen()
    {
        var input = "for (;;1";

        var expected = "Expect ')' after for clauses.";

        AssertInputGeneratesError(input, expected);
    }

    [Test]
    public static void For_Empty()
    {
        var input = "for (;;) { }";

        var expected = """
        ( while
            ( condition
                ( true )
            )
            ( body
                ( block )
            )
        )
        """;

        AssertInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void For_Standard()
    {
        var input = "for ( var i = 0; i < 10; i = i + 1 ) { print i; }";

        var expected = """
        ( block
            ( var i =
                ( 0 )
            )
            ( while
                ( condition
                    ( <
                        ( i )
                        ( 10 )
                    )
                )
                ( body
                    ( block
                        ( block
                            ( print
                                ( i )
                            )
                        )
                        ( expression
                            ( i =
                                ( +
                                    ( i )
                                    ( 1 )
                                )
                            )
                        )
                    )
                )
            )
        )
        """;

        AssertInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void For_ExpressionInit()
    {
        var input = "for ( i = 0; i < 10; i = i + 1 ) { print i; }";

        var expected = """
        ( block
            ( expression
                ( i =
                    ( 0 )
                )
            )
            ( while
                ( condition
                    ( <
                        ( i )
                        ( 10 )
                    )
                )
                ( body
                    ( block
                        ( block
                            ( print
                                ( i )
                            )
                        )
                        ( expression
                            ( i =
                                ( +
                                    ( i )
                                    ( 1 )
                                )
                            )
                        )
                    )
                )
            )
        )
        """;

        AssertInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void FunctionStatement_MissingName()
    {
        var input = "fun;";

        var expected = "Expect function name.";

        AssertInputGeneratesError(input, expected);
    }

    [Test]
    public static void FunctionStatement_MissingLeftParen()
    {
        var input = "fun foo;";

        var expected = "Expect '(' after function name.";

        AssertInputGeneratesError(input, expected);
    }

    [Test]
    public static void FunctionStatement_MissingRightParen()
    {
        var input = "fun foo( a;";

        var expected = "Expect ')' after parameters.";

        AssertInputGeneratesError(input, expected);
    }

    [Test]
    public static void FunctionStatement_TooManyArgs()
    {
        var argCount = 256;
        var parameters = new string[argCount];

        for (var i = 0; i < argCount; i++)
        {
            parameters[i] = $"a{i}";
        }

        var input = $"fun foo( {string.Join(", ", parameters)} ) {{ print true; }}";

        var expectedOutput = new StringBuilder();
        expectedOutput.AppendLine("( function foo");
        expectedOutput.AppendLine("    ( parameters");
        for (var i = 0; i < argCount; i++)
        {
            expectedOutput.AppendLine($"        ( a{i} )");
        }
        expectedOutput.AppendLine("    )");
        expectedOutput.AppendLine("    ( body");
        expectedOutput.AppendLine("        ( print");
        expectedOutput.AppendLine("            ( true )");
        expectedOutput.AppendLine("        )");
        expectedOutput.AppendLine("    )");
        expectedOutput.Append(")");

        var expectedError = "Can't have more than 255 parameters.";

        AssertInputGeneratesExpected(input, expectedOutput.ToString(), expectedError);
    }

    [Test]
    public static void FunctionStatement_InvalidParameter()
    {
        var input = "fun foo( 1 ) { print true; }";

        var expected = """
        ( print
            ( true )
        )
        """;

        AssertInputGeneratesExpected(input, expected, "Expect parameter name.", "Expect expression.");
    }

    [Test]
    public static void FunctionStatement_NoParameters()
    {
        var input = "fun foo() { print true; }";

        var expected = """
        ( function foo
            ( parameters )
            ( body
                ( print
                    ( true )
                )
            )
        )
        """;

        AssertInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void FunctionStatement_WithParameters()
    {
        var input = "fun foo( a, b, c ) { print a + b + c; }";

        var expected = """
        ( function foo
            ( parameters
                ( a )
                ( b )
                ( c )
            )
            ( body
                ( print
                    ( +
                        ( +
                            ( a )
                            ( b )
                        )
                        ( c )
                    )
                )
            )
        )
        """;

        AssertInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void FunctionStatement_WithEmptyReturn()
    {
        var input = "fun foo() { return; }";

        var expected = """
        ( function foo
            ( parameters )
            ( body
                ( return )
            )
        )
        """;

        AssertInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void FunctionStatement_WithReturn()
    {
        var input = "fun foo( a, b, c ) { return a + b + c; }";

        var expected = """
        ( function foo
            ( parameters
                ( a )
                ( b )
                ( c )
            )
            ( body
                ( return
                    ( +
                        ( +
                            ( a )
                            ( b )
                        )
                        ( c )
                    )
                )
            )
        )
        """;

        AssertInputGeneratesProperTree(input, expected);
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

        AssertInputGeneratesProperTree(input, expected);
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

        AssertInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void IfStatement_Blocks()
    {
        var input = """
        if ( true ) 
        {
            print 1; 
        }
        else 
        {
            print 2;
        }
        """;

        var expected = """
        ( if
            ( condition
                ( true )
            )
            ( then
                ( block
                    ( print
                        ( 1 )
                    )
                )
            )
            ( else
                ( block
                    ( print
                        ( 2 )
                    )
                )
            )
        )
        """;

        AssertInputGeneratesProperTree(input, expected);
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

        AssertInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void While_MissingLeftParen()
    {
        var input = "while 1;";

        var expected = "Expect '(' after 'while'.";

        AssertInputGeneratesError(input, expected);
    }

    [Test]
    public static void While_MissingRightParen()
    {
        var input = "while ( true 1;";

        var expected = "Expect ')' after condition.";

        AssertInputGeneratesError(input, expected);
    }

    [Test]
    public static void While_SimpleBody()
    {
        var input = """
        while ( true )
            print "hi!";
        """;

        var expected = """
        ( while
            ( condition
                ( true )
            )
            ( body
                ( print
                    ( "hi!" )
                )
            )
        )
        """;

        AssertInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void While_BlockBody()
    {
        var input = """
        while ( true )
        {
            var a = 1;
            var b = 2;
            print a + b;
        }
        """;

        var expected = """
        ( while
            ( condition
                ( true )
            )
            ( body
                ( block
                    ( var a =
                        ( 1 )
                    )
                    ( var b =
                        ( 2 )
                    )
                    ( print
                        ( +
                            ( a )
                            ( b )
                        )
                    )
                )
            )
        )
        """;

        AssertInputGeneratesProperTree(input, expected);
    }

    [Test]
    public static void VariableDeclaration_NoInitializer()
    {
        var input = "var a;";

        var expected = "( var a )";

        AssertInputGeneratesProperTree(input, expected);
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

        AssertInputGeneratesProperTree(input, expected);
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

    private static void AssertInputGeneratesProperTree(string input, string expected) =>
        AssertInputGeneratesExpected(input, expected);

    private static void AssertInputGeneratesError(string input, params string[] expected) =>
        AssertInputGeneratesExpected(input, null, expected);

    private static void AssertInputGeneratesExpected(string input, string? expectedOutput = null, params string[] expectedErrors)
    {
        var (statements, parseErrors) = ProcessInput(input);

        if (!expectedErrors.Any())
        {
            Assert.That(parseErrors, Is.Empty);
        }
        else
        {
            var errors = parseErrors.ToList();
            Assert.That(errors, Has.Count.EqualTo(expectedErrors.Length));

            for (var i = 0; i < errors.Count; i++)
            {
                Assert.That(errors[i], Is.EqualTo(expectedErrors[i]));
            }
        }

        if (expectedOutput == null)
        {
            Assert.That(statements, Is.Empty);
        }
        else
        {
            Assert.That(printer.Print(statements), Is.EqualTo(expectedOutput));
        }
    }
    #endregion
}
