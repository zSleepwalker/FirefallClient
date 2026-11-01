namespace Firefall.Reconstruction.Startup;

public static class CharacterKeyedRecipientNativeLayout
{
    public const uint RawTextOffsetToVirtualAddressDelta = 0x400C00;
    public const uint RawRdataOffsetToVirtualAddressDelta = 0x401400;

    public const uint CallbackFileOffset = 0x4C9B80;
    public const uint CallbackVirtualAddress = 0x8CA780;
    public const uint RegistryAccessorFileOffset = 0x35E700;
    public const uint RegistryAccessorVirtualAddress = 0x75F300;
    public const int RegistryMapOffsetFromAccessorResult = 0x54;
    public const uint RegistryLookupFileOffset = 0x4093E0;
    public const uint RegistryInsertionFileOffset = 0x407F00;
    public const uint RegistryInsertionVirtualAddress = 0x808B00;
    public const uint RegistryNodeInsertionHelperFileOffset = 0x40C470;
    public const uint RegistryNodeAllocationHelperFileOffset = 0x40BBC0;
    public const uint RegistryNodeFieldInitializerFileOffset = 0x40E6C0;
    public const int RegistryNodeAllocationSize = 0x18;
    public const int RegistryEntryRecipientPointerOffset = 0x10;

    public const uint GenericCharacterInsertionProducerFileOffset = 0x943360;
    public const uint GenericCharacterInsertionProducerVirtualAddress = 0xD43F60;
    public const uint GenericCharacterFactoryFileOffset = 0x425C90;
    public const uint GenericCharacterFactoryVirtualAddress = 0x826890;
    public const uint GenericCharacterAllocationSizePushFileOffset = 0x425DD7;
    public const uint GenericCharacterAllocatorCallFileOffset = 0x425DE2;
    public const int GenericCharacterAllocationSize = 0x5AE0;
    public const uint GenericCharacterCounterIdCallFileOffset = 0x425DF7;
    public const uint GenericCharacterCounterIdHelperFileOffset = 0xBC9A20;
    public const uint GenericCharacterConstructorFileOffset = 0x422E80;
    public const uint GenericCharacterConstructorVirtualAddress = 0x823A80;
    public const uint GenericCharacterConstructorCallFileOffset = 0x425E08;
    public const uint GenericCharacterPrimaryVptrStoreFileOffset = 0x422FEA;
    public const uint GenericCharacterMemberPointerLoadFileOffset = 0x425E17;
    public const uint GenericCharacterAuxiliaryPointerStoreFileOffset = 0x425EBC;
    public const int GenericCharacterMemberPointerFieldOffset = 0x46A4;
    public const int GenericCharacterAuxiliaryPointerFieldOffset = 0x58A0;
    public const uint GenericCharacterVtableVirtualAddress = 0x1C63998;
    public const uint GenericCharacterVtableFileOffset = 0x1862598;
    public const string GenericCharacterRttiName = ".?AVtfGenericCharacter@@";
    public const int GenericCharacterVptrOffset = 0;
    public const uint GenericCharacterFactoryCallSiteFileOffset = 0x94377C;
    public const uint GenericCharacterFactoryCallSiteVirtualAddress = 0xD4437C;
    public const uint GenericCharacterReturnedPointerMoveFileOffset = 0x943781;
    public const uint GenericCharacterRegistryAccessorCallFileOffset = 0x94378C;
    public const uint GenericCharacterRegistryInsertionCallFileOffset = 0x943793;
    public const uint GenericCharacterRegistryInsertionCallVirtualAddress = 0xD44393;
    public const uint RegistryInsertionPayloadSetupFileOffset = 0x407FA4;

    public const int ObjectKeyFirstDwordOffset = 8;
    public const int ObjectKeySecondDwordOffset = 0x0C;
    public const uint ObjectKeyFirstDwordConstructorMask = 0xFFFFFF01;
    public const uint ObjectKeyFirstDwordRegistryMask = 0xFFFFFF00;
    public const byte ObjectKeyLowByteMarker = 1;
    public const int CallbackRecipientGateByteOffset = 8;
    public const int CallbackFallbackHighestReferencedOffset = 0x58A0;
    public const int CallbackFallbackHighestReferencedExtent = 0x58A4;
}
