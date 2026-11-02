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

public readonly record struct MatrixMessageDispatchVtableCaseSnapshot(
    int MessageDiscriminator,
    uint SwitchTargetVirtualAddress,
    uint SwitchTargetFileOffset,
    uint VtableCallFileOffset,
    int VtableSlotOffset);

public readonly record struct MatrixMessageDiscriminatorProvenanceSnapshot(
    int ConstructionSelector,
    int MessageDiscriminator,
    string MessageRttiName,
    uint ParserCaseFileOffset,
    uint ParserStackLocalDwordStoreFileOffset,
    uint ParserStackLocalDwordValue,
    uint ParserConstructorCallFileOffset,
    uint ParserConstructorResultObjectMoveFileOffset,
    uint ConstructorFileOffset,
    uint ConstructorDiscriminatorStackStoreFileOffset,
    uint ConstructorBaseHelperCallFileOffset,
    uint BaseHelperFileOffset,
    uint BaseHelperAuxiliaryPointerVirtualAddress,
    uint BaseHelperFinalVtableVirtualAddress,
    uint BaseHelperFinalVtableStoreFileOffset,
    uint BaseHelperDiscriminatorFieldStoreFileOffset,
    uint BaseHelperEnvelopeKeyStoresInstructionOffset,
    uint MessageVtableVirtualAddress,
    uint ConstructorPrimaryVtableStoreFileOffset,
    int MessageVtableCompleteObjectLocatorObjectOffset,
    uint ReceiverSwitchTargetVirtualAddress,
    uint ReceiverSwitchTargetFileOffset,
    uint ReceiverVtableCallFileOffset,
    int ReceiverVtableSlotOffset,
    uint ReceiverMethodVirtualAddress,
    uint ReceiverMethodFileOffset);

public enum MatrixMessageReceiverVtableTargetKind
{
    DirectFunction,
    TailJumpThunk
}

public readonly record struct MatrixMessageReceiverDialogSlotSnapshot(
    int MessageDiscriminator,
    int VtableSlotOffset,
    uint MethodVirtualAddress,
    uint MethodFileOffset,
    MatrixMessageReceiverVtableTargetKind TargetKind,
    uint? TailJumpTargetVirtualAddress,
    uint? TailJumpTargetFileOffset);

public readonly record struct MatrixMessageReceiverVtableSnapshot(
    string RttiName,
    uint VtableVirtualAddress,
    uint CompleteObjectLocatorVirtualAddress,
    uint TypeDescriptorVirtualAddress,
    int ObjectVtableFieldOffset,
    int CompleteObjectLocatorObjectOffset,
    uint ConstructorFileOffset,
    IReadOnlyList<MatrixMessageReceiverDialogSlotSnapshot> DialogSlots);

/// <summary>
/// Records parser return pass-through, the ReplayReader callback registration,
/// invocation, result-queue path, and primary dialog-message vftable slots.
/// The ReplayReader context's primary dispatch methods and final-release path,
/// concrete message-recipient construction path, representative message
/// discriminator provenance, and dialog receiver slots are recorded without
/// assigning unverified meanings to fields.
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
    public const uint NpcInteractionDispatchMessageArgumentPushFileOffset = 0x81CA2B;
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
    public const uint CallbackContextDispatchHelperVirtualAddress = 0xBFE860;
    public const uint CallbackContextMessageLabelLookupFileOffset = 0x809D20;
    public const uint CallbackContextNestedMessageParserFileOffset = 0xBCA850;
    public const uint CallbackContextMessageConsumerFileOffset = 0x81BB90;
    public const uint CallbackContextConditionalDispatchFileOffset = 0xBD7060;
    public const int CallbackContextMessageDispatchReceiverFieldOffset = 8;
    public const uint ConditionalFanoutStateLazyConstructionMethodFileOffset = 0x7FE1C0;
    public const uint ConditionalFanoutStateConstructorFileOffset = 0xBD6E10;
    public const int ConditionalFanoutStateAllocationSize = 0x48;
    public const int ConditionalFanoutStateQwordArgumentLowFieldOffset = 0x08;
    public const int ConditionalFanoutStateQwordArgumentHighFieldOffset = 0x0C;
    public const int ConditionalFanoutStateContextPairSourceOffset = 0x0C;
    public const int ConditionalFanoutStateContextPairLowFieldOffset = 0x18;
    public const int ConditionalFanoutStateContextPairHighFieldOffset = 0x1C;
    public const int ConditionalFanoutStateRecipientVectorBeginFieldOffset = 0x2C;
    public const int ConditionalFanoutStateRecipientVectorEndFieldOffset = 0x30;
    public const int ConditionalFanoutStateRecipientVectorCapacityFieldOffset = 0x34;
    public const uint ConditionalFanoutStateRecipientVectorAppendFileOffset = 0xBD8C90;
    public const uint ConditionalFanoutStateRecipientRegisterMethodFileOffset = 0xBD6F30;
    public const uint ConditionalFanoutStateRecipientFanoutMethodFileOffset = 0xBD7460;
    public const int ConditionalFanoutStateRecipientRegistrationVtableSlotOffset = 4;
    public const int ConditionalFanoutStateRecipientDispatchVtableSlotOffset = 0x0C;
    public const int ConditionalFanoutStateRecipientPointerSize = 4;
    public const int ConditionalFanoutStateRecipientDispatchRecordSize = 0x0C;
    public const int DefaultReplayWriterObjectAllocationSize = 0x64;
    public const uint DefaultReplayWriterConstructorFileOffset = 0xBD7EA0;
    public const uint DefaultReplayWriterVtableStoreFileOffset = 0xBD7EE6;
    public const uint DefaultReplayWriterVtableVirtualAddress = 0x1C73ADC;
    public const uint DefaultReplayWriterCompleteObjectLocatorVirtualAddress = 0x1CCDF9C;
    public const uint DefaultReplayWriterTypeDescriptorVirtualAddress = 0x1E2C7D4;
    public const string DefaultReplayWriterRttiName = ".?AVDefaultReplayWriter@nl@@";
    public const uint DefaultReplayWriterFanoutMethodVirtualAddress = 0xFD9010;
    public const uint DefaultReplayWriterFanoutMethodFileOffset = 0xBD8410;
    public const uint DefaultReplayWriterFanoutMethodHelperFileOffset = 0x12A0660;
    public const uint CallbackContextProtocolMessagePointerLoadFileOffset = 0x7FDC6E;
    public const uint CallbackContextMessageDispatchReceiverLoadFileOffset = 0x7FDD47;
    public const uint CallbackContextProtocolMessageDispatchCallFileOffset = 0x7FDD4F;
    public const uint CallbackContextMessageSelectorSwitchFileOffset = 0x81BBBA;
    public const uint MessageDispatcherDiscriminatorLoadFileOffset = 0x81BBB7;
    public const uint MessageDispatcherArgumentLoadFileOffset = 0x81BB95;
    public const uint CallbackContextMessageSelectorTableVirtualAddress = 0xC1D6DC;
    public const uint CallbackContextMessageSelectorTableFileOffset = 0x81CADC;
    public const int CallbackContextMessageSelectorFirstDiscriminator = 0x20;
    public const int CallbackContextMessageSelectorLastDiscriminator = 0x7A;
    public const int CallbackContextMessageSelectorCaseCount = 0x5B;
    public const int MessagePrimaryVtableFieldOffset = 0;
    public const int MessageBaseAuxiliaryPointerFieldOffset = 0xC;
    public const int MessageDiscriminatorFieldOffset = 0x10;
    public const int MessageEnvelopeKeyLowDwordFieldOffset = 0x18;
    public const int MessageEnvelopeKeyHighDwordFieldOffset = 0x1C;
    public const int ParserStackLocalDwordOffset = 0x20;
    public const uint Route2MessagePointerEaxRestoreFileOffset = 0x81EF58;
    public const uint StreamParserCallerPostCallFileOffset = 0x821D8C;
    public const uint ParserWrapperPostCallFileOffset = 0x821E23;
    public const uint RegisteredParserResultEaxMoveFileOffset = 0xBD6D41;
    public const uint RegisteredParserResultRecordStoreFileOffset = 0xBD6D63;
    public const int ForcedMovementInlineStateOffset = 0x28;
    public const uint ForcedMovementInlineStateInitializerFileOffset = 0x92B8A0;
    public const uint ConstructorDiscriminatorStackStoreInstructionOffset = 0x12;
    public const uint ConstructorBaseHelperCallInstructionOffset = 0x18;
    public const uint BaseHelperInitialVtableVirtualAddress = 0x1C61B84;
    public const uint BaseHelperFinalVtableStoreInstructionOffset = 0x5D;
    public const uint BaseHelperDiscriminatorFieldStoreInstructionOffset = 0x21;
    public const uint BaseHelperConstructorArgumentStoresInstructionOffset = 0x28;
    public const uint ReplayReaderMessageRecipientGetterCallFileOffset = 0x65DB10;
    public const uint ReplayReaderMessageRecipientFieldLoadFileOffset = 0x65DB38;
    public const int ReplayReaderMessageRecipientSourceFieldOffset = 0x78;
    public const string ReplayReaderMessageRecipientGetterOwnerRttiName = ".?AVtfLookingGlass@@";
    public const uint ReplayReaderMessageRecipientGetterOwnerVtableVirtualAddress = 0x1C6781C;
    public const uint ReplayReaderMessageRecipientGetterOwnerCompleteObjectLocatorVirtualAddress = 0x1CA6FB4;
    public const uint ReplayReaderMessageRecipientGetterOwnerTypeDescriptorVirtualAddress = 0x1E12C48;
    public const int ReplayReaderMessageRecipientGetterOwnerVtableSlotOffset = 0x24;
    public const uint ReplayReaderMessageRecipientGetterVirtualAddress = 0xA5A370;
    public const uint ReplayReaderMessageRecipientGetterFileOffset = 0x659770;
    public const int ReplayReaderMessageRecipientGetterContextFieldOffset = 0x50;
    public const uint LookingGlassOwnerContextSetupCallFileOffset = 0x65A903;
    public const uint LookingGlassOwnerContextSetupHelperFileOffset = 0x9B2D00;
    public const uint LookingGlassOwnerPointerStoreFileOffset = 0x9B2D22;
    public const uint ReplayMessageOwnerFactoryFileOffset = 0x9B73F0;
    public const uint ReplayMessageOwnerFactoryCallFileOffset = 0x9B2D1A;
    public const uint ReplayMessageOwnerConstructorCallFileOffset = 0x9B743D;
    public const uint ReplayMessageOwnerConstructorFileOffset = 0x9B82F0;
    public const int ReplayMessageOwnerReceiverFieldOffset = 0x78;
    public const uint ReplayMessageOwnerReceiverInitialNullStoreFileOffset = 0x9B8411;
    public const uint ReplayMessageOwnerReceiverConstructorCallFileOffset = 0x9B7B21;
    public const uint ReplayMessageOwnerReceiverFieldStoreFileOffset = 0x9B7B56;
    public const uint ReplayMessageReceiverConstructorFileOffset = 0xAC8CA0;
    public const uint ReplayMessageReceiverConstructorVtableStoreFileOffset = 0xAC8CE2;
    public const uint OpenMovieDialogHandlerOrchestratorCallFileOffset = 0xACE2F3;
    public const uint OpenMovieDialogApplicationDispatcherFileOffset = 0x991320;
    public const uint OpenMovieDialogEventCodeFileOffset = 0x99147C;
    public const int OpenMovieDialogEventCode = 228;
    public const uint PrivateDialogEventCodeFileOffset = 0x99165B;
    public const int PrivateDialogEventCode = 229;
    public const uint PublicDialogDispatchContextFactoryCallFileOffset = 0xACE3A8;
    public const uint PublicDialogApplicationHelperCallFileOffset = 0xACE3AF;
    public const uint PublicDialogEventCodeFileOffset = 0x4D6D43;
    public const int PublicDialogEventCode = 357;

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

    public static IReadOnlyList<MatrixMessageDispatchVtableCaseSnapshot> MessageDispatchVtableCases { get; } =
        Array.AsReadOnly(new[]
        {
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x20, 0xC1C7CD, 0x81BBCD, 0x81BBEC, 4),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x21, 0xC1C7F5, 0x81BBF5, 0x81BC14, 8),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x40, 0xC1CCD0, 0x81C0D0, 0x81C0EF, 0x84),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x53, 0xC1D001, 0x81C401, 0x81C420, 0xD0),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x54, 0xC1D02C, 0x81C42C, 0x81C44B, 0xD4),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x55, 0xC1D057, 0x81C457, 0x81C476, 0xD8),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x56, 0xC1D082, 0x81C482, 0x81C4A1, 0xDC),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x57, 0xC1D0AD, 0x81C4AD, 0x81C4CC, 0xE0),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x58, 0xC1D0D8, 0x81C4D8, 0x81C4F7, 0xE4),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x59, 0xC1D103, 0x81C503, 0x81C522, 0xE8),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x5A, 0xC1D12E, 0x81C52E, 0x81C54D, 0xEC),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x5B, 0xC1D159, 0x81C559, 0x81C578, 0xF0),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x5C, 0xC1D184, 0x81C584, 0x81C5A3, 0xF4),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x5D, 0xC1D1AF, 0x81C5AF, 0x81C5CE, 0xF8),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x5E, 0xC1D1DA, 0x81C5DA, 0x81C5F9, 0xFC),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x5F, 0xC1D205, 0x81C605, 0x81C624, 0x100),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x60, 0xC1D230, 0x81C630, 0x81C64F, 0x104),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x61, 0xC1D25B, 0x81C65B, 0x81C67A, 0x108),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x62, 0xC1D286, 0x81C686, 0x81C6A5, 0x10C),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x63, 0xC1D2B1, 0x81C6B1, 0x81C6D0, 0x110),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x64, 0xC1D2DC, 0x81C6DC, 0x81C6FB, 0x114),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x65, 0xC1D307, 0x81C707, 0x81C726, 0x118),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x66, 0xC1D332, 0x81C732, 0x81C751, 0x11C),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x67, 0xC1D35D, 0x81C75D, 0x81C77C, 0x120),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x68, 0xC1D388, 0x81C788, 0x81C7A7, 0x124),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x69, 0xC1D3B3, 0x81C7B3, 0x81C7D2, 0x128),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x6A, 0xC1D3DE, 0x81C7DE, 0x81C7FD, 0x12C),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x6B, 0xC1D409, 0x81C809, 0x81C828, 0x130),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x6C, 0xC1D434, 0x81C834, 0x81C853, 0x134),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x6D, 0xC1D45F, 0x81C85F, 0x81C87E, 0x138),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x71, 0xC1D50B, 0x81C90B, 0x81C92A, 0x148),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x74, 0xC1D58C, 0x81C98C, 0x81C9AB, 0x154),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x75, 0xC1D5B7, 0x81C9B7, 0x81C9D6, 0x158),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x76, 0xC1D5E2, 0x81C9E2, 0x81CA01, 0x15C),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x77, 0xC1D60D, 0x81CA0D, 0x81CA2C, 0x160),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x78, 0xC1D638, 0x81CA38, 0x81CA57, 0x164),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x79, 0xC1D663, 0x81CA63, 0x81CA82, 0x168),
            new MatrixMessageDispatchVtableCaseSnapshot(
                0x7A, 0xC1D68E, 0x81CA8E, 0x81CAAD, 0x16C)
        });

    public static IReadOnlyList<MatrixMessageDiscriminatorProvenanceSnapshot> MessageDiscriminatorProvenance { get; } =
        Array.AsReadOnly(new[]
        {
            new MatrixMessageDiscriminatorProvenanceSnapshot(
                0x71, 0x71, ".?AVForcedMovement@Character@firefall@@",
                0x81D54F, 0x81D560, 0x79, 0x81D573, 0x81D578,
                0x526230, 0x526242, 0x526248, 0x5A1170, 0x1B5BDBC, 0x1C65AF0, 0x5A11CD, 0x5A1191, 0x28,
                0x1C67098, 0x526252, 0,
                0xC1D50B, 0x81C90B, 0x81C92A, 0x148, 0xECED30, 0xACE130),
            new MatrixMessageDiscriminatorProvenanceSnapshot(
                0x77, 0x77, ".?AVNPCInteraction@Character@firefall@@",
                0x81D73C, 0x81D74A, 0x7F, 0x81D761, 0x81D766,
                0x5266A0, 0x5266B2, 0x5266B8, 0x5A1430, 0x1B5BD84, 0x1C65AD0, 0x5A148D, 0x5A1451, 0x28,
                0x1C67048, 0x5266BD, 0,
                0xC1D60D, 0x81CA0D, 0x81CA2C, 0x160, 0xECEEA0, 0xACE2A0),
            new MatrixMessageDiscriminatorProvenanceSnapshot(
                0x79, 0x79, ".?AVPrivateDialog@Character@firefall@@",
                0x81D79E, 0x81D7AC, 0x81, 0x81D7BF, 0x81D7C4,
                0x526840, 0x526852, 0x526858, 0x5A1510, 0x1B5BD7C, 0x1C65A70, 0x5A156D, 0x5A1531, 0x28,
                0x1C67018, 0x52685D, 0,
                0xC1D663, 0x81CA63, 0x81CA82, 0x168, 0xECEF00, 0xACE300),
            new MatrixMessageDiscriminatorProvenanceSnapshot(
                0x53, 0x53, ".?AVLootObjectCollected@AreaVisualData@firefall@@",
                0x81EF1C, 0x81EF2A, 0xCD, 0x81EF3D, 0x81EF42,
                0xADBAA0, 0xADBAB2, 0xADBAB8, 0xB07660, 0x1BB0224, 0x1C72204, 0xB076BD, 0xB07681, 0x28,
                0x1C723C0, 0xADBABD, 0,
                0xC1D001, 0x81C401, 0x81C420, 0xD0, 0xECB180, 0xACA580),
            new MatrixMessageDiscriminatorProvenanceSnapshot(
                0x53, 0x53, ".?AVAbilityActivated@Vehicle@firefall@@",
                0x81EF83, 0x81EF91, 0xD0, 0x81EFA4, 0x81EFA9,
                0xB093C0, 0xB093D2, 0xB093D8, 0xB474B0, 0x1BB04E0, 0x1C72554, 0xB4750D, 0xB474D1, 0x28,
                0x1C72998, 0xB093DD, 0,
                0xC1D001, 0x81C401, 0x81C420, 0xD0, 0xECB180, 0xACA580)
        });

    public static MatrixMessageReceiverVtableSnapshot ReplayReaderMessageReceiver { get; } =
        new(
            ".?AVtfGameProtocolHandler@@",
            0x1C71EC0,
            0x1CCA404,
            0x1E29DF8,
            0,
            0,
            ReplayMessageReceiverConstructorFileOffset,
            Array.AsReadOnly(new[]
            {
                new MatrixMessageReceiverDialogSlotSnapshot(
                    0x78,
                    0x164,
                    0xECEEE0,
                    0xACE2E0,
                    MatrixMessageReceiverVtableTargetKind.DirectFunction,
                    null,
                    null),
                new MatrixMessageReceiverDialogSlotSnapshot(
                    0x79,
                    0x168,
                    0xECEF00,
                    0xACE300,
                    MatrixMessageReceiverVtableTargetKind.TailJumpThunk,
                    0xD92110,
                    0x991510),
                new MatrixMessageReceiverDialogSlotSnapshot(
                    0x7A,
                    0x16C,
                    0xECEF90,
                    0xACE390,
                    MatrixMessageReceiverVtableTargetKind.DirectFunction,
                    null,
                    null)
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
