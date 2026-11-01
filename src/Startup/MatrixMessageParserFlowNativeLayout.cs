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

public enum MatrixCallbackContextVtableSlotRole
{
    LogOnly,
    ModeDependentMessageDispatch
}

public readonly record struct MatrixCallbackContextVtableSlotSnapshot(
    int SlotOffset,
    uint MethodVirtualAddress,
    uint MethodFileOffset,
    MatrixCallbackContextVtableSlotRole Role);

public readonly record struct MatrixCallbackContextVtableSnapshot(
    string RttiName,
    uint VtableVirtualAddress,
    int ObjectVtableFieldOffset,
    IReadOnlyList<MatrixCallbackContextVtableSlotSnapshot> PrimarySlots);

/// <summary>
/// Records parser return pass-through, the ReplayReader callback registration,
/// invocation, result-queue path, and primary dialog-message vftable slots.
/// The ReplayReader context's primary dispatch methods and final-release path
/// are recorded without assigning semantic meanings to the message tags.
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
    public const uint ReplayReaderCallbackRecordFunctionPointerStoreFileOffset = 0x65DAEA;
    public const uint ReplayReaderAllocationCallFileOffset = 0x65DAF2;
    public const uint ReplayReaderAllocationHelperFileOffset = 0x12B7FB0;
    public const uint ReplayReaderAllocatorBackendCallFileOffset = 0x12B7FC7;
    public const uint ReplayReaderAllocatorBackendFileOffset = 0x12C8A20;
    public const uint ReplayReaderCallbackObjectCallFileOffset = 0x65DB3F;
    public const uint ReplayReaderCallbackRegistrationCallFileOffset = 0x65DBD3;
    public const uint ReplayReaderCallbackRegistrationHelperFileOffset = 0xBD6260;
    public const uint ReplayReaderCallbackRegistrationFieldStoresFileOffset = 0xBD62AE;
    public const uint ReplayReaderReadinessProbeFileOffset = 0xBD68D0;
    public const uint ReplayReaderReadinessProbeCallFileOffset = 0xBD68F4;
    public const uint RegisteredParserInvokerFileOffset = 0xBD6B40;
    public const uint RegisteredParserCallbackLoadFileOffset = 0xBD6D29;
    public const uint RegisteredParserCallbackCallFileOffset = 0xBD6D3C;
    public const uint CallbackResultQueueInsertionCallFileOffset = 0xBD6D66;
    public const uint CallbackResultQueueHelperFileOffset = 0xBD8B80;
    public const uint CallbackResultConsumerFileOffset = 0xBD62F0;
    public const uint CallbackResultConsumerQueueFieldSetupFileOffset = 0xBD65B6;
    public const uint CallbackResultDispatchQueueFieldSetupFileOffset = 0xBD6615;
    public const uint CallbackResultDispatchQueueInsertionCallFileOffset = 0xBD661B;
    public const uint CallbackResultConsumerContextLoadFileOffset = 0xBD66B3;
    public const uint CallbackResultConsumerContextDispatchFileOffset = 0xBD66F2;
    public const uint CallbackResultConsumerAlternateContextDispatchFileOffset = 0xBD6702;
    public const uint CallbackResultConsumerContextDispatchOtherFileOffset = 0xBD6710;
    public const uint CallbackResultReferenceDecrementFileOffset = 0xBD6721;
    public const uint CallbackResultFinalReleaseCallFileOffset = 0xBD6733;
    public const uint CallbackResultQueueRecordCopyFileOffset = 0xBD8BE3;
    public const uint ReplayReaderContextPointerStoreFileOffset = 0x65DB75;
    public const int CallbackResultQueueFieldOffset = 0xB4;
    public const int CallbackResultDispatchQueueFieldOffset = 0xC8;
    public const int ReplayReaderCallbackRecordParserFieldOffset = 4;
    public const int ReplayReaderRegisteredParserFieldOffset = 0x1C;
    public const int ReplayReaderRegisteredContextFieldOffset = 0x20;
    public const int ReplayReaderContextPointerOwnerFieldOffset = 0x54;
    public const uint CallbackObjectConstructorFileOffset = 0x7FD670;
    public const uint CallbackObjectVtableStoreFileOffset = 0x7FD6B7;
    public const uint CallbackObjectParserPointerStoreFileOffset = 0x7FD820;
    public const uint ReplayReaderContextModeFlagSetFileOffset = 0x65DBC5;
    public const int CallbackObjectVtableFieldOffset = 0;
    public const int CallbackObjectParserPointerFieldOffset = 0x10;
    public const int CallbackObjectModeFlagFieldOffset = 0x18;
    public const int CallbackObjectDispatchSinkFieldOffset = 8;
    public const int CallbackObjectAlternateDispatchSinkFieldOffset = 0xB0;
    public const uint CallbackContextVtableVirtualAddress = 0x1C6D624;
    public const uint CallbackContextCompleteObjectLocatorVirtualAddress = 0x1CBF794;
    public const int CallbackContextCompleteObjectLocatorObjectOffset = 0;
    public const uint CallbackContextRttiTypeDescriptorVirtualAddress = 0x1E239BC;
    public const string CallbackContextRttiName = ".?AVtfGameSocketHandler@@";
    public const uint CallbackContextLogMethodVirtualAddress = 0xBFE7E0;
    public const uint CallbackContextLogMethodFileOffset = 0x7FDBE0;
    public const uint CallbackContextDispatchMethodVirtualAddress = 0xBFE800;
    public const uint CallbackContextDispatchMethodFileOffset = 0x7FDC00;
    public const uint CallbackContextDispatchHelperFileOffset = 0x7FDC60;
    public const uint CallbackContextMessageLabelLookupFileOffset = 0x809D20;
    public const uint CallbackContextNestedMessageParserFileOffset = 0xBCA850;
    public const uint CallbackContextMessageConsumerFileOffset = 0x81BB90;
    public const uint CallbackContextConditionalDispatchFileOffset = 0xBD7060;

    public static IReadOnlyList<MatrixParserCallSiteSnapshot> DirectParserCallSites { get; } =
        Array.AsReadOnly(new[]
        {
            new MatrixParserCallSiteSnapshot(StreamParserCallerFileOffset, StreamParserCallSiteFileOffset),
            new MatrixParserCallSiteSnapshot(ParserWrapperFileOffset, ParserWrapperCallSiteFileOffset)
        });

    public static MatrixCallbackContextVtableSnapshot ReplayReaderCallbackContextVtable { get; } =
        new(
            CallbackContextRttiName,
            CallbackContextVtableVirtualAddress,
            CallbackObjectVtableFieldOffset,
            Array.AsReadOnly(new[]
            {
                new MatrixCallbackContextVtableSlotSnapshot(
                    4,
                    CallbackContextLogMethodVirtualAddress,
                    CallbackContextLogMethodFileOffset,
                    MatrixCallbackContextVtableSlotRole.LogOnly),
                new MatrixCallbackContextVtableSlotSnapshot(
                    8,
                    CallbackContextDispatchMethodVirtualAddress,
                    CallbackContextDispatchMethodFileOffset,
                    MatrixCallbackContextVtableSlotRole.ModeDependentMessageDispatch)
            }));

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
