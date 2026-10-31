using System.Runtime.InteropServices;

namespace Firefall.Reconstruction.Serialization;

[StructLayout(LayoutKind.Explicit, Size = 0x08)]
public struct SdbNativeRowHandle32
{
    [FieldOffset(0x00)]
    public uint VTablePointer32;

    [FieldOffset(0x04)]
    public uint DataPointer32;
}

public static class SdbNativeRowHandleVTable32
{
    public const uint VirtualAddress = 0x1C6883C;
    public const uint FileOffset = 0x186743C;
    public const uint Slot0TargetFileOffset = 0x70D3F0;
    public const uint Slot1TargetFileOffset = 0x716B20;
}
