using System.Collections;
using System.Globalization;
using System.Reflection;

namespace Sigurn.CommandLine;

class ValueParser : ITokenParser
{
    private readonly object _instance;
    private readonly PropertyInfo _propInfo;
    private readonly ITokenParser _parent;

    protected PropertyInfo PropInfo => _propInfo;

    public bool IsFlag => Helpers.IsFlagProperty(_propInfo);

    public bool IsArray => Helpers.IsArrayProperty(_propInfo);

    public bool IsEnumFlag => Helpers.IsEnumFlagProperty(_propInfo);

    public bool IsSet { get; private set; }

    public ITokenParser Parent => _parent;

    public string? Value
    {
        get
        {
            var value = _propInfo.GetValue(_instance);

            if (value == null)
                return string.Empty;

            if (value is bool bl)
                return bl ? "true" : "false";

            if (IsArray)
            {
                if (value is IEnumerable e)
                {
                    var items = new List<string>();
                    foreach (var obj in e)
                        items.Add(obj?.ToString() ?? "null");

                    return string.Join(' ', items.ToArray());
                }
            }

            if (value is Enum en)
            {
                var strs = en.ToString().Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                return string.Join(' ', strs.Select(x => Helpers.ToDashCase(x)));
            }

            return value?.ToString();
        }
    }

    public ValueParser(object instance, PropertyInfo propInfo, ITokenParser parent)
    {
        _instance = instance;
        _propInfo = propInfo;
        _parent = parent;
    }

    protected virtual ITokenParser ParseTokenImp(string token)
    {
        var type = _propInfo.PropertyType;

        try
        {
            if (type.IsArray)
            {
                type = type.GetElementType();
                if (type == null)
                    throw new Exception("Unknown array element type");

                if (type == typeof(string) && (token.StartsWith("--") || token.StartsWith("-")))
                    return _parent.ParseToken(token);

                var value = Helpers.ParseValue(token, type);

                Array? arr = _propInfo.GetValue(_instance) as Array;
                if (arr == null)
                {
                    arr = Array.CreateInstance(type, 1);
                    arr.SetValue(value, 0);
                    value = arr;
                }
                else
                {
                    var newArray = Array.CreateInstance(type, arr.Length + 1);
                    arr.CopyTo(newArray, 0);
                    newArray.SetValue(value, arr.Length);
                    value = newArray;
                }

                _propInfo.SetValue(_instance, value);
                IsSet = true;

                return this;
            }
            else if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>) ||
                type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>) ||
                type.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                var itemType = type.GetGenericArguments()[0];
                if (itemType == null)
                    throw new Exception("Unknown array element type");

                if (itemType == typeof(string) && (token.StartsWith("--") || token.StartsWith("-")))
                    return _parent.ParseToken(token);

                var value = Helpers.ParseValue(token, itemType);

                var list = _propInfo.GetValue(_instance) as IList;
                if (list == null || !IsSet)
                {
                    list = Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType)) as IList;
                    if (list == null)
                        throw new Exception($"Failed to create instance of {type}");

                    _propInfo.SetValue(_instance, list);
                }

                list.Add(value);
                IsSet = true;

                return this;
            }
            else if (Helpers.IsEnumFlagProperty(_propInfo))
            {
                if (token.StartsWith("--") || token.StartsWith("-"))
                    return _parent.ParseToken(token);

                var flags = token.Split('+');
                foreach(var flag in flags)
                {
                    var propValue = Helpers.ParseValue(flag, type);

                    var obj = _propInfo.GetValue(_instance);
                    if (obj == null || !IsSet)
                        obj = propValue;
                    else
                        obj = Helpers.OrValues(obj, propValue, type);

                    _propInfo.SetValue(_instance, obj);
                    IsSet = true;
                }

                return this;
            }
            else
            {
                var propValue = Helpers.ParseValue(token, type);
                _propInfo.SetValue(_instance, propValue);
                IsSet = true;
                return _parent;
            }
        }
        catch (FormatException)
        {
            if (type == typeof(bool))
            {
                _propInfo.SetValue(_instance, true);
                return _parent.ParseToken(token);
            }

            throw;
        }
    }

    public ITokenParser ParseToken(string token)
    {
        return ParseTokenImp(token);
    }
}
