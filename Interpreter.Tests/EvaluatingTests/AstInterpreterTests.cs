using Interpreter.Framework;
using Interpreter.Framework.Evaluating;
using Interpreter.Framework.Parsing;
using Interpreter.Framework.Scanning;

namespace Interpreter.Tests.EvaluatingTests;
internal static class AstInterpreterTests
{
    #region Setup and Teardown
    private static AstInterpreter interpreter;

    private static readonly List<string> output = new();

    private static void OnOutput(object? sender, InterpreterOutEventArgs output)
    {
        AstInterpreterTests.output.Add(output.Content);
    }

    [SetUp]
    public static void Setup()
    {
        interpreter = new AstInterpreter();

        interpreter.Out += OnOutput;

        output.Clear();
    }

    [TearDown]
    public static void TearDown()
    {
        interpreter.Out -= OnOutput;
    }
    #endregion

    [Test]
    public static void Interpreter_StartsWithBlankState()
    {
        var input = "print a;";

        var expected = "Undefined variable 'a'.";

        AssertInputGeneratesProperError(input, expected);
    }

    [Test]
    public static void Interpreter_CanBeReset()
    {
        var input = """
        var a = 1;
        print a;
        """;

        var expected = "1";

        AssertInputGeneratesProperOutput(input, expected);

        interpreter.Reset();
        output.Clear();

        AssertInputGeneratesProperError("print a;", "Undefined variable 'a'.");
    }

    #region Binary Expressions
    [Test]
    public static void Binary_Plus_Numbers()
    {
        var input = "print 1 + 1;";

        var expected = "2";

        AssertInputGeneratesProperOutput(input, expected);
    }

    [Test]
    public static void Binary_Plus_Strings()
    {
        var input = """print "foo" + "bar";""";

        var expected = "foobar";

        AssertInputGeneratesProperOutput(input, expected);
    }


    [Test]
    public static void Binary_Plus_Invalid()
    {
        var input = """print 1 + "foo";""";

        var expected = "Operands must be two numbers or two strings.";

        AssertInputGeneratesProperError(input, expected);
    }


    [Test]
    public static void Binary_Minus_Valid()
    {
        var input = "print 3.14 - 1;";

        var expected = "2.14";

        AssertInputGeneratesProperOutput(input, expected);
    }


    [Test]
    public static void Binary_Minus_Invalid()
    {
        var inputs = new List<string>
        {
            "1 - true;",
            "false - 1;"
        };

        var expected = "Operands must be numbers.";

        foreach (var input in inputs)
        {
            AssertInputGeneratesProperError(input, expected);
        }
    }

    [Test]
    public static void Binary_Slash_Valid()
    {
        var input = "print 9 / 3;";

        var expected = "3";

        AssertInputGeneratesProperOutput(input, expected);
    }


    [Test]
    public static void Binary_Slash_Invalid()
    {
        var inputs = new List<string>
        {
            "2 / true;",
            "false / 2;"
        };

        var expected = "Operands must be numbers.";

        foreach (var input in inputs)
        {
            AssertInputGeneratesProperError(input, expected);
        }
    }

    [Test]
    public static void Binary_Star_Valid()
    {
        var input = "print 4 * 5;";

        var expected = "20";

        AssertInputGeneratesProperOutput(input, expected);
    }


    [Test]
    public static void Binary_Star_Invalid()
    {
        var inputs = new List<string>
        {
            "3 * true;",
            "false * 3;"
        };

        var expected = "Operands must be numbers.";

        foreach (var input in inputs)
        {
            AssertInputGeneratesProperError(input, expected);
        }
    }

    [Test]
    public static void Binary_Greater_Valid()
    {
        var values = new List<(string, string)>
        {
            ( "print 7 >= 8;", "false" ),
            ( "print 9 >= 9;", "true" )
        };

        AssertInputsGenerateProperOutputs(values);
    }


    [Test]
    public static void Binary_Greater_Invalid()
    {
        var inputs = new List<string>
        {
            "4 > true;",
            "false > 4;"
        };

        var expected = "Operands must be numbers.";

        foreach (var input in inputs)
        {
            AssertInputGeneratesProperError(input, expected);
        }
    }

    [Test]
    public static void Binary_GreaterEqual_Valid()
    {
        var values = new List<(string, string)>
        {
            ( "print 10 >= 11;", "false" ),
            ( "print 12 >= 12;", "true" )
        };

        AssertInputsGenerateProperOutputs(values);
    }

    [Test]
    public static void Binary_GreaterEqual_Invalid()
    {
        var inputs = new List<string>
        {
            "5 >= true;",
            "false >= 5;"
        };

        var expected = "Operands must be numbers.";

        foreach (var input in inputs)
        {
            AssertInputGeneratesProperError(input, expected);
        }
    }

    [Test]
    public static void Binary_Less_Valid()
    {
        var values = new List<(string, string)>
        {
            ( "print 13 < 14;", "true" ),
            ( "print 15 < 15;", "false" )
        };

        AssertInputsGenerateProperOutputs(values);
    }


    [Test]
    public static void Binary_Less_Invalid()
    {
        var inputs = new List<string>
        {
            "6 < true;",
            "false < 6;"
        };

        var expected = "Operands must be numbers.";

        foreach (var input in inputs)
        {
            AssertInputGeneratesProperError(input, expected);
        }
    }

    [Test]
    public static void Binary_LessEqual_Valid()
    {
        var values = new List<(string, string)>
        {
            ( "print 16 <= 16;", "true" ),
            ( "print 18 <= 17;", "false" )
        };

        AssertInputsGenerateProperOutputs(values);
    }

    [Test]
    public static void Binary_LessEqual_Invalid()
    {
        var inputs = new List<string>
        {
            "7 <= true;",
            "false <= 7;"
        };

        var expected = "Operands must be numbers.";

        foreach (var input in inputs)
        {
            AssertInputGeneratesProperError(input, expected);
        }
    }

    [Test]
    public static void Binary_BangEqual_Valid()
    {
        var values = new List<(string, string)>
        {
            ( "print 19 != 20;", "true" ),
            ( "print 21 != 21;", "false" ),
            ( "print nil != nil;", "false" ),
            ( "print true != false;", "true" ),
            ( "print nil != true;", "true" )
        };

        AssertInputsGenerateProperOutputs(values);
    }

    [Test]
    public static void Binary_EqualEqual_Valid()
    {
        var values = new List<(string, string)>
        {
            ( "print 19 == 20;", "false" ),
            ( "print 21 == 21;", "true" ),
            ( "print nil == nil;", "true" ),
            ( "print true == false;", "false" ),
            ( "print nil == true;", "false" )
        };

        AssertInputsGenerateProperOutputs(values);
    }
    #endregion


    [Test]
    public static void GroupingExpression()
    {
        var input = "print 6 / ( 3 - 1 );";

        var expected = "3";

        AssertInputGeneratesProperOutput(input, expected);
    }

    [Test]
    public static void LogicalExpression_Or_False()
    {
        var input = "print false or 1;";

        var expected = "1";

        AssertInputGeneratesProperOutput(input, expected);
    }

    [Test]
    public static void LogicalExpression_Or_True()
    {
        var input = "print true or 1;";

        var expected = "true";

        AssertInputGeneratesProperOutput(input, expected);
    }

    [Test]
    public static void LogicalExpression_And_False()
    {
        var input = "print false and 1;";

        var expected = "false";

        AssertInputGeneratesProperOutput(input, expected);
    }

    [Test]
    public static void LogicalExpression_And_True()
    {
        var input = "print true and 1;";

        var expected = "1";

        AssertInputGeneratesProperOutput(input, expected);
    }

    [Test]
    public static void UnaryExpression_Minus_Valid()
    {
        var values = new List<(string, string)>
        {
            ( "print -1;", "-1" ),
            ( "print -(-1);", "1" ),
            ( "print --1;", "1" ),
        };

        AssertInputsGenerateProperOutputs(values);
    }

    [Test]
    public static void UnaryExpression_Minus_Invalid()
    {
        var input = "-true;";

        var expected = "Operand must be a number.";

        AssertInputGeneratesProperError(input, expected);
    }

    [Test]
    public static void UnaryExpression_Bang_Valid()
    {
        var values = new List<(string, string)>
        {
            ( "print !nil;", "true" ),
            ( "print !true;", "false" ),
            ( "print !false;", "true" ),
            ( "print !!true;", "true" ),
            ( "print !!false;", "false" ),
            ( "print !0;", "false" ),
            ( "print !1;", "false" ),
            ( "print !\"foo\";", "false" ),
        };

        AssertInputsGenerateProperOutputs(values);
    }


    [Test]
    public static void Variables()
    {
        var input = """
        var a;
        a = 1;
        var b = 2;
        print a + b;
        """;

        var expected = "3";

        AssertInputGeneratesProperOutput(input, expected);
    }


    [Test]
    public static void Block()
    {
        var input = """
        var a = 7;
        var b;

        {
            b = 6;
            var c = 10;
            print a * b * c;
        }
        """;

        var expected = "420";

        AssertInputGeneratesProperOutput(input, expected);
    }


    [Test]
    public static void Block_VariablesUndefinedAfterBlock()
    {
        var input = """
        {
            var a = 1;
            var b = 2;
        }

        print a + b;
        """;

        var expected = "Undefined variable 'a'.";

        AssertInputGeneratesProperError(input, expected);
    }

    [Test]
    public static void Block_VariablesUndefinedRecursiveLookup()
    {
        var input = """
        {
            a = 1;
        }
        """;

        var expected = "Undefined variable 'a'.";

        AssertInputGeneratesProperError(input, expected);
    }


    [Test]
    public static void If_True_NoElse()
    {
        var input = """
        if ( true ) print 1;
        """;

        var expected = "1";

        AssertInputGeneratesProperOutput(input, expected);
    }

    [Test]
    public static void If_False_NoElse()
    {
        var input = """
        if ( false ) print 1;
        """;

        AssertInputGeneratesNoOutput(input);
    }

    [Test]
    public static void If_True_WithElse()
    {
        var input = """
        if ( true ) print 1; else print 2;
        """;

        var expected = "1";

        AssertInputGeneratesProperOutput(input, expected);
    }

    [Test]
    public static void If_False_WithElse()
    {
        var input = """
        if ( false ) print 1; else print 2;
        """;

        var expected = "2";

        AssertInputGeneratesProperOutput(input, expected);
    }

    [Test]
    public static void Print()
    {
        var input = "print 3.14;";

        var expected = "3.14";

        AssertInputGeneratesProperOutput(input, expected);
    }

    [Test]
    public static void While()
    {
        var input = """
        var i = 0;
        
        while ( i < 3 )
        {
            print i;
            i = i + 1;
        }
        """;

        var expected = new List<string>
        {
            "0", "1", "2"
        };

        AssertInputGeneratesProperOutputs(input, expected);
    }

    #region Helper Methods
    private static LoxRuntimeError? ProcessInput(string input)
    {
        var (tokens, scanErrors) = Scanner.ScanTokens(input);

        Assert.That(scanErrors, Is.Empty);

        var (statements, parseErrors) = Parser.Parse(tokens);

        Assert.That(parseErrors, Is.Empty);

        return interpreter.Interpret(statements);
    }

    private static void AssertInputGeneratesProperOutput(string input, string expected)
    {
        var error = ProcessInput(input);

        Assert.That(error, Is.Null);

        Assert.That(output, Has.Count.EqualTo(1));
        Assert.That(output[0], Is.EqualTo(expected));
    }

    private static void AssertInputGeneratesProperOutputs(string input, List<string> expected)
    {
        var error = ProcessInput(input);

        Assert.That(error, Is.Null);

        Assert.That(output.Count, Is.EqualTo(expected.Count));
        
        for(var i = 0; i < expected.Count; i++)
        {
            Assert.That(output[i], Is.EqualTo(expected[i]));
        }
    }

    private static void AssertInputGeneratesNoOutput(string input)
    {
        var error = ProcessInput(input);

        Assert.That(error, Is.Null);
        Assert.That(output, Is.Empty);
    }

    private static void AssertInputsGenerateProperOutputs(List<(string, string)> values)
    {
        var i = 0;
        foreach (var (input, expected) in values)
        {
            var error = ProcessInput(input);

            Assert.That(error, Is.Null);

            Assert.That(output, Has.Count.EqualTo(i + 1));
            Assert.That(output[i++], Is.EqualTo(expected));
        }
    }

    private static void AssertInputGeneratesProperError(string input, string expected)
    {
        var error = ProcessInput(input);

        Assert.That(error, Is.Not.Null);
        Assert.That(error.Message, Is.EqualTo(expected));

        Assert.That(output, Is.Empty);
    }
    #endregion
}
