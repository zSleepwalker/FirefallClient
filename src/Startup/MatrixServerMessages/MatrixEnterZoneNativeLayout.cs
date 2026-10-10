namespace Firefall.Reconstruction.Startup;

public readonly record struct MatrixEnterZoneObservedFields(ulong InstanceId, uint ZoneId, ulong MatchId, byte PreviewFlags);
public readonly record struct MatrixPreviewOverrides(bool Disabled, bool ForceZonePreview, bool ForceAssetPreview, bool ForceDataPreview);
public readonly record struct MatrixEnterZoneObservedScalarUpdate(
    int ConnectionState, MatrixEnterZoneObservedFields Fields, byte ConnectionFlagsAt83D, int GlobalWordAt34);

/// <summary>
/// Named log fields and scalar state/preview changes from msg-37.
/// Text references, nested fields, the 64-byte block, events and downstream initialization are not simulated.
/// </summary>
public static class MatrixEnterZoneNativeLayout
{
    public const uint HandlerFileOffset = 0x490C10;
    public const uint CopyZoneFieldsMethodFileOffset = 0x491070;
    public const uint DecodedMessageId = 37;
    public const int RequiredConnectionState = 4;
    public const int StateAfterHandling = 6;
    public const int DescriptorInstanceIdOffset = 0x28;
    public const int DescriptorZoneIdOffset = 0x30;
    public const int DescriptorPreviewFlagsOffset = 0x40;
    public const int DescriptorMatchIdOffset = 0x58;
    public const int DescriptorZoneNameOffset = 0x68;
    public const int DescriptorOpaqueBlockOffset = 0x80;
    public const int DescriptorSpectatorByteOffset = 0xC0;
    public const int ConnectionZoneFieldsOffset = 0x878;
    public const int ConnectionInstanceIdOffset = 0x878;
    public const int ConnectionZoneIdOffset = 0x880;
    public const int ConnectionPreviewFlagsOffset = 0x890;
    public const int ConnectionMatchIdOffset = 0x8A8;
    public const int ConnectionZoneNameOffset = 0x8B8;
    public const int ConnectionOpaqueBlockOffset = 0x8D0;
    public const int OpaqueBlockSize = 64;
    public const byte ZonePreviewMask = 1;
    public const byte DataPreviewMask = 2;
    public const byte AssetPreviewMask = 4;
    public const byte SpectatorMaskAt83D = 2;
    public const uint GlobalObjectPointerVirtualAddress = 0x1E46770;
    public const int GlobalWordOffset = 0x34;
    public const string NativeLogFormat = "EnterZone \"%s\": zone %u, instance %016llx, match %016llx";

    public static MatrixEnterZoneObservedScalarUpdate CaptureScalarUpdate(
        int connectionState, MatrixEnterZoneObservedFields fields, byte connectionFlagsAt83D,
        MatrixPreviewOverrides previewOverrides, bool connectAsSpectator)
    {
        if (connectionState != RequiredConnectionState)
            throw new ArgumentOutOfRangeException(nameof(connectionState),
                "The native EnterZone handler asserts unless connection state is 4.");
        if (fields.InstanceId == 0 || (fields.InstanceId & 0xFFFFFFFFFFFFFF00UL) == 0xFFFFFFFFFFFFFF00UL)
            throw new ArgumentOutOfRangeException(nameof(fields),
                "The native instance-ID guard rejects zero and the all-FF upper-56-bit sentinel.");

        byte flags = fields.PreviewFlags;
        if (previewOverrides.Disabled)
            flags &= 0xF8;
        if (previewOverrides.ForceZonePreview)
            flags |= ZonePreviewMask;
        if (previewOverrides.ForceAssetPreview)
            flags |= AssetPreviewMask;
        if (previewOverrides.ForceDataPreview)
            flags |= DataPreviewMask;
        if (connectAsSpectator)
            connectionFlagsAt83D |= SpectatorMaskAt83D;
        return new(StateAfterHandling, fields with { PreviewFlags = flags }, connectionFlagsAt83D,
            (flags & 7) == 0 ? 15 : 30);
    }
}
