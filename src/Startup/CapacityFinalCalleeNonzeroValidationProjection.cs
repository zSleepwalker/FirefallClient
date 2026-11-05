using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeNonzeroValidationCapture(
    bool CaptureValidatedEntry,
    uint EntryEAX,
    uint EntryEDI,
    uint EntryESI,
    uint EntryEBX,
    uint EntryEBP,
    uint? GlobalDword,
    uint? EsiPlus8Dword,
    uint? EsiPlusCDword);

public enum CapacityFinalCalleeNonzeroValidationOutcome
{
    EntryNotValidated,
    EntryEsiUnsignedBelowEntryEbx,
    MissingGlobalDword,
    EntryEsiEqualsGlobalDword,
    ShiftedEaxUnsignedAboveOrEqual32,
    MissingEsiPlus8Dword,
    MissingEsiPlusCDword,
    EntryEsiNotEqualEsiPlusCDword,
    EntryEsiEqualsEsiPlusCDword
}

public enum CapacityFinalCalleeNonzeroValidationReadKind
{
    GlobalDword,
    EsiPlus8Dword,
    EsiPlusCDword
}

public readonly record struct CapacityFinalCalleeNonzeroValidationReadDescriptor(
    uint RawFileOffset,
    CapacityFinalCalleeNonzeroValidationReadKind Kind,
    uint? PeVirtualAddress,
    string? BaseRegister,
    uint Displacement);

public readonly record struct CapacityFinalCalleeNonzeroValidationRead(
    uint RawFileOffset,
    CapacityFinalCalleeNonzeroValidationReadKind Kind,
    uint Address,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeNonzeroValidationWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public readonly record struct CapacityFinalCalleeNonzeroValidationFlags(
    bool Carry,
    bool Parity,
    bool AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeNonzeroValidationPlan(
    CapacityFinalCalleeNonzeroValidationCapture Capture,
    CapacityFinalCalleeNonzeroValidationOutcome Outcome,
    ImmutableArray<CapacityFinalCalleeNonzeroValidationRead> Reads,
    ImmutableArray<CapacityFinalCalleeNonzeroValidationWrite> Writes,
    bool EdiAddReached,
    uint? EdiAfterAdd,
    bool CompareEsiEbxReached,
    CapacityFinalCalleeNonzeroValidationFlags? CompareEsiEbxFlags,
    bool UnsignedBelowEbxBranchReached,
    bool? EntryEsiUnsignedBelowEntryEbx,
    bool GlobalReadReached,
    bool GlobalDwordCaptured,
    uint? GlobalDword,
    bool CompareEsiGlobalReached,
    CapacityFinalCalleeNonzeroValidationFlags? CompareEsiGlobalFlags,
    bool GlobalEqualsBranchReached,
    bool? EntryEsiEqualsGlobalDword,
    bool ShiftEaxReached,
    uint? ShiftedEax,
    bool StoreShiftedEaxReached,
    uint? ShiftedEaxStackAddress,
    bool CompareShiftedEaxReached,
    CapacityFinalCalleeNonzeroValidationFlags? CompareShiftedEaxFlags,
    bool CapacityJnbReached,
    bool? ShiftedEaxUnsignedAboveOrEqual32,
    bool EsiPlus8ReadReached,
    uint? EsiPlus8Address,
    bool EsiPlus8DwordCaptured,
    uint? EsiPlus8Dword,
    bool StoreEsiPlus8Reached,
    uint? EsiPlus8StackAddress,
    bool EsiPlusCReadReached,
    uint? EsiPlusCAddress,
    bool EsiPlusCDwordCaptured,
    uint? EsiPlusCDword,
    bool StoreEsiPlusCReached,
    uint? EsiPlusCStackAddress,
    bool CompareEsiEsiPlusCReached,
    CapacityFinalCalleeNonzeroValidationFlags? CompareEsiEsiPlusCFlags,
    bool EsiNotEqualBranchReached,
    bool? EntryEsiNotEqualEsiPlusCDword,
    bool UnreachedGlobalDwordCaptureProvided,
    bool UnreachedEsiPlus8DwordCaptureProvided,
    bool UnreachedEsiPlusCDwordCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the bounded scalar validation window at the final-callee entry. It models only
/// the supplied register and memory captures, observed arithmetic, comparisons, and stack writes.
/// </summary>
public static class CapacityFinalCalleeNonzeroValidationProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C8E45;
    public const uint EdiAddRawFileOffset = 0x012C8E45;
    public const uint CompareEsiEbxRawFileOffset = 0x012C8E47;
    public const uint UnsignedBelowEbxBranchRawFileOffset = 0x012C8E49;
    public const uint GlobalReadRawFileOffset = 0x012C8E4B;
    public const uint CompareEsiGlobalRawFileOffset = 0x012C8E4B;
    public const uint GlobalEqualsBranchRawFileOffset = 0x012C8E51;
    public const uint ShiftEaxRawFileOffset = 0x012C8E57;
    public const uint StoreShiftedEaxRawFileOffset = 0x012C8E5A;
    public const uint CompareShiftedEaxRawFileOffset = 0x012C8E5D;
    public const uint CapacityJnbRawFileOffset = 0x012C8E60;
    public const uint EsiPlus8ReadRawFileOffset = 0x012C8E66;
    public const uint StoreEsiPlus8RawFileOffset = 0x012C8E69;
    public const uint EsiPlusCReadRawFileOffset = 0x012C8E6C;
    public const uint StoreEsiPlusCStackRawFileOffset = 0x012C8E6F;
    public const uint CompareEsiEsiPlusCRawFileOffset = 0x012C8E72;
    public const uint EsiNotEqualBranchRawFileOffset = 0x012C8E74;
    public const uint EntryVirtualAddress = 0x016C9A45;
    public const uint GlobalDwordVirtualAddress = 0x01EDADDC;
    public const uint EsiPlus8Displacement = 8;
    public const uint EsiPlusCDisplacement = 0xC;
    public const uint EntryEsiUnsignedBelowEbxDestinationRawFileOffset = 0x012C8E76;
    public const uint EntryEsiEqualsGlobalDestinationRawFileOffset = 0x012C90A2;
    public const uint CapacityJnbDestinationRawFileOffset = 0x012C8F76;
    public const uint EntryEsiNotEqualDestinationRawFileOffset = 0x012C8E7C;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C8E76;
    public const uint BoundaryVirtualAddressExclusive = 0x016C9A76;

    public const string CapturedWindowBytes =
        "03 F8 3B F3 72 2B 3B 35 DC AD ED 01 0F 84 4B 02 00 00 " +
        "C1 E8 03 89 45 F0 83 F8 20 0F 83 10 01 00 00 8B 4E 08 " +
        "89 4D FC 8B 4E 0C 89 4D EC 3B F1 75 06";

    public static ImmutableArray<CapacityFinalCalleeNonzeroValidationReadDescriptor>
        OrderedReadDescriptors { get; } =
        ImmutableArray.Create(
            new CapacityFinalCalleeNonzeroValidationReadDescriptor(
                GlobalReadRawFileOffset,
                CapacityFinalCalleeNonzeroValidationReadKind.GlobalDword,
                GlobalDwordVirtualAddress,
                null,
                0),
            new CapacityFinalCalleeNonzeroValidationReadDescriptor(
                EsiPlus8ReadRawFileOffset,
                CapacityFinalCalleeNonzeroValidationReadKind.EsiPlus8Dword,
                null,
                "ESI",
                EsiPlus8Displacement),
            new CapacityFinalCalleeNonzeroValidationReadDescriptor(
                EsiPlusCReadRawFileOffset,
                CapacityFinalCalleeNonzeroValidationReadKind.EsiPlusCDword,
                null,
                "ESI",
                EsiPlusCDisplacement));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint EsiPlus8Address(uint entryEsi) =>
        unchecked(entryEsi + EsiPlus8Displacement);

    public static uint EsiPlusCAddress(uint entryEsi) =>
        unchecked(entryEsi + EsiPlusCDisplacement);

    public static uint EbpMinus10Address(uint entryEbp) =>
        unchecked(entryEbp - 0x10);

    public static uint EbpMinus4Address(uint entryEbp) =>
        unchecked(entryEbp - 4);

    public static uint EbpMinus14Address(uint entryEbp) =>
        unchecked(entryEbp - 0x14);

    public static CapacityFinalCalleeNonzeroValidationPlan Project(
        CapacityFinalCalleeNonzeroValidationCapture capture)
    {
        var state = new ProjectionState(capture);
        if (!capture.CaptureValidatedEntry)
        {
            return state.Finish(
                CapacityFinalCalleeNonzeroValidationOutcome.EntryNotValidated,
                EntryRawFileOffset);
        }

        state.EdiAddReached = true;
        state.EdiAfterAdd = unchecked(capture.EntryEDI + capture.EntryEAX);
        state.CompareEsiEbxReached = true;
        state.CompareEsiEbxFlags = SubtractFlags(capture.EntryESI, capture.EntryEBX);
        bool esiBelowEbx = state.CompareEsiEbxFlags.Value.Carry;
        state.UnsignedBelowEbxBranchReached = true;
        state.EntryEsiUnsignedBelowEntryEbx = esiBelowEbx;
        if (esiBelowEbx)
        {
            return state.Finish(
                CapacityFinalCalleeNonzeroValidationOutcome.EntryEsiUnsignedBelowEntryEbx,
                EntryEsiUnsignedBelowEbxDestinationRawFileOffset);
        }

        state.GlobalReadReached = true;
        state.Reads.Add(new(
            GlobalReadRawFileOffset,
            CapacityFinalCalleeNonzeroValidationReadKind.GlobalDword,
            GlobalDwordVirtualAddress,
            capture.GlobalDword.HasValue,
            capture.GlobalDword));
        if (capture.GlobalDword is not uint globalDword)
        {
            return state.Finish(
                CapacityFinalCalleeNonzeroValidationOutcome.MissingGlobalDword,
                GlobalReadRawFileOffset);
        }

        state.GlobalDwordCaptured = true;
        state.GlobalDword = globalDword;
        state.CompareEsiGlobalReached = true;
        state.CompareEsiGlobalFlags = SubtractFlags(capture.EntryESI, globalDword);
        bool esiEqualsGlobal = state.CompareEsiGlobalFlags.Value.Zero;
        state.GlobalEqualsBranchReached = true;
        state.EntryEsiEqualsGlobalDword = esiEqualsGlobal;
        if (esiEqualsGlobal)
        {
            return state.Finish(
                CapacityFinalCalleeNonzeroValidationOutcome.EntryEsiEqualsGlobalDword,
                EntryEsiEqualsGlobalDestinationRawFileOffset);
        }

        state.ShiftEaxReached = true;
        state.ShiftedEax = capture.EntryEAX >> 3;
        state.StoreShiftedEaxReached = true;
        state.ShiftedEaxStackAddress = EbpMinus10Address(capture.EntryEBP);
        state.Writes.Add(new(
            StoreShiftedEaxRawFileOffset,
            state.ShiftedEaxStackAddress.Value,
            state.ShiftedEax.Value));
        state.CompareShiftedEaxReached = true;
        state.CompareShiftedEaxFlags = SubtractFlags(state.ShiftedEax.Value, 0x20);
        bool shiftedEaxAtLeast32 = !state.CompareShiftedEaxFlags.Value.Carry;
        state.CapacityJnbReached = true;
        state.ShiftedEaxUnsignedAboveOrEqual32 = shiftedEaxAtLeast32;
        if (shiftedEaxAtLeast32)
        {
            return state.Finish(
                CapacityFinalCalleeNonzeroValidationOutcome
                    .ShiftedEaxUnsignedAboveOrEqual32,
                CapacityJnbDestinationRawFileOffset);
        }

        state.EsiPlus8ReadReached = true;
        uint esiPlus8Address = EsiPlus8Address(capture.EntryESI);
        state.EsiPlus8Address = esiPlus8Address;
        state.Reads.Add(new(
            EsiPlus8ReadRawFileOffset,
            CapacityFinalCalleeNonzeroValidationReadKind.EsiPlus8Dword,
            esiPlus8Address,
            capture.EsiPlus8Dword.HasValue,
            capture.EsiPlus8Dword));
        if (capture.EsiPlus8Dword is not uint esiPlus8Dword)
        {
            return state.Finish(
                CapacityFinalCalleeNonzeroValidationOutcome.MissingEsiPlus8Dword,
                EsiPlus8ReadRawFileOffset);
        }

        state.EsiPlus8DwordCaptured = true;
        state.EsiPlus8Dword = esiPlus8Dword;
        state.StoreEsiPlus8Reached = true;
        state.EsiPlus8StackAddress = EbpMinus4Address(capture.EntryEBP);
        state.Writes.Add(new(
            StoreEsiPlus8RawFileOffset,
            state.EsiPlus8StackAddress.Value,
            esiPlus8Dword));

        state.EsiPlusCReadReached = true;
        uint esiPlusCAddress = EsiPlusCAddress(capture.EntryESI);
        state.EsiPlusCAddress = esiPlusCAddress;
        state.Reads.Add(new(
            EsiPlusCReadRawFileOffset,
            CapacityFinalCalleeNonzeroValidationReadKind.EsiPlusCDword,
            esiPlusCAddress,
            capture.EsiPlusCDword.HasValue,
            capture.EsiPlusCDword));
        if (capture.EsiPlusCDword is not uint esiPlusCDword)
        {
            return state.Finish(
                CapacityFinalCalleeNonzeroValidationOutcome.MissingEsiPlusCDword,
                EsiPlusCReadRawFileOffset);
        }

        state.EsiPlusCDwordCaptured = true;
        state.EsiPlusCDword = esiPlusCDword;
        state.StoreEsiPlusCReached = true;
        state.EsiPlusCStackAddress = EbpMinus14Address(capture.EntryEBP);
        state.Writes.Add(new(
            StoreEsiPlusCStackRawFileOffset,
            state.EsiPlusCStackAddress.Value,
            esiPlusCDword));
        state.CompareEsiEsiPlusCReached = true;
        state.CompareEsiEsiPlusCFlags =
            SubtractFlags(capture.EntryESI, esiPlusCDword);
        bool esiNotEqual = !state.CompareEsiEsiPlusCFlags.Value.Zero;
        state.EsiNotEqualBranchReached = true;
        state.EntryEsiNotEqualEsiPlusCDword = esiNotEqual;
        return state.Finish(
            esiNotEqual
                ? CapacityFinalCalleeNonzeroValidationOutcome
                    .EntryEsiNotEqualEsiPlusCDword
                : CapacityFinalCalleeNonzeroValidationOutcome
                    .EntryEsiEqualsEsiPlusCDword,
            esiNotEqual
                ? EntryEsiNotEqualDestinationRawFileOffset
                : BoundaryRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleeNonzeroValidationFlags SubtractFlags(
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

    private static bool HasEvenParity(byte value) =>
        System.Numerics.BitOperations.PopCount((uint)value) % 2 == 0;

    private sealed class ProjectionState(
        CapacityFinalCalleeNonzeroValidationCapture capture)
    {
        public readonly ImmutableArray<CapacityFinalCalleeNonzeroValidationRead>.Builder Reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeNonzeroValidationRead>();
        public readonly ImmutableArray<CapacityFinalCalleeNonzeroValidationWrite>.Builder Writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeNonzeroValidationWrite>();

        public bool EdiAddReached;
        public uint? EdiAfterAdd;
        public bool CompareEsiEbxReached;
        public CapacityFinalCalleeNonzeroValidationFlags? CompareEsiEbxFlags;
        public bool UnsignedBelowEbxBranchReached;
        public bool? EntryEsiUnsignedBelowEntryEbx;
        public bool GlobalReadReached;
        public bool GlobalDwordCaptured;
        public uint? GlobalDword;
        public bool CompareEsiGlobalReached;
        public CapacityFinalCalleeNonzeroValidationFlags? CompareEsiGlobalFlags;
        public bool GlobalEqualsBranchReached;
        public bool? EntryEsiEqualsGlobalDword;
        public bool ShiftEaxReached;
        public uint? ShiftedEax;
        public bool StoreShiftedEaxReached;
        public uint? ShiftedEaxStackAddress;
        public bool CompareShiftedEaxReached;
        public CapacityFinalCalleeNonzeroValidationFlags? CompareShiftedEaxFlags;
        public bool CapacityJnbReached;
        public bool? ShiftedEaxUnsignedAboveOrEqual32;
        public bool EsiPlus8ReadReached;
        public uint? EsiPlus8Address;
        public bool EsiPlus8DwordCaptured;
        public uint? EsiPlus8Dword;
        public bool StoreEsiPlus8Reached;
        public uint? EsiPlus8StackAddress;
        public bool EsiPlusCReadReached;
        public uint? EsiPlusCAddress;
        public bool EsiPlusCDwordCaptured;
        public uint? EsiPlusCDword;
        public bool StoreEsiPlusCReached;
        public uint? EsiPlusCStackAddress;
        public bool CompareEsiEsiPlusCReached;
        public CapacityFinalCalleeNonzeroValidationFlags? CompareEsiEsiPlusCFlags;
        public bool EsiNotEqualBranchReached;
        public bool? EntryEsiNotEqualEsiPlusCDword;

        public CapacityFinalCalleeNonzeroValidationPlan Finish(
            CapacityFinalCalleeNonzeroValidationOutcome outcome,
            uint stopAt) =>
            new(
                capture,
                outcome,
                Reads.ToImmutable(),
                Writes.ToImmutable(),
                EdiAddReached,
                EdiAfterAdd,
                CompareEsiEbxReached,
                CompareEsiEbxReached ? CompareEsiEbxFlags : null,
                UnsignedBelowEbxBranchReached,
                UnsignedBelowEbxBranchReached ? EntryEsiUnsignedBelowEntryEbx : null,
                GlobalReadReached,
                GlobalDwordCaptured,
                GlobalDwordCaptured ? GlobalDword : null,
                CompareEsiGlobalReached,
                CompareEsiGlobalReached ? CompareEsiGlobalFlags : null,
                GlobalEqualsBranchReached,
                GlobalEqualsBranchReached ? EntryEsiEqualsGlobalDword : null,
                ShiftEaxReached,
                ShiftEaxReached ? ShiftedEax : null,
                StoreShiftedEaxReached,
                StoreShiftedEaxReached ? ShiftedEaxStackAddress : null,
                CompareShiftedEaxReached,
                CompareShiftedEaxReached ? CompareShiftedEaxFlags : null,
                CapacityJnbReached,
                CapacityJnbReached ? ShiftedEaxUnsignedAboveOrEqual32 : null,
                EsiPlus8ReadReached,
                EsiPlus8ReadReached ? EsiPlus8Address : null,
                EsiPlus8DwordCaptured,
                EsiPlus8DwordCaptured ? EsiPlus8Dword : null,
                StoreEsiPlus8Reached,
                StoreEsiPlus8Reached ? EsiPlus8StackAddress : null,
                EsiPlusCReadReached,
                EsiPlusCReadReached ? EsiPlusCAddress : null,
                EsiPlusCDwordCaptured,
                EsiPlusCDwordCaptured ? EsiPlusCDword : null,
                StoreEsiPlusCReached,
                StoreEsiPlusCReached ? EsiPlusCStackAddress : null,
                CompareEsiEsiPlusCReached,
                CompareEsiEsiPlusCReached ? CompareEsiEsiPlusCFlags : null,
                EsiNotEqualBranchReached,
                EsiNotEqualBranchReached ? EntryEsiNotEqualEsiPlusCDword : null,
                !GlobalReadReached && capture.GlobalDword.HasValue,
                !EsiPlus8ReadReached && capture.EsiPlus8Dword.HasValue,
                !EsiPlusCReadReached && capture.EsiPlusCDword.HasValue,
                stopAt);
    }
}
