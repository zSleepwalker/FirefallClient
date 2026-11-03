namespace Firefall.Reconstruction.Awesomium;

/// <summary>Repeated-value precedence for identical normalized switch-map keys only.</summary>
public static class DuplicateSwitchNativeLayout
{
    public const uint ParsedSwitchLoopVa = 0x1028DCD0;
    public const uint TypeKeyVa = 0x11265DD8;
    public const uint TypeValueGetterVa = 0x1028C8A0;
    public const uint MapInsertionVa = 0x1028D7F0;
    public const uint ExistingKeyLookupVa = 0x1028D310;
    public const uint ExistingValueSlotVa = 0x1028D444;
    public const uint ValueAssignmentVa = 0x1028D8AB;

    public static string CaptureSameNormalizedKeyValue(string earlierValue, string laterValue) =>
        laterValue;
}
