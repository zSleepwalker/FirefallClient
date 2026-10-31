namespace Firefall.Reconstruction.Startup;

public enum MatrixOtherChannelDecoderRoute
{
    RoutedMessage,
    RoutedMultipleMessage,
    SecondaryDispatcher
}

/// <summary>
/// Compact-code dispatch for the shared channel-2/channel-3 decoder.
/// Message payload parsing and factory-result semantics remain unresolved.
/// </summary>
public static class MatrixOtherChannelDecoderNativeLayout
{
    public const uint MethodFileOffset = 0x821BA0;
    public const uint DispatchTableVirtualAddress = 0xC229BC;
    public const uint DispatchTableFileOffset = 0x821DBC;
    public const uint CodeRouteMapVirtualAddress = 0xC229C8;
    public const uint CodeRouteMapFileOffset = 0x821DC8;
    public const int MinimumDispatchedCode = 1;
    public const int MaximumDispatchedCode = 9;
    public const uint GeneralRouteFileOffset = 0x821CF7;
    public const uint SpecialRouteFileOffset = 0x821D41;
    public const uint SecondaryDispatcherRouteCaseFileOffset = 0x821D7C;
    public const uint SecondaryDispatcherFileOffset = 0x81CC50;
    public const uint RoutedMessageFactoryFileOffset = 0xBC9B80;
    public const uint RoutedMultipleMessageFactoryFileOffset = 0xBC9DB0;
    public const uint RoutedMessageSerializeMethodFileOffset = 0xBCB070;
    public const uint RoutedMessageDeserializeWrapperFileOffset = 0xBCB0A0;
    public const uint RoutedMessageDeserializeMethodFileOffset = 0xBC98B0;
    public const uint RoutedMultipleMessageSerializeMethodFileOffset = 0xBCB120;
    public const uint RoutedMultipleMessageDeserializeWrapperFileOffset = 0xBCB160;
    public const uint RoutedMultipleMessageDeserializeMethodFileOffset = 0xBC99A0;
    public const int RoutedMessageBlobSliceOffsetFieldOffset = 0x38;
    public const int RoutedMessageBlobSliceLengthFieldOffset = 0x3C;
    public const int RoutedMultipleMessageBlobSliceOffsetFieldOffset = 0x38;
    public const int RoutedMultipleMessageBlobSliceLengthFieldOffset = 0x3A;
    public const uint RoutedMultipleMessageBaseVtableVirtualAddress = 0x1C7392C;
    public const uint RoutedMessageBaseVtableVirtualAddress = 0x1C7393C;
    public const uint RoutedMessageVtableVirtualAddress = 0x1C7394C;
    public const uint RoutedMultipleMessageVtableVirtualAddress = 0x1C7395C;
    public const string RoutedMultipleMessageBaseRttiName = ".?AV?$MessageBaseT@VRoutedMultipleMessage@nl@@@nl@@";
    public const string RoutedMessageBaseRttiName = ".?AV?$MessageBaseT@VRoutedMessage@nl@@@nl@@";
    public const string RoutedMessageRttiName = ".?AVRoutedMessage@nl@@";
    public const string RoutedMultipleMessageRttiName = ".?AVRoutedMultipleMessage@nl@@";

    public static MatrixOtherChannelDecoderRoute SelectRoute(int compactCode) => compactCode switch
    {
        1 or 2 or 3 or 4 or 5 or 6 or 9 => MatrixOtherChannelDecoderRoute.RoutedMessage,
        8 => MatrixOtherChannelDecoderRoute.RoutedMultipleMessage,
        _ => MatrixOtherChannelDecoderRoute.SecondaryDispatcher
    };
}
