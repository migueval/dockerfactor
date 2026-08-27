using System.Text.RegularExpressions;
using DockerFactor.Engine.Ingress;
using Xunit;

namespace DockerFactor.Engine.Tests.Ingress;

public class QuickTunnelAdapterTests
{
    [Fact]
    public void ProviderName_ReturnsExpectedCloudflareQuickTunnelName()
    {
        var adapter = new QuickTunnelAdapter();
        Assert.Equal("Cloudflare QuickTunnel (trycloudflare.com)", adapter.ProviderName);
    }

    [Theory]
    [InlineData("2026-08-27T14:00:00Z INF +--------------------------------------------------------------------------------------------+", null)]
    [InlineData("2026-08-27T14:00:00Z INF |  Your quick tunnel has been created! Visit it at: https://sample-random-name.trycloudflare.com  |", "https://sample-random-name.trycloudflare.com")]
    [InlineData("https://demo-test-1234.trycloudflare.com", "https://demo-test-1234.trycloudflare.com")]
    public void TryCloudflareRegex_MatchesOutputCorrectly(string logLine, string? expectedUrl)
    {
        var regex = new Regex(@"https://[a-zA-Z0-9-]+\.trycloudflare\.com", RegexOptions.IgnoreCase);
        var match = regex.Match(logLine);

        if (expectedUrl != null)
        {
            Assert.True(match.Success);
            Assert.Equal(expectedUrl, match.Value);
        }
        else
        {
            Assert.False(match.Success);
        }
    }
}
