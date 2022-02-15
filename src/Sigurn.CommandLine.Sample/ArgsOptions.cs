namespace Sigurn.CommandLine.Sample;

internal class ArgsOptions
{
    [Argument(0, IsRequired = true)]
    [HelpText("String argument")]
    public string Arg1 { get; set; } = string.Empty;

    [Argument(1)]
    [HelpText("Integer argument")]
    public int Arg2 { get; set; } = 123;

    [Argument(2)]
    [HelpText("String list argument")]
    public IReadOnlyList<string> Arg3 { get; set; } = new List<string>();

    [HelpText("Test arguments")]
    public static Task Arguments(ArgsOptions options, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Arg1: {options.Arg1}");
        Console.WriteLine($"Arg2: {options.Arg2}");
        Console.WriteLine("Arg3: {0}", string.Join(", ", options.Arg3));

        return Task.CompletedTask;
    }
}

