namespace Sigurn.CommandLine;

internal interface ITokenParser
{
    ITokenParser ParseToken(string token);
}
