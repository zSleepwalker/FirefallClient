namespace Firefall.Reconstruction.Startup;

public readonly record struct TfWorldLayerSnapshot(int NativeOffset, int LayerId);

/// <summary>
/// Address and member-layout observations for the 32-bit tfWorld object.
/// Offsets are native object offsets; the layer identifiers are observed
/// constructor values, not inferred names for the layer types.
/// </summary>
public static class TfWorldNativeLayout
{
    private static readonly TfWorldLayerSnapshot[] Layers =
    [
        new(0x2FC, 0),
        new(0xC4C, 9),
        new(0x159C, 8),
        new(0x1EEC, 9)
    ];

    public const int AllocationSize = 0x2904;
    public const int OwnerContextOffset = 0x10;
    public const int ChatPointerOffset = 0x28E8;
    public const int ChatAllocationSize = MatrixChatFileNativeLayout.NativeAllocationSize;
    public const int ScenePointerOffset = 0x14;
    public const int InitializationMethodFileOffset = 0x64B460;
    public const int ShutdownMethodFileOffset = 0x64B950;
    public const int ObserverStateMethodFileOffset = 0x64BC40;
    public const int LayerModeMethodFileOffset = 0x64C020;
    public const int RecordInputMethodFileOffset = 0x64DCF0;
    public const int CallbackStateResetMethodFileOffset = 0x651A60;
    public const int MemberAt2E8GetterMethodFileOffset = 0x64A0C0;
    public const int MemberAt2ECGetterMethodFileOffset = 0x64A0D0;
    public const int MemberAt2E8Offset = 0x2E8;
    public const int MemberAt2ECOffset = 0x2EC;
    public const int MemberAt2E8InitialValue = 0;
    public const int MemberAt2ECAllocatedBlockSize = 0x2C;
    public const int RecordInputSize = 0x20;
    public const int RecordInputRequiredSceneSubstate = 3;
    public const int RecordInputStoredInModeOffset = 0x20;
    public const int RecordInputFirstObservedFloatOffset = 8;
    public const int RecordInputSecondObservedFloatOffset = 12;
    public const int LayerStackGlobalVirtualAddress = 0x1DFFEF4;

    public static IReadOnlyList<TfWorldLayerSnapshot> LayersByOffset => Layers;
}
