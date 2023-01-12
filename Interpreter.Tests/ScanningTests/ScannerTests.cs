using Interpreter.Framework.Scanning;

namespace Interpreter.Tests.ScanningTests;

[TestFixture]
internal static class ScannerTests
{
    [Test]
    public static void SimpleTokens()
    {
        var inputs = new List<(string, string)>
        {
            ( "(",  "1 LEFT_PAREN (" ),
            ( " )",  "1 RIGHT_PAREN )" ),
            ( "{",  "1 LEFT_BRACE {" ),
            ( "}",  "1 RIGHT_BRACE }" ),
            ( ",",  "1 COMMA ," ),
            ( ".",  "1 DOT ." ),
            ( "-",  "1 MINUS -" ),
            ( "+",  "1 PLUS +" ),
            ( ";",  "1 SEMICOLON ;" ),
            ( "*",  "1 STAR *" ),
            ( "!",  "1 BANG !" ),
            ( "!=", "1 BANG_EQUAL !=" ),
            ( "=",  "1 EQUAL =" ),
            ( "==", "1 EQUAL_EQUAL ==" ),
            ( "<",  "1 LESS <" ),
            ( "<=", "1 LESS_EQUAL <=" ),
            ( ">",  "1 GREATER >" ),
            ( ">=", "1 GREATER_EQUAL >=" ),
            ( "/",  "1 SLASH /" ),
        };

        AssertInputsGenerateProperTokens(inputs);
    }

    [Test]
    public static void Comments()
    {
        var input = """
        / // this is a comment
        // this line is empty
        / // one more comment
        """;

        var expected = "1 SLASH / 3 SLASH /";

        AssertInputGeneratesProperTokens(input, expected, 3);
    }

    [Test]
    public static void SimpleStrings()
    {
        var inputs = new List<(string, string)>
        {
            ( """ "" """, "1 STRING \"\"" ),
            ( """ "foo" """, "1 STRING \"foo\" foo" )
        };

        AssertInputsGenerateProperTokens(inputs);
    }

    [Test]
    public static void MultilineString()
    {
        var input = """
        "this is
        a multiline string"
        """;

        var expected = """
        2 STRING "this is
        a multiline string" this is
        a multiline string
        """;

        AssertInputGeneratesProperTokens(input, expected, 2);
    }

    [Test]
    public static void UnterminatedString()
    {
        var input = "\"foo";

        var expected = "Unterminated string.";

        AssertInputGeneratesError(input, expected);
    }

    [Test]
    public static void Numbers()
    {
        var inputs = new List<(string, string)>
        {
            // all digits
            ( "0",      "1 NUMBER 0 0" ),
            ( "1",      "1 NUMBER 1 1" ),
            ( "2",      "1 NUMBER 2 2" ),
            ( "3",      "1 NUMBER 3 3" ),
            ( "4",      "1 NUMBER 4 4" ),
            ( "5",      "1 NUMBER 5 5" ),
            ( "6",      "1 NUMBER 6 6" ),
            ( "7",      "1 NUMBER 7 7" ),
            ( "8",      "1 NUMBER 8 8" ),
            ( "9",      "1 NUMBER 9 9" ),
            // decimals
            ( "0.0",   "1 NUMBER 0.0 0" ),
            ( "0.1",   "1 NUMBER 0.1 0.1" ),
            ( "3.14",   "1 NUMBER 3.14 3.14" ),
            // not valid decimals
            ( "1.",     "1 NUMBER 1 1 1 DOT ." ),
            ( ".1",     "1 DOT . 1 NUMBER 1 1" ),
        };

        AssertInputsGenerateProperTokens(inputs);
    }

    [Test]
    public static void Identifiers()
    {
        var inputs = new List<(string, string)>
        {
            // key words
            ( "and",    "1 AND and" ),
            ( "class",  "1 CLASS class" ),
            ( "else",   "1 ELSE else" ),
            ( "false",  "1 FALSE false" ),
            ( "for",    "1 FOR for" ),
            ( "fun",    "1 FUN fun" ),
            ( "if",     "1 IF if" ),
            ( "nil",    "1 NIL nil" ),
            ( "or",     "1 OR or" ),
            ( "print",  "1 PRINT print" ),
            ( "return", "1 RETURN return" ),
            ( "super",  "1 SUPER super" ),
            ( "this",   "1 THIS this" ),
            ( "true",   "1 TRUE true" ),
            ( "var",    "1 VAR var" ),
            ( "while",  "1 WHILE while" ),

            // identifiers
            ( "foo",    "1 IDENTIFIER foo" ),
            ( "bar",    "1 IDENTIFIER bar" ),
            ( "THIS",   "1 IDENTIFIER THIS"),
        };

        AssertInputsGenerateProperTokens(inputs);
    }

    [Test]
    public static void UnexpectedCharacters()
    {
        var inputs = new List<(string, string)>
        {
            ( "`", "Unexpected character: '`'." ),
            ( "~", "Unexpected character: '~'." ),
            ( "@", "Unexpected character: '@'." ),
            ( "#", "Unexpected character: '#'." ),
            ( "$", "Unexpected character: '$'." ),
            ( "%", "Unexpected character: '%'." ),
            ( "^", "Unexpected character: '^'." ),
            ( "&", "Unexpected character: '&'." ),
            ( "[", "Unexpected character: '['." ),
            ( "]", "Unexpected character: ']'." ),
            ( "|", "Unexpected character: '|'." ),
            ( ":", "Unexpected character: ':'." ),
            ( "'", "Unexpected character: '''." ),
            ( "?", "Unexpected character: '?'." ),
        };

        AssertInputsGenerateErrors(inputs);
    }

    [Test]
    public static void MultilineInput()
    {
        var input = """
        var a = "before";
        print a; // "before".
        var a = "after";
        print a; // "after".

        var a = 1;
        var b = 2;
        print a + b;
        """;

        var expected = """
        1 VAR var
        1 IDENTIFIER a
        1 EQUAL =
        1 STRING "before" before
        1 SEMICOLON ;
        2 PRINT print
        2 IDENTIFIER a
        2 SEMICOLON ;
        3 VAR var
        3 IDENTIFIER a
        3 EQUAL =
        3 STRING "after" after
        3 SEMICOLON ;
        4 PRINT print
        4 IDENTIFIER a
        4 SEMICOLON ;
        6 VAR var
        6 IDENTIFIER a
        6 EQUAL =
        6 NUMBER 1 1
        6 SEMICOLON ;
        7 VAR var
        7 IDENTIFIER b
        7 EQUAL =
        7 NUMBER 2 2
        7 SEMICOLON ;
        8 PRINT print
        8 IDENTIFIER a
        8 PLUS +
        8 IDENTIFIER b
        8 SEMICOLON ;
        """;

        AssertInputGeneratesProperTokens(input, expected, 8, Environment.NewLine);
    }

    #region Helper Methods
    private static void AssertInputGeneratesProperTokens(string input, string expected, int finalLine = 1, string joiner = " ")
    {
        var (tokens, scanErrors) = Scanner.ScanTokens(input);

        Assert.That(scanErrors, Is.Empty);

        Assert.That(JoinTokens(tokens, joiner), Is.EqualTo($"{expected}{joiner}{finalLine} EOF"));
    }

    private static void AssertInputsGenerateProperTokens(List<(string input, string expected)> inputs, int finalLine = 1)
    {
        foreach (var (input, expected) in inputs)
        {
            AssertInputGeneratesProperTokens(input, expected, finalLine);
        }
    }
    private static string JoinTokens(IEnumerable<Token> tokens, string joiner) => string.Join(joiner, tokens.Select(t => t.ToString().Trim()));

    // TODO: include line #s?
    private static void AssertInputGeneratesError(string input, string expected)
    {
        var (tokens, scanErrors) = Scanner.ScanTokens(input);

        var errors = scanErrors.ToList();

        Assert.That(errors, Has.Count.EqualTo(1));
        Assert.That(errors[0].Message, Is.EqualTo(expected));

        Assert.That(JoinTokens(tokens, string.Empty), Is.EqualTo("1 EOF"));
    }

    private static void AssertInputsGenerateErrors(List<(string input, string expected)> inputs)
    {
        foreach (var (input, expected) in inputs)
        {
            AssertInputGeneratesError(input, expected);
        }
    }
    #endregion 
}
