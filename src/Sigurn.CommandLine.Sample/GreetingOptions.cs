namespace Sigurn.CommandLine.Sample;

internal class GreetingOptions
{
    [Argument(0)]
    public string Name { get; set; } = "Anonymous";

    [Option('s', "short")]
    [HelpText("Show short version of the message")]
    public bool IsShort { get; set; }

    [HelpText("Say hello")]
    public static void Hello(GreetingOptions options)
    {
        if (options.IsShort)
            Console.WriteLine($"Hi {options.Name}!");
        else
            Console.WriteLine($"Hello {options.Name}!");
    }

    [CommandName("goodbye")]
    [HelpText("Say goodbye")]
    public static Task GoodbyeAsync(GreetingOptions options, CancellationToken cancellationToken)
    {
        if (options.IsShort)
            Console.WriteLine($"Bye {options.Name}!");
        else
            Console.WriteLine($"Goodbye {options.Name}!");

        return Task.CompletedTask;
    }
}

