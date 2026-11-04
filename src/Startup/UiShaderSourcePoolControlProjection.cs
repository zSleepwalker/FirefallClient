using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public enum UiShaderSourcePoolControlOutputWriteTarget
{
    ControlPointer,
    HeaderPointer
}

public readonly record struct UiShaderSourcePoolControlFieldWrite(
    int Offset,
    uint Value);

public readonly record struct UiShaderSourcePoolControlOutputWrite(
    UiShaderSourcePoolControlOutputWriteTarget Target,
    uint Value);

public readonly record struct UiShaderSourcePoolControlProjection(
    UiShaderSourceAllocationOutputPair CapturedOutputPair,
    ImmutableArray<UiShaderSourcePoolControlFieldWrite> ControlFieldWrites,
    ImmutableArray<UiShaderSourcePoolControlOutputWrite> OutputPairWrites);

/// <summary>
/// Projects the helper's captured control and output writes without modeling allocation or ownership.
/// </summary>
public static class UiShaderSourcePoolControlProjectionRules
{
    public const uint HelperRawFileOffset = 0x12CC700;
    public const uint ControlAllocationCallRawFileOffset = 0x12CC739;
    public const uint ControlPrimaryCountStoreRawFileOffset = 0x12CC74A;
    public const uint ControlSecondaryCountStoreRawFileOffset = 0x12CC751;
    public const uint ControlVtableStoreRawFileOffset = 0x12CC758;
    public const uint ControlHeaderStoreRawFileOffset = 0x12CC75E;
    public const uint OutputControlStoreRawFileOffset = 0x12CC794;
    public const uint OutputHeaderStoreRawFileOffset = 0x12CC797;

    public const uint ControlAllocationRequestSize = 0x14;
    public const uint ControlVtableVa = 0x1C80C8C;
    public const int ControlPrimaryCountOffset = 0x04;
    public const int ControlSecondaryCountOffset = 0x08;
    public const int ControlVtableOffset = 0;
    public const int ControlHeaderPointerOffset = 0x0C;
    public const int OutputPairHeaderPointerOffset = 0;
    public const int OutputPairControlPointerOffset = 0x04;

    public static UiShaderSourcePoolControlProjection Project(
        uint capturedHeaderPointer,
        uint capturedControlPointer)
    {
        ImmutableArray<UiShaderSourcePoolControlFieldWrite> controlFieldWrites =
            capturedControlPointer == 0
                ? ImmutableArray<UiShaderSourcePoolControlFieldWrite>.Empty
                : ImmutableArray.Create(
                    new UiShaderSourcePoolControlFieldWrite(
                        ControlPrimaryCountOffset,
                        1),
                    new UiShaderSourcePoolControlFieldWrite(
                        ControlSecondaryCountOffset,
                        1),
                    new UiShaderSourcePoolControlFieldWrite(
                        ControlVtableOffset,
                        ControlVtableVa),
                    new UiShaderSourcePoolControlFieldWrite(
                        ControlHeaderPointerOffset,
                        capturedHeaderPointer));

        ImmutableArray<UiShaderSourcePoolControlOutputWrite> outputPairWrites =
            ImmutableArray.Create(
                new UiShaderSourcePoolControlOutputWrite(
                    UiShaderSourcePoolControlOutputWriteTarget.ControlPointer,
                    capturedControlPointer),
                new UiShaderSourcePoolControlOutputWrite(
                    UiShaderSourcePoolControlOutputWriteTarget.HeaderPointer,
                    capturedHeaderPointer));

        return new UiShaderSourcePoolControlProjection(
            new UiShaderSourceAllocationOutputPair(
                capturedHeaderPointer,
                capturedControlPointer),
            controlFieldWrites,
            outputPairWrites);
    }
}
