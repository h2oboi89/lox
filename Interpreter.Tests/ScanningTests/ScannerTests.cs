using Interpreter.Framework.Scanning;
using System;
using System.Reflection.Metadata;

namespace Interpreter.Tests.ScanningTests;

[TestFixture]
internal static class ScannerTests
{
    private static void AssertToken(List<Token> tokens, int index, TokenType expectedType, string expectedLexeme, int expectedLine)
    {
        Assert.Multiple(() =>
        {
            Assert.That(tokens[index].Type, Is.EqualTo(expectedType));
            Assert.That(tokens[index].Lexeme, Is.EqualTo(expectedLexeme));
            Assert.That(tokens[index].Line, Is.EqualTo(expectedLine));
            Assert.That(tokens[index].Literal, Is.Null);
        });
    }

    private static void AssertLiteralToken(List<Token> tokens, int index, TokenType expectedType, string expectedLexeme, int expectedLine, object expectedLiteral)
    {
        Assert.Multiple(() =>
        {
            Assert.That(tokens[index].Type, Is.EqualTo(expectedType));
            Assert.That(tokens[index].Lexeme, Is.EqualTo(expectedLexeme));
            Assert.That(tokens[index].Line, Is.EqualTo(expectedLine));
            Assert.That(tokens[index].Literal, Is.EqualTo(expectedLiteral));
        });
    }

    private static void AssertStringLiteralToken(List<Token> tokens, int index, TokenType expectedType, string expectedLexeme, int expectedLine) =>
        AssertLiteralToken(tokens, index, expectedType, expectedLexeme, expectedLine, expectedLexeme[1..(expectedLexeme.Length - 1)]);

    private static void AssertEofToken(List<Token> tokens, int expectedLine) =>
        AssertToken(tokens, tokens.Count - 1, TokenType.EOF, string.Empty, expectedLine);

    [Test]
    public static void SimpleTokens()
    {
        var input = "() {} , . - + ; * ! != /";

        var (tokens, errors) = Scanner.ScanTokens(input);

        Assert.That(errors, Is.Empty);

        var tokenList = tokens.ToList();

        Assert.That(tokenList, Has.Count.EqualTo(14));

        AssertToken(tokenList, 0, TokenType.LEFT_PAREN, "(", 1);
        AssertToken(tokenList, 1, TokenType.RIGHT_PAREN, ")", 1);
        AssertToken(tokenList, 2, TokenType.LEFT_BRACE, "{", 1);
        AssertToken(tokenList, 3, TokenType.RIGHT_BRACE, "}", 1);
        AssertToken(tokenList, 4, TokenType.COMMA, ",", 1);
        AssertToken(tokenList, 5, TokenType.DOT, ".", 1);
        AssertToken(tokenList, 6, TokenType.MINUS, "-", 1);
        AssertToken(tokenList, 7, TokenType.PLUS, "+", 1);
        AssertToken(tokenList, 8, TokenType.SEMICOLON, ";", 1);
        AssertToken(tokenList, 9, TokenType.STAR, "*", 1);
        AssertToken(tokenList, 10, TokenType.BANG, "!", 1);
        AssertToken(tokenList, 11, TokenType.BANG_EQUAL, "!=", 1);
        AssertToken(tokenList, 12, TokenType.SLASH, "/", 1);
        AssertEofToken(tokenList, 1);
    }

    [Test]
    public static void Comments()
    {
        var input = """
            foo // bar
            // foo bar
            baz
            """;

        var (tokens, errors) = Scanner.ScanTokens(input);

        Assert.That(errors, Is.Empty);

        var tokenList = tokens.ToList();

        Assert.That(tokenList, Has.Count.EqualTo(3));

        AssertToken(tokenList, 0, TokenType.IDENTIFIER, "foo", 1);
        AssertToken(tokenList, 1, TokenType.IDENTIFIER, "baz", 3);
        AssertEofToken(tokenList, 3);
    }

    [Test]
    public static void Whitespace()
    {
        var input = " \r\n\t";

        var (tokens, errors) = Scanner.ScanTokens(input);

        Assert.That(errors, Is.Empty);

        var tokenList = tokens.ToList();

        Assert.That(tokenList, Has.Count.EqualTo(1));
        AssertEofToken(tokenList, 2);
    }

    [Test]
    public static void Strings()
    {
        var input = """
            ""
            "hello world!"
            "1 + 2"
            "multi
            line"
            "unterminated
            """;

        var (tokens, errors) = Scanner.ScanTokens(input);

        var errorList = errors.ToList();
        var tokenList = tokens.ToList();

        Assert.That(errorList, Has.Count.EqualTo(1));
        Assert.That(errorList[0].Message, Is.EqualTo("Unterminated string"));

        Assert.That(tokenList, Has.Count.EqualTo(5));
        AssertStringLiteralToken(tokenList, 0, TokenType.STRING, "\"\"", 1);
        AssertStringLiteralToken(tokenList, 1, TokenType.STRING, "\"hello world!\"", 2);
        AssertStringLiteralToken(tokenList, 2, TokenType.STRING, "\"1 + 2\"", 3);
        AssertStringLiteralToken(tokenList, 3, TokenType.STRING, $"\"multi{Environment.NewLine}line\"", 5);
        AssertEofToken(tokenList, 6);
    }

    [Test]
    public static void Numbers()
    {
        var input = """
            1
            123
            123.456
            .123
            123.
            """;

        var (tokens, errors) = Scanner.ScanTokens(input);

        Assert.That(errors, Is.Empty);

        var tokenList = tokens.ToList();

        Assert.That(tokenList, Has.Count.EqualTo(8));
        AssertLiteralToken(tokenList, 0, TokenType.NUMBER, "1", 1, (double)1);
        AssertLiteralToken(tokenList, 1, TokenType.NUMBER, "123", 2, (double)123);
        AssertLiteralToken(tokenList, 2, TokenType.NUMBER, "123.456", 3, 123.456);
        AssertToken(tokenList, 3, TokenType.DOT, ".", 4);
        AssertLiteralToken(tokenList, 4, TokenType.NUMBER, "123", 4, (double)123);
        AssertLiteralToken(tokenList, 5, TokenType.NUMBER, "123", 5, (double)123);
        AssertToken(tokenList, 6, TokenType.DOT, ".", 5);
        AssertEofToken(tokenList, 5);
    }

    [Test]
    public static void KeyWordsAndIdentifier()
    {
        var input = """
            and     class   else    false
            for     fun     if      nil
            or      print   return  super
            this    var     while   foo
            """;

        var (tokens, errors) = Scanner.ScanTokens(input);

        Assert.That(errors, Is.Empty);

        var tokenList = tokens.ToList();

        Assert.That(tokenList, Has.Count.EqualTo(17));

        AssertToken(tokenList, 0, TokenType.AND, "and", 1);
        AssertToken(tokenList, 1, TokenType.CLASS, "class", 1);
        AssertToken(tokenList, 2, TokenType.ELSE, "else", 1);
        AssertToken(tokenList, 3, TokenType.FALSE, "false", 1);
        AssertToken(tokenList, 4, TokenType.FOR, "for", 2);
        AssertToken(tokenList, 5, TokenType.FUN, "fun", 2);
        AssertToken(tokenList, 6, TokenType.IF, "if", 2);
        AssertToken(tokenList, 7, TokenType.NIL, "nil", 2);
        AssertToken(tokenList, 8, TokenType.OR, "or", 3);
        AssertToken(tokenList, 9, TokenType.PRINT, "print", 3);
        AssertToken(tokenList, 10, TokenType.RETURN, "return", 3);
        AssertToken(tokenList, 11, TokenType.SUPER, "super", 3);
        AssertToken(tokenList, 12, TokenType.THIS, "this", 4);
        AssertToken(tokenList, 13, TokenType.VAR, "var", 4);
        AssertToken(tokenList, 14, TokenType.WHILE, "while", 4);
        AssertToken(tokenList, 15, TokenType.IDENTIFIER, "foo", 4);
        AssertEofToken(tokenList, 4);
    }

    [Test]
    public static void Errors()
    {
        var input = "#";

        var (tokens, errors) = Scanner.ScanTokens(input);

        var tokenList = tokens.ToList();
        var errorList = errors.ToList();

        Assert.That(errorList, Has.Count.EqualTo(1));
        Assert.That(errorList[0].Message, Is.EqualTo("Unexpected character: '#'"));

        Assert.That(tokenList, Has.Count.EqualTo(1));
        AssertEofToken(tokenList, 1);
    }
}
