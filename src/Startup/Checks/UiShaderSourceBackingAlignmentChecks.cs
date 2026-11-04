using Firefall.Reconstruction.Startup;

internal static class UiShaderSourceBackingAlignmentChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Shader-source backing alignment records the constructor arithmetic and header fields",
            UiShaderSourceBackingAlignmentProjectionRules.ConstructorEntryRawFileOffset == 0x12CB700
            && UiShaderSourceBackingAlignmentProjectionRules.CapacityStoreRawFileOffset == 0x12CB70A
            && UiShaderSourceBackingAlignmentProjectionRules.ZeroCapacityDiagnosticRawFileOffset == 0x12CB713
            && UiShaderSourceBackingAlignmentProjectionRules.AllocationSizeAddRawFileOffset == 0x12CB732
            && UiShaderSourceBackingAlignmentProjectionRules.AllocatorCallRawFileOffset == 0x12CB735
            && UiShaderSourceBackingAlignmentProjectionRules.AllocatorTargetRawFileOffset == 0x12B8030
            && UiShaderSourceBackingAlignmentProjectionRules.CapacityStoreInstructionBytesHex == "89 06"
            && UiShaderSourceBackingAlignmentProjectionRules.HeaderCapacityOffset == 0
            && UiShaderSourceBackingAlignmentProjectionRules.HeaderDataPointerOffset == 4
            && UiShaderSourceBackingAlignmentProjectionRules.HeaderAlignmentDisplacementOffset == 8
            && UiShaderSourceBackingAlignmentProjectionRules.HeaderLogicalLengthOffset == 0x0C);

        check("Shader-source backing alignment conditions a zero capacity result on diagnostic return",
            UiShaderSourceBackingAlignmentProjectionRules.Project(
                0, 0, 0x1000, capturedZeroCapacityDiagnosticReturned: false) is null
            && UiShaderSourceBackingAlignmentProjectionRules.Project(
                0, 0, 0x1000, capturedZeroCapacityDiagnosticReturned: true)
                == new UiShaderSourceBackingAlignmentProjection(
                    AllocationByteCount: 0,
                    HeaderCapacity: 0,
                    DataPointer: 0x1000,
                    AlignmentDisplacement: 0,
                    LogicalLength: 0));

        uint[] capacities = [0, 1, 0x20, uint.MaxValue];
        uint[] alignments = [0, 1, uint.MaxValue];
        bool capacityAndAlignmentCasesMatch = true;
        foreach (uint capacity in capacities)
        {
            foreach (uint alignment in alignments)
            {
                UiShaderSourceBackingAlignmentProjection projection =
                    UiShaderSourceBackingAlignmentProjectionRules.Project(
                        capacity,
                        alignment,
                        0x12345678,
                        capturedZeroCapacityDiagnosticReturned: capacity == 0)!.Value;
                uint expectedDisplacement = alignment == 0
                    ? 0
                    : unchecked(alignment - 0x12345678u % alignment);
                capacityAndAlignmentCasesMatch &=
                    projection.AllocationByteCount == unchecked(capacity + alignment)
                    && projection.HeaderCapacity == capacity
                    && projection.DataPointer
                        == unchecked(0x12345678u + expectedDisplacement)
                    && projection.AlignmentDisplacement == expectedDisplacement
                    && projection.LogicalLength == 0;
            }
        }

        check("Shader-source backing header retains capacity across zero, unit, ordinary, and maximum values",
            capacityAndAlignmentCasesMatch
            && UiShaderSourceBackingAlignmentProjectionRules.Project(
                0x20, 0, 0x12345678, capturedZeroCapacityDiagnosticReturned: false)
                == new UiShaderSourceBackingAlignmentProjection(
                    AllocationByteCount: 0x20,
                    HeaderCapacity: 0x20,
                    DataPointer: 0x12345678,
                    AlignmentDisplacement: 0,
                    LogicalLength: 0));

        UiShaderSourceBackingAlignmentProjection alignedBase =
            UiShaderSourceBackingAlignmentProjectionRules.Project(
                0x20, 0x10, 0x1000, capturedZeroCapacityDiagnosticReturned: false)!.Value;
        UiShaderSourceBackingAlignmentProjection unalignedBase =
            UiShaderSourceBackingAlignmentProjectionRules.Project(
                0x20, 0x10, 0x1003, capturedZeroCapacityDiagnosticReturned: false)!.Value;
        check("Shader-source backing alignment advances by a full alignment when already aligned",
            alignedBase.DataPointer == 0x1010
            && alignedBase.AlignmentDisplacement == 0x10
            && unalignedBase.DataPointer == 0x1010
            && unalignedBase.AlignmentDisplacement == 0x0D);

        UiShaderSourceBackingAlignmentProjection unitAlignment =
            UiShaderSourceBackingAlignmentProjectionRules.Project(
                0x20, 1, 0x1000, capturedZeroCapacityDiagnosticReturned: false)!.Value;
        UiShaderSourceBackingAlignmentProjection maximumAlignment =
            UiShaderSourceBackingAlignmentProjectionRules.Project(
                0x20, uint.MaxValue, 0, capturedZeroCapacityDiagnosticReturned: false)!.Value;
        check("Shader-source backing alignment handles unit and maximum unsigned alignment",
            unitAlignment.DataPointer == 0x1001
            && unitAlignment.AlignmentDisplacement == 1
            && maximumAlignment.DataPointer == uint.MaxValue
            && maximumAlignment.AlignmentDisplacement == uint.MaxValue);

        UiShaderSourceBackingAlignmentProjection nullCapturedPointer =
            UiShaderSourceBackingAlignmentProjectionRules.Project(
                0x20, 8, 0, capturedZeroCapacityDiagnosticReturned: false)!.Value;
        check("Shader-source backing alignment preserves zero-pointer arithmetic without modeling runtime faults",
            nullCapturedPointer.DataPointer == 8
            && nullCapturedPointer.AlignmentDisplacement == 8);

        UiShaderSourceBackingAlignmentProjection wrappedRequestAndPointer =
            UiShaderSourceBackingAlignmentProjectionRules.Project(
                uint.MaxValue, 1, uint.MaxValue, capturedZeroCapacityDiagnosticReturned: false)!.Value;
        check("Shader-source backing alignment uses unchecked unsigned allocation and pointer arithmetic",
            wrappedRequestAndPointer.AllocationByteCount == 0
            && wrappedRequestAndPointer.DataPointer == 0
            && wrappedRequestAndPointer.AlignmentDisplacement == 1);
    }
}
