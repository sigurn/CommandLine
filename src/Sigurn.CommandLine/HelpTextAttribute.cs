namespace Sigurn.CommandLine;


/// <summary>
/// Help text attribute. Apply it to method or property to define help text which will be shown in help.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public class HelpTextAttribute : Attribute
{
    /// <summary>
    /// Help text
    /// </summary>
    public string[] HelpText { get; }

    /// <summary>
    /// Initializes a new instance of the class with help text.
    /// </summary>
    /// <param name="helpText">Help text</param>
    public HelpTextAttribute(params string[] helpText)
    {
        HelpText = helpText ?? Array.Empty<string>();
    }
}

