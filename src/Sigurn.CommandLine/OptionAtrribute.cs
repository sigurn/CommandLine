namespace Sigurn.CommandLine;

/// <summary>
/// Option attribute. Apply this attribute to a property to mark it as a command option.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class OptionAttribute : Attribute
{

    /// <summary>
    /// Option long names.
    /// </summary>
    public IReadOnlyList<string>? Names { get; }

    /// <summary>
    /// Option short name.
    /// </summary>
    public char? ShortName { get; }

    /// <summary>
    /// Flag defines if option is required.
    /// </summary>
    public bool IsRequired { get; init; } = false;

    /// <summary>
    /// Initializes a new instance of the class with long names.
    /// </summary>
    /// <param name="names">Option long names.</param>
    public OptionAttribute(params string[] names)
    {
        Names = new List<string>(names);
    }

    /// <summary>
    /// Initializes a new instance of the class with short name.
    /// </summary>
    /// <param name="shortName">Option short name.</param>
    public OptionAttribute(char shortName)
    {
        ShortName = shortName;
    }

    /// <summary>
    /// Initializes a new instance of the class with short and long names.
    /// </summary>
    /// <param name="shortName">Short name.</param>
    /// <param name="names">Long names.</param>
    public OptionAttribute(char shortName, params string[] names)
    {
        Names = new List<string>(names);
        ShortName = shortName;
    }
}

