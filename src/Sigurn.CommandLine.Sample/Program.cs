// See https://aka.ms/new-console-template for more information

using Sigurn.CommandLine;
using Sigurn.CommandLine.Sample;

var parser = Parser.New()
    .WithCommand<GreetingOptions>(GreetingOptions.Hello)
    .WithCommand<GreetingOptions>(GreetingOptions.GoodbyeAsync)
    .WithCommand("test", 
        new Command(() => Console.WriteLine("Please define what feture to test"), "Test different fetures of the parser")
        .WithCommand<CancelOptions>(CancelOptions.Cancel)
        .WithCommand<CancelOptions>(CancelOptions.NoCancel)
        .WithCommand<ArgsOptions>(ArgsOptions.Arguments)
        .WithCommand<EnumOptions>(EnumOptions.TestEnum));

parser.Run(args);
