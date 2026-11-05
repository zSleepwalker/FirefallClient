using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Bounded static observations for the pool-return routine. The projection preserves
/// separately captured header values and records the observed order; it does not model
/// the helper, allocator, fault behavior, or ownership.
/// </summary>
public static class UiShaderSourcePoolReturnNativeLayout
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDelta = 0x400C00;
    public const uint EntryRawFileOffset = 0x12CCA10;
    public const uint EndRawFileOffsetExclusive = 0x12CCA7E;
    public const uint EntryVirtualAddress = EntryRawFileOffset + CodeVirtualAddressDelta;

    public const int HeaderWord0Offset = 0x00;
    public const int HeaderClearedWordOffset = 0x04;
    public const int HeaderSpanOffset = 0x08;
    public const uint HeaderWord0ExpectedValue = 0x800;

    public const uint DiagnosticReporterFunctionPointerVirtualAddress = 0x1C15EA0;
    public const uint HeaderSpanDiagnosticId = 0x3F;
    public const uint HeaderWord0DiagnosticId = 0x40;

    public const uint PoolPointerGlobalVirtualAddress = 0x1E00510;
    public const int PoolCounterOffset = 0x10;
    public const uint PoolCounterXaddDeltaBits = 0xFFFFFFFF;

    public const uint OffsetReleaseHelperRawFileOffset = 0x12C8D60;
    public const uint PoolHandoffHelperRawFileOffset = 0x12CDA60;

    public const uint HeaderSpanCompareRawFileOffset = 0x12CCA15;
    public const uint HeaderSpanDiagnosticBranchRawFileOffset = 0x12CCA19;
    public const uint HeaderSpanDiagnosticIdPushRawFileOffset = 0x12CCA1D;
    public const uint HeaderSpanDiagnosticCallRawFileOffset = 0x12CCA29;
    public const uint HeaderWord0CompareRawFileOffset = 0x12CCA32;
    public const uint HeaderWord0DiagnosticBranchRawFileOffset = 0x12CCA38;
    public const uint HeaderWord0DiagnosticIdPushRawFileOffset = 0x12CCA3C;
    public const uint HeaderWord0DiagnosticCallRawFileOffset = 0x12CCA48;
    public const uint HeaderSpanRereadRawFileOffset = 0x12CCA51;
    public const uint PoolPointerLoadRawFileOffset = 0x12CCA54;
    public const uint HeaderSpanNegateRawFileOffset = 0x12CCA5A;
    public const uint HeaderWord4ClearRawFileOffset = 0x12CCA5C;
    public const uint OffsetReleaseCallRawFileOffset = 0x12CCA63;
    public const uint PoolCounterDeltaSetupRawFileOffset = 0x12CCA68;
    public const uint PoolCounterAddressSetupRawFileOffset = 0x12CCA6B;
    public const uint PoolCounterXaddRawFileOffset = 0x12CCA6E;
    public const uint HandoffHeaderPushRawFileOffset = 0x12CCA72;
    public const uint HandoffPoolRegisterSetupRawFileOffset = 0x12CCA73;
    public const uint PoolHandoffCallRawFileOffset = 0x12CCA75;
    public const uint ReturnRawFileOffset = 0x12CCA7D;

    public static IReadOnlyList<UiShaderSourcePoolReturnInstructionEvidence> Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourcePoolReturnInstructionEvidence(
                "Compare the initially captured header +8 value with zero",
                HeaderSpanCompareRawFileOffset,
                "83 7F 08 00",
                HeaderRelativeOffset: HeaderSpanOffset),
            new UiShaderSourcePoolReturnInstructionEvidence(
                "Skip diagnostic 0x3F when the initial header +8 value is zero",
                HeaderSpanDiagnosticBranchRawFileOffset,
                "74 17"),
            new UiShaderSourcePoolReturnInstructionEvidence(
                "Push diagnostic identifier 0x3F",
                HeaderSpanDiagnosticIdPushRawFileOffset,
                "6A 3F"),
            new UiShaderSourcePoolReturnInstructionEvidence(
                "Call the diagnostic reporter through its VA-resident function pointer",
                HeaderSpanDiagnosticCallRawFileOffset,
                "FF 15 A0 5E C1 01",
                TargetVirtualAddress: DiagnosticReporterFunctionPointerVirtualAddress),
            new UiShaderSourcePoolReturnInstructionEvidence(
                "Compare the initially captured header word 0 with 0x800",
                HeaderWord0CompareRawFileOffset,
                "81 3F 00 08 00 00",
                HeaderRelativeOffset: HeaderWord0Offset),
            new UiShaderSourcePoolReturnInstructionEvidence(
                "Skip diagnostic 0x40 when header word 0 equals 0x800",
                HeaderWord0DiagnosticBranchRawFileOffset,
                "74 17"),
            new UiShaderSourcePoolReturnInstructionEvidence(
                "Push diagnostic identifier 0x40",
                HeaderWord0DiagnosticIdPushRawFileOffset,
                "6A 40"),
            new UiShaderSourcePoolReturnInstructionEvidence(
                "Call the diagnostic reporter through its VA-resident function pointer",
                HeaderWord0DiagnosticCallRawFileOffset,
                "FF 15 A0 5E C1 01",
                TargetVirtualAddress: DiagnosticReporterFunctionPointerVirtualAddress),
            new UiShaderSourcePoolReturnInstructionEvidence(
                "Reread header +8 as a distinct later capture",
                HeaderSpanRereadRawFileOffset,
                "8B 4F 08",
                HeaderRelativeOffset: HeaderSpanOffset),
            new UiShaderSourcePoolReturnInstructionEvidence(
                "Load the pool pointer from the observed global VA",
                PoolPointerLoadRawFileOffset,
                "8B 35 10 05 E0 01",
                TargetVirtualAddress: PoolPointerGlobalVirtualAddress),
            new UiShaderSourcePoolReturnInstructionEvidence(
                "Negate the later header +8 capture into ECX",
                HeaderSpanNegateRawFileOffset,
                "F7 D9",
                HeaderRelativeOffset: HeaderSpanOffset),
            new UiShaderSourcePoolReturnInstructionEvidence(
                "Zero header +4",
                HeaderWord4ClearRawFileOffset,
                "C7 47 04 00 00 00 00",
                HeaderRelativeOffset: HeaderClearedWordOffset),
            new UiShaderSourcePoolReturnInstructionEvidence(
                "Call the helper with the negated capture in ECX",
                OffsetReleaseCallRawFileOffset,
                "E8 F8 C2 FF FF",
                TargetRawFileOffset: OffsetReleaseHelperRawFileOffset),
            new UiShaderSourcePoolReturnInstructionEvidence(
                "Set the XADD operand to -1",
                PoolCounterDeltaSetupRawFileOffset,
                "83 CA FF"),
            new UiShaderSourcePoolReturnInstructionEvidence(
                "Address pool +0x10 for the atomic XADD",
                PoolCounterAddressSetupRawFileOffset,
                "8D 46 10",
                PoolRelativeOffset: PoolCounterOffset),
            new UiShaderSourcePoolReturnInstructionEvidence(
                "Atomically XADD -1 at pool +0x10",
                PoolCounterXaddRawFileOffset,
                "F0 0F C1 10",
                PoolRelativeOffset: PoolCounterOffset),
            new UiShaderSourcePoolReturnInstructionEvidence(
                "Push the header pointer for the handoff call",
                HandoffHeaderPushRawFileOffset,
                "57"),
            new UiShaderSourcePoolReturnInstructionEvidence(
                "Set ECX to the captured pool pointer",
                HandoffPoolRegisterSetupRawFileOffset,
                "8B CE"),
            new UiShaderSourcePoolReturnInstructionEvidence(
                "Call the pool handoff helper",
                PoolHandoffCallRawFileOffset,
                "E8 E6 0F 00 00",
                TargetRawFileOffset: PoolHandoffHelperRawFileOffset),
            new UiShaderSourcePoolReturnInstructionEvidence(
                "Return",
                ReturnRawFileOffset,
                "C3")
        });

    /// <summary>
    /// Projects the two diagnostics from their initial captures, then records the
    /// unconditional continuation reached if any diagnostic calls return. The later
    /// header +8 value is an independent capture, and its negation is not classified
    /// as a data pointer.
    /// </summary>
    public static UiShaderSourcePoolReturnProjection Project(
        uint capturedHeaderPointer,
        uint initialHeaderWord0,
        uint initialHeaderSpan,
        uint laterHeaderSpan,
        uint capturedPoolPointer)
    {
        var actions = ImmutableArray.CreateBuilder<UiShaderSourcePoolReturnAction>();

        if (initialHeaderSpan != 0)
        {
            actions.Add(new UiShaderSourcePoolReturnAction(
                UiShaderSourcePoolReturnActionKind.ReportHeaderSpanDiagnostic,
                HeaderSpanDiagnosticCallRawFileOffset,
                DiagnosticId: HeaderSpanDiagnosticId,
                CapturedValue: initialHeaderSpan,
                TargetVirtualAddress: DiagnosticReporterFunctionPointerVirtualAddress));
        }

        if (initialHeaderWord0 != HeaderWord0ExpectedValue)
        {
            actions.Add(new UiShaderSourcePoolReturnAction(
                UiShaderSourcePoolReturnActionKind.ReportHeaderWord0Diagnostic,
                HeaderWord0DiagnosticCallRawFileOffset,
                DiagnosticId: HeaderWord0DiagnosticId,
                CapturedValue: initialHeaderWord0,
                TargetVirtualAddress: DiagnosticReporterFunctionPointerVirtualAddress));
        }

        uint negatedLaterSpan = unchecked(0u - laterHeaderSpan);
        uint poolCounterAddress = unchecked(capturedPoolPointer + PoolCounterOffset);

        actions.Add(new UiShaderSourcePoolReturnAction(
            UiShaderSourcePoolReturnActionKind.RereadHeaderSpan,
            HeaderSpanRereadRawFileOffset,
            CapturedValue: laterHeaderSpan));
        actions.Add(new UiShaderSourcePoolReturnAction(
            UiShaderSourcePoolReturnActionKind.LoadPoolPointer,
            PoolPointerLoadRawFileOffset,
            CapturedValue: capturedPoolPointer,
            TargetVirtualAddress: PoolPointerGlobalVirtualAddress));
        actions.Add(new UiShaderSourcePoolReturnAction(
            UiShaderSourcePoolReturnActionKind.NegateLaterSpanIntoEcx,
            HeaderSpanNegateRawFileOffset,
            CapturedValue: negatedLaterSpan));
        actions.Add(new UiShaderSourcePoolReturnAction(
            UiShaderSourcePoolReturnActionKind.ClearHeaderWord4,
            HeaderWord4ClearRawFileOffset,
            CapturedValue: 0));
        actions.Add(new UiShaderSourcePoolReturnAction(
            UiShaderSourcePoolReturnActionKind.CallOffsetReleaseHelper,
            OffsetReleaseCallRawFileOffset,
            CapturedValue: negatedLaterSpan,
            TargetRawFileOffset: OffsetReleaseHelperRawFileOffset));
        actions.Add(new UiShaderSourcePoolReturnAction(
            UiShaderSourcePoolReturnActionKind.AtomicXaddPoolCounter,
            PoolCounterXaddRawFileOffset,
            CapturedValue: PoolCounterXaddDeltaBits,
            TargetAddress: poolCounterAddress));
        actions.Add(new UiShaderSourcePoolReturnAction(
            UiShaderSourcePoolReturnActionKind.CallPoolHandoffHelper,
            PoolHandoffCallRawFileOffset,
            CapturedValue: capturedHeaderPointer,
            SecondaryValue: capturedPoolPointer,
            TargetRawFileOffset: PoolHandoffHelperRawFileOffset));
        actions.Add(new UiShaderSourcePoolReturnAction(
            UiShaderSourcePoolReturnActionKind.Return,
            ReturnRawFileOffset));

        return new UiShaderSourcePoolReturnProjection(
            capturedHeaderPointer,
            initialHeaderWord0,
            initialHeaderSpan,
            laterHeaderSpan,
            capturedPoolPointer,
            negatedLaterSpan,
            poolCounterAddress,
            actions.ToImmutable());
    }
}

public readonly record struct UiShaderSourcePoolReturnInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    int? HeaderRelativeOffset = null,
    int? PoolRelativeOffset = null,
    uint? TargetRawFileOffset = null,
    uint? TargetVirtualAddress = null);

public enum UiShaderSourcePoolReturnActionKind
{
    ReportHeaderSpanDiagnostic,
    ReportHeaderWord0Diagnostic,
    RereadHeaderSpan,
    LoadPoolPointer,
    NegateLaterSpanIntoEcx,
    ClearHeaderWord4,
    CallOffsetReleaseHelper,
    AtomicXaddPoolCounter,
    CallPoolHandoffHelper,
    Return
}

public readonly record struct UiShaderSourcePoolReturnAction(
    UiShaderSourcePoolReturnActionKind Kind,
    uint InstructionRawFileOffset,
    uint? DiagnosticId = null,
    uint? CapturedValue = null,
    uint? SecondaryValue = null,
    uint? TargetAddress = null,
    uint? TargetRawFileOffset = null,
    uint? TargetVirtualAddress = null);

public readonly record struct UiShaderSourcePoolReturnProjection(
    uint CapturedHeaderPointer,
    uint InitialHeaderWord0,
    uint InitialHeaderSpan,
    uint LaterHeaderSpan,
    uint CapturedPoolPointer,
    uint NegatedLaterSpan,
    uint PoolCounterAddress,
    ImmutableArray<UiShaderSourcePoolReturnAction> Actions);
