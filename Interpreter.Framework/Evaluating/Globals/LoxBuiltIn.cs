namespace Interpreter.Framework.Evaluating.Globals;
internal abstract class LoxBuiltIn : LoxCallable
{
    public override string ToString() => "<function native>";
}

internal class Clock : LoxBuiltIn
{
    public override object? Call(AstInterpreter interpreter, IEnumerable<object?> arguments)
    {
        return (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }
}

// FUTURE: Print

internal class Reset : LoxBuiltIn
{
    public override object? Call(AstInterpreter interpreter, IEnumerable<object?> arguments)
    {
        interpreter.Reset();

        return null;
    }
}
