using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UiShaderSourcePoolTeardownCapture(
    uint ThisPointer,
    uint CapturedPoolPointer,
    uint CapturedNextPointer);

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
