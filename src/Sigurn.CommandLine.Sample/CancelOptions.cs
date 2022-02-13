namespace Sigurn.CommandLine.Sample;

internal class CancelOptions
{
    [Option('d')]
    [HelpText("Delay between ticks in seconds.")]
    public double Delay { get; set; } = 0.5;

    [HelpText("Test cancel functionality")]
    public static async Task Cancel(CancelOptions options, CancellationToken cancellationToken)
    {
        Console.WriteLine("Press Ctrl-C to stop the counting");

        int count = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine(count++);
            await Task.Delay(TimeSpan.FromSeconds(options.Delay));
        }

        Console.WriteLine("The counting stopped");
    }

    [HelpText("Test behavior when cancel is not possible")]
    public static void NoCancel(CancelOptions options)
    {
        Console.WriteLine("Press Ctrl-C to stop the counting");

        int count = 0;
        while (true)
        {
            Console.WriteLine(count++);
            Task.Delay(TimeSpan.FromSeconds(options.Delay)).Wait();
        }
    }
}
