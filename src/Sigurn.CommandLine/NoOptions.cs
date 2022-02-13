namespace Sigurn.CommandLine;

class NoOptions : IOptions
{
    public object? Value => null;

    public bool IsEmpty => true;

    public IReadOnlyList<OptionInfo> OptionsList => Array.Empty<OptionInfo>();

    public IReadOnlyList<ArgumentInfo> ArgumentsList => Array.Empty<ArgumentInfo>();

    public OptionInfo? GetOption(string name)
    {
        return null;
    }

    public OptionInfo? GetOption(char name)
    {
        return null;
    }

    public ITokenParser? GetArgument(ITokenParser parent)
    {
        return null;
    }
}
