using GuidMaker.Providors;
using GuidMaker.Tests.Support;
using Rw.Core.Generators;
using Rw.Core.StaticValues;

namespace GuidMaker.Tests;

public class GuidMakerToolTests
{
    private static readonly string NewLine = Environment.NewLine;

    private readonly SequentialGuidGenerator _guids = new();

    private static string PresetLine(string guid, string typeName = "<ObjectName>") =>
        "new " + typeName + " { Id = Guid.Parse(\"" + guid + "\"), Name = \"\" },";

    private Task<RunResult> RunAsync(params string[] args) => ToolRunner.RunToolAsync(_guids, args);

    // ---- Command definition -----------------------------------------------------------

    [Fact]
    public void CreateCommand_Always_IsNamedGuidMakerWithADescription()
    {
        var command = new GuidMakerTool(_guids).CreateCommand();

        Assert.Equal("guid-maker", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    [Fact]
    public async Task Help_Always_ListsEveryOptionAndExitsSuccessfully()
    {
        var result = await ToolRunner.RunUnderRootAsync(_guids, "--help");

        Assert.Equal(ExitCodes.Success, result.ExitCode);
        foreach (var name in new[] { "--count", "--begin", "--end", "--preset", "--type", "--upper", "--list-presets" })
        {
            Assert.Contains(name, result.Output);
        }
    }

    // ---- End-to-end behaviour through the parser --------------------------------------

    [Fact]
    public async Task Invoke_NoArguments_PrintsOneLowercaseGuid()
    {
        var result = await RunAsync();

        Assert.Equal(ExitCodes.Success, result.ExitCode);
        Assert.Equal(SequentialGuidGenerator.Lower(1) + NewLine, result.Output);
    }

    [Theory]
    [InlineData("-n")]
    [InlineData("--count")]
    public async Task Invoke_Count_PrintsThatManyGuids(string flag)
    {
        var result = await RunAsync(flag, "3");

        var expected = SequentialGuidGenerator.Lower(1) + NewLine
            + SequentialGuidGenerator.Lower(2) + NewLine
            + SequentialGuidGenerator.Lower(3) + NewLine;
        Assert.Equal(expected, result.Output);
        Assert.Equal(3, _guids.CallCount);
    }

    [Theory]
    [InlineData("-b", "-e")]
    [InlineData("--begin", "--end")]
    public async Task Invoke_BeginAndEnd_WrapEachGuid(string beginFlag, string endFlag)
    {
        var result = await RunAsync("-n", "2", beginFlag, "<", endFlag, ">");

        var expected = "<" + SequentialGuidGenerator.Lower(1) + ">" + NewLine
            + "<" + SequentialGuidGenerator.Lower(2) + ">" + NewLine;
        Assert.Equal(expected, result.Output);
    }

    [Fact]
    public async Task Invoke_TheEfCoreSeedExampleFromTheBrief_ProducesTagInitialisers()
    {
        var result = await RunAsync(
            "-n", "2",
            "-b", "new Tag { Id = Guid.Parse(\"",
            "-e", "\"), Name = \"\" },");

        var expected = PresetLine(SequentialGuidGenerator.Lower(1), "Tag") + NewLine
            + PresetLine(SequentialGuidGenerator.Lower(2), "Tag") + NewLine;
        Assert.Equal(expected, result.Output);
    }

    [Theory]
    [InlineData("-p")]
    [InlineData("--preset")]
    public async Task Invoke_Preset_AppliesThePresetText(string flag)
    {
        var result = await RunAsync("-n", "2", flag, "1");

        var expected = PresetLine(SequentialGuidGenerator.Lower(1)) + NewLine
            + PresetLine(SequentialGuidGenerator.Lower(2)) + NewLine;
        Assert.Equal(expected, result.Output);
    }

    [Theory]
    [InlineData("-t")]
    [InlineData("--type")]
    public async Task Invoke_PresetWithType_ReplacesObjectName(string flag)
    {
        var result = await RunAsync("-n", "2", "-p", "1", flag, "Tag");

        var expected = PresetLine(SequentialGuidGenerator.Lower(1), "Tag") + NewLine
            + PresetLine(SequentialGuidGenerator.Lower(2), "Tag") + NewLine;
        Assert.Equal(expected, result.Output);
        Assert.DoesNotContain(PresetsProvidor.TypeReplaceableString, result.Output);
    }

    [Fact]
    public async Task Invoke_PresetAlongsideBeginAndEnd_PresetWins()
    {
        var result = await RunAsync("-p", "1", "-b", "IGNORED-", "-e", "-IGNORED");

        Assert.Equal(PresetLine(SequentialGuidGenerator.Lower(1)) + NewLine, result.Output);
    }

    [Fact]
    public async Task Invoke_Upper_PrintsUppercaseGuids()
    {
        var result = await RunAsync("--upper", "-n", "2");

        var expected = SequentialGuidGenerator.Upper(1) + NewLine + SequentialGuidGenerator.Upper(2) + NewLine;
        Assert.Equal(expected, result.Output);
    }

    [Fact]
    public async Task Invoke_ListPresets_PrintsPresetsWithoutGeneratingGuids()
    {
        var result = await RunAsync("--list-presets");

        Assert.Equal(ExitCodes.Success, result.ExitCode);
        Assert.Equal(PresetsProvidor.ListPresets() + NewLine, result.Output);
        Assert.Equal(0, _guids.CallCount);
    }

    [Fact]
    public async Task Invoke_OptionsInAnyOrder_GiveTheSameResult()
    {
        var first = await RunAsync("-n", "2", "-p", "1", "-t", "Tag", "--upper");
        var second = await new GuidMakerToolTests().RunAsync("--upper", "-t", "Tag", "-p", "1", "-n", "2");

        Assert.Equal(first.Output, second.Output);
    }

    // ---- Failures ---------------------------------------------------------------------

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    public async Task Invoke_CountZeroOrLess_ExitsWithInvalidInputAndAnError(string count)
    {
        var result = await RunAsync("-n", count);

        Assert.Equal(ExitCodes.InvalidInput, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.NotEmpty(result.Error);
        Assert.Equal(0, _guids.CallCount);
    }

    [Fact]
    public async Task Invoke_UnknownPreset_ExitsWithInvalidInputAndAnError()
    {
        var result = await RunAsync("-p", "99");

        Assert.Equal(ExitCodes.InvalidInput, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.NotEmpty(result.Error);
    }

    [Theory]
    [InlineData("-n", "abc")]
    [InlineData("-p", "abc")]
    [InlineData("--count", "1.5")]
    public async Task Invoke_NonNumericValue_FailsWithAParseErrorAndGeneratesNothing(string flag, string value)
    {
        var result = await RunAsync(flag, value);

        Assert.NotEqual(ExitCodes.Success, result.ExitCode);
        Assert.NotEmpty(result.Error);
        Assert.Equal(0, _guids.CallCount);
    }

    [Fact]
    public async Task Invoke_CountFlagWithoutValue_Fails()
    {
        var result = await RunAsync("-n");

        Assert.NotEqual(ExitCodes.Success, result.ExitCode);
        Assert.NotEmpty(result.Error);
    }

    [Fact]
    public async Task Invoke_UnknownOption_FailsWithoutGeneratingGuids()
    {
        var result = await RunAsync("--nope");

        Assert.NotEqual(ExitCodes.Success, result.ExitCode);
        Assert.NotEmpty(result.Error);
        Assert.Equal(0, _guids.CallCount);
    }

    // ---- State and real generator -----------------------------------------------------

    [Fact]
    public async Task Invoke_RunTwiceWithDifferentArguments_SecondRunIsNotAffectedByTheFirst()
    {
        await RunAsync("-n", "3", "-p", "1", "-t", "Tag", "--upper");
        var second = await ToolRunner.RunToolAsync(new SequentialGuidGenerator());

        Assert.Equal(SequentialGuidGenerator.Lower(1) + NewLine, second.Output);
    }

    [Fact]
    public async Task Invoke_WithTheRealGenerator_PrintsDistinctValidGuids()
    {
        var result = await ToolRunner.RunToolAsync(new GuidGenerator(), "-n", "5");

        var lines = result.Output.Split(NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(5, lines.Length);
        Assert.All(lines, line => Assert.True(Guid.TryParseExact(line, "D", out _)));
        Assert.Equal(5, lines.Distinct().Count());
    }
}
