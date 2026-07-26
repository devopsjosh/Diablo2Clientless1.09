using ConsoleBot.Bots.Types;
using ConsoleBot.Bots.Types.Assist;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace ConsoleBot.Tests.Bots.Types.Assist;

public class AssistConfigurationLeecherValidationTests
{
    [Fact]
    public void Validate_Throws_WhenSafeCommandIsWhitespace()
    {
        var config = BuildValidConfiguration();
        config.SafeCommand = "   ";

        Assert.Throws<ValidationException>(() => config.Validate());
    }

    [Fact]
    public void Validate_Throws_WhenDangerDistanceIsNotPositive()
    {
        var config = BuildValidConfiguration();
        config.DangerDistance = 0;

        Assert.Throws<ValidationException>(() => config.Validate());
    }

    [Fact]
    public void Validate_Throws_WhenDangerMessageIsWhitespace()
    {
        var config = BuildValidConfiguration();
        config.DangerMessage = " ";

        Assert.Throws<ValidationException>(() => config.Validate());
    }

    [Fact]
    public void Validate_Throws_WhenEntryLocationDriftThresholdNotPositive()
    {
        var config = BuildValidConfiguration();
        config.EntryLocationDriftThreshold = 0;

        Assert.Throws<ValidationException>(() => config.Validate());
    }

    [Fact]
    public void Validate_Throws_WhenEnteringLeaderZoneTimeoutSecondsNotPositive()
    {
        var config = BuildValidConfiguration();
        config.EnteringLeaderZoneTimeoutSeconds = 0;

        Assert.Throws<ValidationException>(() => config.Validate());
    }

    [Fact]
    public void Validate_Throws_WhenDangerAnnouncementCooldownSecondsNotPositive()
    {
        var config = BuildValidConfiguration();
        config.DangerAnnouncementCooldownSeconds = 0;

        Assert.Throws<ValidationException>(() => config.Validate());
    }

    [Fact]
    public void Validate_DoesNotThrow_WhenHostAccountIsMarkedAsLeecher()
    {
        var config = BuildValidConfiguration();
        var host = config.Accounts.Find(a => a.Character.Equals(config.HostCharacterName, StringComparison.OrdinalIgnoreCase));
        host.IsLeecher = true;

        var exception = Record.Exception(() => config.Validate());

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_Throws_WhenLeadAccountIsMarkedAsLeecher()
    {
        var config = BuildValidConfiguration();
        var lead = config.Accounts.Find(a => a.Character.Equals(config.LeadCharacterName, StringComparison.OrdinalIgnoreCase));
        lead.IsLeecher = true;

        Assert.Throws<ValidationException>(() => config.Validate());
    }

    [Fact]
    public void Validate_Throws_WhenHostAndLeadShareSameCharacterNameAndThatAccountIsMarkedAsLeecher()
    {
        var config = BuildValidConfiguration();
        config.HostCharacterName = "LeadChar";

        var sharedHostLead = config.Accounts.Find(a =>
            a.Character.Equals(config.HostCharacterName, StringComparison.OrdinalIgnoreCase) &&
            a.Character.Equals(config.LeadCharacterName, StringComparison.OrdinalIgnoreCase));
        sharedHostLead.IsLeecher = true;

        Assert.Throws<ValidationException>(() => config.Validate());
    }

    [Fact]
    public void Validate_Throws_WhenAllAccountsDisabled()
    {
        var config = BuildValidConfiguration();
        config.Accounts.ForEach(a => a.Enabled = false);

        Assert.Throws<ValidationException>(() => config.Validate());
    }

    [Fact]
    public void Validate_Throws_WhenHostCharacterAccountDisabled()
    {
        var config = BuildValidConfiguration();
        var host = config.Accounts.Find(a => a.Character.Equals(config.HostCharacterName, StringComparison.OrdinalIgnoreCase));
        host.Enabled = false;

        Assert.Throws<ValidationException>(() => config.Validate());
    }

    [Fact]
    public void Validate_Throws_WhenLeadCharacterAccountDisabled()
    {
        var config = BuildValidConfiguration();
        var lead = config.Accounts.Find(a => a.Character.Equals(config.LeadCharacterName, StringComparison.OrdinalIgnoreCase));
        lead.Enabled = false;

        Assert.Throws<ValidationException>(() => config.Validate());
    }

    [Fact]
    public void Validate_DoesNotThrow_WhenNonHostNonLeadAccountDisabled()
    {
        var config = BuildValidConfiguration();
        var nonHostNonLead = config.Accounts.Find(a =>
            !a.Character.Equals(config.HostCharacterName, StringComparison.OrdinalIgnoreCase) &&
            !a.Character.Equals(config.LeadCharacterName, StringComparison.OrdinalIgnoreCase));
        nonHostNonLead.Enabled = false;

        var exception = Record.Exception(() => config.Validate());

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_DoesNotThrow_WhenDisabledFollowerSharesLeadUsernameAndUsesSameAsLeadAccount()
    {
        var config = BuildValidConfiguration();
        var lead = config.Accounts.Find(a => a.Character.Equals(config.LeadCharacterName, StringComparison.OrdinalIgnoreCase));
        var follower = config.Accounts.Find(a =>
            !a.Character.Equals(config.HostCharacterName, StringComparison.OrdinalIgnoreCase) &&
            !a.Character.Equals(config.LeadCharacterName, StringComparison.OrdinalIgnoreCase));

        follower.Username = lead.Username;
        follower.SameAsLeadAccount = true;
        follower.Enabled = false;

        var exception = Record.Exception(() => config.Validate());

        Assert.Null(exception);
    }

    private static AssistConfiguration BuildValidConfiguration()
    {
        return new AssistConfiguration
        {
            HostCharacterName = "HostChar",
            LeadCharacterName = "LeadChar",
            SafeCommand = "safe",
            DangerDistance = 5,
            DangerMessage = "Danger Will Robinson! Danger!",
            EntryLocationDriftThreshold = 5,
            EnteringLeaderZoneTimeoutSeconds = 30,
            DangerAnnouncementCooldownSeconds = 2,
            Accounts =
            [
                new AccountConfig
                {
                    Username = "host-user",
                    Password = "pw",
                    Character = "HostChar",
                    SameAsLeadAccount = false,
                    IsLeecher = false
                },
                new AccountConfig
                {
                    Username = "lead-user",
                    Password = "pw",
                    Character = "LeadChar",
                    SameAsLeadAccount = true,
                    IsLeecher = false
                },
                new AccountConfig
                {
                    Username = "leecher-user",
                    Password = "pw",
                    Character = "LeecherOne",
                    SameAsLeadAccount = false,
                    IsLeecher = true
                }
            ]
        };
    }
}
