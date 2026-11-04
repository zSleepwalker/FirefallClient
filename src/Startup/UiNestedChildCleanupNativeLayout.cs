namespace Firefall.Reconstruction.Startup;

public enum UiNestedChildCleanupActionKind
{
    VirtualCall,
    FreeCall,
    ClearField
}

public readonly record struct UiNestedChildCleanupAction(
    int? GuardedByNonzeroNestedFieldOffset,
    UiNestedChildCleanupActionKind Kind,
    int? NestedFieldOffset,
    int? VtableSlotOffset,
    uint InstructionAddress,
    uint? TargetAddress);

/// <summary>
/// Static structural evidence using raw file offsets for instruction addresses.
/// It does not execute calls or model native ownership and aliasing.
/// </summary>
public static class UiNestedChildCleanupNativeLayout
{
    public const uint CleanupEntryAddress = 0xCA6070;
    public const int CleanupReceiverPointerOffsetInOuterChild = 0x150;

    public const int NestedField4Offset = 0x04;
    public const int NestedField8Offset = 0x08;
    public const int NestedField10Offset = 0x10;
    public const int NestedField14Offset = 0x14;
    public const int NestedField18Offset = 0x18;
    public const int NestedField1COffset = 0x1C;
    public const int NestedFieldCOffset = 0x0C;

    public const int NestedPointerVtableSlotOffset = 0x08;
    public const uint FreeFunctionAddress = 0x12C8D60;

    public const uint NestedField1CVirtualCallAddress = 0xCA60AA;
    public const uint NestedField1CClearAddress = 0xCA60AD;
    public const uint NestedField10FreeCallAddress = 0xCA60BB;
    public const uint NestedField10ClearAddress = 0xCA60C0;
    public const uint NestedField14ClearAddress = 0xCA60C7;
    public const uint NestedField18ClearAddress = 0xCA60CE;
    public const uint NestedField4FreeCallAddress = 0xCA60DC;
    public const uint NestedField4ClearAddress = 0xCA60E1;
    public const uint NestedField8ClearAddress = 0xCA60E8;
    public const uint NestedFieldCClearAddress = 0xCA60EF;
    public const uint CleanupReturnAddress = 0xCA6105;

    public const uint NestedStorageFreeCallAddressOutsideCleanup = 0xBFAF57;
    public const uint OuterChildStorageFreeCallAddress = 0xBFAF87;

    public static IReadOnlyList<UiNestedChildCleanupAction> OrderedActions { get; } =
        Array.AsReadOnly(new UiNestedChildCleanupAction[]
        {
            new(UiNestedChildCleanupNativeLayout.NestedField1COffset,
                UiNestedChildCleanupActionKind.VirtualCall,
                UiNestedChildCleanupNativeLayout.NestedField1COffset,
                UiNestedChildCleanupNativeLayout.NestedPointerVtableSlotOffset,
                UiNestedChildCleanupNativeLayout.NestedField1CVirtualCallAddress,
                null),
            new(UiNestedChildCleanupNativeLayout.NestedField1COffset,
                UiNestedChildCleanupActionKind.ClearField,
                UiNestedChildCleanupNativeLayout.NestedField1COffset,
                null,
                UiNestedChildCleanupNativeLayout.NestedField1CClearAddress,
                null),
            new(UiNestedChildCleanupNativeLayout.NestedField10Offset,
                UiNestedChildCleanupActionKind.FreeCall,
                UiNestedChildCleanupNativeLayout.NestedField10Offset,
                null,
                UiNestedChildCleanupNativeLayout.NestedField10FreeCallAddress,
                UiNestedChildCleanupNativeLayout.FreeFunctionAddress),
            new(UiNestedChildCleanupNativeLayout.NestedField10Offset,
                UiNestedChildCleanupActionKind.ClearField,
                UiNestedChildCleanupNativeLayout.NestedField10Offset,
                null,
                UiNestedChildCleanupNativeLayout.NestedField10ClearAddress,
                null),
            new(UiNestedChildCleanupNativeLayout.NestedField10Offset,
                UiNestedChildCleanupActionKind.ClearField,
                UiNestedChildCleanupNativeLayout.NestedField14Offset,
                null,
                UiNestedChildCleanupNativeLayout.NestedField14ClearAddress,
                null),
            new(UiNestedChildCleanupNativeLayout.NestedField10Offset,
                UiNestedChildCleanupActionKind.ClearField,
                UiNestedChildCleanupNativeLayout.NestedField18Offset,
                null,
                UiNestedChildCleanupNativeLayout.NestedField18ClearAddress,
                null),
            new(UiNestedChildCleanupNativeLayout.NestedField4Offset,
                UiNestedChildCleanupActionKind.FreeCall,
                UiNestedChildCleanupNativeLayout.NestedField4Offset,
                null,
                UiNestedChildCleanupNativeLayout.NestedField4FreeCallAddress,
                UiNestedChildCleanupNativeLayout.FreeFunctionAddress),
            new(UiNestedChildCleanupNativeLayout.NestedField4Offset,
                UiNestedChildCleanupActionKind.ClearField,
                UiNestedChildCleanupNativeLayout.NestedField4Offset,
                null,
                UiNestedChildCleanupNativeLayout.NestedField4ClearAddress,
                null),
            new(UiNestedChildCleanupNativeLayout.NestedField4Offset,
                UiNestedChildCleanupActionKind.ClearField,
                UiNestedChildCleanupNativeLayout.NestedField8Offset,
                null,
                UiNestedChildCleanupNativeLayout.NestedField8ClearAddress,
                null),
            new(UiNestedChildCleanupNativeLayout.NestedField4Offset,
                UiNestedChildCleanupActionKind.ClearField,
                UiNestedChildCleanupNativeLayout.NestedFieldCOffset,
                null,
                UiNestedChildCleanupNativeLayout.NestedFieldCClearAddress,
                null)
        });
}
