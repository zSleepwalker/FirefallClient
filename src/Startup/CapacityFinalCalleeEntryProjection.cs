using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeEntryCapture(
    bool EntryValidated,
    uint EntryEsp,
    uint EntryEbp,
    uint EntryEcx,
    uint EntryEbx,
    uint EntryEsi,
    uint EntryEdi,
    byte? TestedByte,
    uint? ZeroPathPoppedEbpValue);

public enum CapacityFinalCalleeEntryOutcome
{
    EntryNotValidated,
    MissingTestedByte,
    NonzeroPrefixCompleted,
    MissingZeroPathPoppedEbpValue,
    ZeroPathReturned
}

public enum CapacityFinalCalleeEntryStackStoreKind
{
    SavedEbp,
    SavedEbx,
    SavedEsi,
    SavedEdi
}

public readonly record struct CapacityFinalCalleeEntryStackStore(
    uint RawFileOffset,
    CapacityFinalCalleeEntryStackStoreKind Kind,
    uint Address,
    uint Value);

public readonly record struct CapacityFinalCalleeEntryInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? PeVirtualAddressOperand = null,
    byte? ImmediateValue = null,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeEntryPlan(
    CapacityFinalCalleeEntryCapture Capture,
    CapacityFinalCalleeEntryOutcome Outcome,
    ImmutableArray<CapacityFinalCalleeEntryStackStore> StackStores,
    bool PushEbpReached,
    uint? PushEbpAddress,
    bool MovEbpEspReached,
    uint? FrameEbp,
    bool SubEspReached,
    uint? FrameEsp,
    uint? StackPointerAfterNonzeroSaves,
    bool TestEcxReached,
    bool? EntryEcxIsZero,
    bool BranchReached,
    bool? ZeroBranchTaken,
    uint? SelectedDestinationRawFileOffset,
    bool TestByteReadReached,
    bool TestedByteCaptured,
    byte? TestedByte,
    bool? TestedBitIsSet,
    bool PushEbxReached,
    bool PushEsiReached,
    bool PushEdiReached,
    bool MovEspFromEbpReached,
    uint? EpilogueEsp,
    bool ZeroPathPopEbpReached,
    uint? ZeroPathPoppedEbp,
    uint? ReturnAddressStackAddress,
    uint? EspAfterReturn,
    bool ReturnReached,
    bool UnreachedTestedByteCaptureProvided,
    bool UnreachedZeroPathPoppedEbpCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the observed callee-entry prefix and zero-path epilogue only; it makes no claims
/// about behavior after the nonzero prefix or about a whole-callee register contract.
/// </summary>
public static class CapacityFinalCalleeEntryProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C8D60;
    public const uint EntryVirtualAddress = 0x016C9960;
    public const uint NonzeroPrefixBoundaryRawFileOffsetExclusive = 0x012C8D78;
    public const uint NonzeroPrefixBoundaryVirtualAddressExclusive = 0x016C9978;
    public const uint ZeroBranchDestinationRawFileOffset = 0x012C9458;
    public const uint ZeroBranchDestinationVirtualAddress = 0x016CA058;
    public const uint ZeroReturnRawFileOffset = 0x012C945B;
    public const uint ZeroReturnBoundaryRawFileOffsetExclusive = 0x012C945C;
    public const uint ZeroReturnBoundaryVirtualAddressExclusive = 0x016CA05C;

    public const uint TestByteVirtualAddress = 0x01EDAF84;
    public const byte TestMask = 0x02;
    public const uint LocalAllocationSize = 0x30;
    public const uint StackSlotSize = 4;

    public const uint PushEbpRawFileOffset = 0x012C8D60;
    public const uint MovEbpEspRawFileOffset = 0x012C8D61;
    public const uint SubEspRawFileOffset = 0x012C8D63;
    public const uint TestEcxRawFileOffset = 0x012C8D66;
    public const uint ZeroBranchRawFileOffset = 0x012C8D68;
    public const uint TestByteRawFileOffset = 0x012C8D6E;
    public const uint PushEbxRawFileOffset = 0x012C8D75;
    public const uint PushEsiRawFileOffset = 0x012C8D76;
    public const uint PushEdiRawFileOffset = 0x012C8D77;
    public const uint ZeroMovEspFromEbpRawFileOffset = 0x012C9458;
    public const uint ZeroPopEbpRawFileOffset = 0x012C945A;

    public const string CapturedWindowBytes =
        "55 8B EC 83 EC 30 85 C9 0F 84 EA 06 00 00 F6 05 84 AF ED 01 02 53 56 57";
    public const string ZeroEpilogueBytes = "8B E5 5D C3";

    public static ImmutableArray<CapacityFinalCalleeEntryInstructionEvidence> Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Push entry EBP",
                PushEbpRawFileOffset, "55"),
            Instruction("Set EBP from ESP",
                MovEbpEspRawFileOffset, "8B EC"),
            Instruction("Allocate 0x30 bytes of locals",
                SubEspRawFileOffset, "83 EC 30",
                immediateValue: 0x30),
            Instruction("Test entry ECX against itself",
                TestEcxRawFileOffset, "85 C9"),
            Instruction("JZ to the zero-path shared epilogue",
                ZeroBranchRawFileOffset, "0F 84 EA 06 00 00",
                branchTargetRawFileOffset: ZeroBranchDestinationRawFileOffset,
                branchTargetVirtualAddress: ZeroBranchDestinationVirtualAddress),
            Instruction("Test bit 1 of the byte at PE VA 1EDAF84 on the nonzero path",
                TestByteRawFileOffset, "F6 05 84 AF ED 01 02",
                peVirtualAddressOperand: TestByteVirtualAddress,
                immediateValue: TestMask),
            Instruction("Push EBX",
                PushEbxRawFileOffset, "53"),
            Instruction("Push ESI",
                PushEsiRawFileOffset, "56"),
            Instruction("Push EDI",
                PushEdiRawFileOffset, "57"));

    public static ImmutableArray<CapacityFinalCalleeEntryInstructionEvidence>
        ZeroEpilogueInstructions { get; } =
        ImmutableArray.Create(
            Instruction("Set ESP from EBP on the zero path",
                ZeroMovEspFromEbpRawFileOffset, "8B E5"),
            Instruction("Pop EBP on the zero path",
                ZeroPopEbpRawFileOffset, "5D"),
            Instruction("Return on the zero path",
                ZeroReturnRawFileOffset, "C3"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint PushAddress(uint stackPointer) =>
        unchecked(stackPointer - StackSlotSize);

    public static uint LocalFrameEsp(uint entryEsp) =>
        unchecked(entryEsp - StackSlotSize - LocalAllocationSize);

    public static CapacityFinalCalleeEntryPlan Project(
        CapacityFinalCalleeEntryCapture capture)
    {
        if (!capture.EntryValidated)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeEntryOutcome.EntryNotValidated,
                unreachedTestedByteCaptureProvided: capture.TestedByte.HasValue,
                unreachedZeroPathPoppedEbpCaptureProvided:
                    capture.ZeroPathPoppedEbpValue.HasValue);
        }

        uint pushEbpAddress = PushAddress(capture.EntryEsp);
        uint frameEsp = LocalFrameEsp(capture.EntryEsp);
        var stores = ImmutableArray.CreateBuilder<CapacityFinalCalleeEntryStackStore>();
        stores.Add(new(
            PushEbpRawFileOffset,
            CapacityFinalCalleeEntryStackStoreKind.SavedEbp,
            pushEbpAddress,
            capture.EntryEbp));

        bool entryEcxIsZero = capture.EntryEcx == 0;
        if (entryEcxIsZero)
        {
            if (capture.ZeroPathPoppedEbpValue is not uint poppedEbp)
            {
                return BuildPlan(
                    capture,
                    CapacityFinalCalleeEntryOutcome.MissingZeroPathPoppedEbpValue,
                    stores,
                    pushEbpReached: true,
                    pushEbpAddress: pushEbpAddress,
                    movEbpEspReached: true,
                    frameEbp: pushEbpAddress,
                    subEspReached: true,
                    frameEsp: frameEsp,
                    testEcxReached: true,
                    entryEcxIsZero: true,
                    branchReached: true,
                    zeroBranchTaken: true,
                    selectedDestinationRawFileOffset: ZeroBranchDestinationRawFileOffset,
                    movEspFromEbpReached: true,
                    epilogueEsp: pushEbpAddress,
                    zeroPathPopEbpReached: true,
                    unreachedTestedByteCaptureProvided: capture.TestedByte.HasValue,
                    stopAt: ZeroPopEbpRawFileOffset);
            }

            uint returnStackAddress = unchecked(pushEbpAddress + StackSlotSize);
            return BuildPlan(
                capture,
                CapacityFinalCalleeEntryOutcome.ZeroPathReturned,
                stores,
                pushEbpReached: true,
                pushEbpAddress: pushEbpAddress,
                movEbpEspReached: true,
                frameEbp: pushEbpAddress,
                subEspReached: true,
                frameEsp: frameEsp,
                testEcxReached: true,
                entryEcxIsZero: true,
                branchReached: true,
                zeroBranchTaken: true,
                selectedDestinationRawFileOffset: ZeroBranchDestinationRawFileOffset,
                movEspFromEbpReached: true,
                epilogueEsp: pushEbpAddress,
                zeroPathPopEbpReached: true,
                zeroPathPoppedEbp: poppedEbp,
                returnAddressStackAddress: returnStackAddress,
                espAfterReturn: unchecked(returnStackAddress + StackSlotSize),
                returnReached: true,
                unreachedTestedByteCaptureProvided: capture.TestedByte.HasValue,
                stopAt: ZeroReturnBoundaryRawFileOffsetExclusive);
        }

        if (capture.TestedByte is not byte testedByte)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeEntryOutcome.MissingTestedByte,
                stores,
                pushEbpReached: true,
                pushEbpAddress: pushEbpAddress,
                movEbpEspReached: true,
                frameEbp: pushEbpAddress,
                subEspReached: true,
                frameEsp: frameEsp,
                testEcxReached: true,
                entryEcxIsZero: false,
                branchReached: true,
                zeroBranchTaken: false,
                testByteReadReached: true,
                unreachedZeroPathPoppedEbpCaptureProvided:
                    capture.ZeroPathPoppedEbpValue.HasValue,
                stopAt: TestByteRawFileOffset);
        }

        uint ebxAddress = PushAddress(frameEsp);
        stores.Add(new(
            PushEbxRawFileOffset,
            CapacityFinalCalleeEntryStackStoreKind.SavedEbx,
            ebxAddress,
            capture.EntryEbx));
        uint esiAddress = PushAddress(ebxAddress);
        stores.Add(new(
            PushEsiRawFileOffset,
            CapacityFinalCalleeEntryStackStoreKind.SavedEsi,
            esiAddress,
            capture.EntryEsi));
        uint ediAddress = PushAddress(esiAddress);
        stores.Add(new(
            PushEdiRawFileOffset,
            CapacityFinalCalleeEntryStackStoreKind.SavedEdi,
            ediAddress,
            capture.EntryEdi));

        return BuildPlan(
            capture,
            CapacityFinalCalleeEntryOutcome.NonzeroPrefixCompleted,
            stores,
            pushEbpReached: true,
            pushEbpAddress: pushEbpAddress,
            movEbpEspReached: true,
            frameEbp: pushEbpAddress,
            subEspReached: true,
            frameEsp: frameEsp,
            testEcxReached: true,
            entryEcxIsZero: false,
            branchReached: true,
            zeroBranchTaken: false,
            testByteReadReached: true,
            testedBitIsSet: (testedByte & TestMask) != 0,
            pushEbxReached: true,
            pushEsiReached: true,
            pushEdiReached: true,
            stackPointerAfterNonzeroSaves: ediAddress,
            unreachedZeroPathPoppedEbpCaptureProvided:
                capture.ZeroPathPoppedEbpValue.HasValue,
            stopAt: NonzeroPrefixBoundaryRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleeEntryInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes,
        uint? peVirtualAddressOperand = null,
        byte? immediateValue = null,
        uint? branchTargetRawFileOffset = null,
        uint? branchTargetVirtualAddress = null) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes,
            peVirtualAddressOperand,
            immediateValue,
            branchTargetRawFileOffset,
            branchTargetVirtualAddress);

    private static CapacityFinalCalleeEntryPlan BuildPlan(
        CapacityFinalCalleeEntryCapture capture,
        CapacityFinalCalleeEntryOutcome outcome,
        ImmutableArray<CapacityFinalCalleeEntryStackStore>.Builder? stores = null,
        bool pushEbpReached = false,
        uint? pushEbpAddress = null,
        bool movEbpEspReached = false,
        uint? frameEbp = null,
        bool subEspReached = false,
        uint? frameEsp = null,
        uint? stackPointerAfterNonzeroSaves = null,
        bool testEcxReached = false,
        bool? entryEcxIsZero = null,
        bool branchReached = false,
        bool? zeroBranchTaken = null,
        uint? selectedDestinationRawFileOffset = null,
        bool testByteReadReached = false,
        bool pushEbxReached = false,
        bool pushEsiReached = false,
        bool pushEdiReached = false,
        bool? testedBitIsSet = null,
        bool movEspFromEbpReached = false,
        uint? epilogueEsp = null,
        bool zeroPathPopEbpReached = false,
        uint? zeroPathPoppedEbp = null,
        uint? returnAddressStackAddress = null,
        uint? espAfterReturn = null,
        bool returnReached = false,
        bool unreachedTestedByteCaptureProvided = false,
        bool unreachedZeroPathPoppedEbpCaptureProvided = false,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            stores?.ToImmutable() ?? ImmutableArray<CapacityFinalCalleeEntryStackStore>.Empty,
            pushEbpReached,
            pushEbpAddress,
            movEbpEspReached,
            frameEbp,
            subEspReached,
            frameEsp,
            stackPointerAfterNonzeroSaves,
            testEcxReached,
            entryEcxIsZero,
            branchReached,
            zeroBranchTaken,
            selectedDestinationRawFileOffset,
            testByteReadReached,
            testByteReadReached && capture.TestedByte.HasValue,
            testByteReadReached ? capture.TestedByte : null,
            testedBitIsSet,
            pushEbxReached,
            pushEsiReached,
            pushEdiReached,
            movEspFromEbpReached,
            epilogueEsp,
            zeroPathPopEbpReached,
            zeroPathPoppedEbp,
            returnAddressStackAddress,
            espAfterReturn,
            returnReached,
            (!testByteReadReached && capture.TestedByte.HasValue)
                || unreachedTestedByteCaptureProvided,
            (!zeroPathPopEbpReached && capture.ZeroPathPoppedEbpValue.HasValue)
                || unreachedZeroPathPoppedEbpCaptureProvided,
            stopAt);
}
