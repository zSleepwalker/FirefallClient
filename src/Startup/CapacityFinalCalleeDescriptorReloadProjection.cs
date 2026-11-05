using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeDescriptorReloadCapture(
    bool CaptureValidatedEntry,
    uint EntryEBP,
    uint EntryEBX,
    uint EntryESI,
    uint? InitialFrameMinus10,
    uint? FreshFrameMinus4,
    uint? FreshFrameMinus10,
    uint? FreshEaxPlusC);

public enum CapacityFinalCalleeDescriptorReloadOutcome
{
    EntryNotValidated,
    MissingInitialFrameMinus10,
    MissingFreshFrameMinus4,
    MissingFreshFrameMinus10,
    FirstCompareEqualExit,
    EntryEaxUnsignedBelowExit,
    MissingFreshEaxPlusC,
    FreshEaxPlusCEqualExit,
    FellThroughToBoundary
}

public readonly record struct CapacityFinalCalleeDescriptorReloadFlags(
    bool Carry,
    bool Parity,
    bool AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeDescriptorReloadInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? PeVirtualAddressOperand = null,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeDescriptorReloadPlan(
    CapacityFinalCalleeDescriptorReloadCapture Capture,
    CapacityFinalCalleeDescriptorReloadOutcome Outcome,
    uint? InitialFrameMinus10Address,
    bool InitialFrameMinus10ReadReached,
    bool InitialFrameMinus10Captured,
    uint? InitialFrameMinus10,
    bool DescriptorAddressComputationReached,
    uint? ComputedDescriptorAddress,
    bool DescriptorAddressWriteReached,
    uint? WrittenFrameMinus10,
    uint? FreshFrameMinus4Address,
    bool FreshFrameMinus4ReadReached,
    bool FreshFrameMinus4Captured,
    uint? FreshFrameMinus4,
    uint? FreshFrameMinus10Address,
    bool FreshFrameMinus10ReadReached,
    bool FreshFrameMinus10Captured,
    uint? FreshFrameMinus10,
    bool FirstCompareReached,
    uint? FirstCompareResult,
    CapacityFinalCalleeDescriptorReloadFlags? FirstCompareFlags,
    bool? FirstCompareEqual,
    bool FirstBranchReached,
    bool? FirstBranchTaken,
    uint? FirstSelectedDestinationRawFileOffset,
    bool EntryEbxCompareReached,
    uint? EntryEbxCompareResult,
    CapacityFinalCalleeDescriptorReloadFlags? EntryEbxCompareFlags,
    bool? EntryEaxUnsignedBelowEntryEbx,
    bool SecondBranchReached,
    bool? SecondBranchTaken,
    uint? SecondSelectedDestinationRawFileOffset,
    uint? FreshEaxPlusCAddress,
    bool FreshEaxPlusCReadReached,
    bool FreshEaxPlusCCaptured,
    uint? FreshEaxPlusC,
    bool FinalCompareReached,
    uint? FinalCompareResult,
    CapacityFinalCalleeDescriptorReloadFlags? FinalCompareFlags,
    bool? FinalCompareEqual,
    bool FinalBranchReached,
    bool? FinalBranchTaken,
    uint? FinalSelectedDestinationRawFileOffset,
    bool UnreachedInitialFrameMinus10CaptureProvided,
    bool UnreachedFreshFrameMinus4CaptureProvided,
    bool UnreachedFreshFrameMinus10CaptureProvided,
    bool UnreachedFreshEaxPlusCCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the standalone descriptor-address write and subsequent fresh reloads.
/// The post-write [EBP-10] read is independent of the value just written there.
/// </summary>
public static class CapacityFinalCalleeDescriptorReloadProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C8E9A;
    public const uint EntryVirtualAddress = 0x016C9A9A;
    public const uint DescriptorAddressRawFileOffset = 0x012C8E9D;
    public const uint StoreRawFileOffset = 0x012C8EA4;
    public const uint FreshFrameMinus4LoadRawFileOffset = 0x012C8EA7;
    public const uint FirstCompareRawFileOffset = 0x012C8EAA;
    public const uint FirstBranchRawFileOffset = 0x012C8EAD;
    public const uint EntryEbxCompareRawFileOffset = 0x012C8EAF;
    public const uint SecondBranchRawFileOffset = 0x012C8EB1;
    public const uint EaxPlusCCompareRawFileOffset = 0x012C8EB3;
    public const uint FinalBranchRawFileOffset = 0x012C8EB6;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C8EB8;
    public const uint BelowDestinationRawFileOffset = 0x012C8EB8;
    public const uint EqualityDestinationRawFileOffset = 0x012C8EBE;
    public const uint BelowDestinationVirtualAddress = 0x016C9AB8;
    public const uint EqualityDestinationVirtualAddress = 0x016C9ABE;

    public const uint FrameMinus10Displacement = 0x10;
    public const uint FrameMinus4Displacement = 4;
    public const uint EaxPlusCDisplacement = 0xC;
    public const uint EncodedDescriptorBasePeVirtualAddress = 0x01EDADF0;
    public const int DescriptorIndexScaleShift = 3;

    public const string CapturedWindowBytes =
        "8B 4D F0 8D 04 CD F0 AD ED 01 89 45 F0 8B 45 FC 3B 45 F0 "
        + "74 0F 3B C3 72 05 39 70 0C 74 06";

    public static ImmutableArray<CapacityFinalCalleeDescriptorReloadInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Load the initial DWORD at entry EBP-10 into ECX",
                EntryRawFileOffset, "8B 4D F0"),
            Instruction("Form the descriptor address from PE VA 1EDADF0 plus ECX*8",
                DescriptorAddressRawFileOffset, "8D 04 CD F0 AD ED 01",
                peVirtualAddressOperand: EncodedDescriptorBasePeVirtualAddress),
            Instruction("Write the computed descriptor address to entry EBP-10",
                StoreRawFileOffset, "89 45 F0"),
            Instruction("Load a fresh DWORD at entry EBP-4 into EAX",
                FreshFrameMinus4LoadRawFileOffset, "8B 45 FC"),
            Instruction("Compare fresh EAX against an independent fresh DWORD at entry EBP-10",
                FirstCompareRawFileOffset, "3B 45 F0"),
            Instruction("JZ to the equality destination when the first comparison is equal",
                FirstBranchRawFileOffset, "74 0F",
                branchTargetRawFileOffset: EqualityDestinationRawFileOffset,
                branchTargetVirtualAddress: EqualityDestinationVirtualAddress),
            Instruction("Compare fresh EAX with entry EBX",
                EntryEbxCompareRawFileOffset, "3B C3"),
            Instruction("JB to the below destination when fresh EAX is unsigned-below entry EBX",
                SecondBranchRawFileOffset, "72 05",
                branchTargetRawFileOffset: BelowDestinationRawFileOffset,
                branchTargetVirtualAddress: BelowDestinationVirtualAddress),
            Instruction("Compare the fresh DWORD at EAX+0C with entry ESI",
                EaxPlusCCompareRawFileOffset, "39 70 0C"),
            Instruction("JZ to the equality destination when the final comparison is equal",
                FinalBranchRawFileOffset, "74 06",
                branchTargetRawFileOffset: EqualityDestinationRawFileOffset,
                branchTargetVirtualAddress: EqualityDestinationVirtualAddress));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFrameMinus10Address(uint entryEbp) =>
        unchecked(entryEbp - FrameMinus10Displacement);

    public static uint DeriveFrameMinus4Address(uint entryEbp) =>
        unchecked(entryEbp - FrameMinus4Displacement);

    public static uint DeriveDescriptorAddress(uint initialFrameMinus10) =>
        unchecked(EncodedDescriptorBasePeVirtualAddress
            + (initialFrameMinus10 << DescriptorIndexScaleShift));

    public static uint DeriveEaxPlusCAddress(uint freshFrameMinus4) =>
        unchecked(freshFrameMinus4 + EaxPlusCDisplacement);

    public static CapacityFinalCalleeDescriptorReloadPlan Project(
        CapacityFinalCalleeDescriptorReloadCapture capture)
    {
        if (!capture.CaptureValidatedEntry)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeDescriptorReloadOutcome.EntryNotValidated,
                unreachedInitialFrameMinus10CaptureProvided:
                    capture.InitialFrameMinus10.HasValue,
                unreachedFreshFrameMinus4CaptureProvided:
                    capture.FreshFrameMinus4.HasValue,
                unreachedFreshFrameMinus10CaptureProvided:
                    capture.FreshFrameMinus10.HasValue,
                unreachedFreshEaxPlusCCaptureProvided:
                    capture.FreshEaxPlusC.HasValue,
                stopAt: EntryRawFileOffset);
        }

        uint frameMinus10Address = DeriveFrameMinus10Address(capture.EntryEBP);
        if (capture.InitialFrameMinus10 is not uint initialFrameMinus10)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeDescriptorReloadOutcome.MissingInitialFrameMinus10,
                initialFrameMinus10Address: frameMinus10Address,
                initialFrameMinus10ReadReached: true,
                unreachedFreshFrameMinus4CaptureProvided:
                    capture.FreshFrameMinus4.HasValue,
                unreachedFreshFrameMinus10CaptureProvided:
                    capture.FreshFrameMinus10.HasValue,
                unreachedFreshEaxPlusCCaptureProvided:
                    capture.FreshEaxPlusC.HasValue,
                stopAt: EntryRawFileOffset);
        }

        uint computedDescriptorAddress = DeriveDescriptorAddress(initialFrameMinus10);
        if (capture.FreshFrameMinus4 is not uint freshFrameMinus4)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeDescriptorReloadOutcome.MissingFreshFrameMinus4,
                initialFrameMinus10Address: frameMinus10Address,
                initialFrameMinus10ReadReached: true,
                initialFrameMinus10Captured: true,
                initialFrameMinus10: initialFrameMinus10,
                descriptorAddressComputationReached: true,
                computedDescriptorAddress: computedDescriptorAddress,
                descriptorAddressWriteReached: true,
                writtenFrameMinus10: computedDescriptorAddress,
                freshFrameMinus4Address: DeriveFrameMinus4Address(capture.EntryEBP),
                freshFrameMinus4ReadReached: true,
                unreachedFreshFrameMinus10CaptureProvided:
                    capture.FreshFrameMinus10.HasValue,
                unreachedFreshEaxPlusCCaptureProvided:
                    capture.FreshEaxPlusC.HasValue,
                stopAt: FreshFrameMinus4LoadRawFileOffset);
        }

        uint freshFrameMinus4Address = DeriveFrameMinus4Address(capture.EntryEBP);
        uint reloadedFrameMinus10Address = frameMinus10Address;
        if (capture.FreshFrameMinus10 is not uint freshFrameMinus10)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeDescriptorReloadOutcome.MissingFreshFrameMinus10,
                initialFrameMinus10Address: frameMinus10Address,
                initialFrameMinus10ReadReached: true,
                initialFrameMinus10Captured: true,
                initialFrameMinus10: initialFrameMinus10,
                descriptorAddressComputationReached: true,
                computedDescriptorAddress: computedDescriptorAddress,
                descriptorAddressWriteReached: true,
                writtenFrameMinus10: computedDescriptorAddress,
                freshFrameMinus4Address: freshFrameMinus4Address,
                freshFrameMinus4ReadReached: true,
                freshFrameMinus4Captured: true,
                freshFrameMinus4: freshFrameMinus4,
                freshFrameMinus10Address: reloadedFrameMinus10Address,
                freshFrameMinus10ReadReached: true,
                unreachedFreshEaxPlusCCaptureProvided:
                    capture.FreshEaxPlusC.HasValue,
                stopAt: FirstCompareRawFileOffset);
        }

        uint firstCompareResult = unchecked(freshFrameMinus4 - freshFrameMinus10);
        CapacityFinalCalleeDescriptorReloadFlags firstCompareFlags =
            CalculateFlags(freshFrameMinus4, freshFrameMinus10, firstCompareResult);
        if (firstCompareFlags.Zero)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeDescriptorReloadOutcome.FirstCompareEqualExit,
                initialFrameMinus10Address: frameMinus10Address,
                initialFrameMinus10ReadReached: true,
                initialFrameMinus10Captured: true,
                initialFrameMinus10: initialFrameMinus10,
                descriptorAddressComputationReached: true,
                computedDescriptorAddress: computedDescriptorAddress,
                descriptorAddressWriteReached: true,
                writtenFrameMinus10: computedDescriptorAddress,
                freshFrameMinus4Address: freshFrameMinus4Address,
                freshFrameMinus4ReadReached: true,
                freshFrameMinus4Captured: true,
                freshFrameMinus4: freshFrameMinus4,
                freshFrameMinus10Address: reloadedFrameMinus10Address,
                freshFrameMinus10ReadReached: true,
                freshFrameMinus10Captured: true,
                freshFrameMinus10: freshFrameMinus10,
                firstCompareReached: true,
                firstCompareResult: firstCompareResult,
                firstCompareFlags: firstCompareFlags,
                firstCompareEqual: true,
                firstBranchReached: true,
                firstBranchTaken: true,
                firstSelectedDestinationRawFileOffset: EqualityDestinationRawFileOffset,
                unreachedFreshEaxPlusCCaptureProvided:
                    capture.FreshEaxPlusC.HasValue,
                stopAt: EqualityDestinationRawFileOffset);
        }

        uint entryEbxCompareResult = unchecked(freshFrameMinus4 - capture.EntryEBX);
        CapacityFinalCalleeDescriptorReloadFlags entryEbxCompareFlags =
            CalculateFlags(freshFrameMinus4, capture.EntryEBX, entryEbxCompareResult);
        if (entryEbxCompareFlags.Carry)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeDescriptorReloadOutcome.EntryEaxUnsignedBelowExit,
                initialFrameMinus10Address: frameMinus10Address,
                initialFrameMinus10ReadReached: true,
                initialFrameMinus10Captured: true,
                initialFrameMinus10: initialFrameMinus10,
                descriptorAddressComputationReached: true,
                computedDescriptorAddress: computedDescriptorAddress,
                descriptorAddressWriteReached: true,
                writtenFrameMinus10: computedDescriptorAddress,
                freshFrameMinus4Address: freshFrameMinus4Address,
                freshFrameMinus4ReadReached: true,
                freshFrameMinus4Captured: true,
                freshFrameMinus4: freshFrameMinus4,
                freshFrameMinus10Address: reloadedFrameMinus10Address,
                freshFrameMinus10ReadReached: true,
                freshFrameMinus10Captured: true,
                freshFrameMinus10: freshFrameMinus10,
                firstCompareReached: true,
                firstCompareResult: firstCompareResult,
                firstCompareFlags: firstCompareFlags,
                firstCompareEqual: false,
                firstBranchReached: true,
                firstBranchTaken: false,
                entryEbxCompareReached: true,
                entryEbxCompareResult: entryEbxCompareResult,
                entryEbxCompareFlags: entryEbxCompareFlags,
                entryEaxUnsignedBelowEntryEbx: true,
                secondBranchReached: true,
                secondBranchTaken: true,
                secondSelectedDestinationRawFileOffset: BelowDestinationRawFileOffset,
                unreachedFreshEaxPlusCCaptureProvided:
                    capture.FreshEaxPlusC.HasValue,
                stopAt: BelowDestinationRawFileOffset);
        }

        uint eaxPlusCAddress = DeriveEaxPlusCAddress(freshFrameMinus4);
        if (capture.FreshEaxPlusC is not uint freshEaxPlusC)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeDescriptorReloadOutcome.MissingFreshEaxPlusC,
                initialFrameMinus10Address: frameMinus10Address,
                initialFrameMinus10ReadReached: true,
                initialFrameMinus10Captured: true,
                initialFrameMinus10: initialFrameMinus10,
                descriptorAddressComputationReached: true,
                computedDescriptorAddress: computedDescriptorAddress,
                descriptorAddressWriteReached: true,
                writtenFrameMinus10: computedDescriptorAddress,
                freshFrameMinus4Address: freshFrameMinus4Address,
                freshFrameMinus4ReadReached: true,
                freshFrameMinus4Captured: true,
                freshFrameMinus4: freshFrameMinus4,
                freshFrameMinus10Address: reloadedFrameMinus10Address,
                freshFrameMinus10ReadReached: true,
                freshFrameMinus10Captured: true,
                freshFrameMinus10: freshFrameMinus10,
                firstCompareReached: true,
                firstCompareResult: firstCompareResult,
                firstCompareFlags: firstCompareFlags,
                firstCompareEqual: false,
                firstBranchReached: true,
                firstBranchTaken: false,
                entryEbxCompareReached: true,
                entryEbxCompareResult: entryEbxCompareResult,
                entryEbxCompareFlags: entryEbxCompareFlags,
                entryEaxUnsignedBelowEntryEbx: false,
                secondBranchReached: true,
                secondBranchTaken: false,
                freshEaxPlusCAddress: eaxPlusCAddress,
                freshEaxPlusCReadReached: true,
                stopAt: EaxPlusCCompareRawFileOffset);
        }

        uint finalCompareResult = unchecked(freshEaxPlusC - capture.EntryESI);
        CapacityFinalCalleeDescriptorReloadFlags finalCompareFlags =
            CalculateFlags(freshEaxPlusC, capture.EntryESI, finalCompareResult);
        bool finalEqual = finalCompareFlags.Zero;
        return BuildPlan(
            capture,
            finalEqual
                ? CapacityFinalCalleeDescriptorReloadOutcome.FreshEaxPlusCEqualExit
                : CapacityFinalCalleeDescriptorReloadOutcome.FellThroughToBoundary,
            initialFrameMinus10Address: frameMinus10Address,
            initialFrameMinus10ReadReached: true,
            initialFrameMinus10Captured: true,
            initialFrameMinus10: initialFrameMinus10,
            descriptorAddressComputationReached: true,
            computedDescriptorAddress: computedDescriptorAddress,
            descriptorAddressWriteReached: true,
            writtenFrameMinus10: computedDescriptorAddress,
            freshFrameMinus4Address: freshFrameMinus4Address,
            freshFrameMinus4ReadReached: true,
            freshFrameMinus4Captured: true,
            freshFrameMinus4: freshFrameMinus4,
            freshFrameMinus10Address: reloadedFrameMinus10Address,
            freshFrameMinus10ReadReached: true,
            freshFrameMinus10Captured: true,
            freshFrameMinus10: freshFrameMinus10,
            firstCompareReached: true,
            firstCompareResult: firstCompareResult,
            firstCompareFlags: firstCompareFlags,
            firstCompareEqual: false,
            firstBranchReached: true,
            firstBranchTaken: false,
            entryEbxCompareReached: true,
            entryEbxCompareResult: entryEbxCompareResult,
            entryEbxCompareFlags: entryEbxCompareFlags,
            entryEaxUnsignedBelowEntryEbx: false,
            secondBranchReached: true,
            secondBranchTaken: false,
            freshEaxPlusCAddress: eaxPlusCAddress,
            freshEaxPlusCReadReached: true,
            freshEaxPlusCCaptured: true,
            freshEaxPlusC: freshEaxPlusC,
            finalCompareReached: true,
            finalCompareResult: finalCompareResult,
            finalCompareFlags: finalCompareFlags,
            finalCompareEqual: finalEqual,
            finalBranchReached: true,
            finalBranchTaken: finalEqual,
            finalSelectedDestinationRawFileOffset:
                finalEqual ? EqualityDestinationRawFileOffset : null,
            stopAt: finalEqual
                ? EqualityDestinationRawFileOffset
                : BoundaryRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleeDescriptorReloadFlags CalculateFlags(
        uint left,
        uint right,
        uint result) =>
        new(
            Carry: left < right,
            Parity: HasEvenParity((byte)result),
            AuxiliaryCarry: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x80000000) != 0);

    private static bool HasEvenParity(byte value)
    {
        value ^= (byte)(value >> 4);
        value &= 0x0F;
        return ((0x6996 >> value) & 1) == 0;
    }

    private static CapacityFinalCalleeDescriptorReloadInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes,
        uint? peVirtualAddressOperand = null,
        uint? branchTargetRawFileOffset = null,
        uint? branchTargetVirtualAddress = null) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes,
            peVirtualAddressOperand,
            branchTargetRawFileOffset,
            branchTargetVirtualAddress);

    private static CapacityFinalCalleeDescriptorReloadPlan BuildPlan(
        CapacityFinalCalleeDescriptorReloadCapture capture,
        CapacityFinalCalleeDescriptorReloadOutcome outcome,
        uint? initialFrameMinus10Address = null,
        bool initialFrameMinus10ReadReached = false,
        bool initialFrameMinus10Captured = false,
        uint? initialFrameMinus10 = null,
        bool descriptorAddressComputationReached = false,
        uint? computedDescriptorAddress = null,
        bool descriptorAddressWriteReached = false,
        uint? writtenFrameMinus10 = null,
        uint? freshFrameMinus4Address = null,
        bool freshFrameMinus4ReadReached = false,
        bool freshFrameMinus4Captured = false,
        uint? freshFrameMinus4 = null,
        uint? freshFrameMinus10Address = null,
        bool freshFrameMinus10ReadReached = false,
        bool freshFrameMinus10Captured = false,
        uint? freshFrameMinus10 = null,
        bool firstCompareReached = false,
        uint? firstCompareResult = null,
        CapacityFinalCalleeDescriptorReloadFlags? firstCompareFlags = null,
        bool? firstCompareEqual = null,
        bool firstBranchReached = false,
        bool? firstBranchTaken = null,
        uint? firstSelectedDestinationRawFileOffset = null,
        bool entryEbxCompareReached = false,
        uint? entryEbxCompareResult = null,
        CapacityFinalCalleeDescriptorReloadFlags? entryEbxCompareFlags = null,
        bool? entryEaxUnsignedBelowEntryEbx = null,
        bool secondBranchReached = false,
        bool? secondBranchTaken = null,
        uint? secondSelectedDestinationRawFileOffset = null,
        uint? freshEaxPlusCAddress = null,
        bool freshEaxPlusCReadReached = false,
        bool freshEaxPlusCCaptured = false,
        uint? freshEaxPlusC = null,
        bool finalCompareReached = false,
        uint? finalCompareResult = null,
        CapacityFinalCalleeDescriptorReloadFlags? finalCompareFlags = null,
        bool? finalCompareEqual = null,
        bool finalBranchReached = false,
        bool? finalBranchTaken = null,
        uint? finalSelectedDestinationRawFileOffset = null,
        bool unreachedInitialFrameMinus10CaptureProvided = false,
        bool unreachedFreshFrameMinus4CaptureProvided = false,
        bool unreachedFreshFrameMinus10CaptureProvided = false,
        bool unreachedFreshEaxPlusCCaptureProvided = false,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            initialFrameMinus10ReadReached ? initialFrameMinus10Address : null,
            initialFrameMinus10ReadReached,
            initialFrameMinus10Captured,
            initialFrameMinus10Captured ? initialFrameMinus10 : null,
            descriptorAddressComputationReached,
            descriptorAddressComputationReached ? computedDescriptorAddress : null,
            descriptorAddressWriteReached,
            descriptorAddressWriteReached ? writtenFrameMinus10 : null,
            freshFrameMinus4ReadReached ? freshFrameMinus4Address : null,
            freshFrameMinus4ReadReached,
            freshFrameMinus4Captured,
            freshFrameMinus4Captured ? freshFrameMinus4 : null,
            freshFrameMinus10ReadReached ? freshFrameMinus10Address : null,
            freshFrameMinus10ReadReached,
            freshFrameMinus10Captured,
            freshFrameMinus10Captured ? freshFrameMinus10 : null,
            firstCompareReached,
            firstCompareReached ? firstCompareResult : null,
            firstCompareReached ? firstCompareFlags : null,
            firstCompareReached ? firstCompareEqual : null,
            firstBranchReached,
            firstBranchReached ? firstBranchTaken : null,
            firstBranchReached && firstBranchTaken == true
                ? firstSelectedDestinationRawFileOffset
                : null,
            entryEbxCompareReached,
            entryEbxCompareReached ? entryEbxCompareResult : null,
            entryEbxCompareReached ? entryEbxCompareFlags : null,
            entryEbxCompareReached ? entryEaxUnsignedBelowEntryEbx : null,
            secondBranchReached,
            secondBranchReached ? secondBranchTaken : null,
            secondBranchReached && secondBranchTaken == true
                ? secondSelectedDestinationRawFileOffset
                : null,
            freshEaxPlusCReadReached ? freshEaxPlusCAddress : null,
            freshEaxPlusCReadReached,
            freshEaxPlusCCaptured,
            freshEaxPlusCCaptured ? freshEaxPlusC : null,
            finalCompareReached,
            finalCompareReached ? finalCompareResult : null,
            finalCompareReached ? finalCompareFlags : null,
            finalCompareReached ? finalCompareEqual : null,
            finalBranchReached,
            finalBranchReached ? finalBranchTaken : null,
            finalBranchReached && finalBranchTaken == true
                ? finalSelectedDestinationRawFileOffset
                : null,
            (!initialFrameMinus10ReadReached && capture.InitialFrameMinus10.HasValue)
                || unreachedInitialFrameMinus10CaptureProvided,
            (!freshFrameMinus4ReadReached && capture.FreshFrameMinus4.HasValue)
                || unreachedFreshFrameMinus4CaptureProvided,
            (!freshFrameMinus10ReadReached && capture.FreshFrameMinus10.HasValue)
                || unreachedFreshFrameMinus10CaptureProvided,
            (!freshEaxPlusCReadReached && capture.FreshEaxPlusC.HasValue)
                || unreachedFreshEaxPlusCCaptureProvided,
            stopAt);
}
