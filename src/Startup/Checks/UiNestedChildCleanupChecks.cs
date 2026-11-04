using Firefall.Reconstruction.Startup;

internal static class UiNestedChildCleanupChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("UI nested-child cleanup identifies its receiver through outer-child +150 and keeps separate storage-release sites",
            UiNestedChildCleanupNativeLayout.CleanupEntryAddress == 0xCA6070
            && UiNestedChildCleanupNativeLayout.CleanupReceiverPointerOffsetInOuterChild == 0x150
            && UiNestedChildCleanupNativeLayout.NestedStorageFreeCallAddressOutsideCleanup == 0xBFAF57
            && UiNestedChildCleanupNativeLayout.OuterChildStorageFreeCallAddress == 0xBFAF87);

        var actions = UiNestedChildCleanupNativeLayout.OrderedActions;
        check("UI nested-child cleanup records the ordered conditional calls and field clears",
            actions.Count == 10
            && actions.SequenceEqual(new[]
            {
                new UiNestedChildCleanupAction(
                    0x1C, UiNestedChildCleanupActionKind.VirtualCall, 0x1C, 0x08, 0xCA60AA, null),
                new UiNestedChildCleanupAction(
                    0x1C, UiNestedChildCleanupActionKind.ClearField, 0x1C, null, 0xCA60AD, null),
                new UiNestedChildCleanupAction(
                    0x10, UiNestedChildCleanupActionKind.FreeCall, 0x10, null, 0xCA60BB, 0x12C8D60),
                new UiNestedChildCleanupAction(
                    0x10, UiNestedChildCleanupActionKind.ClearField, 0x10, null, 0xCA60C0, null),
                new UiNestedChildCleanupAction(
                    0x10, UiNestedChildCleanupActionKind.ClearField, 0x14, null, 0xCA60C7, null),
                new UiNestedChildCleanupAction(
                    0x10, UiNestedChildCleanupActionKind.ClearField, 0x18, null, 0xCA60CE, null),
                new UiNestedChildCleanupAction(
                    0x04, UiNestedChildCleanupActionKind.FreeCall, 0x04, null, 0xCA60DC, 0x12C8D60),
                new UiNestedChildCleanupAction(
                    0x04, UiNestedChildCleanupActionKind.ClearField, 0x04, null, 0xCA60E1, null),
                new UiNestedChildCleanupAction(
                    0x04, UiNestedChildCleanupActionKind.ClearField, 0x08, null, 0xCA60E8, null),
                new UiNestedChildCleanupAction(
                    0x04, UiNestedChildCleanupActionKind.ClearField, 0x0C, null, 0xCA60EF, null)
            })
            && UiNestedChildCleanupNativeLayout.CleanupReturnAddress == 0xCA6105);

        check("UI nested-child field +8 and pointer vtable slot +8 remain distinct layout facts",
            UiNestedChildCleanupNativeLayout.NestedField8Offset == 0x08
            && UiNestedChildCleanupNativeLayout.NestedPointerVtableSlotOffset == 0x08
            && actions[0].NestedFieldOffset == UiNestedChildCleanupNativeLayout.NestedField1COffset
            && actions[0].VtableSlotOffset == UiNestedChildCleanupNativeLayout.NestedPointerVtableSlotOffset
            && actions[8].NestedFieldOffset == UiNestedChildCleanupNativeLayout.NestedField8Offset
            && actions[8].Kind == UiNestedChildCleanupActionKind.ClearField
            && actions[8].GuardedByNonzeroNestedFieldOffset == UiNestedChildCleanupNativeLayout.NestedField4Offset);
    }
}
