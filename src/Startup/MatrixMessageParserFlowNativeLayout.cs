namespace Firefall.Reconstruction.Startup;

public enum MatrixMessageVtableSlotRole
{
    WriteToStream,
    ReadFromStream,
    DeletingDestructor
}

public readonly record struct MatrixMessageVtableSlotSnapshot(
    int SlotOffset,
    uint MethodFileOffset,
    MatrixMessageVtableSlotRole Role);

public readonly record struct MatrixDialogMessageVtableSnapshot(
    string RttiName,
    uint VtableVirtualAddress,
    IReadOnlyList<int> SerializedFieldOffsets,
    IReadOnlyList<MatrixMessageVtableSlotSnapshot> PrimarySlots);

/// <summary>
/// Records parser return pass-through and the observed primary dialog-message
/// vftable slots without claiming an application-level consumer.
/// </summary>
public static class MatrixMessageParserFlowNativeLayout
{
    public const uint NestedCommandParserFileOffset = 0x81CC50;
    public const uint StreamParserCallerFileOffset = 0x821BA0;
    public const uint StreamParserCallSiteFileOffset = 0x821D87;
    public const uint StreamParserCallerVirtualAddress = 0xC227A0;
    public const uint ParserWrapperFileOffset = 0x821DE0;
    public const uint ParserWrapperCallSiteFileOffset = 0x821E1E;
    public const uint ParserWrapperVirtualAddress = 0xC229E0;
    public const uint ReplayReaderFunctionArgumentFileOffset = 0x65DAEA;
    public const uint ReplayReaderRegistrationCallFileOffset = 0x65DAF2;
    public const uint ReplayReaderRegistrationHelperFileOffset = 0x12B7FB0;
    public const uint ReplayReaderCallbackObjectCallFileOffset = 0x65DB3F;
    public const uint CallbackObjectConstructorFileOffset = 0x7FD670;
    public const uint CallbackObjectParserPointerStoreFileOffset = 0x7FD820;
    public const int CallbackObjectParserPointerFieldOffset = 0x10;

    public static IReadOnlyList<MatrixParserCallSiteSnapshot> DirectParserCallSites { get; } =
        Array.AsReadOnly(new[]
        {
            new MatrixParserCallSiteSnapshot(StreamParserCallerFileOffset, StreamParserCallSiteFileOffset),
            new MatrixParserCallSiteSnapshot(ParserWrapperFileOffset, ParserWrapperCallSiteFileOffset)
        });

    public static IReadOnlyList<MatrixDialogMessageVtableSnapshot> DialogMessageVtables { get; } =
        Array.AsReadOnly(new[]
        {
            CreateDialogVtable(
                ".?AVOpenMovieDialog@Character@firefall@@",
                0x1C67028,
                0x5EEC40,
                0x5EEAE0,
                0x526770,
                0x28),
            CreateDialogVtable(
                ".?AVPrivateDialog@Character@firefall@@",
                0x1C67018,
                0x5EEA40,
                0x5EE930,
                0x526880,
                0x28, 0x30, 0x38),
            CreateDialogVtable(
                ".?AVPublicDialog@Character@firefall@@",
                0x1C66FF8,
                0x5EE830,
                0x5EE760,
                0x526900,
                0x28, 0x2C)
        });

    private static MatrixDialogMessageVtableSnapshot CreateDialogVtable(
        string rttiName,
        uint vtableVirtualAddress,
        uint writerFileOffset,
        uint readerFileOffset,
        uint destructorFileOffset,
        params int[] serializedFieldOffsets) =>
        new(
            rttiName,
            vtableVirtualAddress,
            Array.AsReadOnly(serializedFieldOffsets),
            Array.AsReadOnly(new[]
            {
                new MatrixMessageVtableSlotSnapshot(0, writerFileOffset, MatrixMessageVtableSlotRole.WriteToStream),
                new MatrixMessageVtableSlotSnapshot(4, readerFileOffset, MatrixMessageVtableSlotRole.ReadFromStream),
                new MatrixMessageVtableSlotSnapshot(8, destructorFileOffset, MatrixMessageVtableSlotRole.DeletingDestructor)
            }));
}

public readonly record struct MatrixParserCallSiteSnapshot(
    uint CallerFileOffset,
    uint CallSiteFileOffset);
