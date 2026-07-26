using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ConsoleBot.Bots.Types.Assist;

public class AssistConfiguration
{
    public List<AccountConfig> Accounts { get; set; }

    public string HostCharacterName { get; set; }

    public string LeadCharacterName { get; set; }

    public string SafeCommand { get; set; } = "safe";

    public int DangerDistance { get; set; } = 5;

    public string DangerMessage { get; set; } = "Danger Will Robinson! Danger!";

    public int EntryLocationDriftThreshold { get; set; } = 5;

    public int EnteringLeaderZoneTimeoutSeconds { get; set; } = 30;

    public int DangerAnnouncementCooldownSeconds { get; set; } = 2;

    public int LoginStaggerMinSeconds { get; set; } = 5;
    public int LoginStaggerMaxSeconds { get; set; } = 20;
    public int JoinStaggerMinSeconds { get; set; } = 5;
    public int JoinStaggerMaxSeconds { get; set; } = 25;

    public void Validate()
    {
        if (Accounts == null || Accounts.Count == 0)
        {
            throw new ValidationException($"{nameof(Accounts)} is required on assist configuration");
        }

        Accounts.ForEach(a => a.Validate());

        if (string.IsNullOrEmpty(HostCharacterName))
        {
            throw new ValidationException($"{nameof(HostCharacterName)} is required on assist configuration");
        }

        if (string.IsNullOrEmpty(LeadCharacterName))
        {
            throw new ValidationException($"{nameof(LeadCharacterName)} is required on assist configuration");
        }

        if (string.IsNullOrWhiteSpace(SafeCommand))
        {
            throw new ValidationException($"{nameof(SafeCommand)} is required on assist configuration");
        }

        if (DangerDistance <= 0)
        {
            throw new ValidationException($"{nameof(DangerDistance)} should be positive on assist configuration");
        }

        if (string.IsNullOrWhiteSpace(DangerMessage))
        {
            throw new ValidationException($"{nameof(DangerMessage)} is required on assist configuration");
        }

        if (EntryLocationDriftThreshold <= 0)
        {
            throw new ValidationException($"{nameof(EntryLocationDriftThreshold)} should be positive on assist configuration");
        }

        if (EnteringLeaderZoneTimeoutSeconds <= 0)
        {
            throw new ValidationException($"{nameof(EnteringLeaderZoneTimeoutSeconds)} should be positive on assist configuration");
        }

        if (DangerAnnouncementCooldownSeconds <= 0)
        {
            throw new ValidationException($"{nameof(DangerAnnouncementCooldownSeconds)} should be positive on assist configuration");
        }

        if (!Accounts.Any(a => a.Enabled))
        {
            throw new ValidationException($"At least one enabled {nameof(Accounts)} entry is required on assist configuration");
        }

        if (Accounts.Any(a => !a.Enabled && a.Character.Equals(HostCharacterName, System.StringComparison.OrdinalIgnoreCase)))
        {
            throw new ValidationException($"{nameof(HostCharacterName)} cannot be disabled");
        }

        if (Accounts.Any(a => !a.Enabled && a.Character.Equals(LeadCharacterName, System.StringComparison.OrdinalIgnoreCase)))
        {
            throw new ValidationException($"{nameof(LeadCharacterName)} cannot be disabled");
        }

        if (Accounts.Any(a => a.IsLeecher && a.Character.Equals(LeadCharacterName, System.StringComparison.OrdinalIgnoreCase)))
        {
            throw new ValidationException($"{nameof(LeadCharacterName)} cannot be marked as leecher");
        }

        ValidateStaggerRange(nameof(LoginStaggerMinSeconds), LoginStaggerMinSeconds, nameof(LoginStaggerMaxSeconds), LoginStaggerMaxSeconds);
        ValidateStaggerRange(nameof(JoinStaggerMinSeconds), JoinStaggerMinSeconds, nameof(JoinStaggerMaxSeconds), JoinStaggerMaxSeconds);

        var sameUsernameGroups = Accounts
            .GroupBy(a => a.Username, System.StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .ToList();

        foreach (var group in sameUsernameGroups)
        {
            if (group.Any(a => a.SameAsLeadAccount) && group.Any(a => !a.SameAsLeadAccount))
            {
                throw new ValidationException($"All assist accounts with username '{group.Key}' must use the same {nameof(AccountConfig.SameAsLeadAccount)} value");
            }

            var leadAccountInGroup = group.FirstOrDefault(a => a.Character.Equals(LeadCharacterName, System.StringComparison.OrdinalIgnoreCase));
            if (leadAccountInGroup != null && !leadAccountInGroup.SameAsLeadAccount && group.Any(a => a.SameAsLeadAccount))
            {
                throw new ValidationException($"Lead character {LeadCharacterName} must set {nameof(AccountConfig.SameAsLeadAccount)} to true when same-username followers do");
            }
        }
    }

    private static void ValidateStaggerRange(string minName, int minValue, string maxName, int maxValue)
    {
        if (minValue < 0 || maxValue < 0)
        {
            throw new ValidationException($"{minName} and {maxName} cannot be negative");
        }

        if (minValue > maxValue)
        {
            throw new ValidationException($"{minName} cannot be greater than {maxName}");
        }
    }
}
