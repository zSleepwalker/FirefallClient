using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeFirstValidationCapture(
    bool CaptureValidatedEntry,
    uint EntryESI,
    uint? GlobalDword,
    uint? Esi4Dword);

public enum CapacityFinalCalleeFirstValidationOutcome
{
    EntryNotValidated,
    MissingGlobalDword,
    EntryEsiUnsignedBelowGlobal,
    MissingEsi4Dword,
    MaskEqualsOne,
    MaskNotEqualOne
}

public enum CapacityFinalCalleeFirstValidationReadKind
{
    GlobalDword,
    EsiPlusFourDword
}

public readonly record struct CapacityFinalCalleeFirstValidationReadDescriptor(
    uint RawFileOffset,
    CapacityFinalCalleeFirstValidationReadKind Kind,
    uint? PeVirtualAddress,
    string? BaseRegister,
    uint Displacement);

public readonly record struct CapacityFinalCalleeFirstValidationRead(
    uint RawFileOffset,
    CapacityFinalCalleeFirstValidationReadKind Kind,
    uint Address,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeFirstValidationFlags(
    bool Carry,
    bool Parity,
    bool? AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeFirstValidationInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? PeVirtualAddressOperand = null,
    byte? ImmediateValue = null,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeFirstValidationPlan(
    CapacityFinalCalleeFirstValidationCapture Capture,
    CapacityFinalCalleeFirstValidationOutcome Outcome,
    ImmutableArray<CapacityFinalCalleeFirstValidationRead> Reads,
    bool GlobalReadReached,
    bool GlobalDwordCaptured,
    uint? GlobalDword,
    bool CompareEsiEbxReached,
    CapacityFinalCalleeFirstValidationFlags? CompareEsiEbxFlags,
    bool UnsignedBelowBranchReached,
    bool? EntryEsiUnsignedBelowGlobal,
    uint? FirstSelectedDestinationRawFileOffset,
    uint? FirstSelectedDestinationVirtualAddress,
    bool Esi4ReadReached,
    uint? Esi4Address,
    bool Esi4DwordCaptured,
    uint? Esi4Dword,
    bool MovEcxFromEaxReached,
    uint? EcxAfterMove,
    bool AndEcxThreeReached,
    uint? EcxAfterMask,
    CapacityFinalCalleeFirstValidationFlags? AndEcxFlags,
    bool CompareEcxOneReached,
    CapacityFinalCalleeFirstValidationFlags? CompareEcxOneFlags,
    bool EqualOneBranchReached,
    bool? EqualOneBranchTaken,
    uint? FinalSelectedDestinationRawFileOffset,
    uint? FinalSelectedDestinationVirtualAddress,
    uint? EaxAfterLoad,
    uint? EaxAfterValidation,
    bool UnreachedGlobalDwordCaptureProvided,
    bool UnreachedEsi4DwordCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the bounded final-callee validation window after the opaque call. Entry ESI is an
/// independent capture; this projection does not infer pointer ownership or interpretation.
/// </summary>
public static class CapacityFinalCalleeFirstValidationProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C8D88;
    public const uint EntryVirtualAddress = 0x016C9988;
    public const uint CompareEsiEbxRawFileOffset = 0x012C8D8E;
    public const uint UnsignedBelowBranchRawFileOffset = 0x012C8D90;
    public const uint MovEaxFromEsiPlusFourRawFileOffset = 0x012C8D96;
    public const uint MovEcxFromEaxRawFileOffset = 0x012C8D99;
    public const uint AndEcxThreeRawFileOffset = 0x012C8D9B;
    public const uint CompareEcxOneRawFileOffset = 0x012C8D9E;
    public const uint EqualOneBranchRawFileOffset = 0x012C8DA1;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C8DA7;
    public const uint BoundaryVirtualAddressExclusive = 0x016C99A7;

    public const uint GlobalDwordVirtualAddress = 0x01EDADD8;
    public const uint EsiDwordDisplacement = 4;
    public const uint EsiUnsignedBelowDestinationRawFileOffset = 0x012C8E76;
    public const uint EsiUnsignedBelowDestinationVirtualAddress = 0x016C9A76;
    public const uint EsiUnsignedFallthroughRawFileOffset = MovEaxFromEsiPlusFourRawFileOffset;
    public const uint EsiUnsignedFallthroughVirtualAddress = 0x016C9996;
    public const uint MaskEqualsOneDestinationRawFileOffset = 0x012C8E76;
    public const uint MaskEqualsOneDestinationVirtualAddress = 0x016C9A76;
    public const uint MaskNotEqualOneFallthroughRawFileOffset = BoundaryRawFileOffsetExclusive;
    public const uint MaskNotEqualOneFallthroughVirtualAddress = BoundaryVirtualAddressExclusive;

    public const string CapturedWindowBytes =
        "8B 1D D8 AD ED 01 3B F3 0F 82 E0 00 00 00 8B 46 04 8B C8 83 E1 03 83 F9 01 0F 84 CF 00 00 00";

    public static ImmutableArray<CapacityFinalCalleeFirstValidationReadDescriptor>
        OrderedReadDescriptors { get; } =
        ImmutableArray.Create(
            new CapacityFinalCalleeFirstValidationReadDescriptor(
                EntryRawFileOffset,
                CapacityFinalCalleeFirstValidationReadKind.GlobalDword,
                GlobalDwordVirtualAddress,
                null,
                0),
            new CapacityFinalCalleeFirstValidationReadDescriptor(
                MovEaxFromEsiPlusFourRawFileOffset,
                CapacityFinalCalleeFirstValidationReadKind.EsiPlusFourDword,
                null,
                "ESI",
                EsiDwordDisplacement));

    public static ImmutableArray<CapacityFinalCalleeFirstValidationInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Read the dword at PE VA 1EDADD8 into EBX",
                EntryRawFileOffset, "8B 1D D8 AD ED 01",
                peVirtualAddressOperand: GlobalDwordVirtualAddress),
            Instruction("Compare entry ESI with EBX",
                CompareEsiEbxRawFileOffset, "3B F3"),
            Instruction("JB to 12C8E76 when unsigned ESI is below EBX",
                UnsignedBelowBranchRawFileOffset, "0F 82 E0 00 00 00",
                branchTargetRawFileOffset: EsiUnsignedBelowDestinationRawFileOffset,
                branchTargetVirtualAddress: EsiUnsignedBelowDestinationVirtualAddress),
            Instruction("Read the dword at [ESI+4] into EAX on the fallthrough",
                MovEaxFromEsiPlusFourRawFileOffset, "8B 46 04"),
            Instruction("Copy EAX to ECX",
                MovEcxFromEaxRawFileOffset, "8B C8"),
            Instruction("Mask ECX with 3",
                AndEcxThreeRawFileOffset, "83 E1 03",
                immediateValue: 3),
            Instruction("Compare masked ECX with 1",
                CompareEcxOneRawFileOffset, "83 F9 01",
                immediateValue: 1),
            Instruction("JZ to 12C8E76 when masked ECX equals 1",
                EqualOneBranchRawFileOffset, "0F 84 CF 00 00 00",
                branchTargetRawFileOffset: MaskEqualsOneDestinationRawFileOffset,
                branchTargetVirtualAddress: MaskEqualsOneDestinationVirtualAddress));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint EsiPlusFourAddress(uint entryEsi) =>
        unchecked(entryEsi + EsiDwordDisplacement);

    public static CapacityFinalCalleeFirstValidationPlan Project(
        CapacityFinalCalleeFirstValidationCapture capture)
    {
        if (!capture.CaptureValidatedEntry)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeFirstValidationOutcome.EntryNotValidated,
                unreachedGlobalDwordCaptureProvided: capture.GlobalDword.HasValue,
                unreachedEsi4DwordCaptureProvided: capture.Esi4Dword.HasValue,
                stopAt: EntryRawFileOffset);
        }

        var reads = ImmutableArray.CreateBuilder<CapacityFinalCalleeFirstValidationRead>();
        reads.Add(new(
            EntryRawFileOffset,
            CapacityFinalCalleeFirstValidationReadKind.GlobalDword,
            GlobalDwordVirtualAddress,
            capture.GlobalDword.HasValue,
            capture.GlobalDword));

        if (capture.GlobalDword is not uint globalDword)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeFirstValidationOutcome.MissingGlobalDword,
                reads,
                globalReadReached: true,
                unreachedEsi4DwordCaptureProvided: capture.Esi4Dword.HasValue,
                stopAt: EntryRawFileOffset);
        }

        CapacityFinalCalleeFirstValidationFlags compareFlags =
            SubtractFlags(capture.EntryESI, globalDword);
        bool entryEsiUnsignedBelowGlobal = compareFlags.Carry;
        if (entryEsiUnsignedBelowGlobal)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeFirstValidationOutcome.EntryEsiUnsignedBelowGlobal,
                reads,
                globalReadReached: true,
                compareEsiEbxReached: true,
                compareEsiEbxFlags: compareFlags,
                unsignedBelowBranchReached: true,
                entryEsiUnsignedBelowGlobal: true,
                firstSelectedDestinationRawFileOffset:
                    EsiUnsignedBelowDestinationRawFileOffset,
                firstSelectedDestinationVirtualAddress:
                    EsiUnsignedBelowDestinationVirtualAddress,
                unreachedEsi4DwordCaptureProvided: capture.Esi4Dword.HasValue,
                stopAt: EsiUnsignedBelowDestinationRawFileOffset);
        }

        uint esi4Address = EsiPlusFourAddress(capture.EntryESI);
        reads.Add(new(
            MovEaxFromEsiPlusFourRawFileOffset,
            CapacityFinalCalleeFirstValidationReadKind.EsiPlusFourDword,
            esi4Address,
            capture.Esi4Dword.HasValue,
            capture.Esi4Dword));
        if (capture.Esi4Dword is not uint esi4Dword)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeFirstValidationOutcome.MissingEsi4Dword,
                reads,
                globalReadReached: true,
                compareEsiEbxReached: true,
                compareEsiEbxFlags: compareFlags,
                unsignedBelowBranchReached: true,
                entryEsiUnsignedBelowGlobal: false,
                firstSelectedDestinationRawFileOffset:
                    EsiUnsignedFallthroughRawFileOffset,
                firstSelectedDestinationVirtualAddress:
                    EsiUnsignedFallthroughVirtualAddress,
                esi4ReadReached: true,
                esi4Address: esi4Address,
                stopAt: MovEaxFromEsiPlusFourRawFileOffset);
        }

        uint maskedEcx = esi4Dword & 3;
        CapacityFinalCalleeFirstValidationFlags andFlags = AndFlags(maskedEcx);
        CapacityFinalCalleeFirstValidationFlags compareEcxOneFlags =
            SubtractFlags(maskedEcx, 1);
        bool equalOneBranchTaken = compareEcxOneFlags.Zero;
        uint finalDestinationRawFileOffset = equalOneBranchTaken
            ? MaskEqualsOneDestinationRawFileOffset
            : MaskNotEqualOneFallthroughRawFileOffset;
        uint finalDestinationVirtualAddress = equalOneBranchTaken
            ? MaskEqualsOneDestinationVirtualAddress
            : MaskNotEqualOneFallthroughVirtualAddress;

        return BuildPlan(
            capture,
            equalOneBranchTaken
                ? CapacityFinalCalleeFirstValidationOutcome.MaskEqualsOne
                : CapacityFinalCalleeFirstValidationOutcome.MaskNotEqualOne,
            reads,
            globalReadReached: true,
            compareEsiEbxReached: true,
            compareEsiEbxFlags: compareFlags,
            unsignedBelowBranchReached: true,
            entryEsiUnsignedBelowGlobal: false,
            firstSelectedDestinationRawFileOffset: EsiUnsignedFallthroughRawFileOffset,
            firstSelectedDestinationVirtualAddress: EsiUnsignedFallthroughVirtualAddress,
            esi4ReadReached: true,
            esi4Address: esi4Address,
            esi4DwordCaptured: true,
            esi4Dword: esi4Dword,
            movEcxFromEaxReached: true,
            ecxAfterMove: esi4Dword,
            andEcxThreeReached: true,
            ecxAfterMask: maskedEcx,
            andEcxFlags: andFlags,
            compareEcxOneReached: true,
            compareEcxOneFlags: compareEcxOneFlags,
            equalOneBranchReached: true,
            equalOneBranchTaken: equalOneBranchTaken,
            finalSelectedDestinationRawFileOffset: finalDestinationRawFileOffset,
            finalSelectedDestinationVirtualAddress: finalDestinationVirtualAddress,
            eaxAfterLoad: esi4Dword,
            eaxAfterValidation: esi4Dword,
            stopAt: finalDestinationRawFileOffset);
    }

    private static CapacityFinalCalleeFirstValidationFlags SubtractFlags(
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

    private static CapacityFinalCalleeFirstValidationFlags AndFlags(uint result) =>
        new(
            false,
            HasEvenParity((byte)result),
            null,
            result == 0,
            (result & 0x80000000) != 0,
            false);

    private static bool HasEvenParity(byte value) =>
        BitOperations.PopCount((uint)value) % 2 == 0;

    private static CapacityFinalCalleeFirstValidationInstructionEvidence Instruction(
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

    private static CapacityFinalCalleeFirstValidationPlan BuildPlan(
        CapacityFinalCalleeFirstValidationCapture capture,
        CapacityFinalCalleeFirstValidationOutcome outcome,
        ImmutableArray<CapacityFinalCalleeFirstValidationRead>.Builder? reads = null,
        bool globalReadReached = false,
        bool compareEsiEbxReached = false,
        CapacityFinalCalleeFirstValidationFlags? compareEsiEbxFlags = null,
        bool unsignedBelowBranchReached = false,
        bool? entryEsiUnsignedBelowGlobal = null,
        uint? firstSelectedDestinationRawFileOffset = null,
        uint? firstSelectedDestinationVirtualAddress = null,
        bool esi4ReadReached = false,
        uint? esi4Address = null,
        bool esi4DwordCaptured = false,
        uint? esi4Dword = null,
        bool movEcxFromEaxReached = false,
        uint? ecxAfterMove = null,
        bool andEcxThreeReached = false,
        uint? ecxAfterMask = null,
        CapacityFinalCalleeFirstValidationFlags? andEcxFlags = null,
        bool compareEcxOneReached = false,
        CapacityFinalCalleeFirstValidationFlags? compareEcxOneFlags = null,
        bool equalOneBranchReached = false,
        bool? equalOneBranchTaken = null,
        uint? finalSelectedDestinationRawFileOffset = null,
        uint? finalSelectedDestinationVirtualAddress = null,
        uint? eaxAfterLoad = null,
        uint? eaxAfterValidation = null,
        bool unreachedGlobalDwordCaptureProvided = false,
        bool unreachedEsi4DwordCaptureProvided = false,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            reads?.ToImmutable() ?? ImmutableArray<CapacityFinalCalleeFirstValidationRead>.Empty,
            globalReadReached,
            globalReadReached && capture.GlobalDword.HasValue,
            globalReadReached ? capture.GlobalDword : null,
            compareEsiEbxReached,
            compareEsiEbxReached ? compareEsiEbxFlags : null,
            unsignedBelowBranchReached,
            unsignedBelowBranchReached ? entryEsiUnsignedBelowGlobal : null,
            firstSelectedDestinationRawFileOffset,
            firstSelectedDestinationVirtualAddress,
            esi4ReadReached,
            esi4ReadReached ? esi4Address : null,
            esi4ReadReached && capture.Esi4Dword.HasValue && esi4DwordCaptured,
            esi4ReadReached && capture.Esi4Dword.HasValue && esi4DwordCaptured
                ? esi4Dword
                : null,
            movEcxFromEaxReached,
            movEcxFromEaxReached ? ecxAfterMove : null,
            andEcxThreeReached,
            andEcxThreeReached ? ecxAfterMask : null,
            andEcxThreeReached ? andEcxFlags : null,
            compareEcxOneReached,
            compareEcxOneReached ? compareEcxOneFlags : null,
            equalOneBranchReached,
            equalOneBranchReached ? equalOneBranchTaken : null,
            finalSelectedDestinationRawFileOffset,
            finalSelectedDestinationVirtualAddress,
            eaxAfterLoad,
            eaxAfterValidation,
            (!globalReadReached && capture.GlobalDword.HasValue)
                || unreachedGlobalDwordCaptureProvided,
            (!esi4ReadReached && capture.Esi4Dword.HasValue)
                || unreachedEsi4DwordCaptureProvided,
            stopAt);
}
