using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolControlChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Shader-source control projection anchors both observed variants",
            UiShaderSourcePoolControlProjectionRules.PoolHelperRawFileOffset == 0x12CC700
            && UiShaderSourcePoolControlProjectionRules.PoolControlAllocationCallRawFileOffset == 0x12CC739
            && UiShaderSourcePoolControlProjectionRules.PoolControlAllocationRequestSize == 0x14
            && UiShaderSourcePoolControlProjectionRules.PoolControlPrimaryCountStoreRawFileOffset == 0x12CC74A
            && UiShaderSourcePoolControlProjectionRules.PoolControlSecondaryCountStoreRawFileOffset == 0x12CC751
            && UiShaderSourcePoolControlProjectionRules.PoolControlVtableStoreRawFileOffset == 0x12CC758
            && UiShaderSourcePoolControlProjectionRules.PoolControlHeaderStoreRawFileOffset == 0x12CC75E
            && UiShaderSourcePoolControlProjectionRules.PoolOutputControlStoreRawFileOffset == 0x12CC794
            && UiShaderSourcePoolControlProjectionRules.PoolOutputHeaderStoreRawFileOffset == 0x12CC797
            && UiShaderSourcePoolControlProjectionRules.PoolControlVtableVa == 0x1C80C8C
            && UiShaderSourcePoolControlProjectionRules.NonPoolHelperRawFileOffset == 0x12CC7D0
            && UiShaderSourcePoolControlProjectionRules.NonPoolControlAllocationCallRawFileOffset == 0x12CC809
            && UiShaderSourcePoolControlProjectionRules.NonPoolControlAllocationRequestSize == 0x10
            && UiShaderSourcePoolControlProjectionRules.NonPoolControlPrimaryCountStoreRawFileOffset == 0x12CC81A
            && UiShaderSourcePoolControlProjectionRules.NonPoolControlSecondaryCountStoreRawFileOffset == 0x12CC821
            && UiShaderSourcePoolControlProjectionRules.NonPoolControlVtableStoreRawFileOffset == 0x12CC828
            && UiShaderSourcePoolControlProjectionRules.NonPoolControlHeaderStoreRawFileOffset == 0x12CC82E
            && UiShaderSourcePoolControlProjectionRules.NonPoolOutputControlStoreRawFileOffset == 0x12CC864
            && UiShaderSourcePoolControlProjectionRules.NonPoolOutputHeaderStoreRawFileOffset == 0x12CC867
            && UiShaderSourcePoolControlProjectionRules.NonPoolControlVtableVa == 0x1C80CB0
            && UiShaderSourcePoolControlProjectionRules.ControlPrimaryCountOffset == 4
            && UiShaderSourcePoolControlProjectionRules.ControlSecondaryCountOffset == 8
            && UiShaderSourcePoolControlProjectionRules.ControlVtableOffset == 0
            && UiShaderSourcePoolControlProjectionRules.ControlHeaderPointerOffset == 0x0C
            && UiShaderSourcePoolControlProjectionRules.OutputPairHeaderPointerOffset == 0
            && UiShaderSourcePoolControlProjectionRules.OutputPairControlPointerOffset == 4);

        (uint Header, uint Control)[] pairs =
        {
            (0, 0),
            (0, 0x10203040),
            (0xA1B2C3D4, 0),
            (0xA1B2C3D4, 0x10203040)
        };
        UiShaderSourcePoolControlVariant[] variants =
        {
            UiShaderSourcePoolControlVariant.Pool,
            UiShaderSourcePoolControlVariant.NonPool
        };

        bool variantMetadataMatches = true;
        bool capturedPairsMatch = true;
        bool controlFieldCountsMatch = true;
        bool outputPairCountsMatch = true;
        bool outputWriteOrderAndValuesMatch = true;
        bool controlFieldOrderAndValuesMatch = true;
        bool defaultProjectPreservesPoolBehavior = true;

        foreach (UiShaderSourcePoolControlVariant variant in variants)
        {
            uint expectedVtable = variant == UiShaderSourcePoolControlVariant.Pool
                ? UiShaderSourcePoolControlProjectionRules.PoolControlVtableVa
                : UiShaderSourcePoolControlProjectionRules.NonPoolControlVtableVa;
            uint expectedAllocationSize = variant == UiShaderSourcePoolControlVariant.Pool
                ? UiShaderSourcePoolControlProjectionRules.PoolControlAllocationRequestSize
                : UiShaderSourcePoolControlProjectionRules.NonPoolControlAllocationRequestSize;

            foreach ((uint header, uint controlPointer) in pairs)
            {
                UiShaderSourcePoolControlProjection projection =
                    UiShaderSourcePoolControlProjectionRules.Project(
                        header,
                        controlPointer,
                        variant);

                variantMetadataMatches &=
                    projection.Variant == variant
                    && projection.ControlAllocationRequestSize == expectedAllocationSize
                    && projection.ControlVtableVa == expectedVtable;
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
                                expectedVtable)
                        && projection.ControlFieldWrites[3]
                            == new UiShaderSourcePoolControlFieldWrite(0x0C, header);
                }

                if (variant == UiShaderSourcePoolControlVariant.Pool)
                {
                    UiShaderSourcePoolControlProjection defaultProjection =
                        UiShaderSourcePoolControlProjectionRules.Project(header, controlPointer);
                    defaultProjectPreservesPoolBehavior &=
                        defaultProjection.Variant == UiShaderSourcePoolControlVariant.Pool
                        && defaultProjection.ControlAllocationRequestSize == expectedAllocationSize
                        && defaultProjection.ControlVtableVa == expectedVtable
                        && defaultProjection.CapturedOutputPair == projection.CapturedOutputPair
                        && defaultProjection.ControlFieldWrites.SequenceEqual(
                            projection.ControlFieldWrites)
                        && defaultProjection.OutputPairWrites.SequenceEqual(
                            projection.OutputPairWrites);
                }
            }
        }

        check("Shader-source control projection selects the matching Pool/NonPool metadata",
            variantMetadataMatches);
        check("Shader-source control projection preserves all null/non-null captured pairs",
            capturedPairsMatch);
        check("Shader-source control fields use generic primary/secondary counts and variant vptr",
            controlFieldCountsMatch && controlFieldOrderAndValuesMatch);
        check("Shader-source control output pair writes control then header for both variants",
            outputPairCountsMatch && outputWriteOrderAndValuesMatch);
        check("Shader-source control projection keeps Project(header, control) Pool-compatible",
            defaultProjectPreservesPoolBehavior);
    }
}
