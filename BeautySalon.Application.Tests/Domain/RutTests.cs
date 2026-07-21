using BeautySalon.Domain.ValueObjects;
using Xunit;

namespace BeautySalon.Application.Tests.Domain;

public sealed class RutTests
{
    // Check digits below are hand-verified against the modulo-11 algorithm in Rut.cs,
    // not guessed: 12345678 -> 5, 11111111 -> 1, 40000000 -> K.
    [Theory]
    [InlineData("12345678-5")]
    [InlineData("12.345.678-5")]
    [InlineData("11111111-1")]
    [InlineData("40000000-K")]
    [InlineData("40000000-k")]
    public void TryCreate_WithValidFormatAndCheckDigit_Succeeds(string rawValue)
    {
        var success = Rut.TryCreate(rawValue, out var rut, out var error);

        Assert.True(success);
        Assert.NotNull(rut);
        Assert.Null(error);
    }

    [Fact]
    public void TryCreate_NormalizesDotsAndCase()
    {
        Rut.TryCreate("40.000.000-k", out var rut, out _);

        Assert.Equal("40000000-K", rut!.Value);
    }

    [Theory]
    [InlineData("12345678-9")] // correct check digit for this body is 5
    [InlineData("40000000-5")] // correct check digit for this body is K
    public void TryCreate_WithInvalidCheckDigit_Fails(string rawValue)
    {
        var success = Rut.TryCreate(rawValue, out var rut, out var error);

        Assert.False(success);
        Assert.Null(rut);
        Assert.NotNull(error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void TryCreate_WithEmptyValue_Fails(string? rawValue)
    {
        var success = Rut.TryCreate(rawValue, out var rut, out var error);

        Assert.False(success);
        Assert.Null(rut);
        Assert.NotNull(error);
    }

    [Theory]
    [InlineData("ABCDEFGH-5")]
    [InlineData("1")]
    public void TryCreate_WithNonDigitBody_Fails(string rawValue)
    {
        var success = Rut.TryCreate(rawValue, out var rut, out _);

        Assert.False(success);
        Assert.Null(rut);
    }

    [Fact]
    public void Create_WithInvalidValue_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => Rut.Create("12345678-9"));
    }

    [Fact]
    public void TwoRutsWithSameValue_AreEqual()
    {
        var first = Rut.Create("12345678-5");
        var second = Rut.Create("12.345.678-5");

        Assert.Equal(first, second);
        Assert.True(first == second);
    }
}
