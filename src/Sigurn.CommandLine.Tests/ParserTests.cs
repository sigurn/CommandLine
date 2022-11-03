using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Sigurn.CommandLine.Tests;

enum TestValue
{
    None,
    Value1,
    Value2,
    Value3,
}

[Flags]
enum TestFlags
{
    None = 0,
    Value1 = 1,
    Value2 = 2,
    Value4 = 4,
}

class CustomType
{
    public string Value { get; }

    public CustomType(string value)
    {
        Value = value;
    }
}

class DateTimeParser : IValueParser
{
    public object ParseValue(string token, Type type)
    {
        if (type != typeof(DateTime))
            throw new ArgumentException($"This parse method doesn't support parsing to {type}", nameof(type));

        return new DateTime(long.Parse(token));
    }
}

class InvalidParserOption
{
    [Option("value")]
    [ValueParser(typeof(string))]
    public string Value { get; set; }
}

class NullParserOption
{
    [Option("value")]
    [ValueParser(null)]
    public string Value { get; set; }
}

class ParseType
{
    public string Value { get; }

    private ParseType(string value)
    {
        Value = value;
    }

    public static ParseType Parse(string value)
    {
        return new ParseType(value);
    }
}


class TestOptions
{
    [Option("option1")]
    public string Option1 { get; set; } = "";

    [Option("option2")]
    public int Option2 { get; set; }

    [Option("option3")]
    public double Option3 { get; set; }

    [Option('4', "option4")]
    public string Option4 { get; set; } = "";

    [Option("flag")]
    public bool Flag { get; set; }
}

class ShortOptions
{
    [Option('a', "flag-a")]
    public bool FlagA { get; set; }

    [Option('b', "flag-b")]
    public bool FlagB { get; set; }

    [Option('c', "flag-c")]
    public bool FlagC { get; set; }

    [Option('s', "option-s")]
    public short OptionS { get; set; }
}

class ArrayOptions
{
    [Option("string-array")]
    public string[]? StringArray { get; init; }

    [Option("int-array", "int-array-alias", "int-array-alias2")]
    public int[] IntArray { get; init; } = Array.Empty<int>();

    [Option("int-list")]
    public List<int> IntList { get; init; } = new List<int>();

    [Option("int-readonly-list")]
    public IReadOnlyList<int> IntReadOnlyList { get; init; } = new List<int>() { 1, 2, 3 };

    [Option("string-enumerable")]
    public IEnumerable<string> StringEnumerable { get; set; } = Array.Empty<string>();
}

class ArgumentOptions
{
    [Argument(1)]
    public int IntArgument { get; init; } = 0;

    [Argument(0)]
    public string StringArgument { get; init; } = "";

    [Argument(2)]
    public List<string> ArrayArgument { get; init; } = new List<string>();

    [Option]
    public bool TestFlag { get; init; }
}

class TypedOptions
{
    [Option("bool-option")]
    public bool BoolOption { get; set; }

    [Option("sbyte-option")]
    public sbyte SByteOption { get; set; }

    [Option("byte-option")]
    public byte ByteOption { get; set; }

    [Option("short-option")]
    public short ShortOption { get; set; }

    [Option("ushort-option")]
    public ushort UShortOption { get; set; }

    [Option("int-option")]
    public int IntOption { get; set; }

    [Option("uint-option")]
    public uint UIntOption { get; set; }

    [Option("long-option")]
    public long LongOption { get; set; }

    [Option("ulong-option")]
    public ulong ULongOption { get; set; }

    [Option("float-option")]
    public float FloatOption { get; set; }

    [Option("double-option")]
    public double DoubleOption { get; set; }

    [Option("decimal-option")]
    public decimal DecimalOption { get; set; }

    [Option("string-option")]
    public string? StringOption { get; set; }

    [Option("uri-option")]
    public Uri? UriOption { get; set; }

    [Option("file-info-option")]
    public FileInfo? FileInfoOption { get; set; }

    [Option("directory-info-option")]
    public DirectoryInfo? DirectoryInfoOption { get; set; }

    [Option("guid-option")]
    public Guid? GuidOption { get; set; }

    [Option("custom-option")]
    public CustomType? CustomOption { get; set; }

    [Option("parse-type-option")]
    public ParseType? ParseTypeOption { get; set; }

    [Option("enum-option")]
    public TestValue? EnumOption { get; set; }

    [Option("enum-flags-option")]
    public TestFlags EnumFlagsOption { get; set; }

    [Option("date-time-option")]
    [ValueParser(typeof(DateTimeParser))]
    public DateTime DateTimeOption { get; set; }
}

public class ParserTests
{
    [Fact]
    public void ParseMinusOptions()
    {
        var testOptions = new TestOptions();

        var parser = Parser.New<TestOptions>((options) =>
        {
            testOptions = options;
        });

        parser.Run(new string[] { "--option1=value1", "--option2:123", "--option3", "-3.1415", "-4", "value4", "--flag" });

        Assert.Equal("value1", testOptions.Option1);
        Assert.Equal(123, testOptions.Option2);
        Assert.Equal(-3.1415, testOptions.Option3);
        Assert.Equal("value4", testOptions.Option4);
        Assert.True(testOptions.Flag);
    }

    [Fact]
    public void ParseCommands()
    {
        bool testExecuted = false;
        bool test2Executed = false;
        var parser = Parser.New()
            .WithCommand("test", () => testExecuted = true)
            .WithCommand("test2", () => test2Executed = true);

        parser.Run(new string[] { "test" });
        Assert.True(testExecuted);
        Assert.False(test2Executed);

        testExecuted = false;

        parser.Run(new string[] { "test2" });
        Assert.True(test2Executed);
        Assert.False(testExecuted);
    }

    [Fact]
    public void ParseSubCommands()
    {
        bool testExecuted = false;
        bool testSub1Executed = false;
        bool testSub2Executed = false;

        bool test2Executed = false;
        bool test2Sub1Executed = false;
        bool test2Sub2Executed = false;

        var parser = Parser.New()
            .WithCommand("test", new Command(() => testExecuted = true)
                .WithCommand("tsub1", () => testSub1Executed = true)
                .WithCommand("tsub2", () => testSub2Executed = true))
            .WithCommand("test2", new Command(() => test2Executed = true)
                .WithCommand("t2sub1", () => test2Sub1Executed = true)
                .WithCommand("t2sub2", () => test2Sub2Executed = true));

        parser.Run(new string[] { "test", "tsub1" });
        Assert.False(testExecuted);
        Assert.True(testSub1Executed);
        Assert.False(testSub2Executed);
        Assert.False(test2Executed);
        Assert.False(test2Sub1Executed);
        Assert.False(test2Sub2Executed);

        testExecuted = false;
        testSub1Executed = false;
        testSub2Executed = false;

        test2Executed = false;
        test2Sub1Executed = false;
        test2Sub2Executed = false;

        parser.Run(new string[] { "test", "tsub2" });
        Assert.False(testExecuted);
        Assert.False(testSub1Executed);
        Assert.True(testSub2Executed);
        Assert.False(test2Executed);
        Assert.False(test2Sub1Executed);
        Assert.False(test2Sub2Executed);

        testExecuted = false;
        testSub1Executed = false;
        testSub2Executed = false;

        test2Executed = false;
        test2Sub1Executed = false;
        test2Sub2Executed = false;

        parser.Run(new string[] { "test" });
        Assert.True(testExecuted);
        Assert.False(testSub1Executed);
        Assert.False(testSub2Executed);
        Assert.False(test2Executed);
        Assert.False(test2Sub1Executed);
        Assert.False(test2Sub2Executed);

        testExecuted = false;
        testSub1Executed = false;
        testSub2Executed = false;

        test2Executed = false;
        test2Sub1Executed = false;
        test2Sub2Executed = false;

        parser.Run(new string[] { "test2", "t2sub1" });
        Assert.False(testExecuted);
        Assert.False(testSub1Executed);
        Assert.False(testSub2Executed);
        Assert.False(test2Executed);
        Assert.True(test2Sub1Executed);
        Assert.False(test2Sub2Executed);

        testExecuted = false;
        testSub1Executed = false;
        testSub2Executed = false;

        test2Executed = false;
        test2Sub1Executed = false;
        test2Sub2Executed = false;

        parser.Run(new string[] { "test2", "t2sub2" });
        Assert.False(testExecuted);
        Assert.False(testSub1Executed);
        Assert.False(testSub2Executed);
        Assert.False(test2Executed);
        Assert.False(test2Sub1Executed);
        Assert.True(test2Sub2Executed);

        testExecuted = false;
        testSub1Executed = false;
        testSub2Executed = false;

        test2Executed = false;
        test2Sub1Executed = false;
        test2Sub2Executed = false;

        parser.Run(new string[] { "test2" });
        Assert.False(testExecuted);
        Assert.False(testSub1Executed);
        Assert.False(testSub2Executed);
        Assert.True(test2Executed);
        Assert.False(test2Sub1Executed);
        Assert.False(test2Sub2Executed);
    }

    [Fact]
    public void CombineShortOptions()
    {
        var shortOptions = new ShortOptions();

        var parser = Parser.New<ShortOptions>((options) =>
        {
            shortOptions = options;
        });

        parser.Run(new string[] { "-ab" });

        Assert.True(shortOptions.FlagA);
        Assert.True(shortOptions.FlagB);
        Assert.False(shortOptions.FlagC);
        Assert.Equal(0, shortOptions.OptionS);

        shortOptions = new ShortOptions();

        parser = Parser.New<ShortOptions>((options) =>
        {
            shortOptions = options;
        });

        parser.Run(new string[] { "-acs", "15" });

        Assert.True(shortOptions.FlagA);
        Assert.False(shortOptions.FlagB);
        Assert.True(shortOptions.FlagC);
        Assert.Equal(15, shortOptions.OptionS);
    }

    [Fact]
    public void ArrayOptions()
    {
        var arrayOptions = new ArrayOptions();

        var parser = Parser.New<ArrayOptions>((options) =>
        {
            arrayOptions = options;
        });

        parser.Run(new string[] { "--string-array", "value1", "--int-array", "1", "--string-array", "value2", "value3", "--int-array:5", "32", "--int-array=-18" });

        Assert.Equal(new string[] { "value1", "value2", "value3" }, arrayOptions.StringArray);
        Assert.Equal(new int[] { 1, 5, 32, -18 }, arrayOptions.IntArray);

        parser.Run(new string[] { "--int-readonly-list", "3", "4", "5" });

        Assert.Equal(new List<int>() { 3, 4, 5 }, arrayOptions.IntReadOnlyList);

        parser.Run(new string[] { "--string-enumerable", "item1", "item2", "item3" });

        Assert.Equal(new string[] { "item1", "item2", "item3" }, arrayOptions.StringEnumerable.ToArray());
    }

    [Fact]
    public void OptionAliases()
    {
        var arrayOptions = new ArrayOptions();

        var parser = Parser.New<ArrayOptions>((options) =>
        {
            arrayOptions = options;
        });

        parser.Run(new string[] { "--int-array", "1", "--int-array-alias:5", "32", "--int-array-alias2:-18" });
        Assert.Equal(new int[] { 1, 5, 32, -18 }, arrayOptions.IntArray);
    }

    [Fact]
    public void ListOptions()
    {
        var arrayOptions = new ArrayOptions();

        var parser = Parser.New<ArrayOptions>((options) =>
        {
            arrayOptions = options;
        });

        parser.Run(new string[] { "--int-list", "1", "--int-list:5", "32", "-18" });

        Assert.Equal(new int[] { 1, 5, 32, -18 }, arrayOptions.IntList.ToArray());
    }

    [Fact]
    public void TypedOptions()
    {
        var typedOptions = new TypedOptions();

        var parser = Parser.New<TypedOptions>((options) =>
        {
            typedOptions = options;
        });

        parser.Run(new string[] { "--bool-option" });
        Assert.True(typedOptions.BoolOption);

        parser.Run(new string[] { "--bool-option", "false" });
        Assert.False(typedOptions.BoolOption);


        parser.Run(new string[] { "--sbyte-option", "15" });
        Assert.Equal(15, typedOptions.SByteOption);

        parser.Run(new string[] { "--sbyte-option", "-45" });
        Assert.Equal(-45, typedOptions.SByteOption);


        parser.Run(new string[] { "--byte-option", "134" });
        Assert.Equal(134, typedOptions.ByteOption);


        parser.Run(new string[] { "--short-option", "3789" });
        Assert.Equal(3789, typedOptions.ShortOption);

        parser.Run(new string[] { "--short-option", "-5923" });
        Assert.Equal(-5923, typedOptions.ShortOption);


        parser.Run(new string[] { "--ushort-option", ushort.MaxValue.ToString() });
        Assert.Equal(ushort.MaxValue, typedOptions.UShortOption);


        parser.Run(new string[] { "--int-option", "378902945" });
        Assert.Equal(378902945, typedOptions.IntOption);

        parser.Run(new string[] { "--int-option", "-592398450" });
        Assert.Equal(-592398450, typedOptions.IntOption);


        parser.Run(new string[] { "--uint-option", "2048578625" });
        Assert.Equal((uint)2048578625, typedOptions.UIntOption);


        parser.Run(new string[] { "--long-option", "987651343910347803" });
        Assert.Equal(987651343910347803, typedOptions.LongOption);

        parser.Run(new string[] { "--long-option", "-19930423764103845" });
        Assert.Equal(-19930423764103845, typedOptions.LongOption);


        parser.Run(new string[] { "--ulong-option", "798256207857208532" });
        Assert.Equal((ulong)798256207857208532, typedOptions.ULongOption);


        parser.Run(new string[] { "--float-option", "3.5" });
        Assert.Equal(3.5, typedOptions.FloatOption);

        parser.Run(new string[] { "--double-option", "-1.60217e-19" });
        Assert.Equal(-1.60217e-19, typedOptions.DoubleOption);

        parser.Run(new string[] { "--decimal-option", "37854.098736" });
        Assert.Equal((decimal)37854.098736, typedOptions.DecimalOption);


        parser.Run(new string[] { "--string-option", "some@really#weird^&!()*_- string" });
        Assert.Equal("some@really#weird^&!()*_- string", typedOptions.StringOption);


        parser.Run(new string[] { "--uri-option", "https://www.google.com" });
        Assert.Equal(new Uri("https://www.google.com"), typedOptions.UriOption);


        parser.Run(new string[] { "--file-info-option", Assembly.GetCallingAssembly().Location });
        Assert.Equal(new FileInfo(Assembly.GetCallingAssembly().Location).FullName, typedOptions.FileInfoOption?.FullName);

        parser.Run(new string[] { "--directory-info-option", Path.GetDirectoryName(Assembly.GetCallingAssembly().Location) ?? "" });
        Assert.Equal(new DirectoryInfo(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location) ?? "").FullName, typedOptions.DirectoryInfoOption?.FullName);


        parser.Run(new string[] { "--guid-option","{A0CC4F81-9C94-44B2-81D8-3EC378106669}" });
        Assert.Equal(Guid.Parse("{A0CC4F81-9C94-44B2-81D8-3EC378106669}"), typedOptions.GuidOption);


        parser.Run(new string[] { "--custom-option", "custom type string" });
        Assert.Equal("custom type string", typedOptions.CustomOption?.Value);

        parser.Run(new string[] { "--parse-type-option", "parse type string" });
        Assert.Equal("parse type string", typedOptions.ParseTypeOption?.Value);

        parser.Run(new string[] { "--enum-option", "value3" });
        Assert.Equal(TestValue.Value3, typedOptions.EnumOption);

        parser.Run(new string[] { "--enum-flags-option", "value1", "value4", "--guid-option", "{A0CC4F81-9C94-44B2-81D8-3EC378106696}" });
        Assert.Equal(TestFlags.Value1 | TestFlags.Value4, typedOptions.EnumFlagsOption);
        Assert.Equal(Guid.Parse("{A0CC4F81-9C94-44B2-81D8-3EC378106696}"), typedOptions.GuidOption);

        typedOptions.EnumFlagsOption = TestFlags.None;

        parser.Run(new string[] { "--enum-flags-option", "value1", "--enum-flags-option", "value2", "none" });
        Assert.Equal(TestFlags.Value1 | TestFlags.Value2, typedOptions.EnumFlagsOption);

        typedOptions.EnumFlagsOption = TestFlags.None;

        parser.Run(new string[] { "--enum-flags-option=value2+value4" });
        Assert.Equal(TestFlags.Value2 | TestFlags.Value4, typedOptions.EnumFlagsOption);

        var dt = DateTime.UtcNow;
        parser.Run(new string[] { $"--date-time-option={dt.Ticks}" });
        Assert.Equal(dt, typedOptions.DateTimeOption);
    }

    [Fact]
    public void Arguments()
    {
        var argumentOptions = new ArgumentOptions();

        var parser = Parser.New<ArgumentOptions>((options) =>
        {
            argumentOptions = options;
        });

        parser.Run(new string[] { "string-argument", "15", "array-argument-1", "array-argument-2", "array-argument-3", "--test-flag" });

        Assert.Equal("string-argument", argumentOptions.StringArgument);
        Assert.Equal(new string[] { "array-argument-1", "array-argument-2", "array-argument-3" }, argumentOptions.ArrayArgument.ToArray());
        Assert.Equal(15, argumentOptions.IntArgument);
        Assert.True(argumentOptions.TestFlag);
    }

    class InvalidRequiredArguments
    {
        [Argument(0)]
        public string? Arg1 { get; set; }

        [Argument(1, IsRequired = true)]
        public string? Arg2 { get; set; }

        [Argument(2)]
        public string? Arg3 { get; set; }
    }

    [Fact]
    public void RequiredArguments()
    {
        var argumentOptions = new InvalidRequiredArguments();

        var parser = Parser.New<InvalidRequiredArguments>((options) =>
        {
            argumentOptions = options;
        });

        var ex = Assert.Throws<AggregateException>(() => parser.Run(new string[] { "arg1", "arg2", "arg3" }));
        Assert.IsType<ArgumentException>(ex.InnerExceptions[0]);
    }

    class InvalidOrderArguments
    {
        [Argument(0)]
        public string? Arg1 { get; set; }

        [Argument(1)]
        public string? Arg2 { get; set; }

        [Argument(1)]
        public string? Arg3 { get; set; }
    }

    [Fact]
    public void ArgumentsOrder()
    {
        var argumentOptions = new InvalidOrderArguments();

        var parser = Parser.New<InvalidOrderArguments>((options) =>
        {
            argumentOptions = options;
        });

        var ex = Assert.Throws<AggregateException>(() => parser.Run(new string[] { "arg1", "arg2", "arg3" }));
        Assert.IsType<ArgumentException>(ex.InnerExceptions[0]);
    }

    [Fact]
    public void InvalidParserOption()
    {
        var invalidOptions = new InvalidParserOption();

        var parser = Parser.New<InvalidParserOption>((options) =>
        {
            invalidOptions = options;
        });

        var ex = Assert.Throws<AggregateException>(() => parser.Run(new string[] { "--value=123" }));
        Assert.IsType<ArgumentException>(ex.InnerExceptions[0]);
    }

    [Fact]
    public void NullParserOption()
    {
        var nullOptions = new NullParserOption();

        var parser = Parser.New<NullParserOption>((options) =>
        {
            nullOptions = options;
        });

        var ex = Assert.Throws<AggregateException>(() => parser.Run(new string[] { "--value=123" }));
        Assert.IsType<ArgumentNullException>(ex.InnerExceptions[0]);
    }
}
