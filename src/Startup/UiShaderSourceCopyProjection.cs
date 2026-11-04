namespace Firefall.Reconstruction.Startup;

public readonly record struct UiShaderSourceCopyCapture(
    uint DestinationPointer,
    uint SourcePointer,
    uint SourceControlPointer,
    uint SourceBackingPointer,
    uint SourceTrailingDword);

public enum UiShaderSourceCopyActionKind
{
    WriteDestinationVtable,
    ZeroDestinationBackingPointer,
    ZeroDestinationControlPointer,
    CallPairAssignmentHelper,
    CaptureSourceControlPointer,
    CaptureSourceBackingPointer,
    IncrementCapturedControlReferenceCount,
    ReadFreshDestinationControlPointer,
    SkipDestinationControlRelease,
    WriteDestinationControlPointer,
    WriteDestinationBackingPointer,
    CaptureSourceTrailingDword,
    WriteDestinationTrailingDword
}

public readonly record struct UiShaderSourceCopyAction(
    UiShaderSourceCopyActionKind Kind,
    uint InstructionRawFileOffset,
    uint? TargetVirtualAddress = null,
    uint? Value = null);

public readonly record struct UiShaderSourceCopyProjection(
    UiShaderSourceCopyCapture Capture,
    IReadOnlyList<UiShaderSourceCopyAction> Actions);

public readonly record struct UiShaderSourceCopyInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string BytesHex,
    int? ThisRelativeOffset = null,
    int? SourceRelativeOffset = null,
    uint? TargetVirtualAddress = null);

/// <summary>
/// Projects the observed fresh-destination copy-constructor stores and captured reads.
/// This does not dereference memory, copy backing bytes, model alignment calculations,
/// or represent an atomic snapshot.
/// </summary>
public static class UiShaderSourceCopyProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint ImageBase = 0x00400000;
    public const uint TextSectionRva = 0x00001000;
    public const uint TextSectionRawFileOffset = 0x00000400;
    public const uint CodeVirtualAddressOffsetFromRawFileOffset =
        ImageBase + TextSectionRva - TextSectionRawFileOffset;

    public const uint ConstructorEntryRawFileOffset = 0x12CB7F0;
    public const uint DestinationVtableStoreRawFileOffset = 0x12CB801;
    public const uint DestinationBackingZeroRawFileOffset = 0x12CB807;
    public const uint DestinationControlZeroRawFileOffset = 0x12CB80D;
    public const uint PairAssignmentCallRawFileOffset = 0x12CB814;
    public const uint SourceTrailingDwordLoadRawFileOffset = 0x12CB819;
    public const uint DestinationTrailingDwordStoreRawFileOffset = 0x12CB81C;

    public const uint PairAssignmentHelperRawFileOffset = 0x12CC430;
    public const uint SourceControlLoadRawFileOffset = 0x12CC439;
    public const uint SourceBackingLoadRawFileOffset = 0x12CC43E;
    public const uint SourceControlNullTestRawFileOffset = 0x12CC443;
    public const uint SourceControlNullBranchRawFileOffset = 0x12CC445;
    public const uint ReferenceCountIncrementRawFileOffset = 0x12CC44F;
    public const uint FreshDestinationControlLoadRawFileOffset = 0x12CC456;
    public const uint FreshDestinationControlBranchRawFileOffset = 0x12CC45B;
    public const uint DestinationControlStoreRawFileOffset = 0x12CC491;
    public const uint DestinationBackingStoreRawFileOffset = 0x12CC495;
    public const uint DestinationControlReleaseSkippedTargetRawFileOffset = 0x12CC491;

    public const uint WrapperVtableVirtualAddress = 0x1C80CC4;
    public const int BackingPointerOffset = 0x04;
    public const int ControlPointerOffset = 0x08;
    public const int TrailingDwordOffset = 0x0C;
    public const int ReferenceCountOffset = 0x04;

    public static IReadOnlyList<UiShaderSourceCopyInstructionEvidence> Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourceCopyInstructionEvidence(
                "Install the destination wrapper vtable",
                DestinationVtableStoreRawFileOffset,
                "C707C40CC801",
                ThisRelativeOffset: 0,
                TargetVirtualAddress: WrapperVtableVirtualAddress),
            new UiShaderSourceCopyInstructionEvidence(
                "Zero the destination backing pointer before the helper call",
                DestinationBackingZeroRawFileOffset,
                "C70100000000",
                ThisRelativeOffset: BackingPointerOffset),
            new UiShaderSourceCopyInstructionEvidence(
                "Zero the destination control pointer before the helper call",
                DestinationControlZeroRawFileOffset,
                "C7410400000000",
                ThisRelativeOffset: ControlPointerOffset),
            new UiShaderSourceCopyInstructionEvidence(
                "Call the pair-assignment helper",
                PairAssignmentCallRawFileOffset,
                "E8170C0000",
                TargetVirtualAddress: ToCodeVirtualAddress(PairAssignmentHelperRawFileOffset)),
            new UiShaderSourceCopyInstructionEvidence(
                "Load source control pointer from wrapper +8",
                SourceControlLoadRawFileOffset,
                "8B7804",
                SourceRelativeOffset: ControlPointerOffset),
            new UiShaderSourceCopyInstructionEvidence(
                "Load source backing pointer from wrapper +4",
                SourceBackingLoadRawFileOffset,
                "8B10",
                SourceRelativeOffset: BackingPointerOffset),
            new UiShaderSourceCopyInstructionEvidence(
                "Test the captured control pointer",
                SourceControlNullTestRawFileOffset,
                "85FF"),
            new UiShaderSourceCopyInstructionEvidence(
                "Skip the reference-count increment when control is null",
                SourceControlNullBranchRawFileOffset,
                "740F",
                TargetVirtualAddress: ToCodeVirtualAddress(FreshDestinationControlLoadRawFileOffset)),
            new UiShaderSourceCopyInstructionEvidence(
                "Address the captured control pointer's +4 counter",
                0x12CC447,
                "8D4704",
                SourceRelativeOffset: ReferenceCountOffset),
            new UiShaderSourceCopyInstructionEvidence(
                "Set the atomic counter increment to one",
                0x12CC44A,
                "BA01000000"),
            new UiShaderSourceCopyInstructionEvidence(
                "Atomically add one to the captured control pointer's +4 counter",
                ReferenceCountIncrementRawFileOffset,
                "F00FC110",
                SourceRelativeOffset: ReferenceCountOffset),
            new UiShaderSourceCopyInstructionEvidence(
                "Read the already-zero destination control pointer",
                FreshDestinationControlLoadRawFileOffset,
                "8B5E04",
                ThisRelativeOffset: ControlPointerOffset),
            new UiShaderSourceCopyInstructionEvidence(
                "Skip old-destination release for the fresh zeroed destination",
                FreshDestinationControlBranchRawFileOffset,
                "7434",
                TargetVirtualAddress: ToCodeVirtualAddress(DestinationControlReleaseSkippedTargetRawFileOffset)),
            new UiShaderSourceCopyInstructionEvidence(
                "Write the captured control pointer to destination +8",
                DestinationControlStoreRawFileOffset,
                "897E04",
                ThisRelativeOffset: ControlPointerOffset),
            new UiShaderSourceCopyInstructionEvidence(
                "Write the captured backing pointer to destination +4",
                DestinationBackingStoreRawFileOffset,
                "8916",
                ThisRelativeOffset: BackingPointerOffset),
            new UiShaderSourceCopyInstructionEvidence(
                "Load source trailing dword at +0xC after pair assignment",
                SourceTrailingDwordLoadRawFileOffset,
                "8B460C",
                SourceRelativeOffset: TrailingDwordOffset),
            new UiShaderSourceCopyInstructionEvidence(
                "Store the captured trailing dword at destination +0xC",
                DestinationTrailingDwordStoreRawFileOffset,
                "89470C",
                ThisRelativeOffset: TrailingDwordOffset)
        });

    public static uint ToCodeVirtualAddress(uint rawFileOffset) =>
        checked(rawFileOffset + CodeVirtualAddressOffsetFromRawFileOffset);

    public static UiShaderSourceCopyProjection Project(UiShaderSourceCopyCapture capture)
    {
        var actions = new List<UiShaderSourceCopyAction>
        {
            new(
                UiShaderSourceCopyActionKind.WriteDestinationVtable,
                DestinationVtableStoreRawFileOffset,
                capture.DestinationPointer,
                WrapperVtableVirtualAddress),
            new(
                UiShaderSourceCopyActionKind.ZeroDestinationBackingPointer,
                DestinationBackingZeroRawFileOffset,
                unchecked(capture.DestinationPointer + BackingPointerOffset),
                0),
            new(
                UiShaderSourceCopyActionKind.ZeroDestinationControlPointer,
                DestinationControlZeroRawFileOffset,
                unchecked(capture.DestinationPointer + ControlPointerOffset),
                0),
            new(
                UiShaderSourceCopyActionKind.CallPairAssignmentHelper,
                PairAssignmentCallRawFileOffset,
                ToCodeVirtualAddress(PairAssignmentHelperRawFileOffset)),
            new(
                UiShaderSourceCopyActionKind.CaptureSourceControlPointer,
                SourceControlLoadRawFileOffset,
                unchecked(capture.SourcePointer + ControlPointerOffset),
                capture.SourceControlPointer),
            new(
                UiShaderSourceCopyActionKind.CaptureSourceBackingPointer,
                SourceBackingLoadRawFileOffset,
                unchecked(capture.SourcePointer + BackingPointerOffset),
                capture.SourceBackingPointer)
        };

        if (capture.SourceControlPointer != 0)
        {
            actions.Add(new UiShaderSourceCopyAction(
                UiShaderSourceCopyActionKind.IncrementCapturedControlReferenceCount,
                ReferenceCountIncrementRawFileOffset,
                unchecked(capture.SourceControlPointer + ReferenceCountOffset),
                1));
        }

        actions.Add(new UiShaderSourceCopyAction(
            UiShaderSourceCopyActionKind.ReadFreshDestinationControlPointer,
            FreshDestinationControlLoadRawFileOffset,
            unchecked(capture.DestinationPointer + ControlPointerOffset),
            0));
        actions.Add(new UiShaderSourceCopyAction(
            UiShaderSourceCopyActionKind.SkipDestinationControlRelease,
            FreshDestinationControlBranchRawFileOffset,
            ToCodeVirtualAddress(DestinationControlReleaseSkippedTargetRawFileOffset),
            0));
        actions.Add(new UiShaderSourceCopyAction(
            UiShaderSourceCopyActionKind.WriteDestinationControlPointer,
            DestinationControlStoreRawFileOffset,
            unchecked(capture.DestinationPointer + ControlPointerOffset),
            capture.SourceControlPointer));
        actions.Add(new UiShaderSourceCopyAction(
            UiShaderSourceCopyActionKind.WriteDestinationBackingPointer,
            DestinationBackingStoreRawFileOffset,
            unchecked(capture.DestinationPointer + BackingPointerOffset),
            capture.SourceBackingPointer));
        actions.Add(new UiShaderSourceCopyAction(
            UiShaderSourceCopyActionKind.CaptureSourceTrailingDword,
            SourceTrailingDwordLoadRawFileOffset,
            unchecked(capture.SourcePointer + TrailingDwordOffset),
            capture.SourceTrailingDword));
        actions.Add(new UiShaderSourceCopyAction(
            UiShaderSourceCopyActionKind.WriteDestinationTrailingDword,
            DestinationTrailingDwordStoreRawFileOffset,
            unchecked(capture.DestinationPointer + TrailingDwordOffset),
            capture.SourceTrailingDword));

        return new UiShaderSourceCopyProjection(capture, actions.AsReadOnly());
    }
}
