using System.Globalization;
using System.Reflection;

namespace Sigurn.CommandLine
{
    internal class Helpers
    {
        public static string[] GetHelpTextFromDelegate(Delegate method)
        {
            if (method == null)
                return Array.Empty<string>();

            var invocationList = method.GetInvocationList();
            if (invocationList == null || invocationList.Length < 1)
                return Array.Empty<string>();

            var attrs = invocationList[0]?.Method?.GetCustomAttributes(typeof(HelpTextAttribute), true);
            if (attrs == null || attrs.Length == 0)
                return Array.Empty<string>();

            return ((HelpTextAttribute)attrs[0]).HelpText ?? Array.Empty<string>();
        }

        public static bool IsArrayProperty(PropertyInfo property)
        {
            var type = property.PropertyType;
            return type.IsArray ||
                    (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) ||
                    (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)) ||
                    (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        public static bool IsFlagProperty(PropertyInfo property)
        {
            var type = property.PropertyType;
            return type == typeof(bool) ||
                (type.IsGenericType && 
                type.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                type.GetGenericArguments()[0] == typeof(bool));
        }

        public static bool IsEnumProperty(PropertyInfo property)
        {
            var type = property.PropertyType;
            return type.IsEnum ||
                (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                type.GetGenericArguments()[0].IsEnum);
        }

        public static bool IsEnumFlagProperty(PropertyInfo property)
        {
            if (!IsEnumProperty(property))
                return false;

            return property.PropertyType.GetCustomAttribute<FlagsAttribute>() != null;
        }

        public static IEnumerable<string> GetEnumValues(PropertyInfo property)
        {
            var type = property.PropertyType;
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(Nullable<>) && type.GetGenericArguments()[0].IsEnum)
                type = type.GetGenericArguments()[0];

            if (!type.IsEnum)
                throw new ArgumentException("Property is not enum", nameof(property));

            return type.GetEnumNames()
                .Select(x => ToDashCase(x) ?? throw new ArgumentNullException("Enum value cannot be null"));
        }

        public static string[] GetPropertyHelpText(PropertyInfo propInfo)
        {
            if (propInfo == null)
                return Array.Empty<string>();

            var attrs = propInfo.GetCustomAttributes(typeof(HelpTextAttribute), true);
            if (attrs == null || attrs.Length == 0)
                return Array.Empty<string>();

            return ((HelpTextAttribute)attrs[0]).HelpText ?? Array.Empty<string>();
        }

        public static string GetCommandNameFromDelegate(Delegate method)
        {
            if (method == null)
                return string.Empty;

            var invocationList = method.GetInvocationList();
            if (invocationList == null || invocationList.Length < 1)
                return string.Empty;

            var attrs = invocationList[0]?.Method?.GetCustomAttributes(typeof(CommandNameAttribute), true);
            if (attrs != null && attrs.Length >0 && attrs[0] is CommandNameAttribute cna && !string.IsNullOrEmpty(cna.Name))
                return cna.Name;

            return ToDashCase(invocationList[0]?.Method?.Name) ?? string.Empty;
        }

        public static string? ToDashCase(string? str)
        {
            if (str == null)
                return null;

            return string.Concat(str
                .Select((x, i) => i > 0 && char.IsUpper(x) && !char.IsUpper(str[i - 1]) ? $"-{x}" : x.ToString()))
                .ToLower();
        }

        public static object ParseValue(string token, Type type)
        {
            if (type == typeof(bool))
                return bool.Parse(token);
            else if (type == typeof(sbyte))
                return sbyte.Parse(token, CultureInfo.InvariantCulture);
            else if (type == typeof(byte))
                return byte.Parse(token, CultureInfo.InvariantCulture);
            else if (type == typeof(short))
                return short.Parse(token, CultureInfo.InvariantCulture);
            else if (type == typeof(ushort))
                return ushort.Parse(token, CultureInfo.InvariantCulture);
            else if (type == typeof(int))
                return int.Parse(token, CultureInfo.InvariantCulture);
            else if (type == typeof(uint))
                return uint.Parse(token, CultureInfo.InvariantCulture);
            else if (type == typeof(long))
                return long.Parse(token, CultureInfo.InvariantCulture);
            else if (type == typeof(ulong))
                return ulong.Parse(token, CultureInfo.InvariantCulture);
            else if (type == typeof(float))
                return float.Parse(token, CultureInfo.InvariantCulture);
            else if (type == typeof(double))
                return double.Parse(token, CultureInfo.InvariantCulture);
            else if (type == typeof(decimal))
                return decimal.Parse(token, CultureInfo.InvariantCulture);
            else if (type == typeof(string))
                return token;
            else if (type == typeof(Uri))
                return new Uri(token);
            else if (type == typeof(FileInfo))
                return new FileInfo(token);
            else if (type == typeof(DirectoryInfo))
                return new DirectoryInfo(token);
            else if (type == typeof(Guid))
                return Guid.Parse(token);
            else if (type.IsEnum)
                return ParseEnum(type, token);

            var ctor = type.GetConstructor(new Type[] { typeof(string) });
            if (ctor != null)
            {
                var value = Activator.CreateInstance(type, token);
                if (value == null)
                    throw new Exception($"Failed to create instance of {type}");

                return value;
            }

            var pmi = type.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, new Type[] { typeof(string) });
            if (pmi != null)
            {
                var value = pmi.Invoke(null, new object[] { token });

                if (value == null)
                    throw new Exception($"Failed to create instance of {type}");

                return value;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return ParseValue(token, type.GetGenericArguments()[0]);

            throw new ArgumentException($"Unsupported option type {type}");
        }

        private static object ParseEnum(Type type, string token)
        {
            var enumValues = type.GetFields(BindingFlags.Static | BindingFlags.Public)
                .Where(x => x.FieldType == type);

            foreach(var val in enumValues)
            {
                if (string.Equals(val.Name, token, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(ToDashCase(val.Name), token, StringComparison.OrdinalIgnoreCase))
                    return val.GetValue(null) ?? throw new FormatException($"Invalid value {token} for enum {type}");
            }

            throw new FormatException($"Invalid value {token} for enum {type}");
        }

        public static object OrValues(object val1, object val2, Type type)
        {
            ulong lval1 = Convert.ToUInt64(val1);
            ulong lval2 = Convert.ToUInt64(val2);

            return Enum.ToObject(type, lval1 | lval2);
        }
    }
}
