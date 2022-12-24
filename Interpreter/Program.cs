using System.Reflection;

internal class Program
{
    private static bool InPrompt = false;

    private static void Main(string[] args)
    {
        Interpreter.Framework.Interpreter.Out += (_, e) =>
        {
            Console.WriteLine(e.Content);
        };

        Interpreter.Framework.Interpreter.Error += (_, e) =>
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(e.Content);
            Console.ForegroundColor = originalColor;
            if (!InPrompt) Environment.Exit(65);
        };

        if (args.Length > 1)
        {
            Console.WriteLine("Usage: lox [script]");
            Environment.Exit(1);
        }
        else if (args.Length == 1)
        {
            RunFile(args[0]);
        }
        else
        {
            RunPrompt();
        }
    }

    static void RunFile(string path)
    {
        Interpreter.Framework.Interpreter.Run(File.ReadAllText(path));
    }

    static void RunPrompt()
    {
        InPrompt = true;
        var assemblyName = Assembly.GetExecutingAssembly().GetName();
        Console.WriteLine($"{assemblyName.Name} {assemblyName.Version}");

        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line)) break;
            Interpreter.Framework.Interpreter.Run(line);
        }
    }
}