using ConsoleBot.Bots.Types;
using Xunit;

namespace ConsoleBot.Tests.Bots.Types;

public class AccountConfigTests
{
    [Fact]
    public void Enabled_DefaultsToTrue()
    {
        var config = new AccountConfig();

        Assert.True(config.Enabled);
    }
}
