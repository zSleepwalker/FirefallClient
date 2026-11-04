using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolControlChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Shader-source pool control projection anchors the helper's captured stores",
            UiShaderSourcePoolControlProjectionRules.HelperRawFileOffset == 0x12CC700
            && UiShaderSourcePoolControlProjectionRules.ControlAllocationCallRawFileOffset == 0x12CC739
            && UiShaderSourcePoolControlProjectionRules.ControlAllocationRequestSize == 0x14
            && UiShaderSourcePoolControlProjectionRules.ControlPrimaryCountStoreRawFileOffset == 0x12CC74A
            && UiShaderSourcePoolControlProjectionRules.ControlSecondaryCountStoreRawFileOffset == 0x12CC751
            && UiShaderSourcePoolControlProjectionRules.ControlVtableStoreRawFileOffset == 0x12CC758
            && UiShaderSourcePoolControlProjectionRules.ControlHeaderStoreRawFileOffset == 0x12CC75E
            && UiShaderSourcePoolControlProjectionRules.OutputControlStoreRawFileOffset == 0x12CC794
            && UiShaderSourcePoolControlProjectionRules.OutputHeaderStoreRawFileOffset == 0x12CC797
            && UiShaderSourcePoolControlProjectionRules.ControlPrimaryCountOffset == 4
            && UiShaderSourcePoolControlProjectionRules.ControlSecondaryCountOffset == 8
            && UiShaderSourcePoolControlProjectionRules.ControlVtableOffset == 0
            && UiShaderSourcePoolControlProjectionRules.ControlHeaderPointerOffset == 0x0C
            && UiShaderSourcePoolControlProjectionRules.OutputPairHeaderPointerOffset == 0
            && UiShaderSourcePoolControlProjectionRules.OutputPairControlPointerOffset == 4
            && UiShaderSourcePoolControlProjectionRules.ControlVtableVa == 0x1C80C8C);

        (uint Header, uint Control)[] pairs =
        {
            (0, 0),
            (0, 0x10203040),
            (0xA1B2C3D4, 0),
            (0xA1B2C3D4, 0x10203040)
        };

        bool capturedPairsMatch = true;
        bool controlFieldCountsMatch = true;
        bool outputPairCountsMatch = true;
        bool outputWriteOrderAndValuesMatch = true;
        bool controlFieldOrderAndValuesMatch = true;

        foreach ((uint header, uint controlPointer) in pairs)
        {
            UiShaderSourcePoolControlProjection projection =
                UiShaderSourcePoolControlProjectionRules.Project(header, controlPointer);

            capturedPairsMatch &=
                projection.CapturedOutputPair.HeaderPointer == header
                && projection.CapturedOutputPair.ControlPointer == controlPointer;
            controlFieldCountsMatch &=
                projection.ControlFieldWrites.Length == (controlPointer == 0 ? 0 : 4);
            outputPairCountsMatch &= projection.OutputPairWrites.Length == 2;
            outputWriteOrderAndValuesMatch &=
                projection.OutputPairWrites.Length == 2
                && projection.OutputPairWrites[0]
                    == new UiShaderSourcePoolControlOutputWrite(
                        UiShaderSourcePoolControlOutputWriteTarget.ControlPointer,
                        controlPointer)
                && projection.OutputPairWrites[1]
                    == new UiShaderSourcePoolControlOutputWrite(
                        UiShaderSourcePoolControlOutputWriteTarget.HeaderPointer,
                        header);

            if (controlPointer != 0)
            {
                controlFieldOrderAndValuesMatch &=
                    projection.ControlFieldWrites.Length == 4
                    && projection.ControlFieldWrites[0]
                        == new UiShaderSourcePoolControlFieldWrite(4, 1)
                    && projection.ControlFieldWrites[1]
                        == new UiShaderSourcePoolControlFieldWrite(8, 1)
                    && projection.ControlFieldWrites[2]
                        == new UiShaderSourcePoolControlFieldWrite(
                            0,
                            UiShaderSourcePoolControlProjectionRules.ControlVtableVa)
                    && projection.ControlFieldWrites[3]
                        == new UiShaderSourcePoolControlFieldWrite(0x0C, header);
            }
        }

        check("Shader-source pool control projection preserves all null/non-null captured pairs",
            capturedPairsMatch);
        check("Shader-source pool control fields are emitted only for non-null control pointers",
            controlFieldCountsMatch && controlFieldOrderAndValuesMatch);
        check("Shader-source pool output pair always writes control then header, including null values",
            outputPairCountsMatch && outputWriteOrderAndValuesMatch);
    }
}
