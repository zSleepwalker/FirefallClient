namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Static wrapper and backing-record layout anchors. The wrapper's virtual slots and backing-record fields
/// are named separately even where their numeric offsets happen to match.
/// </summary>
public static class UiShaderSourceWrapperNativeLayout
{
    public const uint WrapperVtableVirtualAddress = 0x1C80CC4;
    public const uint WrapperVtableRva = 0x1880CC4;
    public const uint WrapperVtableRawFileOffset = 0x187F8C4;

    public const uint WrapperVtableSlot4EntryVirtualAddress = 0x16CC560;
    public const uint WrapperVtableSlot4EntryRawFileOffset = 0x12CB960;
    public const uint WrapperVtableSlotCEntryVirtualAddress = 0x16CC5C0;
    public const uint WrapperVtableSlotCEntryRawFileOffset = 0x12CB9C0;

    public const int WrapperVtableSlot4Offset = 0x04;
    public const int WrapperVtableSlotCOffset = 0x0C;
    public const int BackingPointerFieldOffset = 0x04;
    public const int BackingDataPointerFieldOffset = 0x04;
    public const int BackingSizeFieldOffset = 0x0C;
}

/// <summary>
/// Returns only the size already captured from the backing record. This does not model wrapper slot 4's
/// unresolved argument/resize behavior.
/// </summary>
public static class UiShaderSourceWrapperSizeProjectionRules
{
    public static uint ReadCapturedSize(uint capturedBackingPointer, uint? capturedBackingFieldC)
    {
        if (capturedBackingPointer == 0)
        {
            if (capturedBackingFieldC.HasValue)
                throw new ArgumentException(
                    "A null backing pointer cannot have a captured backing-field size.",
                    nameof(capturedBackingFieldC));

            return 0;
        }

        return capturedBackingFieldC
            ?? throw new ArgumentException(
                "A nonnull backing pointer requires a captured backing-field size.",
                nameof(capturedBackingFieldC));
    }
}
