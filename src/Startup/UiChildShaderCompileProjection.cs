namespace Firefall.Reconstruction.Startup;

public enum UiChildShaderCompileVariant
{
    GeneralDrawingVertexShader,
    GeneralDrawingPixelShader
}

public readonly record struct UiChildShaderCompileCapture(
    UiChildShaderCompileVariant Variant,
    uint SourceDataVirtualAddress,
    uint SourceDataByteLength,
    uint CodeBlobOutputLocalAddress,
    uint ErrorBlobOutputLocalAddress);

public readonly record struct UiChildShaderCompileArgumentProjection(
    uint Argument1SourceDataVirtualAddress,
    uint Argument2SourceDataByteLength,
    uint Argument3SourceNamePointer,
    uint Argument4DefinesPointer,
    uint Argument5IncludePointer,
    string Argument6EntryPoint,
    string Argument7Target,
    uint Argument8Flags1,
    uint Argument9Flags2,
    uint Argument10CodeBlobOutputLocalAddress,
    uint Argument11ErrorBlobOutputLocalAddress);

/// <summary>
/// Projects the eleven captured D3DCompile arguments without invoking compilation or modeling blob ownership.
/// Pointer roles are assigned by the confirmed D3DCompile parameter positions; this does not assert callee identity
/// or validate any captured pointer.
/// </summary>
public static class UiChildShaderCompileProjectionRules
{
    public const uint D3DCompileImportAddressVirtualAddress = 0x1AC40AC;
    public const uint D3DCompileImportAddressRawFileOffset = 0x16C2CAC;
    public const string D3DCompileImportModule = "D3DCOMPILER_43.dll";
    public const string D3DCompileImportName = "D3DCompile";
    public const uint VertexShaderHelperRawFileOffset = 0xCA7090;
    public const uint PixelShaderHelperRawFileOffset = 0xCA71F0;
    public const uint VertexShaderCallRawFileOffset = 0xCA70FA;
    public const uint PixelShaderCallRawFileOffset = 0xCA725A;

    public const string VertexShaderEntryPoint = "general_drawing_vs";
    public const string VertexShaderTarget = "vs_3_0";
    public const string PixelShaderEntryPoint = "general_drawing_ps";
    public const string PixelShaderTarget = "ps_3_0";
    public const uint NullPointerArgument = 0;
    public const uint Flags1 = 0;
    public const uint Flags2 = 0;

    public static UiChildShaderCompileArgumentProjection Project(UiChildShaderCompileCapture capture)
    {
        (string entryPoint, string target) = capture.Variant switch
        {
            UiChildShaderCompileVariant.GeneralDrawingVertexShader =>
                (VertexShaderEntryPoint, VertexShaderTarget),
            UiChildShaderCompileVariant.GeneralDrawingPixelShader =>
                (PixelShaderEntryPoint, PixelShaderTarget),
            _ => throw new ArgumentOutOfRangeException(
                nameof(capture),
                capture.Variant,
                "The shader compile variant is not one of the observed callsites.")
        };

        return new UiChildShaderCompileArgumentProjection(
            capture.SourceDataVirtualAddress,
            capture.SourceDataByteLength,
            NullPointerArgument,
            NullPointerArgument,
            NullPointerArgument,
            entryPoint,
            target,
            Flags1,
            Flags2,
            capture.CodeBlobOutputLocalAddress,
            capture.ErrorBlobOutputLocalAddress);
    }
}
