using GuidMaker.Providors;

namespace GuidMaker.Tests;

public class PresetsProvidorTests
{
    [Fact]
    public void TypeReplaceableString_Always_IsObjectNameInAngleBrackets()
    {
        Assert.Equal("<ObjectName>", PresetsProvidor.TypeReplaceableString);
    }

    [Fact]
    public void GetPreset_One_ReturnsTheEfCoreSeedText()
    {
        var preset = PresetsProvidor.GetPreset(1);

        Assert.NotNull(preset);
        Assert.Equal("new <ObjectName> { Id = Guid.Parse(\"", preset.Value.Beginning);
        Assert.Equal("\"), Name = \"\" },", preset.Value.End);
    }

    [Fact]
    public void GetPreset_One_ContainsTheTypePlaceholderInItsBeginning()
    {
        var preset = PresetsProvidor.GetPreset(1);

        Assert.NotNull(preset);
        Assert.Contains(PresetsProvidor.TypeReplaceableString, preset.Value.Beginning);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void GetPreset_NumberWithNoPreset_ReturnsNull(int number)
    {
        Assert.Null(PresetsProvidor.GetPreset(number));
    }

    [Fact]
    public void ListPresets_Always_StartsWithPresetOne()
    {
        Assert.StartsWith("Preset 1", PresetsProvidor.ListPresets(), StringComparison.Ordinal);
    }

    [Fact]
    public void ListPresets_Always_ShowsBeginAndEndTextOfPresetOne()
    {
        var preset = PresetsProvidor.GetPreset(1);
        var listing = PresetsProvidor.ListPresets();

        Assert.NotNull(preset);
        Assert.Contains(preset.Value.Beginning, listing);
        Assert.Contains(preset.Value.End, listing);
    }

    [Fact]
    public void ListPresets_Always_HasOneLinePerSelectablePreset()
    {
        var selectable = 0;
        while (PresetsProvidor.GetPreset(selectable + 1) is not null)
        {
            selectable++;
        }

        var lines = PresetsProvidor.ListPresets().Split(Environment.NewLine);

        Assert.Equal(selectable, lines.Length);
    }
}
