using FluentAssertions;
using SlipeServer.Server.Extensions;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Extensions;
public class StringExtensionsTests
{
    [InlineData("#fF0000Nick", "Nick", true)]
    [InlineData("##ff1000ff3000Nick", "Nick", true)]
    [InlineData("#ff000Nick", "#ff000Nick", false)]
    [Theory]
    public void TestStringExtensions(string input, string expectedOutput, bool expectedInputContainsColorCode)
    {
        input.ContainsColorCode().Should().Be(expectedInputContainsColorCode);
        input.StripColorCode().Should().Be(expectedOutput);
    }

    [InlineData("#fF0000Nick", true)]
    [InlineData("", false)]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaa", true)]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaa1", false)]
    [InlineData("ą", false)]
    [Theory]
    public void TestValidNickNameExtensions(string input, bool isValid)
    {
        input.IsValidNickName().Should().Be(isValid);
    }

    [InlineData("a", false)]
    [InlineData("7815696ecbf1c96e6894b779456d330e", false)] // md5(asd), Lowercase is invalid
    [InlineData("7815696ECBF1C96E6894B779456D330E", true)]
    [InlineData("7815696ECBF1C96E6894B779456D330EE", false)]
    [InlineData("7815696ECBF1C96E6894B779456D330", false)]
    [Theory]
    public void TestIsValidSerialExtension(string input, bool isValid)
    {
        input.IsValidSerial().Should().Be(isValid);
    }
}
