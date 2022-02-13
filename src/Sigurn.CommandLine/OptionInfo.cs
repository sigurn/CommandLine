using System.Reflection;
using System.Text;

namespace Sigurn.CommandLine;

class OptionInfo : ValueParser
{
    public IReadOnlyList<string> Names { get; init; } = new List<string>();

    public char ShortName { get; init; }

    public string[] HelpText { get; init; } = Array.Empty<string>();

    public bool IsRequired { get; init; }

    public string HelpValue
    {
        get
        {
            if (IsFlag)
                return "true|false";

            var sb = new StringBuilder();

            if (IsArray)
            {
                sb.Append(Names[0]);
                sb.Append("...");
                return sb.ToString();
            }

            if (Helpers.IsEnumFlagProperty(PropInfo))
            {
                sb.Append('(');
                sb.AppendJoin('|', Helpers.GetEnumValues(PropInfo));
                sb.Append(')');
                sb.Append("...");
                return sb.ToString();
            }

            if (Helpers.IsEnumProperty(PropInfo))
            {
                sb.AppendJoin('|', Helpers.GetEnumValues(PropInfo));
                return sb.ToString();
            }

            return Names[0];
        }
    }

    public OptionInfo(object instance, PropertyInfo propInfo, ITokenParser parent)
        : base(instance, propInfo, parent)
    {
    }

    protected override ITokenParser ParseTokenImp(string token)
    {
        try
        {
            return base.ParseTokenImp(token);
        }
        catch(FormatException ex)
        {
            if (IsArray)
                return Parent.ParseToken(token);

            throw new CommandLineException($"Invalid value '{token}' for --{Names[0]} option", ex);
        }
    }
}

