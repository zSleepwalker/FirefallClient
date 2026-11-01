namespace Firefall.Reconstruction.Startup;

public readonly record struct CharacterKeyedDialogCallbackVtableSlotSnapshot(
    int SlotOffset,
    uint MethodFileOffset,
    uint MethodVirtualAddress);

public static class CharacterKeyedDialogCallbackNativeLayout
{
    public const uint RawTextOffsetToVirtualAddressDelta = 0x400C00;

    public const uint StateFactoryMethodFileOffset = 0xAC9350;
    public const uint StateFactorySelectorMapFileOffset = 0xAC9704;
    public const uint StateFactorySelectorMapVirtualAddress = 0xECA304;
    public const uint StateFactoryJumpTableFileOffset = 0xAC96DC;
    public const uint StateFactoryJumpTableVirtualAddress = 0xECA2DC;
    public const uint GenericCharacterStateFactoryBranchFileOffset = 0xAC9483;
    public const uint UnsupportedNpcStateFactoryBranchFileOffset = 0xAC96A8;
    public const uint GenericCharacterAllocationSizeImmediateFileOffset = 0xAC94B3;
    public const uint GenericCharacterConstructorCallFileOffset = 0xAC94D8;
    public const uint GenericCharacterConstructorFileOffset = 0x422E80;
    public const int GenericCharacterAllocationSize = 0x5AE0;
    public const uint GenericCharacterPrimaryVtableVirtualAddress = 0x1C63998;
    public const uint GenericCharacterPrimaryVtableFileOffset = 0x1862598;
    public const string GenericCharacterRttiName = ".?AVtfGenericCharacter@@";
    public const uint GenericCharacterVtableInstallFileOffset = 0x422FEA;

    public const uint GenericCharacterSetupDispatcherFileOffset = 0x4263C0;
    public const uint GenericCharacterSetupSelectorTwoCaseFileOffset = 0x426436;
    public const uint GenericCharacterSetupMethodFileOffset = 0x426840;
    public const uint StateFactorySetupVirtualCallFileOffset = 0x800A7C;

    public const uint SetupPrivateContextAllocationCallFileOffset = 0x426881;
    public const int SetupPrivateContextAllocationSize = 0x43D8;
    public const uint SetupPrivateContextConstructorCallFileOffset = 0x4268A1;
    public const uint SetupPrivateContextConstructorFileOffset = 0x49EB30;
    public const int SetupPrivateContextStateFieldOffset = 0x4D20;
    public const uint SetupPrivateContextNullPointerInitializationFileOffset = 0x49EBE9;
    public const int SetupIndirectSourceObjectPointerFieldOffset = 0x20;
    public const int SetupIndirectChildObjectPointerFieldOffset = 0x9C;
    public const int SetupIndirectBackPointerFieldOffset = 0x2C;
    public const uint SetupIndirectBackPointerDataflowSequenceFileOffset = 0x4A2EFE;
    public const uint SetupIndirectBackPointerStoreFileOffset = 0x4A2F07;

    public const uint SelectorTwoAdapterAllocationFileOffset = 0x4A182C;
    public const int SelectorTwoAdapterAllocationSize = 0x228;
    public const uint SelectorTwoAdapterConstructorCallFileOffset = 0x4A1853;
    public const uint SelectorTwoAdapterConstructorFileOffset = 0x4C66D0;
    public const uint SelectorTwoAdapterPrivateContextFieldStoreFileOffset = 0x4C6714;
    public const int SelectorTwoAdapterPrivateContextFieldOffset = 0x170;
    public const uint SelectorTwoAdapterVtableStoreFileOffset = 0x4C671D;
    public const uint SelectorTwoAdapterVtableVirtualAddress = 0x1C64648;
    public const uint SelectorTwoAdapterVtableFileOffset = 0x1863248;
    public const string SelectorTwoAdapterRttiName = ".?AVBaseNetController@tfLocalCharacter@@";
    public const uint SelectorTwoAdapterControllerFieldStoreFileOffset = 0x4C672D;
    public const int SelectorTwoAdapterControllerFieldOffset = 0x174;
    public const uint SelectorTwoAdapterBindingMethodFileOffset = 0x4C7360;
    public const uint SelectorTwoAdapterControllerPointerLoadFileOffset = 0x4C738D;
    public const uint SelectorTwoAdapterControllerCallbackPointerStoreFileOffset = 0x4C7393;
    public const uint SelectorTwoAdapterControllerContextPointerRefreshFileOffset = 0x4C7396;
    public const uint SelectorTwoAdapterControllerContextPointerStoreFileOffset = 0x4C73A5;
    public const uint SelectorTwoAdapterStateSetupCallFileOffset = 0x4A184E;
    public const uint SelectorTwoAdapterContextInstallCallFileOffset = 0x4A1867;
    public const uint SelectorTwoAdapterInitialUpdateCallFileOffset = 0x4A18D6;
    public const uint SelectorTwoAdapterContextUpdateCallFileOffset = 0x4A2EE5;

    public static IReadOnlyList<CharacterKeyedDialogCallbackVtableSlotSnapshot>
        SelectorTwoAdapterVtableSlots { get; } =
        Array.AsReadOnly(new[]
        {
            Slot(0x10, 0x4C82E0),
            Slot(0x14, 0x4C8300),
            Slot(0x18, 0x4C9B80)
        });

    public const uint SelectorThreeConditionalAdapterAllocationFileOffset = 0x4A190F;
    public const int SelectorThreeConditionalAdapterAllocationSize = 0xB0;
    public const uint SelectorThreeConditionalAdapterConstructorCallFileOffset = 0x4A192E;
    public const uint SelectorThreeConditionalAdapterConstructorFileOffset = 0x4C6360;
    public const uint SelectorThreeConditionalAdapterPrivateContextFieldStoreFileOffset = 0x4C63AF;
    public const int SelectorThreeConditionalAdapterPrivateContextFieldOffset = 0x10;
    public const uint SelectorThreeConditionalAdapterVtableStoreFileOffset = 0x4C63B5;
    public const uint SelectorThreeConditionalAdapterVtableVirtualAddress = 0x1C64640;
    public const uint SelectorThreeConditionalAdapterVtableFileOffset = 0x1863240;
    public const string SelectorThreeConditionalAdapterRttiName = ".?AVMandMController@tfLocalCharacter@@";
    public const uint SelectorThreeConditionalAdapterControllerFieldStoreFileOffset = 0x4C63BB;
    public const int SelectorThreeConditionalAdapterControllerFieldOffset = 0x14;
    public const uint SelectorThreeConditionalAdapterBindingMethodFileOffset = 0x4C6490;
    public const uint SelectorThreeConditionalAdapterControllerPointerLoadFileOffset = 0x4C649B;
    public const uint SelectorThreeConditionalAdapterControllerCallbackPointerStoreFileOffset = 0x4C64A1;
    public const uint SelectorThreeConditionalAdapterContextUpdateCallFileOffset = 0x4A2EF3;

    public static IReadOnlyList<CharacterKeyedDialogCallbackVtableSlotSnapshot>
        SelectorThreeConditionalAdapterVtableSlots { get; } =
        Array.AsReadOnly(new[]
        {
            Slot(0x14, 0x4C7C50),
            Slot(0x18, 0x4C82E0)
        });

    public const uint SharedKeyedReceiverConstructorHelperFileOffset = 0xBCAD70;
    public const uint SharedOptionalNestedReceiverNullStoreFileOffset = 0xBCAD91;
    public const int SharedOptionalNestedReceiverFieldOffset = 0x10;
    public const uint BaseControllerOptionalNestedForwardCallFileOffset = 0x54039A;
    public const uint NpcControllerOptionalNestedForwardCallFileOffset = 0x54F4BA;

    private static CharacterKeyedDialogCallbackVtableSlotSnapshot Slot(
        int slotOffset,
        uint methodFileOffset) =>
        new(
            slotOffset,
            methodFileOffset,
            methodFileOffset + RawTextOffsetToVirtualAddressDelta);
}
