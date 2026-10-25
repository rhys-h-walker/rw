using System.CommandLine;

namespace HelloWorld.Tests;

public class HelloWorldToolTests
{
    [Fact]
    public void CreateCommand_Always_IsNamedHelloWorld()
    {
        var command = new HelloWorldTool().CreateCommand();

        Assert.Equal("hello-world", command.Name);
    }

    [Fact]
    public async Task Invoke_NoArguments_GreetsWorld()
    {
        var result = await InvokeAsync();

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("Hello, world!" + Environment.NewLine, result.Output);
    }

    [Fact]
    public async Task Invoke_LongNameOption_GreetsThatName()
    {
        var result = await InvokeAsync("--name", "Rhys");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("Hello, Rhys!" + Environment.NewLine, result.Output);
    }

    [Fact]
    public async Task Invoke_ShortNameOption_GreetsThatName()
    {
        var result = await InvokeAsync("-n", "Rhys");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("Hello, Rhys!" + Environment.NewLine, result.Output);
    }

    [Fact]
    public async Task Invoke_UnknownOption_FailsWithoutGreeting()
    {
        var result = await InvokeAsync("--nope");

        Assert.NotEqual(0, result.ExitCode);
        Assert.DoesNotContain("Hello", result.Output);
        Assert.NotEmpty(result.Error);
    }

    private static async Task<(int ExitCode, string Output, string Error)> InvokeAsync(params string[] args)
    {
        using var output = new StringWriter();
        using var error = new StringWriter();
        var configuration = new InvocationConfiguration { Output = output, Error = error };

        var exitCode = await new HelloWorldTool().CreateCommand().Parse(args).InvokeAsync(configuration);

        return (exitCode, output.ToString(), error.ToString());
    }
}
