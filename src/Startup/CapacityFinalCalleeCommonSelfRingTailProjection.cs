using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonSelfRingTailCapture(
    uint? EntryEAX,
    uint? EntryEDX,
    uint? LateEBX,
    uint? LateESI);

public enum CapacityFinalCalleeCommonSelfRingTailOutcome
{
    MissingBothEntryOperands,
    MissingEntryEAX,
    MissingEntryEDX,
    MissingBothLatePointers,
    MissingLateEBX,
    MissingLateESI,
    Completed
}

public readonly record struct CapacityFinalCalleeCommonSelfRingTailFlags(
    bool Carry,
    bool Parity,
    bool? AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeCommonSelfRingTailWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public readonly record struct CapacityFinalCalleeCommonSelfRingTailInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? MemoryOperandPEVirtualAddress = null,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeCommonSelfRingTailPlan(
    CapacityFinalCalleeCommonSelfRingTailCapture Capture,
    CapacityFinalCalleeCommonSelfRingTailOutcome Outcome)
{
    public bool EntryEAXCaptured { get; init; }
    public bool EntryEDXCaptured { get; init; }
    public bool OrReached { get; init; }
    public uint? OrEAX { get; init; }
    public uint? OrEDX { get; init; }
    public uint? OrResult { get; init; }
    public CapacityFinalCalleeCommonSelfRingTailFlags? OrFlags { get; init; }
    public bool GlobalStoreReached { get; init; }
    public uint? GlobalStoreAddress { get; init; }
    public uint? GlobalStoreValue { get; init; }
    public bool LateEBXCaptured { get; init; }
    public bool LateESICaptured { get; init; }
    public bool FirstLocalStoreReached { get; init; }
    public bool FirstLocalStoreAttempted => GlobalStoreReached;
    public bool FirstLocalStoreCommitted => Writes.Length > 1;
    public uint? FirstLocalStoreAddress { get; init; }
    public uint? FirstLocalStoreValue { get; init; }
    public bool SecondLocalStoreReached { get; init; }
    public uint? SecondLocalStoreAddress { get; init; }
    public uint? SecondLocalStoreValue { get; init; }
    public bool ThirdLocalStoreReached { get; init; }
    public uint? ThirdLocalStoreAddress { get; init; }
    public uint? ThirdLocalStoreValue { get; init; }
    public bool FourthLocalStoreReached { get; init; }
    public uint? FourthLocalStoreAddress { get; init; }
    public uint? FourthLocalStoreValue { get; init; }
    public bool JumpReached { get; init; }
    public uint? JumpTargetRawFileOffset { get; init; }
    public uint? JumpTargetVirtualAddress { get; init; }
    public bool JumpTargetEntered => false;
    public ImmutableArray<CapacityFinalCalleeCommonSelfRingTailWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonSelfRingTailWrite>.Empty;
    public CapacityFinalCalleeCommonSelfRingTailFlags? FlagsAtStop { get; init; }
    public bool GlobalStorePreservesFlags { get; init; }
    public bool FirstLocalStorePreservesFlags { get; init; }
    public bool SecondLocalStorePreservesFlags { get; init; }
    public bool ThirdLocalStorePreservesFlags { get; init; }
    public bool FourthLocalStorePreservesFlags { get; init; }
    public bool JumpPreservesFlags { get; init; }
    public bool FlagsPreserved =>
        OrReached
        && FlagsAtStop == OrFlags
        && (!GlobalStoreReached || GlobalStorePreservesFlags)
        && (!FirstLocalStoreReached || FirstLocalStorePreservesFlags)
        && (!SecondLocalStoreReached || SecondLocalStorePreservesFlags)
        && (!ThirdLocalStoreReached || ThirdLocalStorePreservesFlags)
        && (!FourthLocalStoreReached || FourthLocalStorePreservesFlags)
        && (!JumpReached || JumpPreservesFlags);
    public bool UnreachedLateEBXCaptureProvided { get; init; }
    public bool UnreachedLateESICaptureProvided { get; init; }
    public bool ExactBoundedExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the common self-ring tail through its direct jump, without entering the target.
/// </summary>
public static class CapacityFinalCalleeCommonSelfRingTailProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C93AC;
    public const uint EntryVirtualAddress = 0x016C9FAC;
    public const uint OrRawFileOffset = 0x012C93AC;
    public const uint GlobalStoreRawFileOffset = 0x012C93AE;
    public const uint FirstLocalStoreRawFileOffset = 0x012C93B3;
    public const uint SecondLocalStoreRawFileOffset = 0x012C93B5;
    public const uint ThirdLocalStoreRawFileOffset = 0x012C93B8;
    public const uint FourthLocalStoreRawFileOffset = 0x012C93BB;
    public const uint JumpRawFileOffset = 0x012C93BE;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C93C0;
    public const uint GlobalStorePEVirtualAddress = 0x01EDADCC;
    public const uint EsiPlus18Displacement = 0x18;
    public const uint EsiPlusCDisplacement = 0x0C;
    public const uint EsiPlus8Displacement = 0x08;
    public const uint JumpTargetRawFileOffset = 0x012C942F;
    public const uint JumpTargetVirtualAddress = 0x016CA02F;

    public const string CapturedWindowBytes =
        "0B C2 A3 CC AD ED 01 89 33 89 5E 18 89 76 0C 89 76 08 EB 6F";

    public static ImmutableArray<CapacityFinalCalleeCommonSelfRingTailInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("OR entry EAX with entry EDX", OrRawFileOffset, "0B C2"),
            Instruction(
                "Store the OR result at the absolute global virtual address",
                GlobalStoreRawFileOffset,
                "A3 CC AD ED 01",
                memoryOperandPEVirtualAddress: GlobalStorePEVirtualAddress),
            Instruction(
                "Store entry ESI at entry EBX",
                FirstLocalStoreRawFileOffset,
                "89 33"),
            Instruction(
                "Store entry EBX at entry ESI+18",
                SecondLocalStoreRawFileOffset,
                "89 5E 18"),
            Instruction(
                "Store entry ESI at entry ESI+C",
                ThirdLocalStoreRawFileOffset,
                "89 76 0C"),
            Instruction(
                "Store entry ESI at entry ESI+8",
                FourthLocalStoreRawFileOffset,
                "89 76 08"),
            Instruction(
                "Jump directly to raw offset 012C942F",
                JumpRawFileOffset,
                "EB 6F",
                branchTargetRawFileOffset: JumpTargetRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveEsiPlus18Address(uint entryEsi) =>
        unchecked(entryEsi + EsiPlus18Displacement);

    public static uint DeriveEsiPlusCAddress(uint entryEsi) =>
        unchecked(entryEsi + EsiPlusCDisplacement);

    public static uint DeriveEsiPlus8Address(uint entryEsi) =>
        unchecked(entryEsi + EsiPlus8Displacement);

    public static CapacityFinalCalleeCommonSelfRingTailPlan Project(
        CapacityFinalCalleeCommonSelfRingTailCapture capture)
    {
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonSelfRingTailWrite>();
        bool eaxCaptured = capture.EntryEAX.HasValue;
        bool edxCaptured = capture.EntryEDX.HasValue;
        bool orReached = false;
        uint? orResult = null;
        CapacityFinalCalleeCommonSelfRingTailFlags? orFlags = null;
        bool globalStoreReached = false;
        bool lateEbxCaptured = false;
        bool lateEsiCaptured = false;
        bool firstLocalStoreReached = false;
        uint? firstLocalStoreAddress = null;
        uint? firstLocalStoreValue = null;
        bool secondLocalStoreReached = false;
        uint? secondLocalStoreAddress = null;
        uint? secondLocalStoreValue = null;
        bool thirdLocalStoreReached = false;
        uint? thirdLocalStoreAddress = null;
        uint? thirdLocalStoreValue = null;
        bool fourthLocalStoreReached = false;
        uint? fourthLocalStoreAddress = null;
        uint? fourthLocalStoreValue = null;
        bool jumpReached = false;

        CapacityFinalCalleeCommonSelfRingTailPlan Finish(
            CapacityFinalCalleeCommonSelfRingTailOutcome outcome,
            uint stopAt,
            bool exactBoundedExitAdmitted = false,
            bool unreachedLateEBX = false,
            bool unreachedLateESI = false) =>
            new(capture, outcome)
            {
                EntryEAXCaptured = eaxCaptured,
                EntryEDXCaptured = edxCaptured,
                OrReached = orReached,
                OrEAX = capture.EntryEAX,
                OrEDX = capture.EntryEDX,
                OrResult = orResult,
                OrFlags = orFlags,
                GlobalStoreReached = globalStoreReached,
                GlobalStoreAddress = globalStoreReached
                    ? GlobalStorePEVirtualAddress
                    : null,
                GlobalStoreValue = globalStoreReached ? orResult : null,
                LateEBXCaptured = lateEbxCaptured,
                LateESICaptured = lateEsiCaptured,
                FirstLocalStoreReached = firstLocalStoreReached,
                FirstLocalStoreAddress = firstLocalStoreAddress,
                FirstLocalStoreValue = firstLocalStoreValue,
                SecondLocalStoreReached = secondLocalStoreReached,
                SecondLocalStoreAddress = secondLocalStoreAddress,
                SecondLocalStoreValue = secondLocalStoreValue,
                ThirdLocalStoreReached = thirdLocalStoreReached,
                ThirdLocalStoreAddress = thirdLocalStoreAddress,
                ThirdLocalStoreValue = thirdLocalStoreValue,
                FourthLocalStoreReached = fourthLocalStoreReached,
                FourthLocalStoreAddress = fourthLocalStoreAddress,
                FourthLocalStoreValue = fourthLocalStoreValue,
                JumpReached = jumpReached,
                JumpTargetRawFileOffset = jumpReached ? JumpTargetRawFileOffset : null,
                JumpTargetVirtualAddress = jumpReached ? JumpTargetVirtualAddress : null,
                Writes = writes.ToImmutable(),
                FlagsAtStop = orFlags,
                GlobalStorePreservesFlags = globalStoreReached,
                FirstLocalStorePreservesFlags = firstLocalStoreReached,
                SecondLocalStorePreservesFlags = secondLocalStoreReached,
                ThirdLocalStorePreservesFlags = thirdLocalStoreReached,
                FourthLocalStorePreservesFlags = fourthLocalStoreReached,
                JumpPreservesFlags = jumpReached,
                UnreachedLateEBXCaptureProvided = unreachedLateEBX,
                UnreachedLateESICaptureProvided = unreachedLateESI,
                ExactBoundedExitAdmitted = exactBoundedExitAdmitted,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (!eaxCaptured && !edxCaptured)
        {
            return Finish(
                CapacityFinalCalleeCommonSelfRingTailOutcome.MissingBothEntryOperands,
                OrRawFileOffset,
                unreachedLateEBX: capture.LateEBX.HasValue,
                unreachedLateESI: capture.LateESI.HasValue);
        }

        if (!eaxCaptured)
        {
            return Finish(
                CapacityFinalCalleeCommonSelfRingTailOutcome.MissingEntryEAX,
                OrRawFileOffset,
                unreachedLateEBX: capture.LateEBX.HasValue,
                unreachedLateESI: capture.LateESI.HasValue);
        }

        if (!edxCaptured)
        {
            return Finish(
                CapacityFinalCalleeCommonSelfRingTailOutcome.MissingEntryEDX,
                OrRawFileOffset,
                unreachedLateEBX: capture.LateEBX.HasValue,
                unreachedLateESI: capture.LateESI.HasValue);
        }

        orReached = true;
        orResult = capture.EntryEAX!.Value | capture.EntryEDX!.Value;
        orFlags = CalculateOrFlags(orResult.Value);

        globalStoreReached = true;
        writes.Add(new(
            GlobalStoreRawFileOffset,
            GlobalStorePEVirtualAddress,
            orResult.Value));

        lateEbxCaptured = capture.LateEBX.HasValue;
        lateEsiCaptured = capture.LateESI.HasValue;
        firstLocalStoreAddress = capture.LateEBX;
        firstLocalStoreValue = capture.LateESI;
        if (!lateEbxCaptured && !lateEsiCaptured)
        {
            return Finish(
                CapacityFinalCalleeCommonSelfRingTailOutcome.MissingBothLatePointers,
                FirstLocalStoreRawFileOffset);
        }

        if (!lateEbxCaptured)
        {
            return Finish(
                CapacityFinalCalleeCommonSelfRingTailOutcome.MissingLateEBX,
                FirstLocalStoreRawFileOffset);
        }

        if (!lateEsiCaptured)
        {
            return Finish(
                CapacityFinalCalleeCommonSelfRingTailOutcome.MissingLateESI,
                FirstLocalStoreRawFileOffset);
        }

        uint entryEbx = capture.LateEBX!.Value;
        uint entryEsi = capture.LateESI!.Value;
        firstLocalStoreReached = true;
        writes.Add(new(FirstLocalStoreRawFileOffset, entryEbx, entryEsi));

        secondLocalStoreAddress = DeriveEsiPlus18Address(entryEsi);
        secondLocalStoreValue = entryEbx;
        secondLocalStoreReached = true;
        writes.Add(new(
            SecondLocalStoreRawFileOffset,
            secondLocalStoreAddress.Value,
            secondLocalStoreValue.Value));

        thirdLocalStoreAddress = DeriveEsiPlusCAddress(entryEsi);
        thirdLocalStoreValue = entryEsi;
        thirdLocalStoreReached = true;
        writes.Add(new(
            ThirdLocalStoreRawFileOffset,
            thirdLocalStoreAddress.Value,
            thirdLocalStoreValue.Value));

        fourthLocalStoreAddress = DeriveEsiPlus8Address(entryEsi);
        fourthLocalStoreValue = entryEsi;
        fourthLocalStoreReached = true;
        writes.Add(new(
            FourthLocalStoreRawFileOffset,
            fourthLocalStoreAddress.Value,
            fourthLocalStoreValue.Value));

        jumpReached = true;
        return Finish(
            CapacityFinalCalleeCommonSelfRingTailOutcome.Completed,
            JumpTargetRawFileOffset,
            exactBoundedExitAdmitted: true);
    }

    private static CapacityFinalCalleeCommonSelfRingTailFlags CalculateOrFlags(
        uint result) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)result),
            AuxiliaryCarry: null,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: false);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeCommonSelfRingTailInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes,
        uint? memoryOperandPEVirtualAddress = null,
        uint? branchTargetRawFileOffset = null) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes,
            memoryOperandPEVirtualAddress,
            branchTargetRawFileOffset,
            branchTargetRawFileOffset is uint targetRaw
                ? RawFileOffsetToVirtualAddress(targetRaw)
                : null);
}
