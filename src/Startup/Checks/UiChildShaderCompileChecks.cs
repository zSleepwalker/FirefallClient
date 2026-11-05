using Firefall.Reconstruction.Startup;

internal static class UiChildShaderCompileChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("D3DCompile evidence records the import, helper, and both observed callsites",
            UiChildShaderCompileProjectionRules.D3DCompileImportAddressVirtualAddress == 0x1AC40AC
            && UiChildShaderCompileProjectionRules.D3DCompileImportAddressRawFileOffset == 0x16C2CAC
            && UiChildShaderCompileProjectionRules.D3DCompileImportModule == "D3DCOMPILER_43.dll"
            && UiChildShaderCompileProjectionRules.D3DCompileImportName == "D3DCompile"
            && UiChildShaderCompileProjectionRules.VertexShaderHelperRawFileOffset == 0xCA7090
            && UiChildShaderCompileProjectionRules.PixelShaderHelperRawFileOffset == 0xCA71F0
            && UiChildShaderCompileProjectionRules.VertexShaderCallRawFileOffset == 0xCA70FA
            && UiChildShaderCompileProjectionRules.PixelShaderCallRawFileOffset == 0xCA725A);

        var vertex = UiChildShaderCompileProjectionRules.Project(new UiChildShaderCompileCapture(
            UiChildShaderCompileVariant.GeneralDrawingVertexShader,
            0x12345678,
            0x0000ABCD,
            0x00001000,
            0x00002000));
        check("Vertex D3DCompile projection preserves all eleven API arguments in order",
            vertex == new UiChildShaderCompileArgumentProjection(
                0x12345678,
                0x0000ABCD,
                0,
                0,
                0,
                "general_drawing_vs",
                "vs_3_0",
                0,
                0,
                0x00001000,
                0x00002000));

        var pixel = UiChildShaderCompileProjectionRules.Project(new UiChildShaderCompileCapture(
            UiChildShaderCompileVariant.GeneralDrawingPixelShader,
            0x87654321,
            0xFEDCBA98,
            0x80000000,
            0xFFFFFFFF));
        check("Pixel D3DCompile projection preserves source length and high-bit output-local addresses",
            pixel == new UiChildShaderCompileArgumentProjection(
                0x87654321,
                0xFEDCBA98,
                0,
                0,
                0,
                "general_drawing_ps",
                "ps_3_0",
                0,
                0,
                0x80000000,
                0xFFFFFFFF));

        var zeroCapture = UiChildShaderCompileProjectionRules.Project(new UiChildShaderCompileCapture(
            UiChildShaderCompileVariant.GeneralDrawingVertexShader,
            0,
            0,
            0,
            0));
        check("D3DCompile projection preserves zero source and output addresses without pointer validation",
            zeroCapture.Argument1SourceDataVirtualAddress == 0
            && zeroCapture.Argument2SourceDataByteLength == 0
            && zeroCapture.Argument10CodeBlobOutputLocalAddress == 0
            && zeroCapture.Argument11ErrorBlobOutputLocalAddress == 0);

        bool invalidVariantRejected = false;
        try
        {
            UiChildShaderCompileProjectionRules.Project(new UiChildShaderCompileCapture(
                (UiChildShaderCompileVariant)2,
                0,
                0,
                0,
                0));
        }
        catch (ArgumentOutOfRangeException exception)
        {
            invalidVariantRejected = exception.ParamName == "capture";
        }
        check("D3DCompile projection explicitly rejects unobserved variants",
            invalidVariantRejected);
    }
}
