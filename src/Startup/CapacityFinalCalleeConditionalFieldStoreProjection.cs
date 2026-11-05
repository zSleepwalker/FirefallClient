using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeConditionalFieldStoreCapture(
    uint? EntryECX,
    uint? EntryEBX,
    uint? FreshEcxPlus10,
    uint? EntryESI,
    uint? EntryEAX);

public enum CapacityFinalCalleeConditionalFieldStoreOutcome
{
    MissingEntryECX,
    MissingEntryEBX,
    EntryECXUnsignedBelowEntryEBXToValidation,
    MissingFreshEcxPlus10,
    MissingEntryESI,
    MissingEntryEAXForPlus10Store,
    MissingEntryEAXForPlus14Store,
    Plus10StoredThenEaxZeroToTarget,
    Plus10StoredThenEaxNonzeroToBoundary,
    Plus14StoredThenEaxZeroToTarget,
    Plus14StoredThenEaxNonzeroToBoundary
}

public readonly record struct CapacityFinalCalleeConditionalFieldStoreCompareFlags(
    bool Carry,
    bool Parity,
    bool AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeConditionalFieldStoreTestFlags(
    bool Carry,
    bool Parity,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeConditionalFieldStoreInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeConditionalFieldStorePlan(
    CapacityFinalCalleeConditionalFieldStoreCapture Capture,
    CapacityFinalCalleeConditionalFieldStoreOutcome Outcome,
    bool EntryECXCaptured,
    bool EntryEBXCaptured,
    uint? FirstCompareResult,
    CapacityFinalCalleeConditionalFieldStoreCompareFlags? FirstCompareFlags,
    bool? EntryECXUnsignedBelowEntryEBX,
    bool FirstBranchReached,
    bool? FirstBranchTaken,
    uint? FirstBranchSelectedDestinationRawFileOffset,
    uint? FreshEcxPlus10Address,
    bool FreshEcxPlus10ReadReached,
    bool FreshEcxPlus10Captured,
    uint? FreshEcxPlus10,
    bool EntryESICaptured,
    uint? SecondCompareResult,
    CapacityFinalCalleeConditionalFieldStoreCompareFlags? SecondCompareFlags,
    bool? FreshEcxPlus10EqualEntryESI,
    bool SecondBranchReached,
    bool? SecondBranchTaken,
    uint? SecondBranchSelectedDestinationRawFileOffset,
    bool EntryEAXCaptured,
    bool StoreReached,
    uint? StoreAddress,
    uint? StoreValue,
    uint? StoreDisplacement,
    bool JumpToTestReached,
    uint? JumpToTestDestinationRawFileOffset,
    bool TestEAXReached,
    uint? TestEAXValue,
    CapacityFinalCalleeConditionalFieldStoreTestFlags? TestEAXFlags,
    bool ZeroBranchReached,
    bool? ZeroBranchTaken,
    uint? ZeroBranchSelectedDestinationRawFileOffset,
    bool UnreachedEntryEBXCaptureProvided,
    bool UnreachedFreshEcxPlus10CaptureProvided,
    bool UnreachedEntryESICaptureProvided,
    bool UnreachedEntryEAXCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the conditional field stores and following EAX zero test. Fresh
/// memory captures are independent of entry registers and projected writes.
/// </summary>
public static class CapacityFinalCalleeConditionalFieldStoreProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C903B;
    public const uint EntryVirtualAddress = 0x016C9C3B;
    public const uint FirstCompareRawFileOffset = 0x012C903B;
    public const uint FirstBranchRawFileOffset = 0x012C903D;
    public const uint FreshReadRawFileOffset = 0x012C9043;
    public const uint SecondCompareRawFileOffset = 0x012C9043;
    public const uint SecondBranchRawFileOffset = 0x012C9046;
    public const uint Plus10StoreRawFileOffset = 0x012C9048;
    public const uint JumpToTestRawFileOffset = 0x012C904B;
    public const uint Plus14StoreRawFileOffset = 0x012C904D;
    public const uint TestEaxRawFileOffset = 0x012C9050;
    public const uint ZeroBranchRawFileOffset = 0x012C9052;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C9058;

    public const uint FirstBranchDestinationRawFileOffset = 0x012C8E76;
    public const uint FreshCompareDestinationRawFileOffset = 0x012C9043;
    public const uint Plus10StoreDestinationRawFileOffset = 0x012C9048;
    public const uint Plus14StoreDestinationRawFileOffset = 0x012C904D;
    public const uint TestEaxDestinationRawFileOffset = 0x012C9050;
    public const uint ZeroBranchDestinationRawFileOffset = 0x012C8EF2;
    public const uint EbxDisplacement = 0x10;
    public const uint Plus10StoreDisplacement = 0x10;
    public const uint Plus14StoreDisplacement = 0x14;

    public const string CapturedWindowBytes =
        "3B CB 0F 82 33 FE FF FF 39 71 10 75 05 89 41 10 "
        + "EB 03 89 41 14 85 C0 0F 84 9A FE FF FF";

    public static ImmutableArray<CapacityFinalCalleeConditionalFieldStoreInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Compare entry ECX with entry EBX",
                FirstCompareRawFileOffset, "3B CB"),
            Instruction("Jump to the shared validation call when unsigned ECX is below EBX",
                FirstBranchRawFileOffset, "0F 82 33 FE FF FF",
                FirstBranchDestinationRawFileOffset),
            Instruction("Compare a fresh DWORD at ECX+10 with entry ESI",
                SecondCompareRawFileOffset, "39 71 10"),
            Instruction("Select the ECX+14 store when the fresh DWORD differs from ESI",
                SecondBranchRawFileOffset, "75 05", Plus14StoreDestinationRawFileOffset),
            Instruction("Store entry EAX at ECX+10 on equality",
                Plus10StoreRawFileOffset, "89 41 10"),
            Instruction("Jump from the ECX+10 store to the EAX test",
                JumpToTestRawFileOffset, "EB 03", TestEaxDestinationRawFileOffset),
            Instruction("Store entry EAX at ECX+14 on inequality",
                Plus14StoreRawFileOffset, "89 41 14"),
            Instruction("Test retained entry EAX against zero",
                TestEaxRawFileOffset, "85 C0"),
            Instruction("Jump to EF2 when entry EAX is zero",
                ZeroBranchRawFileOffset, "0F 84 9A FE FF FF",
                ZeroBranchDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFreshEcxPlus10Address(uint entryEcx) =>
        unchecked(entryEcx + EbxDisplacement);

    public static uint DeriveStoreAddress(uint entryEcx, uint displacement) =>
        unchecked(entryEcx + displacement);

    public static CapacityFinalCalleeConditionalFieldStorePlan Project(
        CapacityFinalCalleeConditionalFieldStoreCapture capture)
    {
        if (capture.EntryECX is not uint entryEcx)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeConditionalFieldStoreOutcome.MissingEntryECX,
                unreachedEntryEBXCaptureProvided: capture.EntryEBX.HasValue,
                unreachedFreshEcxPlus10CaptureProvided: capture.FreshEcxPlus10.HasValue,
                unreachedEntryESICaptureProvided: capture.EntryESI.HasValue,
                unreachedEntryEAXCaptureProvided: capture.EntryEAX.HasValue,
                stopAt: FirstCompareRawFileOffset);
        }

        if (capture.EntryEBX is not uint entryEbx)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeConditionalFieldStoreOutcome.MissingEntryEBX,
                entryEcxCaptured: true,
                unreachedFreshEcxPlus10CaptureProvided: capture.FreshEcxPlus10.HasValue,
                unreachedEntryESICaptureProvided: capture.EntryESI.HasValue,
                unreachedEntryEAXCaptureProvided: capture.EntryEAX.HasValue,
                stopAt: FirstCompareRawFileOffset);
        }

        uint firstCompareResult = unchecked(entryEcx - entryEbx);
        CapacityFinalCalleeConditionalFieldStoreCompareFlags firstFlags =
            CalculateCompareFlags(entryEcx, entryEbx, firstCompareResult);
        bool below = firstFlags.Carry;
        if (below)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeConditionalFieldStoreOutcome
                    .EntryECXUnsignedBelowEntryEBXToValidation,
                entryEcxCaptured: true,
                entryEbxCaptured: true,
                firstCompareResult: firstCompareResult,
                firstCompareFlags: firstFlags,
                entryEcxUnsignedBelowEntryEbx: true,
                firstBranchReached: true,
                firstBranchTaken: true,
                firstBranchSelectedDestinationRawFileOffset:
                    FirstBranchDestinationRawFileOffset,
                unreachedFreshEcxPlus10CaptureProvided:
                    capture.FreshEcxPlus10.HasValue,
                unreachedEntryESICaptureProvided: capture.EntryESI.HasValue,
                unreachedEntryEAXCaptureProvided: capture.EntryEAX.HasValue,
                stopAt: FirstBranchDestinationRawFileOffset);
        }

        uint freshAddress = DeriveFreshEcxPlus10Address(entryEcx);
        if (capture.FreshEcxPlus10 is not uint freshValue)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeConditionalFieldStoreOutcome.MissingFreshEcxPlus10,
                entryEcxCaptured: true,
                entryEbxCaptured: true,
                firstCompareResult: firstCompareResult,
                firstCompareFlags: firstFlags,
                entryEcxUnsignedBelowEntryEbx: false,
                firstBranchReached: true,
                firstBranchTaken: false,
                firstBranchSelectedDestinationRawFileOffset:
                    FreshCompareDestinationRawFileOffset,
                freshEcxPlus10Address: freshAddress,
                freshEcxPlus10ReadReached: true,
                unreachedEntryESICaptureProvided: capture.EntryESI.HasValue,
                unreachedEntryEAXCaptureProvided: capture.EntryEAX.HasValue,
                stopAt: FreshReadRawFileOffset);
        }

        if (capture.EntryESI is not uint entryEsi)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeConditionalFieldStoreOutcome.MissingEntryESI,
                entryEcxCaptured: true,
                entryEbxCaptured: true,
                firstCompareResult: firstCompareResult,
                firstCompareFlags: firstFlags,
                entryEcxUnsignedBelowEntryEbx: false,
                firstBranchReached: true,
                firstBranchTaken: false,
                firstBranchSelectedDestinationRawFileOffset:
                    FreshCompareDestinationRawFileOffset,
                freshEcxPlus10Address: freshAddress,
                freshEcxPlus10ReadReached: true,
                freshEcxPlus10Captured: true,
                freshEcxPlus10: freshValue,
                unreachedEntryEAXCaptureProvided: capture.EntryEAX.HasValue,
                stopAt: SecondCompareRawFileOffset);
        }

        uint secondCompareResult = unchecked(freshValue - entryEsi);
        CapacityFinalCalleeConditionalFieldStoreCompareFlags secondFlags =
            CalculateCompareFlags(freshValue, entryEsi, secondCompareResult);
        bool equal = secondFlags.Zero;
        bool branchTaken = !equal;
        uint storeDisplacement = equal
            ? Plus10StoreDisplacement
            : Plus14StoreDisplacement;
        uint storeRawOffset = equal
            ? Plus10StoreRawFileOffset
            : Plus14StoreRawFileOffset;

        if (capture.EntryEAX is not uint entryEax)
        {
            return BuildPlan(
                capture,
                equal
                    ? CapacityFinalCalleeConditionalFieldStoreOutcome
                        .MissingEntryEAXForPlus10Store
                    : CapacityFinalCalleeConditionalFieldStoreOutcome
                        .MissingEntryEAXForPlus14Store,
                entryEcxCaptured: true,
                entryEbxCaptured: true,
                firstCompareResult: firstCompareResult,
                firstCompareFlags: firstFlags,
                entryEcxUnsignedBelowEntryEbx: false,
                firstBranchReached: true,
                firstBranchTaken: false,
                firstBranchSelectedDestinationRawFileOffset:
                    FreshCompareDestinationRawFileOffset,
                freshEcxPlus10Address: freshAddress,
                freshEcxPlus10ReadReached: true,
                freshEcxPlus10Captured: true,
                freshEcxPlus10: freshValue,
                entryEsiCaptured: true,
                secondCompareResult: secondCompareResult,
                secondCompareFlags: secondFlags,
                freshEcxPlus10EqualEntryEsi: equal,
                secondBranchReached: true,
                secondBranchTaken: branchTaken,
                secondBranchSelectedDestinationRawFileOffset:
                    equal ? Plus10StoreDestinationRawFileOffset
                        : Plus14StoreDestinationRawFileOffset,
                stopAt: storeRawOffset);
        }

        uint storeAddress = DeriveStoreAddress(entryEcx, storeDisplacement);
        CapacityFinalCalleeConditionalFieldStoreTestFlags testFlags =
            CalculateTestFlags(entryEax);
        bool eaxIsZero = testFlags.Zero;
        return BuildPlan(
            capture,
            (equal, eaxIsZero) switch
            {
                (true, true) =>
                    CapacityFinalCalleeConditionalFieldStoreOutcome
                        .Plus10StoredThenEaxZeroToTarget,
                (true, false) =>
                    CapacityFinalCalleeConditionalFieldStoreOutcome
                        .Plus10StoredThenEaxNonzeroToBoundary,
                (false, true) =>
                    CapacityFinalCalleeConditionalFieldStoreOutcome
                        .Plus14StoredThenEaxZeroToTarget,
                _ =>
                    CapacityFinalCalleeConditionalFieldStoreOutcome
                        .Plus14StoredThenEaxNonzeroToBoundary
            },
            entryEcxCaptured: true,
            entryEbxCaptured: true,
            firstCompareResult: firstCompareResult,
            firstCompareFlags: firstFlags,
            entryEcxUnsignedBelowEntryEbx: false,
            firstBranchReached: true,
            firstBranchTaken: false,
            firstBranchSelectedDestinationRawFileOffset:
                FreshCompareDestinationRawFileOffset,
            freshEcxPlus10Address: freshAddress,
            freshEcxPlus10ReadReached: true,
            freshEcxPlus10Captured: true,
            freshEcxPlus10: freshValue,
            entryEsiCaptured: true,
            secondCompareResult: secondCompareResult,
            secondCompareFlags: secondFlags,
            freshEcxPlus10EqualEntryEsi: equal,
            secondBranchReached: true,
            secondBranchTaken: branchTaken,
            secondBranchSelectedDestinationRawFileOffset:
                equal ? Plus10StoreDestinationRawFileOffset
                    : Plus14StoreDestinationRawFileOffset,
            entryEaxCaptured: true,
            storeReached: true,
            storeAddress: storeAddress,
            storeValue: entryEax,
            storeDisplacement: storeDisplacement,
            jumpToTestReached: equal,
            jumpToTestDestinationRawFileOffset:
                equal ? TestEaxDestinationRawFileOffset : null,
            testEaxReached: true,
            testEaxValue: entryEax,
            testEaxFlags: testFlags,
            zeroBranchReached: true,
            zeroBranchTaken: eaxIsZero,
            zeroBranchSelectedDestinationRawFileOffset:
                eaxIsZero
                    ? ZeroBranchDestinationRawFileOffset
                    : BoundaryRawFileOffsetExclusive,
            stopAt: eaxIsZero
                ? ZeroBranchDestinationRawFileOffset
                : BoundaryRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleeConditionalFieldStoreCompareFlags
        CalculateCompareFlags(uint left, uint right, uint result) =>
        new(
            left < right,
            HasEvenParity((byte)result),
            ((left ^ right ^ result) & 0x10) != 0,
            result == 0,
            (result & 0x80000000) != 0,
            ((left ^ right) & (left ^ result) & 0x80000000) != 0);

    private static CapacityFinalCalleeConditionalFieldStoreTestFlags
        CalculateTestFlags(uint value) =>
        new(
            false,
            HasEvenParity((byte)value),
            value == 0,
            (value & 0x80000000) != 0,
            false);

    private static bool HasEvenParity(byte value) =>
        (System.Numerics.BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeConditionalFieldStorePlan BuildPlan(
        CapacityFinalCalleeConditionalFieldStoreCapture capture,
        CapacityFinalCalleeConditionalFieldStoreOutcome outcome,
        bool entryEcxCaptured = false,
        bool entryEbxCaptured = false,
        uint? firstCompareResult = null,
        CapacityFinalCalleeConditionalFieldStoreCompareFlags? firstCompareFlags = null,
        bool? entryEcxUnsignedBelowEntryEbx = null,
        bool firstBranchReached = false,
        bool? firstBranchTaken = null,
        uint? firstBranchSelectedDestinationRawFileOffset = null,
        uint? freshEcxPlus10Address = null,
        bool freshEcxPlus10ReadReached = false,
        bool freshEcxPlus10Captured = false,
        uint? freshEcxPlus10 = null,
        bool entryEsiCaptured = false,
        uint? secondCompareResult = null,
        CapacityFinalCalleeConditionalFieldStoreCompareFlags? secondCompareFlags = null,
        bool? freshEcxPlus10EqualEntryEsi = null,
        bool secondBranchReached = false,
        bool? secondBranchTaken = null,
        uint? secondBranchSelectedDestinationRawFileOffset = null,
        bool entryEaxCaptured = false,
        bool storeReached = false,
        uint? storeAddress = null,
        uint? storeValue = null,
        uint? storeDisplacement = null,
        bool jumpToTestReached = false,
        uint? jumpToTestDestinationRawFileOffset = null,
        bool testEaxReached = false,
        uint? testEaxValue = null,
        CapacityFinalCalleeConditionalFieldStoreTestFlags? testEaxFlags = null,
        bool zeroBranchReached = false,
        bool? zeroBranchTaken = null,
        uint? zeroBranchSelectedDestinationRawFileOffset = null,
        bool unreachedEntryEBXCaptureProvided = false,
        bool unreachedFreshEcxPlus10CaptureProvided = false,
        bool unreachedEntryESICaptureProvided = false,
        bool unreachedEntryEAXCaptureProvided = false,
        uint stopAt = BoundaryRawFileOffsetExclusive) =>
        new(
            capture,
            outcome,
            entryEcxCaptured,
            entryEbxCaptured,
            firstCompareResult,
            firstCompareFlags,
            entryEcxUnsignedBelowEntryEbx,
            firstBranchReached,
            firstBranchTaken,
            firstBranchSelectedDestinationRawFileOffset,
            freshEcxPlus10Address,
            freshEcxPlus10ReadReached,
            freshEcxPlus10Captured,
            freshEcxPlus10,
            entryEsiCaptured,
            secondCompareResult,
            secondCompareFlags,
            freshEcxPlus10EqualEntryEsi,
            secondBranchReached,
            secondBranchTaken,
            secondBranchSelectedDestinationRawFileOffset,
            entryEaxCaptured,
            storeReached,
            storeAddress,
            storeValue,
            storeDisplacement,
            jumpToTestReached,
            jumpToTestDestinationRawFileOffset,
            testEaxReached,
            testEaxValue,
            testEaxFlags,
            zeroBranchReached,
            zeroBranchTaken,
            zeroBranchSelectedDestinationRawFileOffset,
            unreachedEntryEBXCaptureProvided,
            unreachedFreshEcxPlus10CaptureProvided,
            unreachedEntryESICaptureProvided,
            unreachedEntryEAXCaptureProvided,
            stopAt);

    private static CapacityFinalCalleeConditionalFieldStoreInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes,
        uint? branchTargetRawFileOffset = null) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes,
            branchTargetRawFileOffset,
            branchTargetRawFileOffset is uint targetRaw
                ? RawFileOffsetToVirtualAddress(targetRaw)
                : null);
}
