namespace Sigurn.CommandLine;

/// <summary>
/// Provides a way to parse value of the certain type
/// </summary>
public interface IValueParser
{
    /// <summary>
    /// Parses string value to defined type.
    /// </summary>
    /// <param name="value">String to be parsed</param>
    /// <param name="type">Type of the value to be returned by this method</param>
    /// <returns>Parsed object</returns>
    object ParseValue(string value, Type type);
}
