using System.Runtime.InteropServices;

namespace Firefall.Reconstruction.Serialization;

/// <summary>
/// Raw DWORDs at offsets +0x28 and +0x2C in a compiled SDB catalog entry.
/// The values include pointer and sentinel cases; this offline layout does
/// not dereference them or assign field semantics.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 0x08)]
public struct SdbNativeCatalogOpaqueFields
{
    [FieldOffset(0x00)]
    public uint Field28Raw32;

    [FieldOffset(0x04)]
    public uint Field2CRaw32;
}
