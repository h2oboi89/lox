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
        var input = """
        print 7 >= 8;
        print 9 >= 9;
        """;

        var expected = new string[] { "false", "true" };

        AssertInputGeneratesProperOutputs(input, expected);
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
        var input = """
        print 10 >= 11;
        print 12 >= 12;
        """;

        var expected = new string[] { "false", "true" };

        AssertInputGeneratesProperOutputs(input, expected);
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
        var input = """
        print 13 < 14;
        print 15 < 15;
        """;

        var expected = new string[] { "true", "false" };

        AssertInputGeneratesProperOutputs(input, expected);
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
        var input = """
        print 16 <= 16;
        print 18 <= 17;
        """;

        var expected = new string[] { "true", "false" };

        AssertInputGeneratesProperOutputs(input, expected);
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
        var input = """
        print 19 != 20;
        print 21 != 21;
        print nil != nil;
        print true != false;
        print nil != true;
        """;

        var expected = new string[] { "true", "false", "false", "true", "true" };

        AssertInputGeneratesProperOutputs(input, expected);
    }

    [Test]
    public static void Binary_EqualEqual_Valid()
    {
        var input = """
        print 19 == 20;
        print 21 == 21;
        print nil == nil;
        print true == false;
        print nil == true;
        """;

        var expected = new string[] { "false", "true", "true", "false", "false" };

        AssertInputGeneratesProperOutputs(input, expected);
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
        var input = """
        print -1;
        print -(-1);
        print --1;
        """;

        var expected = new string[] { "-1", "1", "1", };

        AssertInputGeneratesProperOutputs(input, expected);
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
        var input = """
        print !nil;
        print !true;
        print !false;
        print !!true;
        print !!false;
        print !0;
        print !1;
        print !"foo";
        """;

        var expected = new string[] {
            "true", "false", "true", "true", "false", "false", "false", "false",
        };

        AssertInputGeneratesProperOutputs(input, expected);
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
    public static void For()
    {
        var input = """
        var a = 0;
        var temp;

        for (var b = 1; a < 10000; b = temp + b) {
            print a;
            temp = a;
            a = b;
        }
        """;

        var expected = new List<string>
        {
            "0", "1", "1", "2", "3",
            "5", "8", "13", "21", "34",
            "55", "89", "144", "233", "377",
            "610", "987", "1597", "2584", "4181",
            "6765"
        };

        AssertInputGeneratesProperOutputs(input, expected);
    }

    [Test]
    public static void Function_InvalidArity()
    {
        var input = """
        fun foo() { print true; }

        foo( 1 );
        """;

        var expected = "Expected 0 arguments but got 1.";

        AssertInputGeneratesProperError(input, expected);
    }

    [Test]
    public static void Function_NotAFunction()
    {
        var input = "true();";

        var expected = "Can only call functions and classes.";

        AssertInputGeneratesProperError(input, expected);
    }

    [Test]
    public static void Function_NoArgs()
    {
        var input = """
        fun foo() { print true; }

        foo();
        """;

        var expected = "true";

        AssertInputGeneratesProperOutput(input, expected);
    }

    [Test]
    public static void Function_WithArgs()
    {
        var input = """
        fun foo( a, b, c ) { print a + b + c; }

        foo( 1, 2, 3 );
        """;

        var expected = "6";

        AssertInputGeneratesProperOutput(input, expected);
    }

    [Test]
    public static void Function_Print()
    {
        var input = """
        fun foo( a, b, c ) { print a + b + c; }

        print foo;
        """;

        var expected = "<function foo>";

        AssertInputGeneratesProperOutput(input, expected);
    }

    [Test]
    public static void Function_ReturnStatement()
    {
        var input = """
        fun foo( a, b, c ) { return a + b + c; }

        print foo( 1, 2, 3);
        """;

        var expected = "6";

        AssertInputGeneratesProperOutput(input, expected);
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

    #region Globals
    [Test]
    public static void Global_Clock()
    {
        var input = "print clock();";

        ProcessInput(input);

        Assert.That(output, Has.Count.EqualTo(1));
        var parsed = int.TryParse(output[0], out var time);

        Assert.That(parsed, Is.True);
        Assert.That(time, Is.GreaterThan(0));
    }

    [Test]
    public static void Global_Reset()
    {
        var input = """
        var a = 1;

        print a;

        reset();

        print a;
        """;

        AssertInputGeneratesExpected(input, "Undefined variable 'a'.", "1");
    }

    [Test]
    public static void Global_FunctionsPrinted()
    {
        var input = """
        print clock;
        print reset;
        """;

        var expected = new string[]
        {
            "<function native>",
            "<function native>",
        };

        AssertInputGeneratesProperOutputs(input, expected);
    }
    #endregion

    #region Helper Methods
    private static LoxRuntimeError? ProcessInput(string input)
    {
        var (tokens, scanErrors) = Scanner.ScanTokens(input);

        Assert.That(scanErrors, Is.Empty);

        var (statements, parseErrors) = Parser.Parse(tokens);

        Assert.That(parseErrors, Is.Empty);

        return interpreter.Interpret(statements);
    }

    private static void AssertInputGeneratesProperOutput(string input, string expected) =>
        AssertInputGeneratesExpected(input, null, expected);

    private static void AssertInputGeneratesProperOutputs(string input, IEnumerable<string> expected) =>
        AssertInputGeneratesExpected(input, null, expected);

    private static void AssertInputGeneratesNoOutput(string input) =>
        AssertInputGeneratesExpected(input, null);

    private static void AssertInputGeneratesProperError(string input, string expected) =>
        AssertInputGeneratesExpected(input, expected);

    private static void AssertInputGeneratesExpected(string input, string? expectedError = null, params string[] expectedOutput) =>
        AssertInputGeneratesExpected(input, expectedError, expectedOutput.ToList());

    private static void AssertInputGeneratesExpected(string input, string? expectedError, IEnumerable<string> expectedOutput)
    {
        var error = ProcessInput(input);

        if (expectedError == null)
        {
            Assert.That(error, Is.Null);
        }
        else
        {
            Assert.That(error, Has.Message.EqualTo(expectedError));
        }

        if (!expectedOutput.Any())
        {
            Assert.That(output, Is.Empty);
        }
        else
        {
            var expected = expectedOutput.ToArray();

            Assert.That(output, Has.Count.EqualTo(expected.Length));

            for (var i = 0; i < expected.Length; i++)
            {
                Assert.That(output[i], Is.EqualTo(expected[i]));
            }
        }
    }
    #endregion
}
