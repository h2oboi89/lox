namespace Interpreter.Framework;

public class InterpreterOutEventArgs : EventArgs
{
    public readonly string Content;

    public InterpreterOutEventArgs(string content)
    {
        Content = content;
    }
}