namespace Sigurn.CommandLine;

interface IOptions
{
    object? Value { get; }

    IReadOnlyList<OptionInfo> OptionsList { get; }

    IReadOnlyList<ArgumentInfo> ArgumentsList { get; }

    OptionInfo? GetOption(string name);
    OptionInfo? GetOption(char name);
    ITokenParser? GetArgument(ITokenParser parent);
}

