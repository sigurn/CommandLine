using System.Globalization;
using System.Reflection;
using System.Text;

namespace Sigurn.CommandLine;

/// <summary>
/// The class represnts command line command.
/// </summary>
/// <remarks>Every command can have sub-commands, options and arguments.
/// This class represents command without options and arguments.
/// Command with options and arguments represented by class <see cref="Command{T}"/></remarks>
public class Command : ITokenParser
{
    private readonly Func<CancellationToken,Task<int>> _action;
    private readonly Dictionary<string, Command> _commands = new Dictionary<string, Command>();

    private bool IsRootCommand => Parent == null;

    private Command? Parent { get; set; }

    private string? Name { get; set; }

    private string[] HelpText { get; init; }


    private Action _showVersion = ShowVersion;

    private IOptions? _options;
    private IOptions Options
    {
        get
        {
            if (_options == null)
                _options = CreateOptions();

            ValidateOptions(_options);

            return _options;
        }
    }

    private void ValidateOptions(IOptions options)
    {
        var names = options.OptionsList.SelectMany(x => x.Names).ToList();
        var shortNames = options.OptionsList.Select(x => x.ShortName).ToList();

        if (names.Contains("help"))
            throw new ArgumentException($"The option 'help' is already defined");

        if (IsRootCommand && names.Contains("version"))
            throw new ArgumentException($"The option 'version' is already defined");

        if (shortNames.Contains('h'))
            throw new ArgumentException($"The short option 'h' is already defined");

        if (shortNames.Contains('?'))
            throw new ArgumentException($"The short option '?' is already defined");
    }

    /// <summary>
    /// Initializes a new instance of the class with help text.
    /// </summary>
    /// <param name="helpText">Help text to be shown in help.</param>
    public Command(params string[] helpText)
    {
        _action = (_) => Task.FromResult(0);
        HelpText = helpText ?? Array.Empty<string>();
    }

    /// <summary>
    /// Initializes a new instance of the class with action and help text.
    /// </summary>
    /// <param name="action">Action to be called when command is activaled.</param>
    /// <param name="helpText">Help text to be shown in help.</param>
    public Command(Func<CancellationToken,Task<int>> action, params string[] helpText)
    {
        _action = action;
        HelpText = helpText ?? Array.Empty<string>();
    }

    /// <summary>
    /// Initializes a new instance of the class with action and help text.
    /// </summary>
    /// <param name="action">Action to be called when command is activaled.</param>
    /// <param name="helpText">Help text to be shown in help.</param>
    public Command(Func<CancellationToken,Task> action, params string[] helpText)
    {
        _action = async (cancellationToken) =>
        {
            await action(cancellationToken);
            return 0;
        };
        HelpText = helpText ?? Array.Empty<string>();
    }

    /// <summary>
    /// Initializes a new instance of the class with action and help text.
    /// </summary>
    /// <param name="action">Action to be called when command is activaled.</param>
    /// <param name="helpText">Help text to be shown in help.</param>
    public Command(Func<int> action, params string[] helpText)
    {
        _action = (_) => Task.FromResult(action());
        HelpText = helpText ?? Array.Empty<string>();
    }

    /// <summary>
    /// Initializes a new instance of the class with action and help text.
    /// </summary>
    /// <param name="action">Action to be called when command is activaled.</param>
    /// <param name="helpText">Help text to be shown in help.</param>
    public Command(Action action, params string[] helpText)
    {
        _action = (_) =>
        {
            action();
            return Task.FromResult(0);
        };
        HelpText = helpText ?? Array.Empty<string>();
    }

    /// <summary>
    /// Adds subcommand to the command.
    /// </summary>
    /// <param name="name">Name of the subcommand.</param>
    /// <param name="command">Subcommand.</param>
    public void AddCommand(string name, Command command)
    {
        _commands[name] = command;
        command.Name = name;
        command.Parent = this;
    }

    /// <summary>
    /// Adds subcommand to the command.
    /// </summary>
    /// <param name="name">Name of the subcommand.</param>
    /// <param name="command">Subcommand.</param>
    /// <returns>Instance of the command to be able form chain of method calls.</returns>
    public Command WithCommand(string name, Command command)
    {
        AddCommand(name, command);
        return this;
    }

    /// <summary>
    /// Adds subcommand to the command.
    /// </summary>
    /// <param name="name">Name of the subcommand.</param>
    /// <param name="action">Action to be executed when the subcommand is activated.</param>
    /// <param name="helpText">Help text to be shown in help.</param>
    public void AddCommand(string name, Func<CancellationToken, Task> action, params string[] helpText)
    {
        AddCommand(name, new Command(action, helpText));
    }

    /// <summary>
    /// Adds subcommand to the command.
    /// </summary>
    /// <param name="name">Name of the subcommand.</param>
    /// <param name="action">Action to be executed when the subcommand is activated.</param>
    /// <param name="helpText">Help text to be shown in help.</param>
    /// <returns>Instance of the command to be able form chain of method calls.</returns>
    public Command WithCommand(string name, Func<CancellationToken, Task> action, params string[] helpText)
    {
        return WithCommand(name, new Command(action, helpText));
    }

    /// <summary>
    /// Adds subcommand to the command.
    /// </summary>
    /// <param name="name">Name of the subcommand.</param>
    /// <param name="action">Action to be executed when the subcommand is activated.</param>
    /// <param name="helpText">Help text to be shown in help.</param>
    public void AddCommand(string name, Action action, params string[] helpText)
    {
        AddCommand(name, new Command(action, helpText));
    }

    /// <summary>
    /// Adds subcommand to the command.
    /// </summary>
    /// <param name="name">Name of the subcommand.</param>
    /// <param name="action">Action to be executed when the subcommand is activated.</param>
    /// <param name="helpText">Help text to be shown in help.</param>
    /// <returns>Instance of the command to be able form chain of method calls.</returns>
    public Command WithCommand(string name, Action action, params string[] helpText)
    {
        return WithCommand(name, new Command(action, helpText));
    }

    /// <summary>
    /// Adds subcommand to the command.
    /// </summary>
    /// <param name="name">Name of the subcommand.</param>
    /// <param name="action">Action to be executed when the subcommand is activated.</param>
    /// <param name="helpText">Help text to be shown in help.</param>
    public void AddCommand(string name, Func<int> action, params string[] helpText)
    {
        AddCommand(name, new Command(action, helpText));
    }

    /// <summary>
    /// Adds subcommand to the command.
    /// </summary>
    /// <param name="name">Name of the subcommand.</param>
    /// <param name="action">Action to be executed when the subcommand is activated.</param>
    /// <param name="helpText">Help text to be shown in help.</param>
    /// <returns>Instance of the command to be able form chain of method calls.</returns>
    public Command WithCommand(string name, Func<int> action, params string[] helpText)
    {
        return WithCommand(name, new Command(action, helpText));
    }

    /// <summary>
    /// Adds subcommand with options and arguments to the command.
    /// </summary>
    /// <typeparam name="T">Type which describes options and arguments.</typeparam>
    /// <param name="name">Name of the subcommand.</param>
    /// <param name="action">Action to be executed when the subcommand is activated.</param>
    /// <param name="helpText">Help text to be shown in help.</param>
    public void AddCommand<T>(string name, Func<T, CancellationToken, Task> action, params string[] helpText) where T : class, new()
    {
        AddCommand(name, new Command<T>(action, helpText));
    }

    /// <summary>
    /// Adds subcommand with options and arguments to the command.
    /// </summary>
    /// <typeparam name="T">Type which describes options and arguments.</typeparam>
    /// <param name="name">Name of the subcommand.</param>
    /// <param name="action">Action to be executed when the subcommand is activated.</param>
    /// <param name="helpText">Help text to be shown in help.</param>
    /// <returns>Instance of the command to be able form chain of method calls.</returns>
    public Command WithCommand<T>(string name, Func<T, CancellationToken, Task> action, params string[] helpText) where T : class, new()
    {
        return WithCommand(name, new Command<T>(action, helpText));
    }

    /// <summary>
    /// Adds subcommand with options and arguments to the command.
    /// </summary>
    /// <typeparam name="T">Type which describes options and arguments.</typeparam>
    /// <param name="name">Name of the subcommand.</param>
    /// <param name="action">Action to be executed when the subcommand is activated.</param>
    /// <param name="helpText">Help text to be shown in help.</param>
    public void AddCommand<T>(string name, Action<T> action, params string[] helpText) where T : class, new()
    {
        AddCommand(name, new Command<T>(action, helpText));
    }

    /// <summary>
    /// Adds subcommand with options and arguments to the command.
    /// </summary>
    /// <typeparam name="T">Type which describes options and arguments.</typeparam>
    /// <param name="name">Name of the subcommand.</param>
    /// <param name="action">Action to be executed when the subcommand is activated.</param>
    /// <param name="helpText">Help text to be shown in help.</param>
    /// <returns>Instance of the command to be able form chain of method calls.</returns>
    public Command WithCommand<T>(string name, Action<T> action, params string[] helpText) where T : class, new()
    {
        return WithCommand(name, new Command<T>(action, helpText));
    }

    /// <summary>
    /// Adds subcommand with options and arguments to the command.
    /// </summary>
    /// <typeparam name="T">Type which describes options and arguments.</typeparam>
    /// <param name="name">Name of the subcommand.</param>
    /// <param name="action">Action to be executed when the subcommand is activated.</param>
    /// <param name="helpText">Help text to be shown in help.</param>
    public void AddCommand<T>(string name, Func<T, int> action, params string[] helpText) where T : class, new()
    {
        AddCommand(name, new Command<T>(action, helpText));
    }

    /// <summary>
    /// Adds subcommand with options and arguments to the command.
    /// </summary>
    /// <typeparam name="T">Type which describes options and arguments.</typeparam>
    /// <param name="name">Name of the subcommand.</param>
    /// <param name="action">Action to be executed when the subcommand is activated.</param>
    /// <param name="helpText">Help text to be shown in help.</param>
    /// <returns>Instance of the command to be able form chain of method calls.</returns>
    public Command WithCommand<T>(string name, Func<T,int> action, params string[] helpText) where T : class, new()
    {
        return WithCommand(name, new Command<T>(action, helpText));
    }

    /// <summary>
    /// Adds subcommand with options and arguments to the command.
    /// </summary>
    /// <remarks>Command name and help text will be aqured from the method attributes 
    /// <see cref="CommandNameAttribute"/> and <see cref="HelpTextAttribute"/>.
    /// If attributes are missing name will be formed from method name and help text will be empty.</remarks>
    /// <typeparam name="T">Type which defines command options and arguments </typeparam>
    /// <param name="action">Action to be executed when the command is activated.</param>
    /// <returns>Instance of the command to be able form chain of method calls.</returns>
    public Command WithCommand<T>(Func<T, CancellationToken, Task> action) where T : class, new()
    {
        var name = Helpers.GetCommandNameFromDelegate(action);
        var helpText = Helpers.GetHelpTextFromDelegate(action);
        AddCommand(name, action, helpText);
        return this;
    }

    /// <summary>
    /// Adds subcommand with options and arguments to the command.
    /// </summary>
    /// <remarks>Command name and help text will be aqured from the method attributes 
    /// <see cref="CommandNameAttribute"/> and <see cref="HelpTextAttribute"/>.
    /// If attributes are missing name will be formed from method name and help text will be empty.</remarks>
    /// <typeparam name="T">Type which defines command options and arguments </typeparam>
    /// <param name="action">Action to be executed when the command is activated.</param>
    /// <returns>Instance of the command to be able form chain of method calls.</returns>
    public Command WithCommand<T>(Func<T, CancellationToken, Task<int>> action) where T : class, new()
    {
        var name = Helpers.GetCommandNameFromDelegate(action);
        var helpText = Helpers.GetHelpTextFromDelegate(action);
        AddCommand(name, action, helpText);
        return this;
    }

    /// <summary>
    /// Adds subcommand with options and arguments to the command.
    /// </summary>
    /// <remarks>Command name and help text will be aqured from the method attributes 
    /// <see cref="CommandNameAttribute"/> and <see cref="HelpTextAttribute"/>.
    /// If attributes are missing name will be formed from method name and help text will be empty.</remarks>
    /// <typeparam name="T">Type which defines command options and arguments </typeparam>
    /// <param name="action">Action to be executed when the command is activated.</param>
    /// <returns>Instance of the command to be able form chain of method calls.</returns>
    public Command WithCommand<T>(Action<T> action) where T : class, new()
    {
        var name = Helpers.GetCommandNameFromDelegate(action);
        var helpText = Helpers.GetHelpTextFromDelegate(action);
        AddCommand(name, action, helpText);
        return this;
    }

    /// <summary>
    /// Adds subcommand with options and arguments to the command.
    /// </summary>
    /// <remarks>Command name and help text will be aqured from the method attributes 
    /// <see cref="CommandNameAttribute"/> and <see cref="HelpTextAttribute"/>.
    /// If attributes are missing name will be formed from method name and help text will be empty.</remarks>
    /// <typeparam name="T">Type which defines command options and arguments </typeparam>
    /// <param name="action">Action to be executed when the command is activated.</param>
    /// <returns>Instance of the command to be able form chain of method calls.</returns>
    public Command WithCommand<T>(Func<T, int> action) where T : class, new()
    {
        var name = Helpers.GetCommandNameFromDelegate(action);
        var helpText = Helpers.GetHelpTextFromDelegate(action);
        AddCommand(name, action, helpText);
        return this;
    }

    /// <summary>
    /// Adds subcommand to the command.
    /// </summary>
    /// <remarks>Command name and help text will be aqured from the method attributes 
    /// <see cref="CommandNameAttribute"/> and <see cref="HelpTextAttribute"/>.
    /// If attributes are missing name will be formed from method name and help text will be empty.</remarks>
    /// <param name="action">Action to be executed when the command is activated.</param>
    /// <returns>Instance of the command to be able form chain of method calls.</returns>
    public Command WithCommand(Func<CancellationToken, Task> action)
    {
        var name = Helpers.GetCommandNameFromDelegate(action);
        var helpText = Helpers.GetHelpTextFromDelegate(action);
        AddCommand(name, action, helpText);
        return this;
    }

    /// <summary>
    /// Adds subcommand to the command.
    /// </summary>
    /// <remarks>Command name and help text will be aqured from the method attributes 
    /// <see cref="CommandNameAttribute"/> and <see cref="HelpTextAttribute"/>.
    /// If attributes are missing name will be formed from method name and help text will be empty.</remarks>
    /// <param name="action">Action to be executed when the command is activated.</param>
    /// <returns>Instance of the command to be able form chain of method calls.</returns>
    public Command WithCommand(Func<CancellationToken, Task<int>> action)
    {
        var name = Helpers.GetCommandNameFromDelegate(action);
        var helpText = Helpers.GetHelpTextFromDelegate(action);
        AddCommand(name, action, helpText);
        return this;
    }

    /// <summary>
    /// Adds subcommand to the command.
    /// </summary>
    /// <remarks>Command name and help text will be aqured from the method attributes 
    /// <see cref="CommandNameAttribute"/> and <see cref="HelpTextAttribute"/>.
    /// If attributes are missing name will be formed from method name and help text will be empty.</remarks>
    /// <param name="action">Action to be executed when the command is activated.</param>
    /// <returns>Instance of the command to be able form chain of method calls.</returns>
    public Command WithCommand(Action action)
    {
        var name = Helpers.GetCommandNameFromDelegate(action);
        var helpText = Helpers.GetHelpTextFromDelegate(action);
        AddCommand(name, action, helpText);
        return this;
    }

    /// <summary>
    /// Adds subcommand to the command.
    /// </summary>
    /// <remarks>Command name and help text will be aqured from the method attributes 
    /// <see cref="CommandNameAttribute"/> and <see cref="HelpTextAttribute"/>.
    /// If attributes are missing name will be formed from method name and help text will be empty.</remarks>
    /// <param name="action">Action to be executed when the command is activated.</param>
    /// <returns>Instance of the command to be able form chain of method calls.</returns>
    public Command WithCommand(Func<int> action)
    {
        var name = Helpers.GetCommandNameFromDelegate(action);
        var helpText = Helpers.GetHelpTextFromDelegate(action);
        AddCommand(name, action, helpText);
        return this;
    }


    internal void SetShowVersion(Action showVersion)
    {
        if (!IsRootCommand)
            throw new InvalidOperationException("Version command available only on root command");

        _showVersion = showVersion ?? throw new ArgumentNullException(nameof(showVersion));
    }

    private (string, string?) ParseOption(string token)
    {
        string name = token;
        string? value = null;
        if (token.Contains('='))
        {
            var parts = token.Split('=');
            name = parts[0];
            value = parts.Length > 2 ? string.Join('=', parts.Skip(1)) : parts[1];
        }
        else if (token.Contains(':'))
        {
            var parts = token.Split(':');
            name = parts[0];
            value = parts.Length > 2 ? string.Join(':', parts.Skip(1)) : parts[1];
        }

        return (name, value);
    }

    ITokenParser ITokenParser.ParseToken(string token)
    {
        if (token.StartsWith("--"))
        {
            if (token == "--help")
                return new Command(ShowHelp);

            if (token == "--version" && IsRootCommand)
                return new Command(_showVersion);

            (string name, string? value) = ParseOption(token.TrimStart('-'));

            var tokenHandler = Options.GetOption(name);
            if (tokenHandler == null)
                throw new CommandLineException($"Unknown option '{name}'");

            if (value != null)
                return tokenHandler.ParseToken(value);

            return tokenHandler;
        }
        else if (token.StartsWith("-"))
        {
            if (token == "-?" || token == "-h")
                return new Command(ShowHelp);

            (string name, string? value) = ParseOption(token.TrimStart('-'));
            ITokenParser tokenHandler = this;

            foreach (var c in name)
            {
                var handler = Options.GetOption(c);
                if (handler == null)
                    throw new CommandLineException($"Unknown option '{c}'");

                if (handler is ValueParser vp && vp.IsFlag)
                    tokenHandler = handler.ParseToken("true");
                else
                    tokenHandler = handler;
            }

            if (value != null)
                return tokenHandler.ParseToken(value);

            return tokenHandler;
        }
        else
        {
            if (_commands.Count != 0)
            {
                if (token == "help")
                    return new Command(ShowHelp);

                if (token == "version" && IsRootCommand)
                    return new Command(_showVersion);
            }

            if (_commands.ContainsKey(token))
                return _commands[token];

            var handler = Options.GetArgument(this);
            if (handler == null)
                throw new CommandLineException($"Usupported argument {token}");

            return handler.ParseToken(token);
        }
    }

    internal T? GetOptions<T>() where T : class
    {
        ValidateOptions();
        return Options.Value as T;
    }

    internal virtual async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        return await _action(cancellationToken);
    }

    internal virtual IOptions CreateOptions()
    {
        return new NoOptions();
    }

    private void ValidateOptions()
    {
        foreach (var opt in Options.OptionsList)
            if (opt.IsRequired && !opt.IsSet)
                throw new CommandLineException($"Required option '--{opt.Names[0]}' is not provided.");

        foreach (var arg in Options.ArgumentsList)
            if (arg.IsRequired && !arg.IsSet)
                throw new CommandLineException($"Required argument <{arg.Name}> is not provided.");
    }

    private int ShowHelp()
    {
        Console.WriteLine("Description:");
        if (HelpText != null && HelpText.Length > 0)
            foreach(var str in HelpText)
                Console.WriteLine($"  {str}");

        Console.WriteLine();
        Console.WriteLine("Usage:");
        var sb = new StringBuilder();
        sb.AppendFormat(CultureInfo.CurrentCulture, "  {0}", Assembly.GetEntryAssembly()?.GetName().Name);

        if (!IsRootCommand)
        {
            sb.Append(' ');
            sb.AppendJoin(' ', GetCommandPath());
        }

        if (_commands.Count != 0)
            sb.Append(" <command>");

        foreach(var arg in Options.ArgumentsList)
                sb.Append(arg.IsRequired ? (arg.IsArray ? $" <{arg.Name}...>" : $" <{arg.Name}>") : 
                    (arg.IsArray ? $" [<{arg.Name}...>]" : $" [<{arg.Name}>]"));

        sb.Append(" [options]");

        Console.WriteLine(sb.ToString());

        // Show arguments section
        if (Options.ArgumentsList.Count != 0)
        {
            Console.WriteLine();
            Console.WriteLine("Arguments:");

            var cmdList = Options.ArgumentsList
                .Select(x =>
                {
                    sb.Clear();
                    sb.Append("  <");
                    sb.Append(x.Name);
                    if (x.IsArray)
                        sb.Append("...");
                    sb.Append('>');

                    if (!x.IsRequired)
                        sb.Append(' ').Append('{').Append(x.Value).Append('}');

                    return (Name: sb.ToString(), Help: x.HelpText);
                });

            ShowTable(cmdList.ToList());
        }

        // Show Options section
        Console.WriteLine();
        Console.WriteLine("Options:");
        var optList = Options.OptionsList
            .Select(x =>
            {
                sb.Clear();
                sb.Append("  ");
                if (x.ShortName != char.MinValue)
                    sb.Append($"-{x.ShortName}, ");
                sb.Append($"--");
                sb.AppendJoin('|', x.Names);

                sb.Append(' ');
                if (x.IsFlag)
                    sb.Append($"[{x.HelpValue}]");
                else
                    sb.Append($"<{x.HelpValue}>");

                if (!x.IsRequired)
                {
                    sb.Append(' ');
                    sb.Append('{');
                    sb.Append(x.Value);
                    sb.Append('}');
                }

                return (Name: sb.ToString(), Help: x.HelpText);
            });

        if (IsRootCommand)
            optList = optList.Append((Name: "  --version", Help: new string[] { "Show version information" }));

        optList = optList.Append((Name: "  -?, -h, --help", Help: new string[] { "Show help and usage information" }));

        ShowTable(optList.ToList());

        // Show Commands
        if (_commands.Count != 0)
        {
            Console.WriteLine();
            Console.WriteLine("Commands:");

            var cmdList = _commands
                .Select(x => (Name: $"  {x.Key}", Help: x.Value.HelpText));
            if (IsRootCommand)
                cmdList = cmdList.Append((Name: "  version", Help: new string[] { "Show version information" }));
            cmdList = cmdList.Append((Name: "  help", Help: new string[] { "Show help and usage information" }));

            ShowTable(cmdList.ToList());
        }

        return 0;
    }

    private static void ShowTable(IReadOnlyList<(string Name, string[] Help)> items)
    {
        var maxLen = items.Select(x => x.Name.Length).Max();

        var sb = new StringBuilder();
        foreach (var item in items)
        {
            sb.Clear();
            sb.Append(item.Name);
            sb.Append(' ', maxLen - item.Name.Length);
            sb.Append("  ");
            if (item.Help != null && item.Help.Length > 0)
            {
                foreach (var help in item.Help.SkipLast(1))
                {
                    sb.Append(help);
                    Console.WriteLine(sb.ToString());
                    sb.Clear();
                    sb.Append(' ', maxLen + 2);
                }
                sb.Append(item.Help.Last());
            }
            Console.WriteLine(sb.ToString());
        }
    }

    private IEnumerable<string> GetCommandPath()
    {
        return GetCommandPathItems()
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Reverse();
    }

    private IEnumerable<string> GetCommandPathItems()
    {
        var cmd = this;
        while (cmd != null)
        {
            yield return cmd.Name ?? "";
            cmd = cmd.Parent;
        }
    }

    private static void ShowVersion()
    {
        Console.WriteLine(Assembly.GetEntryAssembly()?.GetName().Version);
    }
}

/// <summary>
/// The class represents a command with options and arguments.
/// </summary>
/// <typeparam name="T">Type which describes options and arguments for the command.</typeparam>
public class Command<T> : Command where T : class, new()
{
    private readonly Func<T, CancellationToken, Task<int>> _action;

    /// <summary>
    /// Initializes a new instance of the class with action and help text.
    /// </summary>
    /// <param name="action">Action to be called when command is activated.</param>
    /// <param name="helpText">Help text to be shown in help.</param>
    public Command(Func<T, CancellationToken, Task<int>> action, params string[] helpText)
        : base(helpText)
    {
        _action = action;
    }

    /// <summary>
    /// Initializes a new instance of the class with action and help text.
    /// </summary>
    /// <param name="action">Action to be called when command is activated.</param>
    /// <param name="helpText">Help text to be shown in help.</param>
    public Command(Func<T, CancellationToken, Task> action, params string[] helpText)
        : base(helpText)
    {
        _action = async (options, cancellationToken) =>
        {
            await action(options, cancellationToken);
            return 0;
        };
    }

    /// <summary>
    /// Initializes a new instance of the class with action and help text.
    /// </summary>
    /// <param name="action">Action to be called when command is activated.</param>
    /// <param name="helpText">Help text to be shown in help.</param>
    public Command(Func<T, int> action, params string[] helpText)
        : base(helpText)
    {
        _action = (options, _) => Task.FromResult(action(options));
    }

    /// <summary>
    /// Initializes a new instance of the class with action and help text.
    /// </summary>
    /// <param name="action">Action to be called when command is activated.</param>
    /// <param name="helpText">Help text to be shown in help.</param>
    public Command(Action<T> action, params string[] helpText)
        : base(helpText)
    {
        _action = (options, _) =>
        {
            action(options);
            return Task.FromResult(0);
        };
    }

    internal override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        return await _action(GetOptions<T>() ?? new T(), cancellationToken);
    }

    internal override IOptions CreateOptions()
    {
        return new Options<T>(this);
    }
}
