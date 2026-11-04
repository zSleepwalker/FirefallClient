using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UiShaderSourcePoolTeardownCapture(
    uint ThisPointer,
    uint CapturedPoolPointer,
    uint CapturedNextPointer);

public readonly record struct UiShaderSourcePoolTeardownEntryDiagnosticCapture(
    uint ThisPointer,
    byte CapturedGlobalReadinessByte,
    byte CapturedReceiverFlagByte,
    uint? CapturedPoolPointerAfterNonzeroReceiverFlag,
    uint CapturedLaterSingletonListHead);

public enum UiShaderSourcePoolTeardownWriteTarget
{
    GlobalSingletonListHead,
    ThisField
}

public readonly record struct UiShaderSourcePoolTeardownWrite(
    UiShaderSourcePoolTeardownWriteTarget Target,
    uint InstructionRawFileOffset,
    uint TargetVirtualAddress,
    uint Value,
    int? ThisRelativeOffset = null);

public readonly record struct UiShaderSourcePoolTeardownReleaseBoundary(
    uint TestRawFileOffset,
    uint BranchRawFileOffset,
    uint CapturedPoolPointer,
    bool SelectsUnmodeledPoolDrain);

public readonly record struct UiShaderSourcePoolTeardownPlan(
    UiShaderSourcePoolTeardownCapture Capture,
    ImmutableArray<UiShaderSourcePoolTeardownWrite> OrderedWrites,
    UiShaderSourcePoolTeardownReleaseBoundary ReleaseBoundary);

public readonly record struct UiShaderSourcePoolTeardownInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    int? ThisRelativeOffset = null,
    uint? TargetVirtualAddress = null);

public readonly record struct UiShaderSourcePoolTeardownHelperAnchor(
    string Observation,
    uint CallRawFileOffset,
    string CallBytes,
    uint TargetRawFileOffset);

/// <summary>
/// Captures only the pool-detach writes and the first pool-pointer branch. The
/// projection does not model the subsequent drain loop, allocator behavior, or
/// overall shutdown readiness.
/// </summary>
public static class UiShaderSourcePoolTeardownNativeLayout
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x12CC1D0;
    public const uint EntryReturnRawFileOffset = 0x12CC27E;
    public const uint EntryEndRawFileOffsetExclusive = 0x12CC27F;
    public const uint EntryVirtualAddress =
        EntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;

    public const uint GlobalReadinessTestRawFileOffset = 0x12CC1D0;
    public const uint GlobalReadinessBranchRawFileOffset = 0x12CC1DB;
    public const uint GlobalReadinessDiagnosticIdPushRawFileOffset = 0x12CC1DF;
    public const uint GlobalReadinessDiagnosticCallRawFileOffset = 0x12CC1EB;
    public const uint GlobalReadinessDiagnosticCleanupRawFileOffset = 0x12CC1F1;
    public const uint ReceiverFlagTestRawFileOffset = 0x12CC1F4;
    public const uint ReceiverFlagBranchRawFileOffset = 0x12CC1F8;
    public const uint PoolPointerDiagnosticTestRawFileOffset = 0x12CC1FA;
    public const uint PoolPointerDiagnosticBranchRawFileOffset = 0x12CC1FE;
    public const uint ReceiverFlagDiagnosticSetupRawFileOffset = 0x12CC200;
    public const uint ReceiverFlagDiagnosticIdPushRawFileOffset = 0x12CC202;
    public const uint ReceiverFlagDiagnosticCallRawFileOffset = 0x12CC20E;
    public const uint ReceiverFlagDiagnosticCleanupRawFileOffset = 0x12CC214;
    public const uint SingletonListHeadDiagnosticCompareRawFileOffset = 0x12CC217;
    public const uint SingletonListHeadDiagnosticBranchRawFileOffset = 0x12CC21D;
    public const uint SingletonListHeadDiagnosticIdPushRawFileOffset = 0x12CC221;
    public const uint SingletonListHeadDiagnosticCallRawFileOffset = 0x12CC22D;
    public const uint SingletonListHeadDiagnosticCleanupRawFileOffset = 0x12CC233;
    public const uint DetachContinuationRawFileOffset = 0x12CC236;

    public const uint GlobalReadinessByteVirtualAddress = 0x1E4B095;
    public const uint DiagnosticReporterCellVirtualAddress = 0x1C15EA0;
    public const uint SingletonVtableVirtualAddress = 0x1C80CA0;
    public const uint SingletonVtableRawFileOffset = 0x187F8A0;
    public const uint SingletonVtableSlotRawFileOffset = 0x187F8A8;
    public const uint SingletonVtableSlotBytesTargetVirtualAddress = 0x16CCDD0;
    public const string SingletonVtableSlotBytes = "D0 CD 6C 01";

    public const uint SampleObjectVirtualAddress = 0x1E00500;
    public const uint SampleObjectRawFileOffset = 0x19FE900;
    public const uint SampleObjectVtableVirtualAddress = SingletonVtableVirtualAddress;
    public const string SampleObjectVtableBytes = "A0 0C C8 01";

    public const uint SingletonListHeadGlobalVirtualAddress = 0x1E4B0A8;

    public const int ThisFlagReadOffset = 0x04;
    public const int ThisNextPointerOffset = 0x0C;
    public const int ThisPoolPointerOffset = 0x10;

    public const uint GlobalReadinessDiagnosticId = 0x76;
    public const uint ReceiverFlagOrPoolDiagnosticId = 0x77;
    public const uint SingletonListHeadDiagnosticId = 0x78;

    public const uint ThisFlagCompareRawFileOffset = 0x12CC1F4;
    public const uint SingletonListHeadCompareRawFileOffset = 0x12CC217;
    public const uint PoolPointerCaptureRawFileOffset = 0x12CC236;
    public const uint NextPointerCaptureRawFileOffset = 0x12CC239;
    public const uint SingletonListHeadPublishRawFileOffset = 0x12CC23C;
    public const uint NextPointerClearRawFileOffset = 0x12CC241;
    public const uint PoolPointerClearRawFileOffset = 0x12CC248;
    public const uint PoolPointerReleaseTestRawFileOffset = 0x12CC24F;
    public const uint PoolPointerReleaseBranchRawFileOffset = 0x12CC251;

    public const uint DrainHelperAcquireAnchorRawFileOffset = 0x12CDAE0;
    public const uint DrainHelperReleaseAnchorRawFileOffset = 0x12C8D60;
    public const uint DrainHelperAcquireCallRawFileOffset = 0x12CC256;
    public const uint DrainHelperReleaseCallRawFileOffset = 0x12CC262;

    public static IReadOnlyList<UiShaderSourcePoolTeardownInstructionEvidence>
        EntryDiagnosticInstructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Test the separately captured global readiness byte",
                GlobalReadinessTestRawFileOffset,
                "80 3D 95 B0 E4 01 00",
                TargetVirtualAddress: GlobalReadinessByteVirtualAddress),
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Skip diagnostic 0x76 when the global readiness byte is nonzero",
                GlobalReadinessBranchRawFileOffset,
                "75 17"),
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Push diagnostic ID 0x76 when the global readiness byte is zero",
                GlobalReadinessDiagnosticIdPushRawFileOffset,
                "6A 76"),
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Call the diagnostic reporter for ID 0x76",
                GlobalReadinessDiagnosticCallRawFileOffset,
                "FF 15 A0 5E C1 01",
                TargetVirtualAddress: DiagnosticReporterCellVirtualAddress),
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Clean up diagnostic arguments and continue to the receiver-byte gate",
                GlobalReadinessDiagnosticCleanupRawFileOffset,
                "83 C4 10"),
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Test the separately captured receiver byte at this + 0x04",
                ReceiverFlagTestRawFileOffset,
                "80 7E 04 00",
                ThisRelativeOffset: ThisFlagReadOffset),
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Select diagnostic 0x77 without reading this + 0x10 when the receiver byte is zero",
                ReceiverFlagBranchRawFileOffset,
                "74 06"),
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Test this + 0x10 only after a nonzero receiver byte",
                PoolPointerDiagnosticTestRawFileOffset,
                "83 7E 10 00",
                ThisRelativeOffset: ThisPoolPointerOffset),
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Skip diagnostic 0x77 when the later pool pointer is nonzero",
                PoolPointerDiagnosticBranchRawFileOffset,
                "75 17"),
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Push diagnostic ID 0x77 on the zero-receiver or zero-pool path",
                ReceiverFlagDiagnosticIdPushRawFileOffset,
                "6A 77"),
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Call the diagnostic reporter for ID 0x77",
                ReceiverFlagDiagnosticCallRawFileOffset,
                "FF 15 A0 5E C1 01",
                TargetVirtualAddress: DiagnosticReporterCellVirtualAddress),
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Clean up diagnostic arguments and continue to the later list-head compare",
                ReceiverFlagDiagnosticCleanupRawFileOffset,
                "83 C4 10"),
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Compare the later singleton-list head with this",
                SingletonListHeadDiagnosticCompareRawFileOffset,
                "39 35 A8 B0 E4 01",
                TargetVirtualAddress: SingletonListHeadGlobalVirtualAddress),
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Skip diagnostic 0x78 when the later singleton-list head equals this",
                SingletonListHeadDiagnosticBranchRawFileOffset,
                "74 17"),
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Push diagnostic ID 0x78 when the later singleton-list head differs from this",
                SingletonListHeadDiagnosticIdPushRawFileOffset,
                "6A 78"),
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Call the diagnostic reporter for ID 0x78",
                SingletonListHeadDiagnosticCallRawFileOffset,
                "FF 15 A0 5E C1 01",
                TargetVirtualAddress: DiagnosticReporterCellVirtualAddress),
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Clean up diagnostic arguments and continue at the detach captures",
                SingletonListHeadDiagnosticCleanupRawFileOffset,
                "83 C4 10"),
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Continue at the detach captures after the diagnostics return",
                DetachContinuationRawFileOffset,
                "8B 7E 10 8B 46 0C A3 A8 B0 E4 01",
                ThisRelativeOffset: ThisPoolPointerOffset,
                TargetVirtualAddress: SingletonListHeadGlobalVirtualAddress)
        });

    public static IReadOnlyList<UiShaderSourcePoolTeardownInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Read the byte at this + 0x04 for the preceding diagnostic gate",
                ThisFlagCompareRawFileOffset,
                "80 7E 04 00",
                ThisRelativeOffset: ThisFlagReadOffset),
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Compare the singleton-list head with this before the diagnostic call",
                SingletonListHeadCompareRawFileOffset,
                "39 35 A8 B0 E4 01",
                TargetVirtualAddress: SingletonListHeadGlobalVirtualAddress),
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Capture the pool pointer from this + 0x10",
                PoolPointerCaptureRawFileOffset,
                "8B 7E 10",
                ThisRelativeOffset: ThisPoolPointerOffset),
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Capture the next pointer from this + 0x0C",
                NextPointerCaptureRawFileOffset,
                "8B 46 0C",
                ThisRelativeOffset: ThisNextPointerOffset),
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Publish the captured next pointer to the singleton-list head",
                SingletonListHeadPublishRawFileOffset,
                "A3 A8 B0 E4 01",
                TargetVirtualAddress: SingletonListHeadGlobalVirtualAddress),
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Clear this + 0x0C after publishing its captured value",
                NextPointerClearRawFileOffset,
                "C7 46 0C 00 00 00 00",
                ThisRelativeOffset: ThisNextPointerOffset),
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Clear this + 0x10 after clearing this + 0x0C",
                PoolPointerClearRawFileOffset,
                "C7 46 10 00 00 00 00",
                ThisRelativeOffset: ThisPoolPointerOffset),
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Test the captured pool pointer at the release boundary",
                PoolPointerReleaseTestRawFileOffset,
                "85 FF"),
            new UiShaderSourcePoolTeardownInstructionEvidence(
                "Select the unmodeled pool-drain path only when the captured pool pointer is nonzero",
                PoolPointerReleaseBranchRawFileOffset,
                "74 29")
        });

    public static IReadOnlyList<UiShaderSourcePoolTeardownHelperAnchor>
        DrainHelperAnchors { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourcePoolTeardownHelperAnchor(
                "Later drain-loop helper anchor; behavior is not projected here",
                DrainHelperAcquireCallRawFileOffset,
                "E8 85 18 00 00",
                DrainHelperAcquireAnchorRawFileOffset),
            new UiShaderSourcePoolTeardownHelperAnchor(
                "Later drain-loop helper anchor; behavior is not projected here",
                DrainHelperReleaseCallRawFileOffset,
                "E8 F9 CA FF FF",
                DrainHelperReleaseAnchorRawFileOffset)
        });

    /// <summary>
    /// Projects diagnostic IDs from distinct values captured at their respective
    /// reads. The pool-pointer read is required only after a nonzero receiver byte;
    /// a missing or unexpected pool capture is rejected. Each reporting call is
    /// indirect and, if it returns, execution continues toward the detach captures
    /// at raw 0x12CC236. This does not imply reporter behavior, abort/readiness,
    /// idempotence, or a coherent snapshot across the separate reads.
    /// </summary>
    public static ImmutableArray<uint> ProjectEntryDiagnosticIds(
        UiShaderSourcePoolTeardownEntryDiagnosticCapture capture)
    {
        if ((capture.CapturedReceiverFlagByte != 0)
                != capture.CapturedPoolPointerAfterNonzeroReceiverFlag.HasValue)
        {
            throw new ArgumentException(
                "A pool-pointer capture is required exactly when the receiver flag is nonzero.",
                nameof(capture));
        }

        var diagnosticIds = ImmutableArray.CreateBuilder<uint>(3);
        if (capture.CapturedGlobalReadinessByte == 0)
        {
            diagnosticIds.Add(GlobalReadinessDiagnosticId);
        }

        if (capture.CapturedReceiverFlagByte == 0
            || capture.CapturedPoolPointerAfterNonzeroReceiverFlag == 0)
        {
            diagnosticIds.Add(ReceiverFlagOrPoolDiagnosticId);
        }

        if (capture.CapturedLaterSingletonListHead != capture.ThisPointer)
        {
            diagnosticIds.Add(SingletonListHeadDiagnosticId);
        }

        return diagnosticIds.ToImmutable();
    }

    /// <summary>
    /// Projects the ordered global-head publication and two field clears from
    /// already captured values. Callers must supply the values observed after
    /// preceding diagnostics return; this does not model those diagnostics.
    /// </summary>
    public static UiShaderSourcePoolTeardownPlan Project(
        UiShaderSourcePoolTeardownCapture capture)
    {
        ImmutableArray<UiShaderSourcePoolTeardownWrite> orderedWrites =
            ImmutableArray.Create(
                new UiShaderSourcePoolTeardownWrite(
                    UiShaderSourcePoolTeardownWriteTarget.GlobalSingletonListHead,
                    SingletonListHeadPublishRawFileOffset,
                    SingletonListHeadGlobalVirtualAddress,
                    capture.CapturedNextPointer),
                new UiShaderSourcePoolTeardownWrite(
                    UiShaderSourcePoolTeardownWriteTarget.ThisField,
                    NextPointerClearRawFileOffset,
                    unchecked(capture.ThisPointer + ThisNextPointerOffset),
                    0,
                    ThisNextPointerOffset),
                new UiShaderSourcePoolTeardownWrite(
                    UiShaderSourcePoolTeardownWriteTarget.ThisField,
                    PoolPointerClearRawFileOffset,
                    unchecked(capture.ThisPointer + ThisPoolPointerOffset),
                    0,
                    ThisPoolPointerOffset));

        return new UiShaderSourcePoolTeardownPlan(
            capture,
            orderedWrites,
            new UiShaderSourcePoolTeardownReleaseBoundary(
                PoolPointerReleaseTestRawFileOffset,
                PoolPointerReleaseBranchRawFileOffset,
                capture.CapturedPoolPointer,
                capture.CapturedPoolPointer != 0));
    }
}
