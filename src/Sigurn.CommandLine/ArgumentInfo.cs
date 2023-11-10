using System.Reflection;

namespace Sigurn.CommandLine;

class ArgumentInfo : ValueParser
{
    public int Position { get; init; }

    public string Name { get; init; } = string.Empty;

    public string[] HelpText { get; init; } = Array.Empty<string>();

    public bool IsRequired { get; init; }

    public string HelpPossibleValues
    {
        get
        {
            if (IsFlag)
                return "true|false";

           if (Helpers.IsEnumProperty(PropInfo) || Helpers.IsEnumFlagProperty(PropInfo))
                return string.Join('|', Helpers.GetEnumValues(PropInfo));

            return string.Empty;            
        }
    }

    public ArgumentInfo(PropertyInfo propInfo, object instance, ITokenParser parent)
        : base(instance, propInfo, parent)
    {
    }

    protected override ITokenParser ParseTokenImp(string token)
    {
        try
        {
            return base.ParseTokenImp(token);
        }
        catch (FormatException ex)
        {
            throw new CommandLineException($"Invalid value '{token}' for <{Name}> argument", ex);
        }
    }
}

