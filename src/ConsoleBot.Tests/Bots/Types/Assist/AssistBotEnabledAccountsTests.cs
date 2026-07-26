using ConsoleBot.Bots.Types;
using ConsoleBot.Bots.Types.Assist;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ConsoleBot.Tests.Bots.Types.Assist;

public class AssistBotEnabledAccountsTests
{
    private static readonly string[] ExpectedCharacters = ["A", "C"];
    private static readonly string[] OriginalCharacters = ["A", "B", "C"];

    [Fact]
    public void GetEnabledAccounts_ReturnsOnlyEnabledAccounts_PreservingOrder()
    {
        var accounts = new List<AccountConfig>
        {
            new AccountConfig { Username = "u1", Password = "pw", Character = "A", Enabled = true },
            new AccountConfig { Username = "u2", Password = "pw", Character = "B", Enabled = false },
            new AccountConfig { Username = "u3", Password = "pw", Character = "C", Enabled = true }
        };

        var result = AssistBot.GetEnabledAccounts(accounts);

        Assert.Equal(ExpectedCharacters, result.Select(a => a.Character).ToArray());
        Assert.Equal(3, accounts.Count);
        Assert.Equal(OriginalCharacters, accounts.Select(a => a.Character).ToArray());
    }
}
