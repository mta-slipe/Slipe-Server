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
}
