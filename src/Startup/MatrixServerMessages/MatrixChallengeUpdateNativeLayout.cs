namespace Firefall.Reconstruction.Startup;

public enum MatrixChallengeUpdateAction
{
    Ignore,
    ReleaseActiveChallenge,
    ReplaceRecords,
    NotifyOnly,
    SelectWord
}

public readonly record struct MatrixChallengeUpdateSelection(
    MatrixChallengeUpdateAction Action, uint? RuntimeMessageId);

public readonly record struct MatrixChallengeWordSelection(uint WordAt14, uint IndexAt24, bool EmitRuntime277);

/// <summary>Separate ID policies in server51..55; callback/resource execution is not simulated.</summary>
public static class MatrixChallengeUpdateNativeLayout
{
    public const uint Message51HandlerFileOffset = 0x491D30;
    public const uint Message52HandlerFileOffset = 0x491D90;
    public const uint Message53HandlerFileOffset = 0x491E10;
    public const uint Message54HandlerFileOffset = 0x491E60;
    public const uint Message55HandlerFileOffset = 0x491EC0;
    public const uint LeaveConsumerFileOffset = 0x8FE560;
    public const uint RecordCopyMethodFileOffset = 0x901C70;
    public const uint SelectWordMethodFileOffset = 0x8FAD90;
    public const int RecordVectorOffset = 8;
    public const int SelectedWordOffset = 0x14;
    public const int SelectionRecordVectorOffset = 0x18;
    public const int SelectedIndexOffset = 0x24;
    public const uint NoSelectedIndex = uint.MaxValue;

    public static MatrixChallengeUpdateSelection SelectHandler(
        uint messageCode, bool hasCurrentModeObject, ulong? storedChallengeId, ulong incomingChallengeId)
    {
        if (messageCode is < 51 or > 55)
            throw new ArgumentOutOfRangeException(nameof(messageCode), "This selection covers only server51..55.");
        if (!hasCurrentModeObject || storedChallengeId is not ulong id)
            return new(MatrixChallengeUpdateAction.Ignore, null);
        bool match = id == incomingChallengeId
            || (messageCode is 51 or 52 or 54 && !MatrixChallengeNativeLayout.IsValidStoredId(id));
        if (!match)
            return new(MatrixChallengeUpdateAction.Ignore, null);
        return messageCode switch
        {
            51 => new(MatrixChallengeUpdateAction.ReleaseActiveChallenge, 275),
            52 => new(MatrixChallengeUpdateAction.ReplaceRecords, 273),
            53 => new(MatrixChallengeUpdateAction.NotifyOnly, 276),
            54 => new(MatrixChallengeUpdateAction.SelectWord, 277),
            _ => new(MatrixChallengeUpdateAction.NotifyOnly, 278)
        };
    }

    public static MatrixChallengeWordSelection CaptureWordSelection(
        uint word, IReadOnlyList<MatrixChallengeJoinEntry> records, bool emitRuntime277)
    {
        ArgumentNullException.ThrowIfNull(records);
        for (int index = 0; index < records.Count; index++)
            if (records[index].WordAt0 == word)
                return new(word, (uint)index, emitRuntime277);
        return new(word, NoSelectedIndex, emitRuntime277);
    }
}
