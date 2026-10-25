using System.CommandLine;
using Rw.Core.Interfaces;

namespace HelloWorld;

public sealed class HelloWorldTool : IRwTool
{
    public Command CreateCommand()
    {
        var nameOption = new Option<string>("--name", "-n")
        {
            Description = "Who to greet",
            DefaultValueFactory = _ => "world",
        };

        var command = new Command("hello-world", "Prints a greeting; used to verify rw is wired up");
        command.Options.Add(nameOption);

        command.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(nameOption);
            parseResult.InvocationConfiguration.Output.WriteLine($"Hello, {name}!");
            return 0;
        });

        return command;
    }
}
