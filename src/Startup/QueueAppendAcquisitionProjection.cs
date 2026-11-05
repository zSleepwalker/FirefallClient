namespace Firefall.Reconstruction.Startup;

public enum QueueAppendAcquisitionCaptureKind
{
    Missing,
    Unreached,
    Captured
}

public readonly struct QueueAppendAcquisitionCapture<T> where T : struct
{
    private QueueAppendAcquisitionCapture(QueueAppendAcquisitionCaptureKind kind, T? value)
    {
        Kind = kind;
        Value = value;
    }

    public QueueAppendAcquisitionCaptureKind Kind { get; }
    public T? Value { get; }

    public static QueueAppendAcquisitionCapture<T> Missing() =>
        new(QueueAppendAcquisitionCaptureKind.Missing, null);

    public static QueueAppendAcquisitionCapture<T> Unreached() =>
        new(QueueAppendAcquisitionCaptureKind.Unreached, null);

    public static QueueAppendAcquisitionCapture<T> Captured(T value) =>
        new(QueueAppendAcquisitionCaptureKind.Captured, value);
}

public readonly record struct QueueAppendAcquisitionCaptures(
    uint ReceiverPointer,
    QueueAppendAcquisitionCapture<uint> CounterOldValue,
    QueueAppendAcquisitionCapture<uint> GlobalPoolPointer,
    QueueAppendAcquisitionCapture<uint> AcquisitionResult);

public enum QueueAppendAcquisitionOutcome
{
    MissingCounterCapture,
    MissingGlobalPoolCapture,
    MissingAcquisitionResult,
    NonzeroResultBranchProjected,
    NullResultBranchProjected
}

public enum QueueAppendAcquisitionActionKind
{
    IncrementReceiverCounterAndCaptureOldValue,
    StopAtMissingCounterCapture,
    LoadIndependentGlobalPoolPointerIntoEcx,
    CallAcquisitionFunction,
    StopAtMissingGlobalPoolCapture,
    StoreCapturedAcquisitionResult,
    StopAtMissingAcquisitionResult,
    TestAcquisitionResult,
    ZeroFirstEightResultBytes,
    SetEaxToZero,
    StoreNullAcquisitionResult,
    ComputeReceiverPairAddress,
    AttemptUnconditionalFirstDwordZero,
    AttemptUnconditionalSecondDwordZero,
    SaveReceiverPairAddress,
    StopBeforeReceiverWordCRead
}

public readonly record struct QueueAppendAcquisitionAction(
    QueueAppendAcquisitionActionKind Kind,
    uint InstructionRawFileOffset,
    uint? Address = null,
    uint? Value = null,
    uint? RelatedValue = null,
    uint? TargetRawFileOffset = null,
    uint? TargetVirtualAddress = null,
    int? ByteCount = null);

public readonly record struct QueueAppendAcquisitionPlan(
    QueueAppendAcquisitionCaptures Captures,
    QueueAppendAcquisitionOutcome Outcome,
    IReadOnlyList<QueueAppendAcquisitionAction> Actions,
    uint? CounterAddress,
    uint? CounterOldValue,
    uint? CounterNewValue,
    uint? GlobalPoolPointer,
    uint? AcquisitionResult,
    uint? ReceiverPairAddress);

public readonly record struct QueueAppendAcquisitionInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? BranchTargetRawFileOffset = null);

/// <summary>
/// Projects the captured counter, global-pool, and acquisition-result values
/// through the queue append setup. It does not perform native memory accesses,
/// acquire ownership, or model the subsequent append. Null-result stores are
/// recorded as attempts, without assuming successful writes or survival.
/// </summary>
public static class QueueAppendAcquisitionProjection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x12CE310;
    public const uint EntryVirtualAddress =
        EntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint AcquisitionSetupEndRawFileOffsetExclusive = 0x12CE364;
    public const uint AcquisitionSetupEndVirtualAddress =
        AcquisitionSetupEndRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;

    public const uint ReceiverCounterIncrementRawFileOffset = 0x12CE32A;
    public const int ReceiverCounterOffset = 0x10;
    public const bool CounterXaddOldValueUsed = false;
    public const uint GlobalPoolPointerLoadRawFileOffset = 0x12CE32E;
    public const uint GlobalPoolPointerVirtualAddress = 0x01E003BC;
    public const uint AcquisitionCallRawFileOffset = 0x12CE334;
    public const uint AcquisitionFunctionRawFileOffset = 0x12CDE60;
    public const uint AcquisitionFunctionVirtualAddress =
        AcquisitionFunctionRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint AcquisitionResultStoreRawFileOffset = 0x12CE339;
    public const uint AcquisitionResultTestRawFileOffset = 0x12CE33D;
    public const uint NullResultBranchRawFileOffset = 0x12CE33F;
    public const uint NullResultBranchTargetRawFileOffset = 0x12CE34A;
    public const uint NonNullResultQwordZeroRawFileOffset = 0x12CE344;
    public const int NonNullResultZeroByteCount = 8;
    public const uint NullResultEaxZeroRawFileOffset = 0x12CE34A;
    public const uint NullResultStoreRawFileOffset = 0x12CE34C;
    public const uint ReceiverPairAddressRawFileOffset = 0x12CE350;
    public const int ReceiverPairOffset = 8;
    public const uint FirstUnconditionalDwordZeroRawFileOffset = 0x12CE353;
    public const uint SecondUnconditionalDwordZeroRawFileOffset = 0x12CE359;
    public const int UnconditionalDwordZeroByteCount = 4;
    public const uint ReceiverPairAddressSaveRawFileOffset = 0x12CE360;
    public const uint ReceiverWordCReadBoundaryRawFileOffset =
        AcquisitionSetupEndRawFileOffsetExclusive;
    public const uint FunctionCaptureEndRawFileOffsetExclusive =
        AcquisitionSetupEndRawFileOffsetExclusive;

    public const string CapturedFunctionBytes =
        "55 8B EC 83 E4 F8 83 EC 14 53 8B D9 BA 01 00 00 00 56 57 89 5C 24 14 " +
        "8D 43 10 F0 0F C1 10 8B 0D BC 03 E0 01 E8 27 FB FF FF 89 44 24 0C " +
        "85 C0 74 09 0F 57 C0 66 0F D6 00 EB 06 33 C0 89 44 24 0C " +
        "8D 4B 08 C7 00 00 00 00 00 C7 40 04 00 00 00 00 89 4C 24 10";

    public static IReadOnlyList<QueueAppendAcquisitionInstructionEvidence> Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new QueueAppendAcquisitionInstructionEvidence(
                "Use the receiver counter at +0x10 as the locked XADD destination",
                ReceiverCounterIncrementRawFileOffset,
                "F0 0F C1 10"),
            new QueueAppendAcquisitionInstructionEvidence(
                "Load the independent global pool pointer after the counter XADD",
                GlobalPoolPointerLoadRawFileOffset,
                "8B 0D BC 03 E0 01"),
            new QueueAppendAcquisitionInstructionEvidence(
                "Call the acquisition helper by direct relative call",
                AcquisitionCallRawFileOffset,
                "E8 27 FB FF FF",
                AcquisitionFunctionRawFileOffset),
            new QueueAppendAcquisitionInstructionEvidence(
                "Save the returned pointer before testing it",
                AcquisitionResultStoreRawFileOffset,
                "89 44 24 0C"),
            new QueueAppendAcquisitionInstructionEvidence(
                "Branch to the null-result EAX zeroing block",
                NullResultBranchRawFileOffset,
                "74 09",
                NullResultBranchTargetRawFileOffset),
            new QueueAppendAcquisitionInstructionEvidence(
                "Zero the first eight bytes only for a nonzero result",
                NonNullResultQwordZeroRawFileOffset,
                "66 0F D6 00"),
            new QueueAppendAcquisitionInstructionEvidence(
                "Set EAX to zero on the null-result branch",
                NullResultEaxZeroRawFileOffset,
                "33 C0"),
            new QueueAppendAcquisitionInstructionEvidence(
                "Form receiver+8 before the unconditional zero attempts",
                ReceiverPairAddressRawFileOffset,
                "8D 4B 08"),
            new QueueAppendAcquisitionInstructionEvidence(
                "Attempt the first unconditional DWORD zero through EAX",
                FirstUnconditionalDwordZeroRawFileOffset,
                "C7 00 00 00 00 00"),
            new QueueAppendAcquisitionInstructionEvidence(
                "Attempt the second unconditional DWORD zero through EAX+4",
                SecondUnconditionalDwordZeroRawFileOffset,
                "C7 40 04 00 00 00 00"),
            new QueueAppendAcquisitionInstructionEvidence(
                "Save receiver+8 and stop before the receiver+0xC read",
                ReceiverPairAddressSaveRawFileOffset,
                "89 4C 24 10")
        });

    public static QueueAppendAcquisitionPlan Project(
        QueueAppendAcquisitionCaptures captures)
    {
        ValidateCapture(captures.CounterOldValue, nameof(captures.CounterOldValue));
        ValidateCapture(captures.GlobalPoolPointer, nameof(captures.GlobalPoolPointer));
        ValidateCapture(captures.AcquisitionResult, nameof(captures.AcquisitionResult));

        if (captures.CounterOldValue.Kind == QueueAppendAcquisitionCaptureKind.Unreached)
            throw new ArgumentException("The entry path always executes the counter XADD.");
        if (captures.CounterOldValue.Kind == QueueAppendAcquisitionCaptureKind.Missing)
        {
            RequireUnreached(captures.GlobalPoolPointer, nameof(captures.GlobalPoolPointer));
            RequireUnreached(captures.AcquisitionResult, nameof(captures.AcquisitionResult));
            return Stop(
                captures,
                QueueAppendAcquisitionOutcome.MissingCounterCapture,
                QueueAppendAcquisitionActionKind.StopAtMissingCounterCapture,
                ReceiverCounterIncrementRawFileOffset);
        }

        if (captures.GlobalPoolPointer.Kind == QueueAppendAcquisitionCaptureKind.Unreached)
            throw new ArgumentException("The global pool load follows the captured counter XADD.");

        uint counterOldValue = captures.CounterOldValue.Value!.Value;
        uint counterAddress = unchecked(captures.ReceiverPointer + ReceiverCounterOffset);
        uint counterNewValue = unchecked(counterOldValue + 1);
        var actions = new List<QueueAppendAcquisitionAction>
        {
            new(
                QueueAppendAcquisitionActionKind.IncrementReceiverCounterAndCaptureOldValue,
                ReceiverCounterIncrementRawFileOffset,
                Address: counterAddress,
                Value: counterOldValue,
                RelatedValue: counterNewValue)
        };

        if (captures.GlobalPoolPointer.Kind == QueueAppendAcquisitionCaptureKind.Missing)
        {
            RequireUnreached(captures.AcquisitionResult, nameof(captures.AcquisitionResult));
            actions.Add(new(
                QueueAppendAcquisitionActionKind.StopAtMissingGlobalPoolCapture,
                GlobalPoolPointerLoadRawFileOffset));
            return new(
                captures,
                QueueAppendAcquisitionOutcome.MissingGlobalPoolCapture,
                Array.AsReadOnly(actions.ToArray()),
                counterAddress,
                counterOldValue,
                counterNewValue,
                null,
                null,
                null);
        }

        uint globalPoolPointer = captures.GlobalPoolPointer.Value!.Value;
        if (captures.AcquisitionResult.Kind == QueueAppendAcquisitionCaptureKind.Unreached)
            throw new ArgumentException("A result is reached only after the acquisition call.");

        actions.Add(new(
            QueueAppendAcquisitionActionKind.LoadIndependentGlobalPoolPointerIntoEcx,
            GlobalPoolPointerLoadRawFileOffset,
            Address: GlobalPoolPointerVirtualAddress,
            Value: globalPoolPointer));
        actions.Add(new(
            QueueAppendAcquisitionActionKind.CallAcquisitionFunction,
            AcquisitionCallRawFileOffset,
            Value: globalPoolPointer,
            TargetRawFileOffset: AcquisitionFunctionRawFileOffset,
            TargetVirtualAddress: AcquisitionFunctionVirtualAddress));

        if (captures.AcquisitionResult.Kind == QueueAppendAcquisitionCaptureKind.Missing)
        {
            actions.Add(new(
                QueueAppendAcquisitionActionKind.StopAtMissingAcquisitionResult,
                AcquisitionResultStoreRawFileOffset));
            return new(
                captures,
                QueueAppendAcquisitionOutcome.MissingAcquisitionResult,
                Array.AsReadOnly(actions.ToArray()),
                counterAddress,
                counterOldValue,
                counterNewValue,
                globalPoolPointer,
                null,
                null);
        }

        uint acquisitionResult = captures.AcquisitionResult.Value!.Value;
        actions.Add(new(
            QueueAppendAcquisitionActionKind.StoreCapturedAcquisitionResult,
            AcquisitionResultStoreRawFileOffset,
            Value: acquisitionResult));
        actions.Add(new(
            QueueAppendAcquisitionActionKind.TestAcquisitionResult,
            AcquisitionResultTestRawFileOffset,
            Value: acquisitionResult));

        if (acquisitionResult != 0)
        {
            actions.Add(new(
                QueueAppendAcquisitionActionKind.ZeroFirstEightResultBytes,
                NonNullResultQwordZeroRawFileOffset,
                Address: acquisitionResult,
                ByteCount: NonNullResultZeroByteCount));
        }
        else
        {
            actions.Add(new(
                QueueAppendAcquisitionActionKind.SetEaxToZero,
                NullResultEaxZeroRawFileOffset,
                Value: 0));
            actions.Add(new(
                QueueAppendAcquisitionActionKind.StoreNullAcquisitionResult,
                NullResultStoreRawFileOffset,
                Value: 0));
        }

        uint receiverPairAddress = unchecked(captures.ReceiverPointer + ReceiverPairOffset);
        actions.Add(new(
            QueueAppendAcquisitionActionKind.ComputeReceiverPairAddress,
            ReceiverPairAddressRawFileOffset,
            Address: receiverPairAddress));
        actions.Add(new(
            QueueAppendAcquisitionActionKind.AttemptUnconditionalFirstDwordZero,
            FirstUnconditionalDwordZeroRawFileOffset,
            Address: acquisitionResult,
            Value: 0,
            ByteCount: UnconditionalDwordZeroByteCount));
        actions.Add(new(
            QueueAppendAcquisitionActionKind.AttemptUnconditionalSecondDwordZero,
            SecondUnconditionalDwordZeroRawFileOffset,
            Address: unchecked(acquisitionResult + UnconditionalDwordZeroByteCount),
            Value: 0,
            ByteCount: UnconditionalDwordZeroByteCount));
        actions.Add(new(
            QueueAppendAcquisitionActionKind.SaveReceiverPairAddress,
            ReceiverPairAddressSaveRawFileOffset,
            Address: receiverPairAddress));
        actions.Add(new(
            QueueAppendAcquisitionActionKind.StopBeforeReceiverWordCRead,
            ReceiverWordCReadBoundaryRawFileOffset));

        return new(
            captures,
            acquisitionResult == 0
                ? QueueAppendAcquisitionOutcome.NullResultBranchProjected
                : QueueAppendAcquisitionOutcome.NonzeroResultBranchProjected,
            Array.AsReadOnly(actions.ToArray()),
            counterAddress,
            counterOldValue,
            counterNewValue,
            globalPoolPointer,
            acquisitionResult,
            receiverPairAddress);
    }

    private static QueueAppendAcquisitionPlan Stop(
        QueueAppendAcquisitionCaptures captures,
        QueueAppendAcquisitionOutcome outcome,
        QueueAppendAcquisitionActionKind stopKind,
        uint stopOffset) =>
        new(
            captures,
            outcome,
            Array.AsReadOnly(new[]
            {
                new QueueAppendAcquisitionAction(stopKind, stopOffset)
            }),
            null,
            null,
            null,
            null,
            null,
            null);

    private static void ValidateCapture<T>(
        QueueAppendAcquisitionCapture<T> capture,
        string parameterName) where T : struct
    {
        bool valid = capture.Kind switch
        {
            QueueAppendAcquisitionCaptureKind.Missing =>
                capture.Value is null,
            QueueAppendAcquisitionCaptureKind.Unreached =>
                capture.Value is null,
            QueueAppendAcquisitionCaptureKind.Captured =>
                capture.Value is not null,
            _ => false
        };
        if (!valid)
            throw new ArgumentException("Capture status and value are inconsistent.", parameterName);
    }

    private static void RequireUnreached<T>(
        QueueAppendAcquisitionCapture<T> capture,
        string parameterName) where T : struct
    {
        if (capture.Kind != QueueAppendAcquisitionCaptureKind.Unreached)
            throw new ArgumentException("This capture belongs to a path after a missing value.", parameterName);
    }
}
