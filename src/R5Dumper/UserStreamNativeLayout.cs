namespace Firefall.Reconstruction.R5Dumper;

/// <summary>
/// Observed receiver framing and descriptor layout. Does not read handles or parse metadata.
/// All addresses are preferred PE VAs in the x86 sample.
/// </summary>
public static class UserStreamNativeLayout
{
    public const uint ReadLineVa = 0x4012F0;
    public const uint ExtractCrLfLineVa = 0x401390;
    public const uint SplitFieldsVa = 0x401120;
    public const uint BuildDescriptorsVa = 0x401A60;
    public const uint InvokeDumpWriterVa = 0x401B50;
    public const int ReadChunkByteCount = 0x200;
    public const string LineTerminator = "\r\n";
    public const int DescriptorNativeByteCount = 12;
    public const int DescriptorTypeOffset = 0;
    public const int DescriptorBufferSizeOffset = 4;
    public const int DescriptorBufferPointerOffset = 8;
    public const int Int32BufferByteCount = 4;
    public const int Int64BufferByteCount = 8;
    public const uint MiniDumpWriteDumpPointerVa = 0x4064D0;
    public const uint CrLfLiteralVa = 0x4041DC;
    public const uint DbgHelpWriteExportRva = 0x5E800;
    public const int DbgHelpWriteExportOrdinal = 31;
    public const string DbgHelpModule = "dbghelp_red5.dll";
    public const string DbgHelpWriteExport = "MiniDumpWriteDump";
}
