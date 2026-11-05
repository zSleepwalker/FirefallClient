using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeTagValidationCapture(
    bool CaptureValidatedEntry,
    uint EntryESI,
    uint EntryEDX,
    uint? EdxPlusFourDword,
    uint? GlobalDword);

public enum CapacityFinalCalleeTagValidationOutcome
{
    EntryNotValidated,
    MissingEdxPlusFourDword,
    EntryEsiUnsignedGreaterOrEqualEntryEdx,
    ClBitZeroClear,
    ClBitOneSet,
    MissingGlobalDword,
    EntryEdxNotEqualGlobalDword,
    EntryEdxEqualsGlobalDword
}

public enum CapacityFinalCalleeTagValidationReadKind
{
    EdxPlusFourDword,
    GlobalDword
}

public readonly record struct CapacityFinalCalleeTagValidationReadDescriptor(
    uint RawFileOffset,
    CapacityFinalCalleeTagValidationReadKind Kind,
    uint? PeVirtualAddress,
    string? BaseRegister,
    uint Displacement);

public readonly record struct CapacityFinalCalleeTagValidationRead(
    uint RawFileOffset,
    CapacityFinalCalleeTagValidationReadKind Kind,
    uint Address,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeTagValidationFlags(
    bool Carry,
    bool Parity,
    bool? AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeTagValidationInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? PeVirtualAddressOperand = null,
    byte? ImmediateValue = null,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeTagValidationPlan(
    CapacityFinalCalleeTagValidationCapture Capture,
    CapacityFinalCalleeTagValidationOutcome Outcome,
    ImmutableArray<CapacityFinalCalleeTagValidationRead> Reads,
    bool CompareEsiEdxReached,
    CapacityFinalCalleeTagValidationFlags? CompareEsiEdxFlags,
    bool UnsignedBelowBranchReached,
    bool? EntryEsiUnsignedBelowEntryEdx,
    bool TestClOneReached,
    CapacityFinalCalleeTagValidationFlags? TestClOneFlags,
    bool? ClBitZeroSet,
    bool ClZeroBranchReached,
    bool? ClZeroBranchTaken,
    bool TestClTwoReached,
    CapacityFinalCalleeTagValidationFlags? TestClTwoFlags,
    bool? ClBitOneSet,
    bool ClOneBranchReached,
    bool? ClOneBranchTaken,
    bool EdxPlusFourReadReached,
    uint? EdxPlusFourAddress,
    bool EdxPlusFourDwordCaptured,
    uint? EdxPlusFourDword,
    uint? EcxAfterLoad,
    byte? ClAfterLoad,
    bool GlobalReadReached,
    bool GlobalDwordCaptured,
    uint? GlobalDword,
    bool CompareEdxGlobalReached,
    CapacityFinalCalleeTagValidationFlags? CompareEdxGlobalFlags,
    bool NotEqualBranchReached,
    bool? EntryEdxEqualsGlobalDword,
    bool? NotEqualBranchTaken,
    uint? SelectedDestinationRawFileOffset,
    uint? SelectedDestinationVirtualAddress,
    bool UnreachedEdxPlusFourDwordCaptureProvided,
    bool UnreachedGlobalDwordCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the bounded final-callee tag-validation window from independently validated ESI/EDX
/// entry values and the memory-read captures reached by its control flow.
/// </summary>
public static class CapacityFinalCalleeTagValidationProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C8EF8;
    public const uint EntryVirtualAddress = 0x016C9AF8;
    public const uint CompareEsiEdxRawFileOffset = 0x012C8EF8;
    public const uint UnsignedBelowBranchRawFileOffset = 0x012C8EFA;
    public const uint MovEcxFromEdxPlusFourRawFileOffset = 0x012C8F00;
    public const uint TestClOneRawFileOffset = 0x012C8F03;
    public const uint ClZeroBranchRawFileOffset = 0x012C8F06;
    public const uint TestClTwoRawFileOffset = 0x012C8F0C;
    public const uint ClOneBranchRawFileOffset = 0x012C8F0F;
    public const uint CompareEdxGlobalRawFileOffset = 0x012C8F15;
    public const uint NotEqualBranchRawFileOffset = 0x012C8F1B;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C8F21;
    public const uint BoundaryVirtualAddressExclusive = 0x016C9B21;

    public const uint EdxDwordDisplacement = 4;
    public const uint GlobalDwordVirtualAddress = 0x01EDADE0;
    public const uint UnsignedBelowDestinationRawFileOffset = 0x012C8E76;
    public const uint UnsignedBelowDestinationVirtualAddress = 0x016C9A76;
    public const uint ClZeroDestinationRawFileOffset = 0x012C8E76;
    public const uint ClZeroDestinationVirtualAddress = 0x016C9A76;
    public const uint ClOneDestinationRawFileOffset = 0x012C92DB;
    public const uint ClOneDestinationVirtualAddress = 0x016C9EDB;
    public const uint NotEqualDestinationRawFileOffset = 0x012C90CA;
    public const uint NotEqualDestinationVirtualAddress = 0x016C9CCA;
    public const uint EqualFallthroughRawFileOffset = BoundaryRawFileOffsetExclusive;
    public const uint EqualFallthroughVirtualAddress = BoundaryVirtualAddressExclusive;

    public const string CapturedWindowBytes =
        "3B F2 0F 83 76 FF FF FF 8B 4A 04 F6 C1 01 0F 84 6A FF FF FF " +
        "F6 C1 02 0F 85 C6 03 00 00 3B 15 E0 AD ED 01 0F 85 A9 01 00 00";

    public static ImmutableArray<CapacityFinalCalleeTagValidationReadDescriptor>
        OrderedReadDescriptors { get; } =
        ImmutableArray.Create(
            new CapacityFinalCalleeTagValidationReadDescriptor(
                MovEcxFromEdxPlusFourRawFileOffset,
                CapacityFinalCalleeTagValidationReadKind.EdxPlusFourDword,
                null,
                "EDX",
                EdxDwordDisplacement),
            new CapacityFinalCalleeTagValidationReadDescriptor(
                CompareEdxGlobalRawFileOffset,
                CapacityFinalCalleeTagValidationReadKind.GlobalDword,
                GlobalDwordVirtualAddress,
                null,
                0));

    public static ImmutableArray<CapacityFinalCalleeTagValidationInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Compare entry ESI with entry EDX",
                CompareEsiEdxRawFileOffset, "3B F2"),
            Instruction("JNB to 12C8E76 when unsigned ESI is at least EDX",
                UnsignedBelowBranchRawFileOffset, "0F 83 76 FF FF FF",
                branchTargetRawFileOffset: UnsignedBelowDestinationRawFileOffset,
                branchTargetVirtualAddress: UnsignedBelowDestinationVirtualAddress),
            Instruction("Read the dword at [EDX+4] into ECX on the fallthrough",
                MovEcxFromEdxPlusFourRawFileOffset, "8B 4A 04"),
            Instruction("TEST CL,1",
                TestClOneRawFileOffset, "F6 C1 01", immediateValue: 1),
            Instruction("JZ to 12C8E76 when CL bit 0 is clear",
                ClZeroBranchRawFileOffset, "0F 84 6A FF FF FF",
                branchTargetRawFileOffset: ClZeroDestinationRawFileOffset,
                branchTargetVirtualAddress: ClZeroDestinationVirtualAddress),
            Instruction("TEST CL,2",
                TestClTwoRawFileOffset, "F6 C1 02", immediateValue: 2),
            Instruction("JNZ to 12C92DB when CL bit 1 is set",
                ClOneBranchRawFileOffset, "0F 85 C6 03 00 00",
                branchTargetRawFileOffset: ClOneDestinationRawFileOffset,
                branchTargetVirtualAddress: ClOneDestinationVirtualAddress),
            Instruction("Compare entry EDX with the dword at PE VA 1EDADE0",
                CompareEdxGlobalRawFileOffset, "3B 15 E0 AD ED 01",
                peVirtualAddressOperand: GlobalDwordVirtualAddress),
            Instruction("JNZ to 12C90CA when EDX differs from the global dword",
                NotEqualBranchRawFileOffset, "0F 85 A9 01 00 00",
                branchTargetRawFileOffset: NotEqualDestinationRawFileOffset,
                branchTargetVirtualAddress: NotEqualDestinationVirtualAddress));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint EdxPlusFourAddress(uint entryEdx) =>
        unchecked(entryEdx + EdxDwordDisplacement);

    public static CapacityFinalCalleeTagValidationPlan Project(
        CapacityFinalCalleeTagValidationCapture capture)
    {
        if (!capture.CaptureValidatedEntry)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeTagValidationOutcome.EntryNotValidated,
                unreachedEdxPlusFourDwordCaptureProvided: capture.EdxPlusFourDword.HasValue,
                unreachedGlobalDwordCaptureProvided: capture.GlobalDword.HasValue,
                stopAt: EntryRawFileOffset);
        }

        var reads = ImmutableArray.CreateBuilder<CapacityFinalCalleeTagValidationRead>();
        CapacityFinalCalleeTagValidationFlags compareEsiEdxFlags =
            SubtractFlags(capture.EntryESI, capture.EntryEDX);
        bool entryEsiUnsignedBelowEntryEdx = compareEsiEdxFlags.Carry;
        if (!entryEsiUnsignedBelowEntryEdx)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeTagValidationOutcome.EntryEsiUnsignedGreaterOrEqualEntryEdx,
                reads,
                compareEsiEdxReached: true,
                compareEsiEdxFlags: compareEsiEdxFlags,
                unsignedBelowBranchReached: true,
                entryEsiUnsignedBelowEntryEdx: false,
                selectedDestinationRawFileOffset: UnsignedBelowDestinationRawFileOffset,
                selectedDestinationVirtualAddress: UnsignedBelowDestinationVirtualAddress,
                unreachedEdxPlusFourDwordCaptureProvided: capture.EdxPlusFourDword.HasValue,
                unreachedGlobalDwordCaptureProvided: capture.GlobalDword.HasValue,
                stopAt: UnsignedBelowDestinationRawFileOffset);
        }

        uint edxPlusFourAddress = EdxPlusFourAddress(capture.EntryEDX);
        reads.Add(new(
            MovEcxFromEdxPlusFourRawFileOffset,
            CapacityFinalCalleeTagValidationReadKind.EdxPlusFourDword,
            edxPlusFourAddress,
            capture.EdxPlusFourDword.HasValue,
            capture.EdxPlusFourDword));
        if (capture.EdxPlusFourDword is not uint edxPlusFourDword)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeTagValidationOutcome.MissingEdxPlusFourDword,
                reads,
                compareEsiEdxReached: true,
                compareEsiEdxFlags: compareEsiEdxFlags,
                unsignedBelowBranchReached: true,
                entryEsiUnsignedBelowEntryEdx: true,
                edxPlusFourReadReached: true,
                edxPlusFourAddress: edxPlusFourAddress,
                unreachedGlobalDwordCaptureProvided: capture.GlobalDword.HasValue,
                stopAt: MovEcxFromEdxPlusFourRawFileOffset);
        }

        byte cl = (byte)edxPlusFourDword;
        CapacityFinalCalleeTagValidationFlags testClOneFlags = TestFlags((byte)(cl & 1));
        bool clBitZeroSet = (cl & 1) != 0;
        if (!clBitZeroSet)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeTagValidationOutcome.ClBitZeroClear,
                reads,
                compareEsiEdxReached: true,
                compareEsiEdxFlags: compareEsiEdxFlags,
                unsignedBelowBranchReached: true,
                entryEsiUnsignedBelowEntryEdx: true,
                testClOneReached: true,
                testClOneFlags: testClOneFlags,
                clBitZeroSet: false,
                clZeroBranchReached: true,
                clZeroBranchTaken: true,
                edxPlusFourReadReached: true,
                edxPlusFourAddress: edxPlusFourAddress,
                edxPlusFourDwordCaptured: true,
                edxPlusFourDword: edxPlusFourDword,
                ecxAfterLoad: edxPlusFourDword,
                clAfterLoad: cl,
                selectedDestinationRawFileOffset: ClZeroDestinationRawFileOffset,
                selectedDestinationVirtualAddress: ClZeroDestinationVirtualAddress,
                unreachedGlobalDwordCaptureProvided: capture.GlobalDword.HasValue,
                stopAt: ClZeroDestinationRawFileOffset);
        }

        CapacityFinalCalleeTagValidationFlags testClTwoFlags = TestFlags((byte)(cl & 2));
        bool clBitOneSet = (cl & 2) != 0;
        if (clBitOneSet)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeTagValidationOutcome.ClBitOneSet,
                reads,
                compareEsiEdxReached: true,
                compareEsiEdxFlags: compareEsiEdxFlags,
                unsignedBelowBranchReached: true,
                entryEsiUnsignedBelowEntryEdx: true,
                testClOneReached: true,
                testClOneFlags: testClOneFlags,
                clBitZeroSet: true,
                clZeroBranchReached: true,
                clZeroBranchTaken: false,
                testClTwoReached: true,
                testClTwoFlags: testClTwoFlags,
                clBitOneSet: true,
                clOneBranchReached: true,
                clOneBranchTaken: true,
                edxPlusFourReadReached: true,
                edxPlusFourAddress: edxPlusFourAddress,
                edxPlusFourDwordCaptured: true,
                edxPlusFourDword: edxPlusFourDword,
                ecxAfterLoad: edxPlusFourDword,
                clAfterLoad: cl,
                selectedDestinationRawFileOffset: ClOneDestinationRawFileOffset,
                selectedDestinationVirtualAddress: ClOneDestinationVirtualAddress,
                unreachedGlobalDwordCaptureProvided: capture.GlobalDword.HasValue,
                stopAt: ClOneDestinationRawFileOffset);
        }

        reads.Add(new(
            CompareEdxGlobalRawFileOffset,
            CapacityFinalCalleeTagValidationReadKind.GlobalDword,
            GlobalDwordVirtualAddress,
            capture.GlobalDword.HasValue,
            capture.GlobalDword));
        if (capture.GlobalDword is not uint globalDword)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeTagValidationOutcome.MissingGlobalDword,
                reads,
                compareEsiEdxReached: true,
                compareEsiEdxFlags: compareEsiEdxFlags,
                unsignedBelowBranchReached: true,
                entryEsiUnsignedBelowEntryEdx: true,
                testClOneReached: true,
                testClOneFlags: testClOneFlags,
                clBitZeroSet: true,
                clZeroBranchReached: true,
                clZeroBranchTaken: false,
                testClTwoReached: true,
                testClTwoFlags: testClTwoFlags,
                clBitOneSet: false,
                clOneBranchReached: true,
                clOneBranchTaken: false,
                edxPlusFourReadReached: true,
                edxPlusFourAddress: edxPlusFourAddress,
                edxPlusFourDwordCaptured: true,
                edxPlusFourDword: edxPlusFourDword,
                ecxAfterLoad: edxPlusFourDword,
                clAfterLoad: cl,
                globalReadReached: true,
                stopAt: CompareEdxGlobalRawFileOffset);
        }

        CapacityFinalCalleeTagValidationFlags compareEdxGlobalFlags =
            SubtractFlags(capture.EntryEDX, globalDword);
        bool entryEdxEqualsGlobalDword = compareEdxGlobalFlags.Zero;
        uint destinationRawFileOffset = entryEdxEqualsGlobalDword
            ? EqualFallthroughRawFileOffset
            : NotEqualDestinationRawFileOffset;
        uint destinationVirtualAddress = entryEdxEqualsGlobalDword
            ? EqualFallthroughVirtualAddress
            : NotEqualDestinationVirtualAddress;

        return BuildPlan(
            capture,
            entryEdxEqualsGlobalDword
                ? CapacityFinalCalleeTagValidationOutcome.EntryEdxEqualsGlobalDword
                : CapacityFinalCalleeTagValidationOutcome.EntryEdxNotEqualGlobalDword,
            reads,
            compareEsiEdxReached: true,
            compareEsiEdxFlags: compareEsiEdxFlags,
            unsignedBelowBranchReached: true,
            entryEsiUnsignedBelowEntryEdx: true,
            testClOneReached: true,
            testClOneFlags: testClOneFlags,
            clBitZeroSet: true,
            clZeroBranchReached: true,
            clZeroBranchTaken: false,
            testClTwoReached: true,
            testClTwoFlags: testClTwoFlags,
            clBitOneSet: false,
            clOneBranchReached: true,
            clOneBranchTaken: false,
            edxPlusFourReadReached: true,
            edxPlusFourAddress: edxPlusFourAddress,
            edxPlusFourDwordCaptured: true,
            edxPlusFourDword: edxPlusFourDword,
            ecxAfterLoad: edxPlusFourDword,
            clAfterLoad: cl,
            globalReadReached: true,
            compareEdxGlobalReached: true,
            compareEdxGlobalFlags: compareEdxGlobalFlags,
            notEqualBranchReached: true,
            entryEdxEqualsGlobalDword: entryEdxEqualsGlobalDword,
            notEqualBranchTaken: !entryEdxEqualsGlobalDword,
            selectedDestinationRawFileOffset: destinationRawFileOffset,
            selectedDestinationVirtualAddress: destinationVirtualAddress,
            stopAt: destinationRawFileOffset);
    }

    private static CapacityFinalCalleeTagValidationFlags SubtractFlags(
        uint left,
        uint right)
    {
        uint result = unchecked(left - right);
        return new(
            left < right,
            HasEvenParity((byte)result),
            ((left ^ right ^ result) & 0x10) != 0,
            result == 0,
            (result & 0x80000000) != 0,
            ((left ^ right) & (left ^ result) & 0x80000000) != 0);
    }

    private static CapacityFinalCalleeTagValidationFlags TestFlags(byte result) =>
        new(
            false,
            HasEvenParity(result),
            null,
            result == 0,
            (result & 0x80) != 0,
            false);

    private static bool HasEvenParity(byte value) =>
        BitOperations.PopCount((uint)value) % 2 == 0;

    private static CapacityFinalCalleeTagValidationInstructionEvidence Instruction(
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

    private static CapacityFinalCalleeTagValidationPlan BuildPlan(
        CapacityFinalCalleeTagValidationCapture capture,
        CapacityFinalCalleeTagValidationOutcome outcome,
        ImmutableArray<CapacityFinalCalleeTagValidationRead>.Builder? reads = null,
        bool compareEsiEdxReached = false,
        CapacityFinalCalleeTagValidationFlags? compareEsiEdxFlags = null,
        bool unsignedBelowBranchReached = false,
        bool? entryEsiUnsignedBelowEntryEdx = null,
        bool testClOneReached = false,
        CapacityFinalCalleeTagValidationFlags? testClOneFlags = null,
        bool? clBitZeroSet = null,
        bool clZeroBranchReached = false,
        bool? clZeroBranchTaken = null,
        bool testClTwoReached = false,
        CapacityFinalCalleeTagValidationFlags? testClTwoFlags = null,
        bool? clBitOneSet = null,
        bool clOneBranchReached = false,
        bool? clOneBranchTaken = null,
        bool edxPlusFourReadReached = false,
        uint? edxPlusFourAddress = null,
        bool edxPlusFourDwordCaptured = false,
        uint? edxPlusFourDword = null,
        uint? ecxAfterLoad = null,
        byte? clAfterLoad = null,
        bool globalReadReached = false,
        bool compareEdxGlobalReached = false,
        CapacityFinalCalleeTagValidationFlags? compareEdxGlobalFlags = null,
        bool notEqualBranchReached = false,
        bool? entryEdxEqualsGlobalDword = null,
        bool? notEqualBranchTaken = null,
        uint? selectedDestinationRawFileOffset = null,
        uint? selectedDestinationVirtualAddress = null,
        bool unreachedEdxPlusFourDwordCaptureProvided = false,
        bool unreachedGlobalDwordCaptureProvided = false,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            reads?.ToImmutable() ?? ImmutableArray<CapacityFinalCalleeTagValidationRead>.Empty,
            compareEsiEdxReached,
            compareEsiEdxReached ? compareEsiEdxFlags : null,
            unsignedBelowBranchReached,
            unsignedBelowBranchReached ? entryEsiUnsignedBelowEntryEdx : null,
            testClOneReached,
            testClOneReached ? testClOneFlags : null,
            testClOneReached ? clBitZeroSet : null,
            clZeroBranchReached,
            clZeroBranchReached ? clZeroBranchTaken : null,
            testClTwoReached,
            testClTwoReached ? testClTwoFlags : null,
            testClTwoReached ? clBitOneSet : null,
            clOneBranchReached,
            clOneBranchReached ? clOneBranchTaken : null,
            edxPlusFourReadReached,
            edxPlusFourReadReached ? edxPlusFourAddress : null,
            edxPlusFourReadReached && edxPlusFourDwordCaptured
                && capture.EdxPlusFourDword.HasValue,
            edxPlusFourReadReached && edxPlusFourDwordCaptured
                && capture.EdxPlusFourDword.HasValue ? edxPlusFourDword : null,
            ecxAfterLoad,
            clAfterLoad,
            globalReadReached,
            globalReadReached && capture.GlobalDword.HasValue,
            globalReadReached ? capture.GlobalDword : null,
            compareEdxGlobalReached,
            compareEdxGlobalReached ? compareEdxGlobalFlags : null,
            notEqualBranchReached,
            notEqualBranchReached ? entryEdxEqualsGlobalDword : null,
            notEqualBranchReached ? notEqualBranchTaken : null,
            selectedDestinationRawFileOffset,
            selectedDestinationVirtualAddress,
            (!edxPlusFourReadReached && capture.EdxPlusFourDword.HasValue)
                || unreachedEdxPlusFourDwordCaptureProvided,
            (!globalReadReached && capture.GlobalDword.HasValue)
                || unreachedGlobalDwordCaptureProvided,
            stopAt);
}
