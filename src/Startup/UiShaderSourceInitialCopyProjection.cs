namespace Firefall.Reconstruction.Startup;

public readonly record struct UiShaderSourceInitialCopyCapture(
    uint DestinationPointer,
    uint SourcePointer,
    uint RequestedSize,
    uint? PostResizeBackingPointer,
    uint? PostResizeBackingDataPointer);

public enum UiShaderSourceInitialCopyActionKind
{
    WriteDestinationVtable,
    ZeroDestinationBackingPointer,
    ZeroDestinationControlPointer,
    ZeroDestinationTrailingDword,
    CallResizeWithOptionalPreserve,
    CapturePostResizeBackingPointer,
    SkipCopyForNullBackingPointer,
    CaptureBackingDataPointer,
    PushRequestedSizeToCopyThunk,
    PushSourcePointerToCopyThunk,
    PushBackingDataPointerToCopyThunk,
    CallCopyThunk
}

public readonly record struct UiShaderSourceInitialCopyAction(
    UiShaderSourceInitialCopyActionKind Kind,
    uint InstructionRawFileOffset,
    uint? TargetVirtualAddress = null,
    uint? Value = null,
    uint? Argument0 = null,
    uint? Argument1 = null,
    uint? Argument2 = null);

public readonly record struct UiShaderSourceInitialCopyProjection(
    UiShaderSourceInitialCopyCapture Capture,
    IReadOnlyList<UiShaderSourceInitialCopyAction> Actions);

public readonly record struct UiShaderSourceInitialCopyInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string BytesHex,
    int? ThisRelativeOffset = null,
    uint? TargetVirtualAddress = null,
    uint? BranchTargetRawFileOffset = null);

/// <summary>
/// Projects the constructor stores, resize request, and the later backing-pointer gate.
/// Optional backing captures distinguish an unreached read from an observed null value.
/// The copy-thunk action records only its observed call and arguments; it does not execute
/// the thunk or copy bytes, and a non-null backing pointer is not treated as allocation success.
/// </summary>
public static class UiShaderSourceInitialCopyProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint ImageBase = 0x00400000;
    public const uint TextSectionRva = 0x00001000;
    public const uint TextSectionRawFileOffset = 0x00000400;
    public const uint CodeVirtualAddressOffsetFromRawFileOffset =
        ImageBase + TextSectionRva - TextSectionRawFileOffset;

    public const uint ConstructorEntryRawFileOffset = 0x12CB830;
    public const uint DestinationVtableStoreRawFileOffset = 0x12CB860;
    public const uint DestinationBackingZeroRawFileOffset = 0x12CB866;
    public const uint DestinationControlZeroRawFileOffset = 0x12CB86D;
    public const uint PreserveFlagPushRawFileOffset = 0x12CB874;
    public const uint RequestedSizePushToResizeRawFileOffset = 0x12CB876;
    public const uint DestinationTrailingDwordZeroRawFileOffset = 0x12CB87D;
    public const uint ResizeHelperCallRawFileOffset = 0x12CB884;
    public const uint PostResizeBackingLoadRawFileOffset = 0x12CB889;
    public const uint PostResizeBackingNullTestRawFileOffset = 0x12CB88C;
    public const uint PostResizeBackingNullBranchRawFileOffset = 0x12CB88E;
    public const uint CopySizePushRawFileOffset = 0x12CB890;
    public const uint CopySourcePushRawFileOffset = 0x12CB893;
    public const uint CopyBackingDataPushRawFileOffset = 0x12CB896;
    public const uint CopyThunkCallRawFileOffset = 0x12CB899;

    public const uint ResizeHelperRawFileOffset = 0x12CBCB0;
    public const uint CopyThunkRawFileOffset = 0x159D8F4;
    public const uint WrapperVtableVirtualAddress = 0x1C80CC4;
    public const int BackingPointerOffset = 0x04;
    public const int ControlPointerOffset = 0x08;
    public const int TrailingDwordOffset = 0x0C;
    public const int BackingDataPointerOffset = 0x04;
    public const uint ResizePreserveFlag = 0;
    public const uint NullBackingBranchTargetRawFileOffset = 0x12CB8A1;

    public static IReadOnlyList<UiShaderSourceInitialCopyInstructionEvidence> Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourceInitialCopyInstructionEvidence(
                "Install the destination wrapper vtable",
                DestinationVtableStoreRawFileOffset,
                "C706C40CC801",
                ThisRelativeOffset: 0,
                TargetVirtualAddress: WrapperVtableVirtualAddress),
            new UiShaderSourceInitialCopyInstructionEvidence(
                "Zero destination backing pointer before resizing",
                DestinationBackingZeroRawFileOffset,
                "C7460400000000",
                ThisRelativeOffset: BackingPointerOffset),
            new UiShaderSourceInitialCopyInstructionEvidence(
                "Zero destination control pointer before resizing",
                DestinationControlZeroRawFileOffset,
                "C7460800000000",
                ThisRelativeOffset: ControlPointerOffset),
            new UiShaderSourceInitialCopyInstructionEvidence(
                "Push zero as the optional-preserve flag",
                PreserveFlagPushRawFileOffset,
                "6A00"),
            new UiShaderSourceInitialCopyInstructionEvidence(
                "Pass the requested size to the resize helper",
                RequestedSizePushToResizeRawFileOffset,
                "FF750C"),
            new UiShaderSourceInitialCopyInstructionEvidence(
                "Zero destination trailing dword before the resize call",
                DestinationTrailingDwordZeroRawFileOffset,
                "C7460C00000000",
                ThisRelativeOffset: TrailingDwordOffset),
            new UiShaderSourceInitialCopyInstructionEvidence(
                "Call the resize helper with requested size and preserve flag zero",
                ResizeHelperCallRawFileOffset,
                "E827040000",
                TargetVirtualAddress: ToCodeVirtualAddress(ResizeHelperRawFileOffset)),
            new UiShaderSourceInitialCopyInstructionEvidence(
                "Load destination backing pointer after the resize helper returns",
                PostResizeBackingLoadRawFileOffset,
                "8B4604",
                ThisRelativeOffset: BackingPointerOffset),
            new UiShaderSourceInitialCopyInstructionEvidence(
                "Test the captured post-resize backing pointer",
                PostResizeBackingNullTestRawFileOffset,
                "85C0"),
            new UiShaderSourceInitialCopyInstructionEvidence(
                "Skip the copy-thunk path only when the backing pointer is null",
                PostResizeBackingNullBranchRawFileOffset,
                "7411",
                BranchTargetRawFileOffset: NullBackingBranchTargetRawFileOffset),
            new UiShaderSourceInitialCopyInstructionEvidence(
                "Push the requested size for the copy thunk",
                CopySizePushRawFileOffset,
                "FF750C"),
            new UiShaderSourceInitialCopyInstructionEvidence(
                "Push the independent source pointer for the copy thunk",
                CopySourcePushRawFileOffset,
                "FF7508"),
            new UiShaderSourceInitialCopyInstructionEvidence(
                "Push backing data pointer from the captured backing object's +4",
                CopyBackingDataPushRawFileOffset,
                "FF7004",
                ThisRelativeOffset: BackingDataPointerOffset),
            new UiShaderSourceInitialCopyInstructionEvidence(
                "Call the copy thunk after the non-null backing test",
                CopyThunkCallRawFileOffset,
                "E856202D00",
                TargetVirtualAddress: ToCodeVirtualAddress(CopyThunkRawFileOffset))
        });

    public static uint ToCodeVirtualAddress(uint rawFileOffset) =>
        checked(rawFileOffset + CodeVirtualAddressOffsetFromRawFileOffset);

    public static UiShaderSourceInitialCopyProjection Project(
        UiShaderSourceInitialCopyCapture capture)
    {
        ValidateReachedBackingCaptures(capture);

        var actions = new List<UiShaderSourceInitialCopyAction>
        {
            new(
                UiShaderSourceInitialCopyActionKind.WriteDestinationVtable,
                DestinationVtableStoreRawFileOffset,
                capture.DestinationPointer,
                WrapperVtableVirtualAddress),
            new(
                UiShaderSourceInitialCopyActionKind.ZeroDestinationBackingPointer,
                DestinationBackingZeroRawFileOffset,
                unchecked(capture.DestinationPointer + BackingPointerOffset),
                0),
            new(
                UiShaderSourceInitialCopyActionKind.ZeroDestinationControlPointer,
                DestinationControlZeroRawFileOffset,
                unchecked(capture.DestinationPointer + ControlPointerOffset),
                0),
            new(
                UiShaderSourceInitialCopyActionKind.ZeroDestinationTrailingDword,
                DestinationTrailingDwordZeroRawFileOffset,
                unchecked(capture.DestinationPointer + TrailingDwordOffset),
                0),
            new(
                UiShaderSourceInitialCopyActionKind.CallResizeWithOptionalPreserve,
                ResizeHelperCallRawFileOffset,
                ToCodeVirtualAddress(ResizeHelperRawFileOffset),
                Argument0: capture.RequestedSize,
                Argument1: ResizePreserveFlag)
        };

        if (capture.PostResizeBackingPointer is not uint backingPointer)
        {
            return new UiShaderSourceInitialCopyProjection(capture, actions.AsReadOnly());
        }

        actions.Add(new UiShaderSourceInitialCopyAction(
            UiShaderSourceInitialCopyActionKind.CapturePostResizeBackingPointer,
            PostResizeBackingLoadRawFileOffset,
            unchecked(capture.DestinationPointer + BackingPointerOffset),
            backingPointer));

        if (backingPointer == 0)
        {
            actions.Add(new UiShaderSourceInitialCopyAction(
                UiShaderSourceInitialCopyActionKind.SkipCopyForNullBackingPointer,
                PostResizeBackingNullBranchRawFileOffset,
                ToCodeVirtualAddress(NullBackingBranchTargetRawFileOffset),
                0));
            return new UiShaderSourceInitialCopyProjection(capture, actions.AsReadOnly());
        }

        uint backingDataPointer = capture.PostResizeBackingDataPointer!.Value;
        actions.Add(new UiShaderSourceInitialCopyAction(
            UiShaderSourceInitialCopyActionKind.PushRequestedSizeToCopyThunk,
            CopySizePushRawFileOffset,
            Value: capture.RequestedSize));
        actions.Add(new UiShaderSourceInitialCopyAction(
            UiShaderSourceInitialCopyActionKind.PushSourcePointerToCopyThunk,
            CopySourcePushRawFileOffset,
            Value: capture.SourcePointer));
        actions.Add(new UiShaderSourceInitialCopyAction(
            UiShaderSourceInitialCopyActionKind.CaptureBackingDataPointer,
            CopyBackingDataPushRawFileOffset,
            unchecked(backingPointer + BackingDataPointerOffset),
            backingDataPointer));
        actions.Add(new UiShaderSourceInitialCopyAction(
            UiShaderSourceInitialCopyActionKind.PushBackingDataPointerToCopyThunk,
            CopyBackingDataPushRawFileOffset,
            Value: backingDataPointer));
        actions.Add(new UiShaderSourceInitialCopyAction(
            UiShaderSourceInitialCopyActionKind.CallCopyThunk,
            CopyThunkCallRawFileOffset,
            ToCodeVirtualAddress(CopyThunkRawFileOffset),
            Argument0: backingDataPointer,
            Argument1: capture.SourcePointer,
            Argument2: capture.RequestedSize));

        return new UiShaderSourceInitialCopyProjection(capture, actions.AsReadOnly());
    }

    private static void ValidateReachedBackingCaptures(UiShaderSourceInitialCopyCapture capture)
    {
        if (capture.PostResizeBackingPointer is not uint backingPointer)
        {
            if (capture.PostResizeBackingDataPointer is not null)
            {
                throw new ArgumentException(
                    "Backing data is captured only after the backing pointer is read and found non-null.",
                    nameof(capture));
            }

            return;
        }

        if (backingPointer == 0 && capture.PostResizeBackingDataPointer is not null)
        {
            throw new ArgumentException(
                "Backing data is not read when the captured backing pointer is null.",
                nameof(capture));
        }

        if (backingPointer != 0 && capture.PostResizeBackingDataPointer is null)
        {
            throw new ArgumentException(
                "Backing data is captured on the non-null backing path.",
                nameof(capture));
        }
    }
}
