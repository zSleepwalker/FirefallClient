using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UnreliableReceiveHighCountCounterCapture(
    bool ValidatedEntry,
    uint ReceiverEdi,
    uint? IndependentCounter4CReload,
    uint? Counter48IfNonzeroPostDecrement);

public enum UnreliableReceiveHighCountCounterOutcome
{
    InvalidEntry,
    MissingIndependentCounter4CReload,
    MissingCounter48ForNonzeroPostDecrement,
    Counter4CDecrementedAndCounter48Zeroed,
    Counter4CDecrementedAndCounter48Incremented
}

public readonly record struct UnreliableReceiveHighCountCounterPlan(
    UnreliableReceiveHighCountCounterCapture Capture,
    UnreliableReceiveHighCountCounterOutcome Outcome,
    uint? Counter4CAddress,
    bool Counter4CRead,
    uint? Counter4COldValue,
    uint? Counter4CNewValue,
    bool Counter4CWriteProjected,
    bool? Counter4CIsZeroAfterDecrement,
    bool? NonzeroPostDecrementBranchTaken,
    uint? Counter48Address,
    bool Counter48Read,
    uint? Counter48OldValue,
    uint? Counter48NewValue,
    bool Counter48WriteProjected,
    bool Counter48ZeroStoreProjected,
    bool UnreachedIndependentCounter4CReloadCaptureProvided,
    bool UnreachedCounter48CaptureProvided,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct UnreliableReceiveHighCountCounterInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset);

/// <summary>
/// Projects the bounded DEC/JNZ and receiver+0x48 counter update only.
/// </summary>
public static class UnreliableReceiveHighCountCounterProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE5BFE;
    public const uint EntryVirtualAddress = 0x00FE67FE;
    public const uint NonzeroDecrementBranchRawFileOffset = 0x00BE5C01;
    public const uint ZeroDecrementStoreRawFileOffset = 0x00BE5C03;
    public const uint ZeroDecrementJumpRawFileOffset = 0x00BE5C0A;
    public const uint Counter48IncrementRawFileOffset = 0x00BE5C0C;
    public const uint JoinRawFileOffset = 0x00BE5C0F;
    public const uint EndRawFileOffsetExclusive = JoinRawFileOffset;

    public const uint ReceiverCounter4COffset = 0x4C;
    public const uint ReceiverCounter48Offset = 0x48;
    public const uint NonzeroDecrementBranchTargetRawFileOffset = Counter48IncrementRawFileOffset;
    public const uint ZeroDecrementBranchTargetRawFileOffset = JoinRawFileOffset;

    public const uint Counter4CReadModifyWriteRawFileOffset = EntryRawFileOffset;
    public const uint Counter48ZeroStoreRawFileOffset = ZeroDecrementStoreRawFileOffset;
    public const uint Counter48IncrementWriteRawFileOffset = Counter48IncrementRawFileOffset;

    public const string CapturedWindowBytes =
        "FF 4F 4C 75 09 C7 47 48 00 00 00 00 EB 03 FF 47 48";

    public static ImmutableArray<UnreliableReceiveHighCountCounterInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            new UnreliableReceiveHighCountCounterInstructionEvidence(
                "Decrement the DWORD at [EDI+0x4C] with 32-bit wraparound",
                Counter4CReadModifyWriteRawFileOffset,
                RawFileOffsetToVirtualAddress(Counter4CReadModifyWriteRawFileOffset),
                "FF 4F 4C",
                null),
            new UnreliableReceiveHighCountCounterInstructionEvidence(
                "Branch on the DEC zero flag to the [EDI+0x48] increment when the result is nonzero",
                NonzeroDecrementBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(NonzeroDecrementBranchRawFileOffset),
                "75 09",
                NonzeroDecrementBranchTargetRawFileOffset),
            new UnreliableReceiveHighCountCounterInstructionEvidence(
                "Store zero to the DWORD at [EDI+0x48] when the decremented counter is zero",
                ZeroDecrementStoreRawFileOffset,
                RawFileOffsetToVirtualAddress(ZeroDecrementStoreRawFileOffset),
                "C7 47 48 00 00 00 00",
                null),
            new UnreliableReceiveHighCountCounterInstructionEvidence(
                "Jump from the zero-store path to the join",
                ZeroDecrementJumpRawFileOffset,
                RawFileOffsetToVirtualAddress(ZeroDecrementJumpRawFileOffset),
                "EB 03",
                ZeroDecrementBranchTargetRawFileOffset),
            new UnreliableReceiveHighCountCounterInstructionEvidence(
                "Increment the DWORD at [EDI+0x48] with 32-bit wraparound",
                Counter48IncrementRawFileOffset,
                RawFileOffsetToVirtualAddress(Counter48IncrementRawFileOffset),
                "FF 47 48",
                null));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ReceiverCounter4CAddress(uint receiverEdi) =>
        unchecked(receiverEdi + ReceiverCounter4COffset);

    public static uint ReceiverCounter48Address(uint receiverEdi) =>
        unchecked(receiverEdi + ReceiverCounter48Offset);

    public static UnreliableReceiveHighCountCounterPlan Project(
        UnreliableReceiveHighCountCounterCapture capture)
    {
        if (!capture.ValidatedEntry)
        {
            return new UnreliableReceiveHighCountCounterPlan(
                capture,
                UnreliableReceiveHighCountCounterOutcome.InvalidEntry,
                Counter4CAddress: null,
                Counter4CRead: false,
                Counter4COldValue: null,
                Counter4CNewValue: null,
                Counter4CWriteProjected: false,
                Counter4CIsZeroAfterDecrement: null,
                NonzeroPostDecrementBranchTaken: null,
                Counter48Address: null,
                Counter48Read: false,
                Counter48OldValue: null,
                Counter48NewValue: null,
                Counter48WriteProjected: false,
                Counter48ZeroStoreProjected: false,
                UnreachedIndependentCounter4CReloadCaptureProvided:
                    capture.IndependentCounter4CReload.HasValue,
                UnreachedCounter48CaptureProvided:
                    capture.Counter48IfNonzeroPostDecrement.HasValue,
                StopAtRawFileOffsetExclusive: EntryRawFileOffset);
        }

        uint counter4CAddress = ReceiverCounter4CAddress(capture.ReceiverEdi);
        if (capture.IndependentCounter4CReload is not uint oldCounter4C)
        {
            return new UnreliableReceiveHighCountCounterPlan(
                capture,
                UnreliableReceiveHighCountCounterOutcome.MissingIndependentCounter4CReload,
                counter4CAddress,
                Counter4CRead: false,
                Counter4COldValue: null,
                Counter4CNewValue: null,
                Counter4CWriteProjected: false,
                Counter4CIsZeroAfterDecrement: null,
                NonzeroPostDecrementBranchTaken: null,
                Counter48Address: null,
                Counter48Read: false,
                Counter48OldValue: null,
                Counter48NewValue: null,
                Counter48WriteProjected: false,
                Counter48ZeroStoreProjected: false,
                UnreachedIndependentCounter4CReloadCaptureProvided: false,
                UnreachedCounter48CaptureProvided:
                    capture.Counter48IfNonzeroPostDecrement.HasValue,
                StopAtRawFileOffsetExclusive: Counter4CReadModifyWriteRawFileOffset);
        }

        uint newCounter4C = unchecked(oldCounter4C - 1);
        bool counter4CIsZero = newCounter4C == 0;
        uint counter48Address = ReceiverCounter48Address(capture.ReceiverEdi);
        if (counter4CIsZero)
        {
            return new UnreliableReceiveHighCountCounterPlan(
                capture,
                UnreliableReceiveHighCountCounterOutcome.Counter4CDecrementedAndCounter48Zeroed,
                counter4CAddress,
                Counter4CRead: true,
                oldCounter4C,
                newCounter4C,
                Counter4CWriteProjected: true,
                Counter4CIsZeroAfterDecrement: true,
                NonzeroPostDecrementBranchTaken: false,
                counter48Address,
                Counter48Read: false,
                Counter48OldValue: null,
                Counter48NewValue: 0,
                Counter48WriteProjected: true,
                Counter48ZeroStoreProjected: true,
                UnreachedIndependentCounter4CReloadCaptureProvided: false,
                UnreachedCounter48CaptureProvided:
                    capture.Counter48IfNonzeroPostDecrement.HasValue,
                StopAtRawFileOffsetExclusive: JoinRawFileOffset);
        }

        if (capture.Counter48IfNonzeroPostDecrement is not uint oldCounter48)
        {
            return new UnreliableReceiveHighCountCounterPlan(
                capture,
                UnreliableReceiveHighCountCounterOutcome.MissingCounter48ForNonzeroPostDecrement,
                counter4CAddress,
                Counter4CRead: true,
                oldCounter4C,
                newCounter4C,
                Counter4CWriteProjected: true,
                Counter4CIsZeroAfterDecrement: false,
                NonzeroPostDecrementBranchTaken: true,
                counter48Address,
                Counter48Read: false,
                Counter48OldValue: null,
                Counter48NewValue: null,
                Counter48WriteProjected: false,
                Counter48ZeroStoreProjected: false,
                UnreachedIndependentCounter4CReloadCaptureProvided: false,
                UnreachedCounter48CaptureProvided: false,
                StopAtRawFileOffsetExclusive: Counter48IncrementRawFileOffset);
        }

        return new UnreliableReceiveHighCountCounterPlan(
            capture,
            UnreliableReceiveHighCountCounterOutcome.Counter4CDecrementedAndCounter48Incremented,
            counter4CAddress,
            Counter4CRead: true,
            oldCounter4C,
            newCounter4C,
            Counter4CWriteProjected: true,
            Counter4CIsZeroAfterDecrement: false,
            NonzeroPostDecrementBranchTaken: true,
            counter48Address,
            Counter48Read: true,
            oldCounter48,
            unchecked(oldCounter48 + 1),
            Counter48WriteProjected: true,
            Counter48ZeroStoreProjected: false,
            UnreachedIndependentCounter4CReloadCaptureProvided: false,
            UnreachedCounter48CaptureProvided: false,
            StopAtRawFileOffsetExclusive: JoinRawFileOffset);
    }
}
