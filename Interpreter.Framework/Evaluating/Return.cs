namespace Interpreter.Framework.Evaluating;
internal class Return : Exception
{
    public readonly object? Value;

    public Return(object? value) { Value = value; }
}
