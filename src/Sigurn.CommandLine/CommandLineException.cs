namespace Sigurn.CommandLine;

internal class CommandLineException : Exception
{
    public CommandLineException(string message)
        : base(message)
    {
    }

    public CommandLineException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

