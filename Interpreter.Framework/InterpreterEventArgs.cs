namespace Interpreter.Framework;

public class InterpreterEventArgs : EventArgs
{
    public readonly string Content;

    public InterpreterEventArgs(string content)
    {
        Content = content;
    }
}