using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeSizeFieldsCapture(
    uint? ValidatedEntryESI,
    uint? ValidatedEntryEBP,
    uint? ValidatedEntryEBX,
    uint? FreshEsiPlus18,
    uint? FreshEsiPlusC,
    uint? FreshEsiPlus8,
    uint? FreshEcxPlusC,
    uint? FreshEaxPlus8);

public enum CapacityFinalCalleeLargeSizeFieldsOutcome
{
    MissingValidatedEntryESI,
    MissingFreshEsiPlus18,
    MissingValidatedEntryEBP,
    MissingFreshEsiPlusC,
    EsiPlusCEqualsEntryESI,
    MissingFreshEsiPlus8,
    MissingValidatedEntryEBX,
    EsiPlus8UnsignedBelowEntryEBX,
    MissingFreshEcxPlusC,
    EcxPlusCNotEqualEntryESI,
    MissingFreshEaxPlus8,
    EaxPlus8NotEqualEntryESI,
    Completed
}

public readonly record struct CapacityFinalCalleeLargeSizeFieldsInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes);

public readonly record struct CapacityFinalCalleeLargeSizeFieldsPlan(
    CapacityFinalCalleeLargeSizeFieldsCapture Capture,
    CapacityFinalCalleeLargeSizeFieldsOutcome Outcome,
    bool FreshEsiPlus18ReadReached,
    bool FreshEsiPlus18Captured,
    uint? FreshEsiPlus18Address,
    uint? EaxLoadedFromEsiPlus18,
    bool FrameWriteReached,
    uint? FrameWriteAddress,
    uint? FrameWriteValue,
    bool FreshEsiPlusCReadReached,
    bool FreshEsiPlusCCaptured,
    uint? FreshEsiPlusCAddress,
    uint? EaxLoadedFromEsiPlusC,
    bool EsiPlusCCompareReached,
    bool? EsiPlusCEqualsEntryESI,
    bool FreshEsiPlus8ReadReached,
    bool FreshEsiPlus8Captured,
    uint? FreshEsiPlus8Address,
    uint? EcxLoadedFromEsiPlus8,
    bool EcxEntryEbxCompareReached,
    bool? EsiPlus8UnsignedBelowEntryEBX,
    bool FreshEcxPlusCReadReached,
    bool FreshEcxPlusCCaptured,
    uint? FreshEcxPlusCAddress,
    uint? FreshEcxPlusCValue,
    bool EcxPlusCEntryEsiCompareReached,
    bool? EcxPlusCEqualsEntryESI,
    bool FreshEaxPlus8ReadReached,
    bool FreshEaxPlus8Captured,
    uint? FreshEaxPlus8Address,
    uint? FreshEaxPlus8Value,
    bool EaxPlus8EntryEsiCompareReached,
    bool? EaxPlus8EqualsEntryESI,
    bool FirstPairedWriteReached,
    uint? FirstPairedWriteAddress,
    uint? FirstPairedWriteValue,
    bool SecondPairedWriteReached,
    uint? SecondPairedWriteAddress,
    uint? SecondPairedWriteValue,
    bool UnreachedValidatedEntryEBPCaptureProvided,
    bool UnreachedValidatedEntryEBXCaptureProvided,
    bool UnreachedFreshEsiPlus18CaptureProvided,
    bool UnreachedFreshEsiPlusCCaptureProvided,
    bool UnreachedFreshEsiPlus8CaptureProvided,
    bool UnreachedFreshEcxPlusCCaptureProvided,
    bool UnreachedFreshEaxPlus8CaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the scalar reads, branch gates, frame write, and paired field writes
/// in the bounded large-size window. Each memory value is a fresh independent
/// capture; no ownership, validity, or simulated memory effects are inferred.
/// </summary>
public static class CapacityFinalCalleeLargeSizeFieldsProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C8F76;
    public const uint EntryVirtualAddress = 0x016C9B76;
    public const uint FrameWriteRawFileOffset = 0x012C8F79;
    public const uint EsiPlusCLoadRawFileOffset = 0x012C8F7C;
    public const uint EsiPlusCCompareRawFileOffset = 0x012C8F7F;
    public const uint EsiPlusCBranchRawFileOffset = 0x012C8F81;
    public const uint EsiPlus8LoadRawFileOffset = 0x012C8F83;
    public const uint EsiPlus8CompareRawFileOffset = 0x012C8F86;
    public const uint EsiPlus8BranchRawFileOffset = 0x012C8F88;
    public const uint EcxPlusCCompareRawFileOffset = 0x012C8F8E;
    public const uint EcxPlusCBranchRawFileOffset = 0x012C8F91;
    public const uint EaxPlus8CompareRawFileOffset = 0x012C8F97;
    public const uint EaxPlus8BranchRawFileOffset = 0x012C8F9A;
    public const uint FirstPairedWriteRawFileOffset = 0x012C8FA0;
    public const uint SecondPairedWriteRawFileOffset = 0x012C8FA3;
    public const uint FinalJumpRawFileOffset = 0x012C8FA6;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C8FA8;

    public const uint EsiPlus18Displacement = 0x18;
    public const uint EsiPlusCDisplacement = 0xC;
    public const uint EsiPlus8Displacement = 8;
    public const uint EcxPlusCDisplacement = 0xC;
    public const uint EaxPlus8Displacement = 8;
    public const uint FrameMinus14Displacement = 0x14;
    public const uint BranchExitRawFileOffset = 0x012C8E76;
    public const uint EsiPlusCEqualsExitRawFileOffset = 0x012C8FA8;
    public const uint PairedWritesExitRawFileOffset = 0x012C8FF8;

    public const string CapturedWindowBytes =
        "8B 46 18 89 45 EC 8B 46 0C 3B C6 74 25 "
        + "8B 4E 08 3B CB 0F 82 E8 FE FF FF "
        + "39 71 0C 0F 85 DF FE FF FF "
        + "39 70 08 0F 85 D6 FE FF FF "
        + "89 41 0C 89 48 08 EB 50";

    public static ImmutableArray<CapacityFinalCalleeLargeSizeFieldsInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Load a fresh DWORD at entry ESI+18 into EAX",
                EntryRawFileOffset, "8B 46 18"),
            Instruction("Write loaded EAX to entry EBP-14",
                FrameWriteRawFileOffset, "89 45 EC"),
            Instruction("Load a fresh DWORD at entry ESI+C into EAX",
                EsiPlusCLoadRawFileOffset, "8B 46 0C"),
            Instruction("Compare loaded EAX with entry ESI",
                EsiPlusCCompareRawFileOffset, "3B C6"),
            Instruction("Stop at FA8 when loaded EAX equals entry ESI",
                EsiPlusCBranchRawFileOffset, "74 25"),
            Instruction("Load a fresh DWORD at entry ESI+8 into ECX",
                EsiPlus8LoadRawFileOffset, "8B 4E 08"),
            Instruction("Compare loaded ECX unsigned-below entry EBX",
                EsiPlus8CompareRawFileOffset, "3B CB"),
            Instruction("Branch to E76 when loaded ECX is unsigned-below entry EBX",
                EsiPlus8BranchRawFileOffset, "0F 82 E8 FE FF FF"),
            Instruction("Compare a fresh DWORD at loaded ECX+C with entry ESI",
                EcxPlusCCompareRawFileOffset, "39 71 0C"),
            Instruction("Branch to E76 when the fresh ECX+C DWORD differs from entry ESI",
                EcxPlusCBranchRawFileOffset, "0F 85 DF FE FF FF"),
            Instruction("Compare a fresh DWORD at loaded EAX+8 with entry ESI",
                EaxPlus8CompareRawFileOffset, "39 70 08"),
            Instruction("Branch to E76 when the fresh EAX+8 DWORD differs from entry ESI",
                EaxPlus8BranchRawFileOffset, "0F 85 D6 FE FF FF"),
            Instruction("Write loaded EAX to loaded ECX+C",
                FirstPairedWriteRawFileOffset, "89 41 0C"),
            Instruction("Write loaded ECX to loaded EAX+8",
                SecondPairedWriteRawFileOffset, "89 48 08"),
            Instruction("Jump to FF8 after both paired writes",
                FinalJumpRawFileOffset, "EB 50"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveEsiPlus18Address(uint entryEsi) =>
        unchecked(entryEsi + EsiPlus18Displacement);

    public static uint DeriveFrameWriteAddress(uint entryEbp) =>
        unchecked(entryEbp - FrameMinus14Displacement);

    public static uint DeriveEsiPlusCAddress(uint entryEsi) =>
        unchecked(entryEsi + EsiPlusCDisplacement);

    public static uint DeriveEsiPlus8Address(uint entryEsi) =>
        unchecked(entryEsi + EsiPlus8Displacement);

    public static uint DeriveEcxPlusCAddress(uint loadedEcx) =>
        unchecked(loadedEcx + EcxPlusCDisplacement);

    public static uint DeriveEaxPlus8Address(uint loadedEax) =>
        unchecked(loadedEax + EaxPlus8Displacement);

    public static CapacityFinalCalleeLargeSizeFieldsPlan Project(
        CapacityFinalCalleeLargeSizeFieldsCapture capture)
    {
        bool esiPlus18ReadReached = false;
        bool esiPlus18Captured = false;
        uint? esiPlus18Address = null;
        uint? eaxLoadedFromEsiPlus18 = null;
        bool frameWriteReached = false;
        uint? frameWriteAddress = null;
        uint? frameWriteValue = null;
        bool esiPlusCReadReached = false;
        bool esiPlusCCaptured = false;
        uint? esiPlusCAddress = null;
        uint? eaxLoadedFromEsiPlusC = null;
        bool esiPlusCCompareReached = false;
        bool? esiPlusCEqualsEntryEsi = null;
        bool esiPlus8ReadReached = false;
        bool esiPlus8Captured = false;
        uint? esiPlus8Address = null;
        uint? ecxLoadedFromEsiPlus8 = null;
        bool ecxEntryEbxCompareReached = false;
        bool? esiPlus8UnsignedBelowEntryEbx = null;
        bool ecxPlusCReadReached = false;
        bool ecxPlusCCaptured = false;
        uint? ecxPlusCAddress = null;
        uint? ecxPlusCValue = null;
        bool ecxPlusCEntryEsiCompareReached = false;
        bool? ecxPlusCEqualsEntryEsi = null;
        bool eaxPlus8ReadReached = false;
        bool eaxPlus8Captured = false;
        uint? eaxPlus8Address = null;
        uint? eaxPlus8Value = null;
        bool eaxPlus8EntryEsiCompareReached = false;
        bool? eaxPlus8EqualsEntryEsi = null;
        bool firstPairedWriteReached = false;
        uint? firstPairedWriteAddress = null;
        uint? firstPairedWriteValue = null;
        bool secondPairedWriteReached = false;
        uint? secondPairedWriteAddress = null;
        uint? secondPairedWriteValue = null;

        CapacityFinalCalleeLargeSizeFieldsPlan BuildPlan(
            CapacityFinalCalleeLargeSizeFieldsOutcome outcome,
            uint stopAt) =>
            new(
                capture,
                outcome,
                esiPlus18ReadReached,
                esiPlus18Captured,
                esiPlus18Address,
                eaxLoadedFromEsiPlus18,
                frameWriteReached,
                frameWriteAddress,
                frameWriteValue,
                esiPlusCReadReached,
                esiPlusCCaptured,
                esiPlusCAddress,
                eaxLoadedFromEsiPlusC,
                esiPlusCCompareReached,
                esiPlusCEqualsEntryEsi,
                esiPlus8ReadReached,
                esiPlus8Captured,
                esiPlus8Address,
                ecxLoadedFromEsiPlus8,
                ecxEntryEbxCompareReached,
                esiPlus8UnsignedBelowEntryEbx,
                ecxPlusCReadReached,
                ecxPlusCCaptured,
                ecxPlusCAddress,
                ecxPlusCValue,
                ecxPlusCEntryEsiCompareReached,
                ecxPlusCEqualsEntryEsi,
                eaxPlus8ReadReached,
                eaxPlus8Captured,
                eaxPlus8Address,
                eaxPlus8Value,
                eaxPlus8EntryEsiCompareReached,
                eaxPlus8EqualsEntryEsi,
                firstPairedWriteReached,
                firstPairedWriteAddress,
                firstPairedWriteValue,
                secondPairedWriteReached,
                secondPairedWriteAddress,
                secondPairedWriteValue,
                !frameWriteReached && capture.ValidatedEntryEBP.HasValue,
                !ecxEntryEbxCompareReached && capture.ValidatedEntryEBX.HasValue,
                !esiPlus18ReadReached && capture.FreshEsiPlus18.HasValue,
                !esiPlusCReadReached && capture.FreshEsiPlusC.HasValue,
                !esiPlus8ReadReached && capture.FreshEsiPlus8.HasValue,
                !ecxPlusCReadReached && capture.FreshEcxPlusC.HasValue,
                !eaxPlus8ReadReached && capture.FreshEaxPlus8.HasValue,
                stopAt);

        if (capture.ValidatedEntryESI is not uint entryEsi)
        {
            return BuildPlan(
                CapacityFinalCalleeLargeSizeFieldsOutcome.MissingValidatedEntryESI,
                EntryRawFileOffset);
        }

        esiPlus18ReadReached = true;
        esiPlus18Address = DeriveEsiPlus18Address(entryEsi);
        if (capture.FreshEsiPlus18 is not uint freshEsiPlus18)
        {
            return BuildPlan(
                CapacityFinalCalleeLargeSizeFieldsOutcome.MissingFreshEsiPlus18,
                EntryRawFileOffset);
        }

        esiPlus18Captured = true;
        eaxLoadedFromEsiPlus18 = freshEsiPlus18;
        if (capture.ValidatedEntryEBP is not uint entryEbp)
        {
            return BuildPlan(
                CapacityFinalCalleeLargeSizeFieldsOutcome.MissingValidatedEntryEBP,
                FrameWriteRawFileOffset);
        }

        frameWriteReached = true;
        frameWriteAddress = DeriveFrameWriteAddress(entryEbp);
        frameWriteValue = freshEsiPlus18;

        esiPlusCReadReached = true;
        esiPlusCAddress = DeriveEsiPlusCAddress(entryEsi);
        if (capture.FreshEsiPlusC is not uint freshEsiPlusC)
        {
            return BuildPlan(
                CapacityFinalCalleeLargeSizeFieldsOutcome.MissingFreshEsiPlusC,
                EsiPlusCLoadRawFileOffset);
        }

        esiPlusCCaptured = true;
        eaxLoadedFromEsiPlusC = freshEsiPlusC;
        esiPlusCCompareReached = true;
        esiPlusCEqualsEntryEsi = freshEsiPlusC == entryEsi;
        if (freshEsiPlusC == entryEsi)
        {
            return BuildPlan(
                CapacityFinalCalleeLargeSizeFieldsOutcome.EsiPlusCEqualsEntryESI,
                EsiPlusCEqualsExitRawFileOffset);
        }

        esiPlus8ReadReached = true;
        esiPlus8Address = DeriveEsiPlus8Address(entryEsi);
        if (capture.FreshEsiPlus8 is not uint freshEsiPlus8)
        {
            return BuildPlan(
                CapacityFinalCalleeLargeSizeFieldsOutcome.MissingFreshEsiPlus8,
                EsiPlus8LoadRawFileOffset);
        }

        esiPlus8Captured = true;
        ecxLoadedFromEsiPlus8 = freshEsiPlus8;
        if (capture.ValidatedEntryEBX is not uint entryEbx)
        {
            return BuildPlan(
                CapacityFinalCalleeLargeSizeFieldsOutcome.MissingValidatedEntryEBX,
                EsiPlus8CompareRawFileOffset);
        }

        ecxEntryEbxCompareReached = true;
        esiPlus8UnsignedBelowEntryEbx = freshEsiPlus8 < entryEbx;
        if (freshEsiPlus8 < entryEbx)
        {
            return BuildPlan(
                CapacityFinalCalleeLargeSizeFieldsOutcome.EsiPlus8UnsignedBelowEntryEBX,
                BranchExitRawFileOffset);
        }

        ecxPlusCReadReached = true;
        ecxPlusCAddress = DeriveEcxPlusCAddress(freshEsiPlus8);
        if (capture.FreshEcxPlusC is not uint freshEcxPlusC)
        {
            return BuildPlan(
                CapacityFinalCalleeLargeSizeFieldsOutcome.MissingFreshEcxPlusC,
                EcxPlusCCompareRawFileOffset);
        }

        ecxPlusCCaptured = true;
        ecxPlusCValue = freshEcxPlusC;
        ecxPlusCEntryEsiCompareReached = true;
        ecxPlusCEqualsEntryEsi = freshEcxPlusC == entryEsi;
        if (freshEcxPlusC != entryEsi)
        {
            return BuildPlan(
                CapacityFinalCalleeLargeSizeFieldsOutcome.EcxPlusCNotEqualEntryESI,
                BranchExitRawFileOffset);
        }

        eaxPlus8ReadReached = true;
        eaxPlus8Address = DeriveEaxPlus8Address(freshEsiPlusC);
        if (capture.FreshEaxPlus8 is not uint freshEaxPlus8)
        {
            return BuildPlan(
                CapacityFinalCalleeLargeSizeFieldsOutcome.MissingFreshEaxPlus8,
                EaxPlus8CompareRawFileOffset);
        }

        eaxPlus8Captured = true;
        eaxPlus8Value = freshEaxPlus8;
        eaxPlus8EntryEsiCompareReached = true;
        eaxPlus8EqualsEntryEsi = freshEaxPlus8 == entryEsi;
        if (freshEaxPlus8 != entryEsi)
        {
            return BuildPlan(
                CapacityFinalCalleeLargeSizeFieldsOutcome.EaxPlus8NotEqualEntryESI,
                BranchExitRawFileOffset);
        }

        firstPairedWriteReached = true;
        firstPairedWriteAddress = ecxPlusCAddress;
        firstPairedWriteValue = freshEsiPlusC;
        secondPairedWriteReached = true;
        secondPairedWriteAddress = eaxPlus8Address;
        secondPairedWriteValue = freshEsiPlus8;
        return BuildPlan(
            CapacityFinalCalleeLargeSizeFieldsOutcome.Completed,
            PairedWritesExitRawFileOffset);
    }

    private static CapacityFinalCalleeLargeSizeFieldsInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes);
}
