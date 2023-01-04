using System.Reflection;
using static Interpreter.Framework.Interpreter;
using static Interpreter.Framework.InterpreterErrorEventArgs;

internal class Program
{
    private static bool InPrompt = false;

    private static void Main(string[] args)
    {
        Out += (_, e) =>
        {
            Console.WriteLine(e.Content);
        };

        Error += (_, e) =>
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(e.Content);
            Console.ForegroundColor = originalColor;
            if (!InPrompt)
            {
                var exitCode = e.Error switch
                {
                    ErrorType.ScanError or ErrorType.ParseError => 65,
                    ErrorType.RuntimeError => 70,
                    _ => 0,
                };

                Environment.Exit(exitCode);
            }
        };

        // TODO: make command line argument
        if (System.Diagnostics.Debugger.IsAttached)
        {
            const string DEBUG_LOG = "./debug.log";

            File.Create(DEBUG_LOG).Close();

            Debug += (_, e) =>
            {
                System.Diagnostics.Debug.WriteLine(e.Content);
                File.AppendAllText(DEBUG_LOG, $"{e.Content}{Environment.NewLine}");
            };
        }

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

    static void RunFile(string path) => Run(File.ReadAllText(path));

    static void RunPrompt()
    {
        InPrompt = true;
        var assemblyName = Assembly.GetExecutingAssembly().GetName();
        Console.WriteLine($"{assemblyName.Name} {assemblyName.Version}");

        while (true)
        {
            Console.Write("> ");
            Run(Console.ReadLine());
        }
    }
}