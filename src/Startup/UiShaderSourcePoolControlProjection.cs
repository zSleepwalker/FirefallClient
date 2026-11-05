using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public enum UiShaderSourcePoolControlOutputWriteTarget
{
    ControlPointer,
    HeaderPointer
}

public enum UiShaderSourcePoolControlVariant
{
    Pool,
    NonPool
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
    ImmutableArray<UiShaderSourcePoolControlOutputWrite> OutputPairWrites)
{
    public UiShaderSourcePoolControlVariant Variant { get; init; }
    public uint ControlAllocationRequestSize { get; init; }
    public uint ControlVtableVa { get; init; }
}

/// <summary>
/// Projects the helper's captured control and output writes without modeling allocation or ownership.
/// </summary>
public static class UiShaderSourcePoolControlProjectionRules
{
    public const uint PoolHelperRawFileOffset = 0x12CC700;
    public const uint PoolControlAllocationCallRawFileOffset = 0x12CC739;
    public const uint PoolControlPrimaryCountStoreRawFileOffset = 0x12CC74A;
    public const uint PoolControlSecondaryCountStoreRawFileOffset = 0x12CC751;
    public const uint PoolControlVtableStoreRawFileOffset = 0x12CC758;
    public const uint PoolControlHeaderStoreRawFileOffset = 0x12CC75E;
    public const uint PoolOutputControlStoreRawFileOffset = 0x12CC794;
    public const uint PoolOutputHeaderStoreRawFileOffset = 0x12CC797;
    public const uint PoolControlAllocationRequestSize = 0x14;
    public const uint PoolControlVtableVa = 0x1C80C8C;

    public const uint NonPoolHelperRawFileOffset = 0x12CC7D0;
    public const uint NonPoolControlAllocationCallRawFileOffset = 0x12CC809;
    public const uint NonPoolControlPrimaryCountStoreRawFileOffset = 0x12CC81A;
    public const uint NonPoolControlSecondaryCountStoreRawFileOffset = 0x12CC821;
    public const uint NonPoolControlVtableStoreRawFileOffset = 0x12CC828;
    public const uint NonPoolControlHeaderStoreRawFileOffset = 0x12CC82E;
    public const uint NonPoolOutputControlStoreRawFileOffset = 0x12CC864;
    public const uint NonPoolOutputHeaderStoreRawFileOffset = 0x12CC867;
    public const uint NonPoolControlAllocationRequestSize = 0x10;
    public const uint NonPoolControlVtableVa = 0x1C80CB0;

    // Preserve the original pool-specific constants for existing callers.
    public const uint HelperRawFileOffset = PoolHelperRawFileOffset;
    public const uint ControlAllocationCallRawFileOffset = PoolControlAllocationCallRawFileOffset;
    public const uint ControlPrimaryCountStoreRawFileOffset = PoolControlPrimaryCountStoreRawFileOffset;
    public const uint ControlSecondaryCountStoreRawFileOffset = PoolControlSecondaryCountStoreRawFileOffset;
    public const uint ControlVtableStoreRawFileOffset = PoolControlVtableStoreRawFileOffset;
    public const uint ControlHeaderStoreRawFileOffset = PoolControlHeaderStoreRawFileOffset;
    public const uint OutputControlStoreRawFileOffset = PoolOutputControlStoreRawFileOffset;
    public const uint OutputHeaderStoreRawFileOffset = PoolOutputHeaderStoreRawFileOffset;
    public const uint ControlAllocationRequestSize = PoolControlAllocationRequestSize;
    public const uint ControlVtableVa = PoolControlVtableVa;

    public const int ControlPrimaryCountOffset = 0x04;
    public const int ControlSecondaryCountOffset = 0x08;
    public const int ControlVtableOffset = 0;
    public const int ControlHeaderPointerOffset = 0x0C;
    public const int OutputPairHeaderPointerOffset = 0;
    public const int OutputPairControlPointerOffset = 0x04;

    public static UiShaderSourcePoolControlProjection Project(
        uint capturedHeaderPointer,
        uint capturedControlPointer,
        UiShaderSourcePoolControlVariant variant = UiShaderSourcePoolControlVariant.Pool)
    {
        (uint allocationRequestSize, uint vtableVa) = variant switch
        {
            UiShaderSourcePoolControlVariant.Pool =>
                (PoolControlAllocationRequestSize, PoolControlVtableVa),
            UiShaderSourcePoolControlVariant.NonPool =>
                (NonPoolControlAllocationRequestSize, NonPoolControlVtableVa),
            _ => throw new ArgumentOutOfRangeException(nameof(variant))
        };

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
                        vtableVa),
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
            outputPairWrites)
        {
            Variant = variant,
            ControlAllocationRequestSize = allocationRequestSize,
            ControlVtableVa = vtableVa
        };
    }
}
