namespace Sigurn.CommandLine;

/// <summary>
/// Value parser attribute. Apply this attribute to a property to provide custom parser.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class ValueParserAttribute : Attribute
{
    /// <summary>
    /// Parser type whic implements interface <see cref="IValueParser"/>
    /// </summary>
    public Type ParserType { get; }

    /// <summary>
    /// Initializes a new instance of the attribute
    /// </summary>
    /// <param name="parserType">Parser type</param>
    /// <exception cref="ArgumentNullException">It is thrown if <paramref name="parserType"/> is null.</exception>
    /// <exception cref="ArgumentException">It is thrown if <paramref name="parserType"/> doesn't implement <see cref="IValueParser"/> interface</exception>
    public ValueParserAttribute(Type parserType)
    {
        if (parserType == null)
            throw new ArgumentNullException($"Parser type cannot be null", nameof(parserType));

        if (!parserType.IsAssignableTo(typeof(IValueParser)))
            throw new ArgumentException($"Parser class should implement {typeof(IValueParser)} interface", nameof(parserType));

        ParserType = parserType;
    }

    internal IValueParser GetParser()
    {
        return (IValueParser)(Activator.CreateInstance(ParserType) 
            ?? throw new Exception($"Failed to create instance of {ParserType} type"));
    }
}
