namespace Firefall.Reconstruction.Startup;

public readonly record struct MatrixChannelCodec(ushort OpaqueTag, uint DecoderFileOffset, uint DecoderVirtualAddress);

/// <summary>Constructor-installed decoder routes. Tag derivation and complete wire decoding remain unresolved.</summary>
public static class MatrixChannelCodecNativeLayout
{
    public const int ConnectionChannel1TagOffset = 0x824;
    public const int ConnectionChannel1DecoderOffset = 0x828;
    public const int ConnectionOtherChannelsTagOffset = 0x82C;
    public const int ConnectionOtherChannelsDecoderOffset = 0x830;
    public const ushort Channel1OpaqueTag = 0xB968;
    public const ushort OtherChannelsOpaqueTag = 0x4C5F;
    public const uint Channel1DecoderFileOffset = 0x33E2E0;
    public const uint OtherChannelsDecoderFileOffset = 0x821BA0;
    public const uint Channel1HeaderReaderFileOffset = 0x34A020;
    public const uint Channel1DescriptorFactoryFileOffset = 0x33D4B0;
    public const uint Channel1FactoryJumpTableFileOffset = 0x33E250;

    public static MatrixChannelCodec GetForChannel(int channel) => channel switch
    {
        1 => new(Channel1OpaqueTag, Channel1DecoderFileOffset, 0x73EEE0),
        2 or 3 => new(OtherChannelsOpaqueTag, OtherChannelsDecoderFileOffset, 0xC227A0),
        _ => throw new ArgumentOutOfRangeException(nameof(channel),
            "Only matrix data channels 1, 2 and 3 use these constructor-installed codecs.")
    };
}
