namespace Sigurn.CommandLine;

/// <summary>
/// Command line parser.
/// </summary>
public class Parser
{
    /// <summary>
    /// Creates a new instance of the command line parser.
    /// </summary>
    /// <typeparam name="T">Type which defines root command options and arguments.</typeparam>
    /// <param name="action">Root action which is executed when no commands are provided.</param>
    /// <param name="helpText">Help text to be shown for root command in help.</param>
    /// <returns>A new intsance of the command line parser.</returns>
    public static Parser New<T>(Func<T, int> action, params string[] helpText) where T : class, new()
    {
        var parser = new Parser(new Command<T>(action, helpText));
        return parser;
    }

    /// <summary>
    /// Creates a new instance of the command line parser.
    /// </summary>
    /// <typeparam name="T">Type which defines root command options and arguments.</typeparam>
    /// <param name="action">Root action which is executed when no commands are provided.</param>
    /// <param name="helpText">Help text to be shown for root command in help.</param>
    /// <returns>A new intsance of the command line parser.</returns>
    public static Parser New<T>(Action<T> action, params string[] helpText) where T : class, new()
    {
        var parser = new Parser(new Command<T>(action, helpText));
        return parser;
    }

    /// <summary>
    /// Creates a new instance of the command line parser.
    /// </summary>
    /// <typeparam name="T">Type which defines root command options and arguments.</typeparam>
    /// <param name="action">Root action which is executed when no commands are provided.</param>
    /// <param name="helpText">Help text to be shown for root command in help.</param>
    /// <returns>A new intsance of the command line parser.</returns>
    public static Parser New<T>(Func<T, CancellationToken, Task<int>> action, params string[] helpText) where T : class, new()
    {
        var parser = new Parser(new Command<T>(action, helpText));
        return parser;
    }

    /// <summary>
    /// Creates a new instance of the command line parser.
    /// </summary>
    /// <typeparam name="T">Type which defines root command options and arguments.</typeparam>
    /// <param name="action">Root action which is executed when no commands are provided.</param>
    /// <param name="helpText">Help text to be shown for root command in help.</param>
    /// <returns>A new intsance of the command line parser.</returns>
    public static Parser New<T>(Func<T, CancellationToken, Task> action, params string[] helpText) where T : class, new()
    {
        var parser = new Parser(new Command<T>(action, helpText));
        return parser;
    }

    /// <summary>
    /// Creates a new instance of the command line parser.
    /// </summary>
    /// <remarks>This method creates a new instance of the command line parser with empty root command.
    /// That means that nothing will be executed if no command is provided.</remarks>
    /// <returns>A new intsance of the command line parser.</returns>
    public static Parser New()
    {
        var parser = new Parser(new Command((_) => Task.CompletedTask));
        return parser;
    }

    /// <summary>
    /// Creates a new instance of the command line parser.
    /// </summary>
    /// <remarks>This method creates a new instance of the command line parser with empty root command.
    /// That means that nothing will be executed if no command is provided.</remarks>
    /// <param name="helpText">Help text</param>
    /// <returns>A new intsance of the command line parser.</returns>
    public static Parser New(params string[] helpText)
    {
        var parser = new Parser(new Command((_) => Task.CompletedTask, helpText));
        return parser;
    }

    /// <summary>
    /// Creates a new instance of the command line parser.
    /// </summary>
    /// <remarks>The created instance will be with command which does not need options and arguments.</remarks>
    /// <param name="action">Root action which is executed when no commands are provided.</param>
    /// <param name="helpText">Help text to be shown for root command in help.</param>
    /// <returns>A new intsance of the command line parser.</returns>
    public static Parser New(Action action, params string[] helpText)
    {
        var parser = new Parser(new Command(action, helpText));
        return parser;
    }

    /// <summary>
    /// Creates a new instance of the command line parser.
    /// </summary>
    /// <remarks>The created instance will be with command which does not need options and arguments.</remarks>
    /// <param name="action">Root action which is executed when no commands are provided.</param>
    /// <param name="helpText">Help text to be shown for root command in help.</param>
    /// <returns>A new intsance of the command line parser.</returns>
    public static Parser New(Func<int> action, string[] helpText)
    {
        var parser = new Parser(new Command(action, helpText));
        return parser;
    }

    private readonly Command _command;
    private Action? _showCancellationNotification = ShowCancellationNotification;

    private Parser(Command command)
    {
        _command = command;
    }


    /// <summary>
    /// Defines action which will be used for 'version' command.
    /// </summary>
    /// <param name="action">Action which will be executed when 'version' command is activated.</param>
    /// <returns>The current instance of the parser to be able to chain commands.</returns>
    public Parser WithVersionCommand(Action action)
    {
        _command.SetShowVersion(action);
        return this;
    }

    /// <summary>
    /// Defines method to be called to show custom cancellation notification message.
    /// </summary>
    /// <param name="action">Action to be called on cancellation.</param>
    /// <returns>The current instance of the parser to be able to chain commands.</returns>
    public Parser WithCustomCancellationNotification(Action? action)
    {
        _showCancellationNotification = action;

        return this;
    }

    /// <summary>
    /// Adds first level command to the parser.
    /// </summary>
    /// <param name="name">Name of the command.</param>
    /// <param name="command">Instance of the command which defines options, arguments and actions for the command.</param>
    /// <returns>The current instance of the parser to be able to chain commands.</returns>
    public Parser WithCommand(string name, Command command)
    {
        _command.AddCommand(name, command);
        return this;
    }

    /// <summary>
    /// Adds first level command to the parser.
    /// </summary>
    /// <remarks>This command looks like a first level command in command line.
    /// This command doesn't have any options or arguments.</remarks>
    /// <param name="name">Name of the command.</param>
    /// <param name="action">Action to be executed when the command is activated.</param>
    /// <param name="helpText">Help text to be shown for root command in help.</param>
    /// <returns>The current instance of the parser to be able to chain commands.</returns>
    public Parser WithCommand(string name, Action action, string helpText = "")
    {
        _command.AddCommand(name, action, helpText);
        return this;
    }

    /// <summary>
    /// Adds first level command to the parser.
    /// </summary>
    /// <remarks>This command looks like a first level command in command line.
    /// This command doesn't have any options or arguments.</remarks>
    /// <param name="name">Name of the command.</param>
    /// <param name="action">Action to be executed when the command is activated.</param>
    /// <param name="helpText">Help text to be shown for root command in help.</param>
    /// <returns>The current instance of the parser to be able to chain commands.</returns>
    public Parser WithCommand(string name, Func<int> action, string helpText = "")
    {
        _command.AddCommand(name, action, helpText);
        return this;
    }

    /// <summary>
    /// Adds first level command to the parser.
    /// </summary>
    /// <typeparam name="T">Type which defines command options and arguments.</typeparam>
    /// <param name="name">Command name</param>
    /// <param name="action">Action to be executed when the command is activated.</param>
    /// <param name="helpText">Help text to be shown for the command in help.</param>
    /// <returns>The current instance of the parser to be able to chain commands.</returns>
    public Parser WithCommand<T>(string name, Action<T> action, string helpText = "") where T : class, new()
    {
        _command.AddCommand(name, action, helpText);
        return this;
    }

    /// <summary>
    /// Adds first level command to the parser.
    /// </summary>
    /// <typeparam name="T">Type which defines command options and arguments.</typeparam>
    /// <param name="name">Command name</param>
    /// <param name="action">Action to be executed when the command is activated.</param>
    /// <param name="helpText">Help text to be shown for the command in help.</param>
    /// <returns>The current instance of the parser to be able to chain commands.</returns>
    public Parser WithCommand<T>(string name, Func<T, int> action, string helpText = "") where T : class, new()
    {
        _command.AddCommand(name, action, helpText);
        return this;
    }

    /// <summary>
    /// Adds first level command to the parser.
    /// </summary>
    /// <remarks>This command looks like a first level command in command line.
    /// This command doesn't have any options or arguments.</remarks>
    /// <param name="name">Name of the command.</param>
    /// <param name="action">Action to be executed when the command is activated.</param>
    /// <param name="helpText">Help text to be shown for root command in help.</param>
    /// <returns>The current instance of the parser to be able to chain commands.</returns>
    public Parser WithCommand(string name, Func<CancellationToken, Task> action, string helpText = "")
    {
        _command.AddCommand(name, action, helpText);
        return this;
    }

    /// <summary>
    /// Adds first level command to the parser.
    /// </summary>
    /// <typeparam name="T">Type which defines command options and arguments.</typeparam>
    /// <param name="name">Command name</param>
    /// <param name="action">Action to be executed when the command is activated.</param>
    /// <param name="helpText">Help text to be shown for the command in help.</param>
    /// <returns>The current instance of the parser to be able to chain commands.</returns>
    public Parser WithCommand<T>(string name, Func<T, CancellationToken, Task> action, string helpText = "") where T : class, new()
    {
        _command.AddCommand(name, action, helpText);
        return this;
    }

    /// <summary>
    /// Adds first level command to the parser.
    /// </summary>
    /// <remarks>This command looks like a first level command in command line.
    /// This command doesn't have any options or arguments.</remarks>
    /// <param name="name">Name of the command.</param>
    /// <param name="action">Action to be executed when the command is activated.</param>
    /// <param name="helpText">Help text to be shown for root command in help.</param>
    /// <returns>The current instance of the parser to be able to chain commands.</returns>
    public Parser WithCommand(string name, Func<CancellationToken, Task<int>> action, string helpText = "")
    {
        _command.AddCommand(name, action, helpText);
        return this;
    }

    /// <summary>
    /// Adds first level command to the parser.
    /// </summary>
    /// <typeparam name="T">Type which defines command options and arguments.</typeparam>
    /// <param name="name">Command name</param>
    /// <param name="action">Action to be executed when the command is activated.</param>
    /// <param name="helpText">Help text to be shown for the command in help.</param>
    /// <returns>The current instance of the parser to be able to chain commands.</returns>
    public Parser WithCommand<T>(string name, Func<T, CancellationToken, Task<int>> action, string helpText = "") where T : class, new()
    {
        _command.AddCommand(name, action, helpText);
        return this;
    }

    /// <summary>
    /// Adds first level command to the parser.
    /// </summary>
    /// <remarks>Command name and help text will be aqured from the method attributes 
    /// <see cref="CommandNameAttribute"/> and <see cref="HelpTextAttribute"/>.
    /// If attributes are missing name will be formed from method name and help text will be empty.</remarks>
    /// <typeparam name="T">Type which defines command options and arguments </typeparam>
    /// <param name="action">Action to be executed when the command is activated.</param>
    /// <returns>The current instance of the parser to be able to chain commands.</returns>
    public Parser WithCommand<T>(Func<T, CancellationToken, Task<int>> action) where T : class, new()
    {
        var name = Helpers.GetCommandNameFromDelegate(action);
        var helpText = Helpers.GetHelpTextFromDelegate(action);
        _command.AddCommand(name, action, helpText);
        return this;
    }

    /// <summary>
    /// Adds first level command to the parser.
    /// </summary>
    /// <remarks>Command name and help text will be aqured from the method attributes 
    /// <see cref="CommandNameAttribute"/> and <see cref="HelpTextAttribute"/>.
    /// If attributes are missing name will be formed from method name and help text will be empty.</remarks>
    /// <typeparam name="T">Type which defines command options and arguments </typeparam>
    /// <param name="action">Action to be executed when the command is activated.</param>
    /// <returns>The current instance of the parser to be able to chain commands.</returns>
    public Parser WithCommand<T>(Func<T, CancellationToken, Task> action) where T : class, new()
    {
        var name = Helpers.GetCommandNameFromDelegate(action);
        var helpText = Helpers.GetHelpTextFromDelegate(action);
        _command.AddCommand(name, action, helpText);
        return this;
    }

    /// <summary>
    /// Adds first level command to the parser.
    /// </summary>
    /// <remarks>Command name and help text will be aqured from the method attributes 
    /// <see cref="CommandNameAttribute"/> and <see cref="HelpTextAttribute"/>.
    /// If attributes are missing name will be formed from method name and help text will be empty.</remarks>
    /// <typeparam name="T">Type which defines command options and arguments </typeparam>
    /// <param name="action">Action to be executed when the command is activated.</param>
    /// <returns>The current instance of the parser to be able to chain commands.</returns>
    public Parser WithCommand<T>(Func<T, int> action) where T : class, new()
    {
        var name = Helpers.GetCommandNameFromDelegate(action);
        var helpText = Helpers.GetHelpTextFromDelegate(action);
        _command.AddCommand(name, action, helpText);
        return this;
    }

    /// <summary>
    /// Adds first level command to the parser.
    /// </summary>
    /// <remarks>Command name and help text will be aqured from the method attributes 
    /// <see cref="CommandNameAttribute"/> and <see cref="HelpTextAttribute"/>.
    /// If attributes are missing name will be formed from method name and help text will be empty.</remarks>
    /// <typeparam name="T">Type which defines command options and arguments </typeparam>
    /// <param name="action">Action to be executed when the command is activated.</param>
    /// <returns>The current instance of the parser to be able to chain commands.</returns>
    public Parser WithCommand<T>(Action<T> action) where T : class, new()
    {
        var name = Helpers.GetCommandNameFromDelegate(action);
        var helpText = Helpers.GetHelpTextFromDelegate(action);
        _command.AddCommand(name, action, helpText);
        return this;
    }

    /// <summary>
    /// Parses command line and executes appropriate command asynchronously.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>Task to be awaited.</returns>
    public async Task RunAsync(string[] args)
    {
        using CancellationTokenSource cancellationSource = new CancellationTokenSource();

        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            if (!cancellationSource.IsCancellationRequested)
            {
                cancellationSource.Cancel();
                eventArgs.Cancel = true;
                _showCancellationNotification?.Invoke();
            }
        };

        ITokenParser handler = _command;

        try
        {
            foreach (var arg in args)
                handler = handler.ParseToken(arg);

            if (handler is ValueParser vp)
            {
                if (vp.IsFlag)
                    handler = handler.ParseToken("true");
                else if (vp.IsArray || vp.IsEnumFlag)
                    handler = vp.Parent;
                else
                    throw new CommandLineException("Option value is missing");
            }
        }
        catch(CommandLineException ex)
        {
            Console.WriteLine(ex.Message);
            Environment.ExitCode = -1;
            return;
        }

        if (handler is Command cmd)
        {
            try
            {
                Environment.ExitCode = await cmd.ExecuteAsync(cancellationSource.Token);
            }
            catch (CommandLineException ex)
            {
                Console.WriteLine(ex.Message);
                Environment.ExitCode = -2;
                return;
            }
            catch(Exception)
            {
                Environment.ExitCode = -3;
                throw;
            }
        }
    }

    /// <summary>
    /// Parses command line and executes appropriate command synchronously.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    public void Run(string[] args)
    {
        RunAsync(args).Wait();
    }

    private static void ShowCancellationNotification()
    {
        Console.WriteLine("Cancellation requested.");
        Console.WriteLine("To force stop press Ctrl-C again.");
    }

}
