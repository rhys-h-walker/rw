using System.CommandLine;
using GuidMaker.Providors;

namespace GuidMaker.Tests;

public class OptionsProvidorTests
{
    private static ParseResult ParseWithAllOptions(params string[] args)
    {
        var command = new Command("options-under-test");
        foreach (var option in OptionsProvidor.GetOptions())
        {
            command.Options.Add(option);
        }

        return command.Parse(args);
    }

    [Fact]
    public void GetOptions_Always_ReturnsSevenOptions()
    {
        Assert.Equal(7, OptionsProvidor.GetOptions().Count);
    }

    [Fact]
    public void GetOptions_Always_ReturnsEveryOptionDefinedOnTheProvider()
    {
        var options = OptionsProvidor.GetOptions();

        Assert.Contains(OptionsProvidor.guidCountOption, options);
        Assert.Contains(OptionsProvidor.beginingTextOption, options);
        Assert.Contains(OptionsProvidor.endingTextOption, options);
        Assert.Contains(OptionsProvidor.presetOption, options);
        Assert.Contains(OptionsProvidor.typeOption, options);
        Assert.Contains(OptionsProvidor.upperOption, options);
        Assert.Contains(OptionsProvidor.listPresetOptions, options);
    }

    [Fact]
    public void Defaults_NoArguments_AreOneGuidWithNoTextNoPresetAndLowercase()
    {
        var result = ParseWithAllOptions();

        Assert.Equal(1, result.GetValue(OptionsProvidor.guidCountOption));
        Assert.Equal(string.Empty, result.GetValue(OptionsProvidor.beginingTextOption));
        Assert.Equal(string.Empty, result.GetValue(OptionsProvidor.endingTextOption));
        Assert.Equal(0, result.GetValue(OptionsProvidor.presetOption));
        Assert.Null(result.GetValue(OptionsProvidor.typeOption));
        Assert.False(result.GetValue(OptionsProvidor.upperOption));
        Assert.False(result.GetValue(OptionsProvidor.listPresetOptions));
    }

    [Theory]
    [InlineData("-n")]
    [InlineData("--count")]
    public void Count_ShortAndLongName_AreParsed(string name)
    {
        var result = ParseWithAllOptions(name, "7");

        Assert.Empty(result.Errors);
        Assert.Equal(7, result.GetValue(OptionsProvidor.guidCountOption));
    }

    [Theory]
    [InlineData("-b")]
    [InlineData("--begin")]
    public void Begin_ShortAndLongName_AreParsed(string name)
    {
        var result = ParseWithAllOptions(name, "start");

        Assert.Empty(result.Errors);
        Assert.Equal("start", result.GetValue(OptionsProvidor.beginingTextOption));
    }

    [Theory]
    [InlineData("-e")]
    [InlineData("--end")]
    public void End_ShortAndLongName_AreParsed(string name)
    {
        var result = ParseWithAllOptions(name, "finish");

        Assert.Empty(result.Errors);
        Assert.Equal("finish", result.GetValue(OptionsProvidor.endingTextOption));
    }

    [Theory]
    [InlineData("-p")]
    [InlineData("--preset")]
    public void Preset_ShortAndLongName_AreParsed(string name)
    {
        var result = ParseWithAllOptions(name, "1");

        Assert.Empty(result.Errors);
        Assert.Equal(1, result.GetValue(OptionsProvidor.presetOption));
    }

    [Theory]
    [InlineData("-t")]
    [InlineData("--type")]
    public void Type_ShortAndLongName_AreParsed(string name)
    {
        var result = ParseWithAllOptions(name, "Tag");

        Assert.Empty(result.Errors);
        Assert.Equal("Tag", result.GetValue(OptionsProvidor.typeOption));
    }

    [Fact]
    public void Upper_Flag_IsParsedWithoutAValue()
    {
        var result = ParseWithAllOptions("--upper");

        Assert.Empty(result.Errors);
        Assert.True(result.GetValue(OptionsProvidor.upperOption));
    }

    [Fact]
    public void ListPresets_Flag_IsParsedWithoutAValue()
    {
        var result = ParseWithAllOptions("--list-presets");

        Assert.Empty(result.Errors);
        Assert.True(result.GetValue(OptionsProvidor.listPresetOptions));
    }

    [Theory]
    [InlineData("-n", "abc")]
    [InlineData("--count", "1.5")]
    [InlineData("-p", "abc")]
    [InlineData("--preset", "one")]
    public void NumericOptions_NonNumericValue_ProduceParseErrors(string name, string value)
    {
        var result = ParseWithAllOptions(name, value);

        Assert.NotEmpty(result.Errors);
    }
}
