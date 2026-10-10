using System.Runtime.InteropServices;

namespace Firefall.Reconstruction.Serialization;

/// <summary>
/// Observed 32-bit native result-holder layout created by the SDB row mapper.
/// Pointer fields are address tokens from the client process, not pointers
/// that can be dereferenced by this offline reconstruction.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 0x20)]
public struct SdbNativeMappedTableResult
{
    [FieldOffset(0x00)]
    public uint CatalogEntryPointer32;

    [FieldOffset(0x04)]
    public uint RowCount;

    [FieldOffset(0x0C)]
    public uint ExposedRowsPointer32;

    [FieldOffset(0x18)]
    public uint RefCountedHandlePointer32;
}
