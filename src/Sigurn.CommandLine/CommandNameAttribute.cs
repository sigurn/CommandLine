namespace Sigurn.CommandLine;


/// <summary>
/// Command name attribute. Apply it to a method to define command name to be used in command line.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class CommandNameAttribute : Attribute
{
    /// <summary>
    /// Name of the command
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the attribute with command name.
    /// </summary>
    /// <param name="name">Command name</param>
    public CommandNameAttribute(string name)
    {
        Name = name;
    }
}
