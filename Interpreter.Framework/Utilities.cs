namespace Interpreter.Framework;
internal static class Utilities
{
    public static string Stringify(object? value)
    {
        if (value is double dbl) return dbl.ToString();

        if (value is bool boolean) return boolean.ToString().ToLower();

        return value?.ToString() ?? "nil";
    }
}
