# CommandLine
![main workflow](https://github.com/sigurn/CommandLine/actions/workflows/build.yml/badge.svg) [![nuget](https://img.shields.io/nuget/v/Sigurn.CommandLine)](https://www.nuget.org/packages/Sigurn.CommandLine/)


## Why

I looked through NuGet.org packages and haven't found decent command line parser.
They are either not command oriented so doesn't support deep command nesting (which is what I need)
or they are in integral part of other projects which next to impossible use separately.
[System.CommandLine](https://github.com/dotnet/command-line-api) looks promisisng but it is in pre-release state for two years.
So I decided to make a parser to cover my needs.

## Supported Frameworks

Currently the project is built for .NET 6 only


## Features

* Command based parser which supports nested commands
* Options and arguments are supported
* Provides help and version commands and options out of the box
* Enums (including flags) and arrays are supported
* Supports boolean flags
* Supports many built-in types as well as custom types which can be cnstructed from string or provide static method Parse.
* Basic validation (checks that all required options and arguments are provided)
* Default values for options and arguments
* Async commands
* Termination handling
* Supports setting application exit code


## Terms and conventions used in parser

* Command -- command line parameter which goes first in the command line and defines how the rest of the command line should be parsed.
* Argument -- command line parameter without name which is identified by position in the command line string.
* Option -- command line parameter which have a name and starts with `--` or `-`.

* \[name\] -- Optional value.
* \[\<name\>\] -- Optional argument.
* \<name\> -- Required argument, or value.
* {value} -- Default value for optional arguments and options.


Basically command line looks like this:
```
<application-name> [commands] [arguments] [options]
```

## Get started

Add reference to `Sigurn.CommandLine` NuGet package to your project.

```xml
  <ItemGroup>
    <PackageReference Include="Sigurn.CommandLine" Version="1.*" />
  </ItemGroup>
```

Define class describing options and arguments. Use `Option`, `Argument`, `HelpText` attributes to define options, arguments and help texts.
Then create parser with root command and options:
```C#
using Sigurn.CommandLine;

namespace GetStarted;

class GreetingOptions
{
    [Argument(0)]
    [HelpText("Name to be shown in the greeting")]
    public string Name { get; set; } = "world";

    [Option('s', "short")]
    [HelpText("Flag defines if the short version of the greeting should be shown")]
    public bool IsShort { get; set; }
}

class Program
{
    public static void Main(string[] args)
    {
        var parser = Parser.New<GreetingOptions>(options =>
        {
            if (options.IsShort)
                Console.WriteLine($"Hi, {options.Name}!");
            else
                Console.WriteLine($"Hello, {options.Name}!");
        }, "Basic 'Get started' application");

        parser.Run(args);
    }
}
```

Here is the `--help` option output for the `GetStarted` application:
```shell
Description:
  Basic 'Get started' application

Usage:
  GetStarted [<name>] [options]

Arguments:
  <name>  Name to be shown in the greeting {world}

Options:
  -s, --short [true|false]  Flag defines if the short version of the greeting should be shown {false}
  --version                 Show version information
  -?, -h, --help            Show help and usage information
```

## Advanced example

For more advanced example look at [Sigurn.CommandLine.Sample](src/Sigurn.CommandLine.Sample) project.

## Details

By default all names are optional. Commands, arguments, options will be named after methods and properties using dash notation.
So the name `IsShort` like in the 'get started' example will be `is-short`. To override it the name is defined in the `Option` attribute.
The same applies to arguments and commands.

### HelpText attribute

The attribute can be applied to properties and methods. It allows to define help text which will be shown when help command is activated.

HelpText attribute supports multi lines helps. If you want to provide several lines of help text just pass several strings into construxtor like this:
```C#
[HelpText(
    "This is the first line of the help text",
    "This is the second line of the help text",
    "You may define as many lines of the help text as you like")]
```

### Option attribute

The attribute can be applied to any public property to consider that property as an option by the command line parser.

The attribute optionally allows to define short and long name for the option.

There can be several long names defined for the single option: first name is considered as a option name and all others are treated like aliases of the option. You can define as many aliases as you like.

The attribute also supports flag `IsRequired` to define thet the option is mandatory.

Here is an example of the option with all possible parameters.
```C#
[Option('c', "color", "colour", IsRequired=true)]
```

### Argument attribute

The attrubute can be applied to any public property to consider that property as an attribute by the command line parser.

The attribute has a manatory parameter `Position` it is an integer number which defines order of the attribute in the command line. The bigger numbers go later in the command line.

Optionally the attribute allows to define name of the argument which will be shown in help and flag `IsRequired` to define that the argument is mandatory.

Here is an example of the argument with all possible parameters.
```C#
[Argument(0, Name="test", IsRequired=true)]
```

### CommandName attribute

The attribute can be applied to a method to change the default command name. By default command will be named after the method name with dash notation.

The attrobute takes a signe parameter: the command name.
Here is an example of the command name.
```C#
[CommandName("hello")]
```

### Default values

To defien a default value for option or argument just provide a default value for class property.

### Commands

The command line parser is command based so the command is a central part of the parser configuration.

Command is represented by class `Command` and generic сlass `Command<T>` the later represents a command with options or arguments where as the first one represents a command without options and arguments.

Everything starts from the root command which has empty name and is executed when application without parameters is started.

Static methods `New` of the `Parser` class accept a delegate or `Command` instance as a parameter this very command will be a root command.

There is also method `New` without arguments which adds and empty root command that doesn't do anything.

#### Empty root command
```C#
var parser = Parser.New();
```

#### Empty root command with help text
```C#
var parser = Parser.New("Help text here");
```

```C#
var parser = Parser.New(new Command("Help text here"));
```

#### Root command with help text
```C#
var parser = Parser.New(() => Console.WriteLine("Hello, world!"), "Hello world application");
```

```C#
var parser = Parser.New(new Command(
  () => Console.WriteLine("Hello, world!"),
  "Hello world application"));
```

#### Root command with help text which returns result
```C#
var parser = Parser.New(() => 
{
  Console.WriteLine("Hello, world!");
  return 0;
}, "Hello world application");
```

```C#
var parser = Parser.New(new Command(() => 
{
  Console.WriteLine("Hello, world!");
  return 0;
}, "Hello world application"));
```

#### Async root command with help text
```C#
var parser = Parser.New(() => 
{
  Console.WriteLine("Hello, world!");
  return Task.CompletedTask;
}, "Hello world application");
```

```C#
var parser = Parser.New(new Command(() => 
{
  Console.WriteLine("Hello, world!");
  return Task.CompletedTask;
}, "Hello world application"));
```

#### Async root command with help text which returns result
```C#
var parser = Parser.New(() => 
{
  Console.WriteLine("Hello, world!");
  return Task.FromResult(0);
}, "Hello world application");
```

```C#
var parser = Parser.New(new Command(() => 
{
  Console.WriteLine("Hello, world!");
  return Task.FromResult(0);
}, "Hello world application"));
```

#### Root command with options and help text
```C#
var parser = Parser.New<TestOptions>(options => Console.WriteLine("Hello, world!"), "Hello world application");
```

```C#
var parser = Parser.New(new Command<TestOptions>(
  options => Console.WriteLine("Hello, world!"),
  "Hello world application"));
```

#### Root command with options and help text and returning result
```C#
var parser = Parser.New<TestOptions>(options => 
{
  Console.WriteLine("Hello, world!");
  return 0;
}, "Hello world application");
```

```C#
var parser = Parser.New(new Command<TestOptions>(options => 
{
  Console.WriteLine("Hello, world!");
  return 0;
}, "Hello world application"));
```

#### Async root command with options and help text
```C#
var parser = Parser.New<TestOptions>((options, cancellationToken) => 
{
  Console.WriteLine("Hello, world!");
  return Task.CompletedTask;
}, "Hello world application");
```

```C#
var parser = Parser.New(new Command<TestOptions>((options, cancellationToken) => 
{
  Console.WriteLine("Hello, world!");
  return Task.CompletedTask;
}, "Hello world application"));
```

#### Async root command with options and help text and returning result
```C#
var parser = Parser.New<TestOptions>((options, cancellationToken) => 
{
  Console.WriteLine("Hello, world!");
  return Task.FromResult(0);
}, "Hello world application");
```

```C#
var parser = Parser.New(new Command<TestOptions>((options, cancellationToken) => 
{
  Console.WriteLine("Hello, world!");
  return Task.FromResult(0);
}, "Hello world application"));
```

### Subcommands

Subcommands are just commands which were added to parent command.
Subcommands may also have their own subcommands and so on as many time as needed.

#### Add subcommand to root command
```C#
var parser = Parser.New()
  .WithCommand("hello", () => Console.WriteLine("Hello, world!"), "Hello command help text");
```

#### Add subcommand to subcommand
```C#
var parser = Parser.New()
  .WithCommand(new Command("hello", () => {Console.WriteLine("Hello, world!"), "Hello command help text")
    .WitrhCommand("subcommand", () => Concole.WriteLine("subcommand")));
```

### Use methods as commands
```C#
static class Commands
{
  [CommandName("hello")]
  [HelpText("Hello comamahd help text")]
  public static void Hello()
  {
    Console.WriteLine("Hello, world!");
  }

  public static void Subcommand()
  {
    Console.WriteLine("subcommand");
  }
}

class Program
{
  public static void Main(string[] args)
  {
    var parser = Parser.New()
      .WithCommand(new Command(Commands.Hello)
        .WitrhCommand(Commands.Subcommand));
  }
}
```

### Custom version handler
By default option --version prints assembly version to the console like
```shell
1.0.0.0
```

You can provide custom handler for version command
```C#
var parser = Parser.New()
  .WithVersionCommand(() => Console.WriteLine("Custom version"));
```
