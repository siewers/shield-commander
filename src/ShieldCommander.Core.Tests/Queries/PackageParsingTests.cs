using ShieldCommander.Core.Services.Queries;

namespace ShieldCommander.Core.Tests.Queries;

public class PackageParsingTests
{
    // Captured from NVIDIA Shield at 10.0.0.99: dumpsys package com.google.android.youtube.tv
    private const string RealDumpsys = """
        userId=10085
        codePath=/data/app/~~3KpKnNaW1nyo6LfTEgOCAQ==/com.google.android.youtube.tv-OSvgdO772P_L-dQEU1jm8Q==
        versionCode=644303330 minSdk=29 targetSdk=34
        versionName=6.44.303
        dataDir=/data/user/0/com.google.android.youtube.tv
        firstInstallTime=2026-01-11 21:48:34
        lastUpdateTime=2026-02-19 12:56:49
        installerPackageName=com.android.vending
        """;

    [Test]
    public async Task ParseDumpsys_RealOutput_AllFields()
    {
        var result = PackageParsing.ParseDumpsys("com.google.android.youtube.tv", RealDumpsys);

        await Assert.That(result.PackageName).IsEqualTo("com.google.android.youtube.tv");
        await Assert.That(result.VersionName).IsEqualTo("6.44.303");
        await Assert.That(result.VersionCode).IsEqualTo("644303330");
        await Assert.That(result.MinSdk).IsEqualTo("29");
        await Assert.That(result.TargetSdk).IsEqualTo("34");
        await Assert.That(result.InstallerPackageName).IsEqualTo("com.android.vending");
        await Assert.That(result.FirstInstallTime).IsEqualTo("2026-01-11 21:48:34");
        await Assert.That(result.LastUpdateTime).IsEqualTo("2026-02-19 12:56:49");
        await Assert.That(result.DataDir).IsEqualTo("/data/user/0/com.google.android.youtube.tv");
        await Assert.That(result.CodePath).IsEqualTo("/data/app/~~3KpKnNaW1nyo6LfTEgOCAQ==/com.google.android.youtube.tv-OSvgdO772P_L-dQEU1jm8Q==");
        await Assert.That(result.Uid).IsEqualTo("10085");
    }

    [Test]
    public async Task ParseDumpsys_EmptyInput_ReturnsAllNulls()
    {
        var result = PackageParsing.ParseDumpsys("com.example.empty", "");

        await Assert.That(result.PackageName).IsEqualTo("com.example.empty");
        await Assert.That(result.VersionName).IsNull();
        await Assert.That(result.VersionCode).IsNull();
    }

    [Test]
    public async Task ParseSize_MultipleLines_SumsBytes()
    {
        var result = PackageParsing.ParseSize("1024\n2048\n512");
        await Assert.That(result).IsEqualTo(3584);
    }

    [Test]
    public async Task ParseSize_NullInput_ReturnsNull()
    {
        var result = PackageParsing.ParseSize(null);
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task ParseSize_EmptyString_ReturnsNull()
    {
        var result = PackageParsing.ParseSize("");
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task ParseSize_AllZeros_ReturnsNull()
    {
        var result = PackageParsing.ParseSize("0\n0");
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task ParseSize_MixedValidAndInvalidLines_SumsValidOnly()
    {
        var result = PackageParsing.ParseSize("1024\nnot_a_number\n2048");
        await Assert.That(result).IsEqualTo(3072);
    }
}
