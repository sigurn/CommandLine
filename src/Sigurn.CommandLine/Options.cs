namespace Sigurn.CommandLine;

class Options<T> : IOptions where T : class, new()
{
    private readonly IReadOnlyList<OptionInfo> _options;
    private readonly IReadOnlyList<ArgumentInfo> _arguments;

    private readonly T _value = new();
    private int _currentArgument = 0;

    public Options(ITokenParser parent)
    {
        var options = new List<OptionInfo>();
        var arguments = new List<ArgumentInfo>();

        foreach (var propInfo in typeof(T).GetProperties())
        {
            var optAttrs = propInfo.GetCustomAttributes(typeof(OptionAttribute), true);
            if (optAttrs != null && optAttrs.Length > 0 && optAttrs[0] is OptionAttribute optAttr)
            {
                IReadOnlyList<string> names = (optAttr.Names ?? Array.Empty<string>())
                    .Where(x => !string.IsNullOrEmpty(x)).ToList();

                if (!names.Any())
                    names = new List<string>(new string[] { Helpers.ToDashCase(propInfo.Name) ?? string.Empty });

                var shortName = optAttr.ShortName ?? char.MinValue;
                var repeats = options
                    .SelectMany(x => x.Names)
                    .Intersect(names);

                if (repeats.Any())
                {
                    var str = string.Join(',', repeats.ToArray());
                    throw new ArgumentException($"The option '{options.First()}' is already defined");
                }

                if (shortName != char.MinValue && options.Select(x => x.ShortName).Contains(shortName))
                    throw new ArgumentException($"The short option '{shortName}' is already defined");

                options.Add
                (
                    new OptionInfo(_value, propInfo, parent)
                    {
                        Names = names,
                        ShortName = shortName,
                        HelpText = Helpers.GetPropertyHelpText(propInfo),
                        IsRequired = optAttr.IsRequired,
                    }
                );
            }

            var argAttrs = propInfo.GetCustomAttributes(typeof(ArgumentAttribute), true);
            if (argAttrs != null && argAttrs.Length > 0 && argAttrs[0] is ArgumentAttribute argAttr)
            {
                arguments.Add
                (
                    new ArgumentInfo(propInfo, _value, parent)
                    {
                        Name = !string.IsNullOrEmpty(argAttr.Name) ? argAttr.Name : Helpers.ToDashCase(propInfo.Name) ?? string.Empty,
                        HelpText = Helpers.GetPropertyHelpText(propInfo),
                        IsRequired = argAttr.IsRequired,
                        Position = argAttr.Position,
                    }
                );
            }
        }

        _options = options;
        _arguments = arguments.OrderBy(x => x.Position).ToList();
        bool isRequired = false;
        bool isArray = false;
        foreach(var arg in _arguments.Reverse())
        {
            if (isArray && arg.IsArray)
                throw new ArgumentException("Only last argument may be an array.");

            if (isRequired && !arg.IsRequired)
                throw new ArgumentException("If one argument is requied all the previous arguments must be required too.");

            isRequired = arg.IsRequired ? true : isRequired;
            isArray = arg.IsArray ? true : isArray;
        }
    }

    public object? Value => _value;

    public IReadOnlyList<OptionInfo> OptionsList => _options;

    public IReadOnlyList<ArgumentInfo> ArgumentsList => _arguments;

    public OptionInfo? GetOption(string name)
    {
        return _options.Where(x => x.Names.Contains(name)).FirstOrDefault();
    }

    public OptionInfo? GetOption(char name)
    {
        return _options.Where(x => x.ShortName == name).FirstOrDefault();
    }

    public ITokenParser? GetArgument(ITokenParser parent)
    {
        if (_currentArgument >= _arguments.Count)
            return null;

        return _arguments[_currentArgument++];
    }
}

