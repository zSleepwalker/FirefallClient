using System.Runtime.InteropServices;

namespace Firefall.Reconstruction.Serialization;

/// <summary>
/// Observed 32-bit member for the database tree at +0x28. Its pointer fields
/// are address tokens from the client process and are never dereferenced here.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 0x08)]
public struct SdbNativeDatabaseTree28Member32
{
    [FieldOffset(0x00)]
    public uint HeaderPointer32;

    [FieldOffset(0x04)]
    public uint EntryCount;
}

/// <summary>
/// Observed 32-bit header for the separate database tree at +0x28.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 0x18)]
public struct SdbNativeDatabaseTree28Header32
{
    [FieldOffset(0x00)]
    public uint LeftmostNodePointer32;

    [FieldOffset(0x04)]
    public uint RootNodePointer32;

    [FieldOffset(0x08)]
    public uint RightmostNodePointer32;

    [FieldOffset(0x0C)]
    public byte Flag0C;

    [FieldOffset(0x0D)]
    public byte SentinelMarker;
}

/// <summary>
/// Observed 32-bit node layout for the separate database tree at +0x28.
/// Key32 remains an opaque derived value; ValuePointer32 is an address token.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 0x18)]
public struct SdbNativeDatabaseTree28Node32
{
    [FieldOffset(0x00)]
    public uint LeftPointer32;

    [FieldOffset(0x04)]
    public uint ParentPointer32;

    [FieldOffset(0x08)]
    public uint RightPointer32;

    [FieldOffset(0x0C)]
    public byte Flag0C;

    [FieldOffset(0x0D)]
    public byte SentinelMarker;

    [FieldOffset(0x10)]
    public uint Key32;

    [FieldOffset(0x14)]
    public uint ValuePointer32;
}

public static class SdbNativeDatabaseTree28Layout
{
    public const int DatabaseMemberOffset = 0x28;
    public const int DatabaseEntryCountOffset = 0x2C;
    public const int DatabaseValueReleaseGateOffset = 0x1C;
    public const int DatabaseBackingBufferPointerOffset = 0x20;
    public const int DatabaseBackingBufferLengthOffset = 0x24;
    public const int DatabaseLookupContextPointerOffset = 0x30;
    public const int HeaderSize = 0x18;
    public const int NodeSize = 0x18;
    public const int NodeKeyOffset = 0x10;
    public const int NodeValuePointerOffset = 0x14;
    public const uint InitializerFileOffset = 0x130FCB0;
    public const uint HeaderCreateFileOffset = 0x1314980;
    public const uint LookupFileOffset = 0x13125F0;
    public const uint KeyAndValueCallerFileOffset = 0x13122D0;
    public const uint NodeCreateFileOffset = 0x1316120;
    public const uint InsertFileOffset = 0x1316150;
    public const uint ResetFileOffset = 0x1312030;
    public const uint DestructorFileOffset = 0x130FE30;
}
