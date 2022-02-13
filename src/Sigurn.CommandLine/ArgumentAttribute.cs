namespace Sigurn.CommandLine;

/// <summary>
/// Argument attribute. Apply this attribute to a property to mark it as a command argument.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class ArgumentAttribute : Attribute
{
    /// <summary>
    /// Number which defines argument position in the list of arguments.
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// Argument name to be shown in help.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Flag defines if argument is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="position">Argument position.</param>
    public ArgumentAttribute(int position)
    {
        Position = position;
    }
}
