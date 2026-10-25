using Rw.Core.Generators;

namespace GuidMaker.Tests;

public class GuidGeneratorTests
{
    [Fact]
    public void NewGuid_Always_ReturnsANonEmptyGuid()
    {
        var generator = new GuidGenerator();

        Assert.NotEqual(Guid.Empty, generator.NewGuid());
    }

    [Fact]
    public void NewGuid_CalledRepeatedly_ReturnsDistinctGuids()
    {
        var generator = new GuidGenerator();

        var guids = Enumerable.Range(0, 100).Select(_ => generator.NewGuid()).ToList();

        Assert.Equal(100, guids.Distinct().Count());
    }
}
