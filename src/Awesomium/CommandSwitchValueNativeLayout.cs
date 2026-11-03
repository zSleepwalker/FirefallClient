namespace Firefall.Reconstruction.Awesomium;

public readonly record struct SwitchValueLengthDecision(uint StoredValueLength, bool ConsumesFollowingArgument);

/// <summary>Value-length policy after switch recognition; not an argv or command-line parser.</summary>
public static class CommandSwitchValueNativeLayout
{
    public const uint SwitchSplitterVa = 0x1028BC60;
    public const int ParsedValueLengthOffset = 0x10;
    public const uint ParsedValueLengthInitializationVa = 0x1028BC98;
    public const uint NoSeparatorBranchVa = 0x1028BD26;
    public const uint MapValueCopySequenceVa = 0x1028D8A5;
    public const uint WideDoubleDashVa = 0x1127B3F8;
    public const uint WideEqualsVa = 0x1127B400;
    public const uint TypeKeyVa = 0x11265DD8;

    public static SwitchValueLengthDecision CaptureValueLength(bool hasEqualsSeparator, uint suffixLength) =>
        new(hasEqualsSeparator ? suffixLength : 0, false);
}
