namespace Firefall.Reconstruction.Startup;

public enum UiManagerInitializationOutcome
{
    PartialInitializationFailed,
    Initialized
}

public readonly record struct UiManagerInitializationFlagSnapshot(
    byte PartialInitializationFlag,
    byte ReadyFlag)
{
    public ushort ShutdownStateWord => (ushort)(PartialInitializationFlag | (ReadyFlag << 8));
}

public readonly record struct UiManagerPayloadCleanupStep(
    string Operation,
    uint CallSiteFileOffset,
    uint TargetMethodFileOffset);

public readonly record struct UiManagerChild24ShutdownStep(
    string Operation,
    uint CallSiteFileOffset,
    uint? TargetMethodFileOffset);

public readonly record struct UiManagerStateSyncInput(
    byte GetterResult,
    byte ReadyFlag,
    byte Field670,
    uint Field58C,
    byte Field590,
    uint Field588,
    uint Field584,
    byte Field598,
    uint CursorFlags,
    uint CursorHandle);

public readonly record struct UiManagerStateSyncCall(byte Argument0, uint Argument1);

public readonly record struct UiManagerStateSyncProjection(
    byte GateResult,
    UiManagerStateSyncCall? ComponentCall);

/// <summary>
/// Static layout and lifecycle observations for the client's UI manager.
/// This does not construct the object, dispatch application events, or perform cleanup.
/// </summary>
public static class UiManagerNativeLayout
{
    public const uint SingletonRegistrationStubFileOffset = 0x3EE8C0;
    public const uint SingletonInitializeMethodFileOffset = 0x3E44F0;
    public const uint SingletonListWalkerFileOffset = 0x12C3A80;
    public const uint ClientApplicationBootstrapFileOffset = 0x2EAE70;
    public const uint UiManagerConstructorFileOffset = 0xBF7A80;
    public const uint UiManagerInitializeMethodFileOffset = 0xBFB310;
    public const uint UiManagerInitializationFlagGetterMethodFileOffset = 0xBFA0C0;
    public const uint UiManagerInitializePartialFlagWriteFileOffset = 0xBFB350;
    public const uint UiManagerInitializeDescriptorCallbackCallSiteFileOffset = 0xBFB8C6;
    public const uint UiManagerInitializeDescriptorStoreFileOffset = 0xBFB8CE;
    public const uint UiManagerInitializeReadyFlagWriteFileOffset = 0xBFB8DC;
    public const uint UiManagerStateSyncMethodFileOffset = 0xBFFA90;
    public const uint UiManagerStateSyncCallerMethodFileOffset = 0xBFF930;
    public const uint UiManagerStateSyncCallerCallSiteFileOffset = 0xBFFA3F;
    public const uint UiManagerStateSyncGetCursorInfoIatVirtualAddress = 0x1AC4C24;
    public const uint UiManagerStateSyncGetCursorInfoIatFileOffset = 0x16C3824;
    public const int UiManagerStateSyncCursorInfoSize = 0x14;
    public const int UiManagerStateSyncCursorInfoCbSizeOffset = 0;
    public const int UiManagerStateSyncCursorInfoFlagsOffset = 4;
    public const int UiManagerStateSyncCursorInfoCursorHandleOffset = 8;
    public const int UiManagerStateSyncCursorInfoXOffset = 12;
    public const int UiManagerStateSyncCursorInfoYOffset = 16;
    public const uint UiManagerDescriptorGetterCallSiteFileOffset = 0x30B1B4;
    public const int UiManagerInitializeDescriptorArgumentOrdinal = 8;
    public const int UiManagerInitializeDescriptorArgumentFrameOffset = 0x24;
    public const int UiManagerInitializeDescriptorCallbackReceiverOffset = 0x974;
    public const int UiManagerInitializeDescriptorCallbackVtableOffset = 0x30;
    public const int UiManagerInitializeDescriptorCallbackArgument = 1;
    public const bool UiManagerInitializeDescriptorCallbackReturnIsUsed = false;
    public const int UiManagerDescriptorGetterResultAddressOffset = 0x48;
    public const int UiManagerPayloadDescriptorOffset = 0xA08;
    public const uint UiPayloadDescriptorConsumerMethodFileOffset = 0xC95400;
    public const uint UiPayloadDescriptorConsumerMethodVirtualAddress = 0x1096000;
    public const uint UiPayloadDescriptorAppendMethodFileOffset = 0xC97380;
    public const uint UiPayloadDescriptorGrowMethodFileOffset = 0xC978F0;
    public const uint UiManagerShutdownMethodFileOffset = 0xBFA0D0;
    public const uint UiManagerShutdownChild24PointerLoadFileOffset = 0xBFAD2A;
    public const uint UiManagerShutdownChild24InitialNullTestFileOffset = 0xBFAD37;
    public const uint UiManagerShutdownChild24PrimaryCallSiteFileOffset = 0xBFAD3B;
    public const uint UiManagerShutdownChild24PrimaryCallTargetFileOffset = 0xCA7330;
    public const uint UiManagerShutdownChild24ReloadFileOffset = 0xBFAD40;
    public const uint UiManagerShutdownChild24SecondNullTestFileOffset = 0xBFAD43;
    public const uint UiManagerShutdownChild24SecondNullBranchFileOffset = 0xBFAD45;
    public const uint UiManagerShutdownChild24ReleaseHelperCallSiteFileOffset = 0xBFAD48;
    public const uint UiManagerShutdownChild24ReleaseHelperFileOffset = 0xBFAF40;
    public const uint UiManagerShutdownChild24PointerClearFileOffset = 0xBFAD4D;
    public const int UiManagerShutdownChild24PointerOffset = 0x24;
    public const bool UiManagerShutdownChild24CallsAreNullGated = true;
    public const uint UiManagerChild24ShutdownResetMethodFileOffset = 0xCA7330;
    public const uint UiManagerChild24ShutdownResetFirstLoopCallFileOffset = 0xCA734A;
    public const uint UiManagerChild24ShutdownResetFirstLoopStoreFileOffset = 0xCA734D;
    public const uint UiManagerChild24ShutdownResetSecondLoopCallFileOffset = 0xCA735D;
    public const uint UiManagerChild24ShutdownResetSecondLoopStoreFileOffset = 0xCA7360;
    public const uint UiManagerChild24ShutdownResetChild8StoreFileOffset = 0xCA7379;
    public const uint UiManagerChild24ShutdownResetChild64FreeCallFileOffset = 0xCA73F2;
    public const uint UiManagerChild24ShutdownResetChild68ClearFileOffset = 0xCA73F7;
    public const uint UiManagerChild24ShutdownResetReturnFileOffset = 0xCA7401;
    public const int UiManagerChildShutdownResetPairLoopCount = 8;
    public const int UiManagerChildShutdownResetLoopPointerFieldCount = 16;
    public static IReadOnlyList<int> UiManagerChildShutdownResetLoopPointerOffsets { get; } =
        Array.AsReadOnly(Enumerable.Range(0, UiManagerChildShutdownResetLoopPointerFieldCount)
            .Select(index => 0x24 + index * 4)
            .ToArray());
    public static IReadOnlyList<int> UiManagerChildShutdownResetLaterPointerOffsets { get; } =
        Array.AsReadOnly(new[] { 0x08, 0x14, 0x0C, 0x10, 0x74 });
    public const int UiManagerChildObjectCleanupFieldOffset = 0x150;
    public const bool UiManagerChildReleaseHelperUsesThiscallReceiverInEcx = true;
    public const bool UiManagerChildReleaseHelperCallerPushesUnreadChildCopy = true;
    public const int UiManagerChildReleaseHelperReturnStackBytes = 4;
    public const uint UiManagerChild150CleanupCallSiteFileOffset = 0xBFAF50;
    public const uint UiManagerChild150CleanupMethodFileOffset = 0xCA6070;
    public const uint UiManagerChild150NativeFreeCallSiteFileOffset = 0xBFAF57;
    public const uint UiManagerChildDependencyZeroTestFileOffset = 0xBFAF5C;
    public const uint UiManagerChildDependencyZeroBranchFileOffset = 0xBFAF60;
    public const uint UiManagerChildDependencyLoadFileOffset = 0xBFAF7C;
    public const uint UiManagerChildDependencyVirtualCallFileOffset = 0xBFAF82;
    public const int UiManagerChildDependencyPointerOffset = 4;
    public const int UiManagerChildDependencyVtableOffset = 8;
    public const uint UiManagerChildObjectNativeFreeCallSiteFileOffset = 0xBFAF87;
    public const uint UiManagerChildObjectNativeFreeMethodFileOffset = 0x12C8D60;
    public const uint UiManagerChildReleaseHelperReturnFileOffset = 0xBFAF90;
    public const uint UiManagerPayloadTreeInternalCloseCallSiteFileOffset = 0xBFA278;
    public const uint UiManagerPayloadTreeNativeObjectFreeCallSiteFileOffset = 0xBFA27F;
    public const uint UiManagerPayloadTreeIdentityRemovalCallSiteFileOffset = 0xBFA28B;
    public const uint UiManagerPayloadTreeKeyReleaseCallSiteFileOffset = 0xBFA29C;
    public const uint UiManagerPayloadInternalCloseMethodFileOffset = 0xC33060;
    public const uint UiManagerPayloadNativeObjectFreeMethodFileOffset = 0x12C8D60;
    public const uint UiManagerPayloadIdentityRemovalMethodFileOffset = 0xBFCF30;
    public const uint UiManagerPayloadKeyReleaseMethodFileOffset = 0xC09D10;
    public const uint UiManagerEventNameToIdMethodFileOffset = 0xBF9AB0;
    public const uint UiManagerEventNameMapLookupHelperFileOffset = 0x3E63B0;
    public const uint UiManagerEventNameMapInsertHelperFileOffset = 0xC079A0;
    public const uint UiManagerEventNameArrayAppendHelperFileOffset = 0xC07D10;
    public const uint UiManagerEventNameMapLookupCallSiteFileOffset = 0xBF9ACE;
    public const uint UiManagerEventNameMapInsertCallSiteFileOffset = 0xBF9AFE;
    public const uint UiManagerEventNameArrayAppendCallSiteFileOffset = 0xBF9B2B;
    public const uint UiManagerEventNameExistingIdReadFileOffset = 0xBF9ADA;
    public const uint UiManagerEventNameArrayCountCalculationFileOffset = 0xBF9AE6;
    public const uint UiManagerEventNameMapIdCountArgumentPreparationFileOffset = 0xBF9AF3;
    public const uint UiManagerEventNameMapEntryPairConstructionCallSiteFileOffset = 0xC079FE;
    public const uint UiManagerEventNameMapEntryPairIdInputFileOffset = 0xC079F4;
    public const uint UiManagerEventNameMapEntryPairConstructionMethodFileOffset = 0xC14660;
    public const uint UiManagerEventNameMapEntryPairIdInputLoadInstructionFileOffset = 0xC14692;
    public const uint UiManagerEventNameMapEntryPairIdStoreInstructionFileOffset = 0xC14694;
    public const uint UiManagerEventNameMapNodeConstructionCallSiteFileOffset = 0xC07A11;
    public const uint UiManagerEventNameMapNodeConstructionMethodFileOffset = 0xC16CB0;
    public const uint UiManagerEventNameMapNodeInsertCallSiteFileOffset = 0xC07A23;
    public const uint UiManagerEventNameMapNodeInsertMethodFileOffset = 0xC16D60;
    public const uint UiManagerEventNameMapNodeIdBaseInitializationFileOffset = 0xC16CEC;
    public const uint UiManagerEventNameMapNodePairIdInputLoadInstructionFileOffset = 0xC16D2A;
    public const uint UiManagerEventNameMapNodePairIdStoreInstructionFileOffset = 0xC16D2D;
    public const uint UiManagerEventNameMapSharedStringCleanupCallSiteFileOffset = 0xC07A34;
    public const uint UiManagerEventNameMapSharedStringCleanupMethodFileOffset = 0xC08B40;
    public const uint UiManagerEventListenerRegisterMethodFileOffset = 0xBF9B40;
    public const uint UiManagerEventListenerUnregisterMethodFileOffset = 0xBF9BD0;
    public const uint UiManagerEventDispatchMethodFileOffset = 0xBF9D80;
    public const uint UiManagerEventReadyFlagTestFileOffset = 0xBF9DBA;
    public const uint UiManagerEventReadyFlagInitializeFileOffset = 0xBF7DCB;
    public const uint UiManagerEventListenerDispatchFileOffset = 0xBF9C40;
    public const uint UiManagerEventListenerLookupFileOffset = 0xC0AA20;
    public const uint UiManagerEventCallbackRouteFileOffset = 0xC38400;
    public const uint UiManagerNamedSubscriptionFileOffset = 0xC35BB0;
    public const uint UiComponentRootXmlParserFileOffset = 0xC34F50;
    public const uint UiComponentEventsSectionParserFileOffset = 0xC35740;
    public const uint UiComponentEventsSectionParserCallSiteFileOffset = 0xC353B0;
    public const uint UiComponentEventNameToIdCallSiteFileOffset = 0xC358AF;
    public const uint UiComponentEventBindLookupCallSiteFileOffset = 0xC358FE;
    public const uint UiComponentEventListenerRegisterCallSiteFileOffset = 0xC35AD8;
    public const uint ComponentBindEventApiFileOffset = 0xC32460;
    public const uint ComponentBindEventNamedSubscriptionCallSiteFileOffset = 0xC32643;
    public const uint ComponentEventsSectionNameVirtualAddress = 0x1BBCC6C;
    public const uint ComponentEventElementNameVirtualAddress = 0x1BBCCDC;
    public const uint ComponentEventNameAttributeVirtualAddress = 0x1BBCD1C;
    public const uint ComponentEventBindAttributeVirtualAddress = 0x1BBCCD4;
    public const uint ComponentBindEventUsageVirtualAddress = 0x1BBC760;
    public const uint LoginModeMessage145GlobalLoadFileOffset = 0x63C1E2;
    public const uint LoginModeMessage145CallFileOffset = 0x63C1F5;
    public const uint SingletonObjectVirtualAddress = 0x1E002A8;
    public const uint SingletonVtableVirtualAddress = 0x1C62FF0;
    public const uint SingletonTypeDescriptorVirtualAddress = 0x1E06728;
    public const uint UiManagerGlobalPointerVirtualAddress = 0x1E002B8;
    public const uint UiManagerVtableVirtualAddress = 0x1C73EBC;
    public const uint UiManagerTypeDescriptorVirtualAddress = 0x1E2CC90;
    public const uint SingletonFactoryListHeadVirtualAddress = 0x1E4B0A4;
    public const uint InitializedSingletonListHeadVirtualAddress = 0x1E4B0A8;
    public const int SingletonNextPointerOffset = 0x08;
    public const int SingletonInitializedFlagOffset = 0x04;
    public const int SingletonInstancePointerOffset = 0x10;
    public const int UiManagerAllocationSize = 0xA10;
    public const int UiManagerEventDispatchVtableOffset = 0xAC;
    public const int UiManagerEventRegistryOffset = 0x6F0;
    public const int UiManagerEventNameMapOffset = 0x6E8;
    public const int UiManagerEventNameArrayBeginOffset = 0x71C;
    public const int UiManagerEventNameArrayEndOffset = 0x720;
    public const int UiManagerEventNameArrayBeginInitialValue = 0;
    public const int UiManagerEventNameArrayEndInitialValue = 0;
    public const int UiManagerEventNameArrayEntrySize = 4;
    public const int UiManagerEventNameMapEntryEventIdOffset = 0x14;
    public const uint UiManagerEventNameArrayBeginInitializationFileOffset = 0xBF7FF6;
    public const uint UiManagerEventNameArrayEndInitializationFileOffset = 0xBF8000;
    public const int UiManagerEventReadyFlagOffset = 0x672;
    public const int UiPayloadDescriptorConsumerObjectFlagOffset = 0x72C;
    public const int UiPayloadDescriptorBeginPointerOffset = 0;
    public const int UiPayloadDescriptorEndPointerOffset = 4;
    public const int UiPayloadDescriptorCapacityPointerOffset = 8;
    public const int UiPayloadDescriptorPositionRecordByteCount = 0x10;
    public const int UiPayloadDescriptorPositionRecordCopiedDwordCount = 4;
    public const int UiPayloadDescriptorPositionRecordEndAdvance = 0x10;
    public const int UiPayloadDescriptorPositionField0Offset = 0x33C;
    public const int UiPayloadDescriptorPositionField1Offset = 0x338;
    public const int UiPayloadDescriptorPositionField2Offset = 0x340;
    public const int UiPayloadDescriptorPositionField3Offset = 0x344;
    public const uint UiManagerCursorRegistrationMethodFileOffset = 0xBFD6D0;
    public const uint UiManagerCursorLoadCursorAIatVirtualAddress = 0x1AC4C2C;
    public const uint UiManagerCursorResourceIdentifier = 0x7F8A;
    public const int UiManagerCursorTreeOffset = 0x6C8;
    public const uint UiManagerCursorTreeInsertMissingMethodFileOffset = 0xC06D40;
    public const uint UiManagerCursorTreeNodePayloadStoreFileOffset = 0xC1675D;
    public const int UiManagerCursorTreeNodeKeyOffset = 0x10;
    public const int UiManagerCursorTreeNodePayloadOffset = 0x14;
    public const int UiManagerConstructorStateWordOffset = 0x670;
    public const int UiManagerInitializationPartialFlagOffset = 0x671;
    public const int UiManagerInitializationFlagGetterVtableOffset = 0x04;
    public const int UiManagerStateSyncComponentReceiverOffset = 0x974;
    public const int UiManagerStateSyncComponentVtableOffset = 0x1C;
    public const bool UiManagerStateSyncComponentReturnIsUsed = false;
    public const int UiManagerStateSyncField58COffset = 0x58C;
    public const int UiManagerStateSyncField590Offset = 0x590;
    public const int UiManagerStateSyncField588Offset = 0x588;
    public const int UiManagerStateSyncField584Offset = 0x584;
    public const int UiManagerStateSyncField598Offset = 0x598;
    public const int UiManagerShutdownStateWordOffset = 0x671;
    public const int UiManagerShutdownStateWordSize = 2;
    public const int UiManagerConstructorStateWordValue = 1;
    public const int UiManagerPayloadTreeOffset = 0x5E4;
    public const int UiManagerPayloadTreeNodeValueOffset = 0x14;
    public const int UiManagerListenerNameOffset = 0x70;
    public const int LoginModeMessageId = 0x145;
    public const int EnterZonePausedNotificationId = 110;
    public const string SingletonRttiName = ".?AV?$slSingleton@VuiManager@@@@";
    public const string UiManagerRttiName = ".?AVuiManager@@";

    public static IReadOnlyList<UiManagerPayloadCleanupStep> PayloadTreeCleanupSequence { get; } =
        Array.AsReadOnly(new[]
        {
            new UiManagerPayloadCleanupStep(
                "InternalClose",
                UiManagerPayloadTreeInternalCloseCallSiteFileOffset,
                UiManagerPayloadInternalCloseMethodFileOffset),
            new UiManagerPayloadCleanupStep(
                "NativeObjectFree",
                UiManagerPayloadTreeNativeObjectFreeCallSiteFileOffset,
                UiManagerPayloadNativeObjectFreeMethodFileOffset),
            new UiManagerPayloadCleanupStep(
                "IdentityRemoval",
                UiManagerPayloadTreeIdentityRemovalCallSiteFileOffset,
                UiManagerPayloadIdentityRemovalMethodFileOffset),
            new UiManagerPayloadCleanupStep(
                "KeyRelease",
                UiManagerPayloadTreeKeyReleaseCallSiteFileOffset,
                UiManagerPayloadKeyReleaseMethodFileOffset)
        });

    public static IReadOnlyList<UiManagerChild24ShutdownStep> Child24ShutdownSequence { get; } =
        Array.AsReadOnly(new[]
        {
            new UiManagerChild24ShutdownStep(
                "Child24PrimaryCall",
                UiManagerShutdownChild24PrimaryCallSiteFileOffset,
                UiManagerShutdownChild24PrimaryCallTargetFileOffset),
            new UiManagerChild24ShutdownStep(
                "Child24ReleaseHelperCall",
                UiManagerShutdownChild24ReleaseHelperCallSiteFileOffset,
                UiManagerShutdownChild24ReleaseHelperFileOffset),
            new UiManagerChild24ShutdownStep(
                "Child150Cleanup",
                UiManagerChild150CleanupCallSiteFileOffset,
                UiManagerChild150CleanupMethodFileOffset),
            new UiManagerChild24ShutdownStep(
                "Child150NativeFree",
                UiManagerChild150NativeFreeCallSiteFileOffset,
                UiManagerChildObjectNativeFreeMethodFileOffset),
            new UiManagerChild24ShutdownStep(
                "ChildDependencyVirtualCall",
                UiManagerChildDependencyVirtualCallFileOffset,
                null),
            new UiManagerChild24ShutdownStep(
                "ChildObjectNativeFree",
                UiManagerChildObjectNativeFreeCallSiteFileOffset,
                UiManagerChildObjectNativeFreeMethodFileOffset)
        });

    public static UiManagerInitializationFlagSnapshot CaptureObservedInitializationFlags(
        UiManagerInitializationOutcome outcome) =>
        outcome switch
        {
            UiManagerInitializationOutcome.PartialInitializationFailed => new(1, 0),
            UiManagerInitializationOutcome.Initialized => new(1, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(outcome))
        };

    public static bool ShouldEnterInitialization(byte getterResult) => getterResult == 0;

    public static bool ShouldDrainPayloadTree(byte partialInitializationFlag) => partialInitializationFlag != 0;

    public static UiManagerStateSyncProjection ProjectStateSync(UiManagerStateSyncInput input)
    {
        if (input.GetterResult == 0 || input.ReadyFlag == 0)
            return new(0, null);

        uint selected = input.Field58C;
        if (input.Field590 == 0 || selected == 0)
            selected = input.Field588;
        if (selected == 0)
            selected = input.Field584;

        if (input.Field670 == 0
            || (input.Field598 != 0
                && (input.CursorFlags == 0 || input.CursorHandle != selected)))
            return new(1, new UiManagerStateSyncCall(1, selected));

        if (input.Field598 == 0 && input.CursorFlags == 1)
            return new(1, new UiManagerStateSyncCall(0, 0));

        return new(1, null);
    }
}
