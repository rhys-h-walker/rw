using System.Globalization;
using Rw.Core.Interfaces;

namespace GuidMaker.Tests.Support;

/// <summary>
/// Hands out predictable guids (…0001, …0002, …) and counts calls. The prefix contains hex
/// letters so upper and lower case output can be told apart.
/// </summary>
internal sealed class SequentialGuidGenerator : IGuidGenerator
{
    private const string Prefix = "abcdef00-0000-0000-0000-";

    public int CallCount { get; private set; }

    public Guid NewGuid()
    {
        CallCount++;
        return new Guid(Lower(CallCount));
    }

    public static string Lower(int sequence) =>
        Prefix + sequence.ToString("D12", CultureInfo.InvariantCulture);

    public static string Upper(int sequence) => Lower(sequence).ToUpperInvariant();
}
