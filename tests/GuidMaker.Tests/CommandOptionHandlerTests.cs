using GuidMaker.Models;
using GuidMaker.Providors;
using GuidMaker.Tests.Support;
using Rw.Core.StaticValues;

namespace GuidMaker.Tests;

public class CommandOptionHandlerTests
{
    private static readonly string NewLine = Environment.NewLine;

    private readonly SequentialGuidGenerator _guids = new();

    private static string PresetLine(string guid, string typeName = "<ObjectName>") =>
        "new " + typeName + " { Id = Guid.Parse(\"" + guid + "\"), Name = \"\" },";

    private Task<RunResult> RunAsync(GuidMakerOptionValues options) =>
        ToolRunner.RunHandlerAsync(options, _guids);

    // ---- Guid generation --------------------------------------------------------------

    [Fact]
    public async Task Handle_SingleGuid_PrintsOneLowercaseGuidAndSucceeds()
    {
        var result = await RunAsync(new GuidMakerOptionValues { GuidCount = 1 });

        Assert.Equal(ExitCodes.Success, result.ExitCode);
        Assert.Equal(SequentialGuidGenerator.Lower(1) + NewLine, result.Output);
        Assert.Empty(result.Error);
    }

    [Fact]
    public async Task Handle_MultipleGuids_PrintsOnePerLineInGenerationOrder()
    {
        var result = await RunAsync(new GuidMakerOptionValues { GuidCount = 3 });

        var expected = SequentialGuidGenerator.Lower(1) + NewLine
            + SequentialGuidGenerator.Lower(2) + NewLine
            + SequentialGuidGenerator.Lower(3) + NewLine;
        Assert.Equal(expected, result.Output);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(20)]
    public async Task Handle_Count_AsksTheGeneratorOncePerGuid(int count)
    {
        await RunAsync(new GuidMakerOptionValues { GuidCount = count });

        Assert.Equal(count, _guids.CallCount);
    }

    [Fact]
    public async Task Handle_RealGeneratorOutput_IsLowercaseHyphenatedGuid()
    {
        var result = await ToolRunner.RunHandlerAsync(
            new GuidMakerOptionValues { GuidCount = 1 },
            new Rw.Core.Generators.GuidGenerator());

        var line = result.Output.TrimEnd();
        Assert.True(Guid.TryParseExact(line, "D", out _));
        Assert.Equal(line.ToLowerInvariant(), line);
    }

    // ---- Count guard ------------------------------------------------------------------

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public async Task Handle_CountZeroOrLess_ReturnsInvalidInputWithErrorOnly(int count)
    {
        var result = await RunAsync(new GuidMakerOptionValues { GuidCount = count });

        Assert.Equal(ExitCodes.InvalidInput, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.NotEmpty(result.Error);
        Assert.Equal(0, _guids.CallCount);
    }

    // ---- Casing -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_Upper_PrintsUppercaseGuids()
    {
        var result = await RunAsync(new GuidMakerOptionValues { GuidCount = 2, Upper = true });

        var expected = SequentialGuidGenerator.Upper(1) + NewLine
            + SequentialGuidGenerator.Upper(2) + NewLine;
        Assert.Equal(expected, result.Output);
    }

    [Fact]
    public async Task Handle_Upper_LeavesBeginAndEndTextUntouched()
    {
        var result = await RunAsync(new GuidMakerOptionValues
        {
            GuidCount = 1,
            Upper = true,
            Beginning = "abc-",
            End = "-xyz",
        });

        Assert.Equal("abc-" + SequentialGuidGenerator.Upper(1) + "-xyz" + NewLine, result.Output);
    }

    // ---- Begin and end text -----------------------------------------------------------

    [Fact]
    public async Task Handle_BeginAndEnd_WrapEveryGuid()
    {
        var result = await RunAsync(new GuidMakerOptionValues { GuidCount = 2, Beginning = "[", End = "]," });

        var expected = "[" + SequentialGuidGenerator.Lower(1) + "]," + NewLine
            + "[" + SequentialGuidGenerator.Lower(2) + "]," + NewLine;
        Assert.Equal(expected, result.Output);
    }

    [Fact]
    public async Task Handle_BeginOnly_PrefixesEveryGuid()
    {
        var result = await RunAsync(new GuidMakerOptionValues { GuidCount = 2, Beginning = "id=" });

        var expected = "id=" + SequentialGuidGenerator.Lower(1) + NewLine
            + "id=" + SequentialGuidGenerator.Lower(2) + NewLine;
        Assert.Equal(expected, result.Output);
    }

    [Fact]
    public async Task Handle_EndOnly_SuffixesEveryGuid()
    {
        var result = await RunAsync(new GuidMakerOptionValues { GuidCount = 2, End = ";" });

        var expected = SequentialGuidGenerator.Lower(1) + ";" + NewLine
            + SequentialGuidGenerator.Lower(2) + ";" + NewLine;
        Assert.Equal(expected, result.Output);
    }

    [Fact]
    public async Task Handle_BeginAndEndContainingBracesAndQuotes_AreCopiedLiterally()
    {
        var result = await RunAsync(new GuidMakerOptionValues
        {
            GuidCount = 1,
            Beginning = "new Tag { Id = Guid.Parse(\"",
            End = "\"), Name = \"\" },",
        });

        Assert.Equal(PresetLine(SequentialGuidGenerator.Lower(1), "Tag") + NewLine, result.Output);
    }

    // ---- Presets ----------------------------------------------------------------------

    [Fact]
    public async Task Handle_Preset1_WrapsGuidsInTheEfCoreSeedText()
    {
        var result = await RunAsync(new GuidMakerOptionValues { GuidCount = 2, Preset = 1 });

        Assert.Equal(ExitCodes.Success, result.ExitCode);
        var expected = PresetLine(SequentialGuidGenerator.Lower(1)) + NewLine
            + PresetLine(SequentialGuidGenerator.Lower(2)) + NewLine;
        Assert.Equal(expected, result.Output);
    }

    [Fact]
    public async Task Handle_Preset1_LeavesTheObjectNamePlaceholderWhenNoTypeGiven()
    {
        var result = await RunAsync(new GuidMakerOptionValues { GuidCount = 1, Preset = 1 });

        Assert.Contains(PresetsProvidor.TypeReplaceableString, result.Output);
    }

    [Fact]
    public async Task Handle_PresetZero_MeansNoPresetSoBeginAndEndApply()
    {
        var result = await RunAsync(new GuidMakerOptionValues { GuidCount = 1, Preset = 0, Beginning = "x", End = "y" });

        Assert.Equal("x" + SequentialGuidGenerator.Lower(1) + "y" + NewLine, result.Output);
    }

    [Fact]
    public async Task Handle_Preset_OverridesBeginAndEndText()
    {
        var result = await RunAsync(new GuidMakerOptionValues
        {
            GuidCount = 1,
            Preset = 1,
            Beginning = "IGNORED-",
            End = "-IGNORED",
        });

        Assert.Equal(PresetLine(SequentialGuidGenerator.Lower(1)) + NewLine, result.Output);
        Assert.DoesNotContain("IGNORED", result.Output);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(99)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public async Task Handle_UnknownPreset_ReturnsInvalidInputWithErrorOnly(int preset)
    {
        var result = await RunAsync(new GuidMakerOptionValues { GuidCount = 3, Preset = preset });

        Assert.Equal(ExitCodes.InvalidInput, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.NotEmpty(result.Error);
    }

    // ---- Type replacement -------------------------------------------------------------

    [Fact]
    public async Task Handle_PresetWithType_ReplacesObjectNameOnEveryLineWithoutDuplicatingOutput()
    {
        var result = await RunAsync(new GuidMakerOptionValues { GuidCount = 3, Preset = 1, Type = "Tag" });

        var expected = PresetLine(SequentialGuidGenerator.Lower(1), "Tag") + NewLine
            + PresetLine(SequentialGuidGenerator.Lower(2), "Tag") + NewLine
            + PresetLine(SequentialGuidGenerator.Lower(3), "Tag") + NewLine;
        Assert.Equal(expected, result.Output);
        Assert.DoesNotContain(PresetsProvidor.TypeReplaceableString, result.Output);
    }

    [Fact]
    public async Task Handle_TypeWithoutPlaceholderInOutput_LeavesOutputUnchanged()
    {
        var result = await RunAsync(new GuidMakerOptionValues { GuidCount = 1, Beginning = "id=", Type = "Tag" });

        Assert.Equal("id=" + SequentialGuidGenerator.Lower(1) + NewLine, result.Output);
    }

    [Fact]
    public async Task Handle_EmptyType_LeavesThePlaceholderInPlace()
    {
        var result = await RunAsync(new GuidMakerOptionValues { GuidCount = 1, Preset = 1, Type = string.Empty });

        Assert.Equal(PresetLine(SequentialGuidGenerator.Lower(1)) + NewLine, result.Output);
    }

    // Documents current behaviour: the replacement runs over the finished text, so it also
    // applies to a placeholder typed into -b / -e, not only one that came from a preset.
    [Fact]
    public async Task Handle_TypeWithCustomBeginContainingPlaceholder_ReplacesEveryOccurrence()
    {
        var result = await RunAsync(new GuidMakerOptionValues
        {
            GuidCount = 1,
            Beginning = "<ObjectName>/<ObjectName>:",
            Type = "Tag",
        });

        Assert.Equal("Tag/Tag:" + SequentialGuidGenerator.Lower(1) + NewLine, result.Output);
    }

    [Fact]
    public async Task Handle_UpperPresetAndType_UppercasesOnlyTheGuids()
    {
        var result = await RunAsync(new GuidMakerOptionValues { GuidCount = 1, Preset = 1, Type = "Tag", Upper = true });

        Assert.Equal(PresetLine(SequentialGuidGenerator.Upper(1), "Tag") + NewLine, result.Output);
    }

    // ---- List presets -----------------------------------------------------------------

    [Fact]
    public async Task Handle_ListPresets_PrintsThePresetListAndSucceeds()
    {
        var result = await RunAsync(new GuidMakerOptionValues { ListPreset = true });

        Assert.Equal(ExitCodes.Success, result.ExitCode);
        Assert.Equal(PresetsProvidor.ListPresets() + NewLine, result.Output);
        Assert.Empty(result.Error);
    }

    [Fact]
    public async Task Handle_ListPresets_GeneratesNoGuids()
    {
        await RunAsync(new GuidMakerOptionValues { ListPreset = true, GuidCount = 5 });

        Assert.Equal(0, _guids.CallCount);
    }

    [Fact]
    public async Task Handle_ListPresetsWithInvalidCountAndPreset_StillListsPresets()
    {
        var result = await RunAsync(new GuidMakerOptionValues { ListPreset = true, GuidCount = 0, Preset = 99 });

        Assert.Equal(ExitCodes.Success, result.ExitCode);
        Assert.Equal(PresetsProvidor.ListPresets() + NewLine, result.Output);
    }

    // ---- Output streams ---------------------------------------------------------------

    [Fact]
    public async Task Handle_Success_WritesNothingToError()
    {
        var result = await RunAsync(new GuidMakerOptionValues { GuidCount = 2, Preset = 1, Type = "Tag" });

        Assert.Empty(result.Error);
    }
}
