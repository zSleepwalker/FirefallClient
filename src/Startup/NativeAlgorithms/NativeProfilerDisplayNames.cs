namespace Firefall.Reconstruction.Startup;

public enum NativeProfilerDisplayNameSource
{
    CachedOverride,
    RootNameTable,
    NodeNameTable,
    ExclusiveLabel,
    WaitingForServer,
    ErrorLabel
}

public sealed record NativeProfilerDisplayNameResult(
    NativeProfilerDisplayNameSource Source, ReadOnlyMemory<byte> LogicalTextBytes);

/// <summary>Raw logical text selected by the virtual getter; no text decoding, GUI or shared-text refcounts.</summary>
public static class NativeProfilerDisplayNames
{
    public const uint GetterFileOffset = 0xE8CD70;
    public const uint RootLookupFileOffset = 0xE648E0;
    public const uint NodeLookupFileOffset = 0xE64920;
    public const uint NativeVtableVirtualAddress = 0x1C78680;
    public const int NativeGetterVtableSlotOffset = 0x10;
    public const int NativeNameMapsOffset = 0x8E0;
    public const int NativeCachedNameOffset = 0x8E4;

    public static NativeProfilerDisplayNameResult Resolve(
        NativeProfilerTreeKind kind, ushort id, MatrixProfilerNamesState names,
        ReadOnlyMemory<byte> sampledCachedName = default)
    {
        ArgumentNullException.ThrowIfNull(names);
        if (!sampledCachedName.IsEmpty)
            return new(NativeProfilerDisplayNameSource.CachedOverride, sampledCachedName.ToArray());
        switch (kind)
        {
            case NativeProfilerTreeKind.Exclusive:
                return new(NativeProfilerDisplayNameSource.ExclusiveLabel, "[Exclusive]"u8.ToArray());
            case NativeProfilerTreeKind.Root:
                if (names.RootNames.TryGetValue(id, out var root))
                    return new(NativeProfilerDisplayNameSource.RootNameTable, root.ToArray());
                break;
            case NativeProfilerTreeKind.Node:
                if (names.NodeNames.TryGetValue(id, out var node))
                    return new(NativeProfilerDisplayNameSource.NodeNameTable, node.ToArray());
                break;
            default:
                return new(NativeProfilerDisplayNameSource.ErrorLabel, "<Error>"u8.ToArray());
        }
        return new(NativeProfilerDisplayNameSource.WaitingForServer, "<Waiting For Server>"u8.ToArray());
    }
}
