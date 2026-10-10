namespace Firefall.Reconstruction.Startup;

public readonly record struct LoginZoneRecord(
    uint ZoneId,
    string ZoneName,
    ushort ProtocolVersion,
    string Build,
    string MatrixUrl,
    ushort Players);

public enum LoginZoneSelectionOutcome
{
    Deferred,
    Selected,
    NoMatchingCandidate
}

public readonly record struct LoginZoneSelectionResult(
    LoginZoneSelectionOutcome Outcome,
    int NextAutoLoginState,
    string DestinationText,
    int EligibleCandidateCount);

public static class LoginZoneListNativeLayout
{
    public const uint RecordDecoderFileOffset = 0x635BE0;
    public const uint ListDecoderFileOffset = 0x646500;
    public const uint ResponseHandlerFileOffset = 0x63D270;
    public const uint NameMatcherFileOffset = 0x63D160;
    public const uint SemicolonMatcherFileOffset = 0x63D110;
    public const uint CaseInsensitiveSubstringMethodFileOffset = 0x12B9D60;
    public const int RecordStride = 0x18;
    public const int ZoneIdOffset = 0x00;
    public const int ZoneNameOffset = 0x04;
    public const int ProtocolVersionOffset = 0x08;
    public const int BuildOffset = 0x0C;
    public const int MatrixUrlOffset = 0x10;
    public const int PlayersOffset = 0x14;
    public const ushort RequiredProtocolVersion = 0x4C5F;
    public const int MinimumAutoLoginState = 3;
    public const int SelectionTextMemberOffset = 0xA20;
    public const string ResponseListKey = "zone_list";
    public const string RandomSelectionText = "random";
    public const string FailureLogFormat = "AutoLogin: failed to pick zone by \"%s\"";
}

/// <summary>
/// Reconstructs candidate filtering and selection from already decoded records.
/// The caller supplies the native zone-name/SDB match decision and random sample;
/// this model does not emulate JSON conversion, CRT locale, or global PRNG state.
/// </summary>
public static class LoginZoneListSelection
{
    public static LoginZoneSelectionResult Select(
        int autoLoginState,
        string currentSelectionText,
        IReadOnlyList<LoginZoneRecord> records,
        bool selectRandom,
        uint randomSample,
        Func<uint, bool> matchesZone)
    {
        ArgumentNullException.ThrowIfNull(currentSelectionText);
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(matchesZone);

        if (autoLoginState < LoginZoneListNativeLayout.MinimumAutoLoginState)
            return new(LoginZoneSelectionOutcome.Deferred, autoLoginState, currentSelectionText, 0);

        var candidates = records.ToList();
        for (int i = 0; i < candidates.Count;)
        {
            var record = candidates[i];
            if (record.ProtocolVersion == LoginZoneListNativeLayout.RequiredProtocolVersion
                && record.Players != 0)
            {
                i++;
                continue;
            }

            // Native filtering replaces a rejected entry with the last entry.
            candidates[i] = candidates[^1];
            candidates.RemoveAt(candidates.Count - 1);
        }

        var eligible = selectRandom
            ? candidates
            : candidates.Where(record => matchesZone(record.ZoneId)).ToList();
        int nextState = unchecked(autoLoginState + 1);
        if (eligible.Count == 0)
            return new(LoginZoneSelectionOutcome.NoMatchingCandidate, nextState, "", 0);

        int index = (int)(randomSample % (uint)eligible.Count);
        return new(LoginZoneSelectionOutcome.Selected, nextState, eligible[index].MatrixUrl, eligible.Count);
    }
}
