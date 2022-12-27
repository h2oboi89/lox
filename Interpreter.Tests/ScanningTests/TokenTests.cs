using Interpreter.Framework.Scanning;

namespace Interpreter.Tests.ScanningTests;

[TestFixture]
internal class TokenTests
{
    [Test]
    public void StandardToken()
    {
        var token = new Token(TokenType.IDENTIFIER, "foo", 1);

        Assert.That(token.ToString(), Is.EqualTo("1 IDENTIFIER foo "));
    }

    [Test]
    public void LiteralToken()
    {
        var token = new Token(TokenType.NUMBER, "123", 1, (double)123);

        Assert.That(token.ToString(), Is.EqualTo("1 NUMBER 123 123"));
    }
}
