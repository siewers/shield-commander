using ShieldCommander.Core.Services.Queries;

namespace ShieldCommander.Core.Tests.Queries;

public class SpanParsingExtensionsTests
{
    [Test]
    [Arguments("Mem:   1024 kB", 1024 * 1024L)]
    [Arguments("MemFree:   256 kB", 256 * 1024L)]
    public async Task KbToBytes_ParsesLabelAndValue(string input, long expected)
    {
        var result = input.KbToBytes();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task KbToBytes_SingleToken_ReturnsZero()
    {
        var result = "novalue".KbToBytes();
        await Assert.That(result).IsEqualTo(0);
    }

    [Test]
    [Arguments("1024", 1024 * 1024L)]
    [Arguments("512", 512 * 1024L)]
    public async Task ParseSizeWithUnit_PlainNumber_TreatsAsKb(string input, long expected)
    {
        var result = input.ParseSizeWithUnit();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("12G", 12L * 1024 * 1024 * 1024)]
    [Arguments("8M", 8L * 1024 * 1024)]
    [Arguments("512K", 512L * 1024)]
    [Arguments("1T", 1L * 1024 * 1024 * 1024 * 1024)]
    public async Task ParseSizeWithUnit_WithSuffix_ConvertsCorrectly(string input, long expected)
    {
        var result = input.ParseSizeWithUnit();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("8.5M", (long)(8.5 * 1024 * 1024))]
    public async Task ParseSizeWithUnit_FractionalValue_ConvertsCorrectly(string input, long expected)
    {
        var result = input.ParseSizeWithUnit();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("")]
    [Arguments("   ")]
    public async Task ParseSizeWithUnit_EmptyOrWhitespace_ReturnsZero(string input)
    {
        var result = input.ParseSizeWithUnit();
        await Assert.That(result).IsEqualTo(0);
    }

    [Test]
    public async Task ParseSizeWithUnit_UnknownSuffix_ReturnsZero()
    {
        var result = "100X".ParseSizeWithUnit();
        await Assert.That(result).IsEqualTo(0);
    }

    [Test]
    public async Task ExtractDumpsysFields_BothFields_ExtractsCorrectly()
    {
        ReadOnlySpan<char> input = "mName=CPU, mValue=42}";
        input.ExtractDumpsysFields(out var name, out var value);
        var nameStr = name.ToString();
        var valueStr = value.ToString();
        await Assert.That(nameStr).IsEqualTo("CPU");
        await Assert.That(valueStr).IsEqualTo("42");
    }

    [Test]
    public async Task ExtractDumpsysFields_NoName_ReturnsUnknown()
    {
        ReadOnlySpan<char> input = "mValue=99}";
        input.ExtractDumpsysFields(out var name, out var value);
        var nameStr = name.ToString();
        var valueStr = value.ToString();
        await Assert.That(nameStr).IsEqualTo("Unknown");
        await Assert.That(valueStr).IsEqualTo("99");
    }

    [Test]
    public async Task ExtractDumpsysFields_ValueTerminatedByComma()
    {
        ReadOnlySpan<char> input = "mValue=37, mName=Battery";
        input.ExtractDumpsysFields(out var name, out var value);
        var valueStr = value.ToString();
        var nameStr = name.ToString();
        await Assert.That(valueStr).IsEqualTo("37");
        await Assert.That(nameStr).IsEqualTo("Battery");
    }

    [Test]
    [Arguments("12345", true)]
    [Arguments("0", true)]
    [Arguments("123a5", false)]
    [Arguments("", true)] // vacuous truth: empty span has no non-digit characters
    public async Task IsAllDigits_ValidatesCorrectly(string input, bool expected)
    {
        var result = input.IsAllDigits();
        await Assert.That(result).IsEqualTo(expected);
    }
}
