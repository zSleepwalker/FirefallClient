using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolResetGrowthChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Reset growth helper identity, mapping, bounds, and tail target are exact",
            UiShaderSourcePoolResetGrowthProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolResetGrowthProjectionRules.EntryRawFileOffset == 0x006A5500
            && UiShaderSourcePoolResetGrowthProjectionRules.EntryVirtualAddress == 0x00AA6100
            && UiShaderSourcePoolResetGrowthProjectionRules.EntryRawFileOffset
                + UiShaderSourcePoolResetGrowthProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == UiShaderSourcePoolResetGrowthProjectionRules.EntryVirtualAddress
            && UiShaderSourcePoolResetGrowthProjectionRules.FunctionEndRawFileOffsetExclusive
                == 0x006A5573
            && UiShaderSourcePoolResetGrowthProjectionRules.TailTargetRawFileOffset == 0x006A5900
            && UiShaderSourcePoolResetGrowthProjectionRules.ReporterCellVirtualAddress
                == 0x01AC45C0
            && UiShaderSourcePoolResetGrowthProjectionRules.ReporterArgumentVirtualAddress
                == 0x01C5EA14
            && UiShaderSourcePoolResetGrowthProjectionRules.MaximumCapacityCount == 0x3FFFFFFF);

        check("Captured helper bytes exactly cover the function",
            UiShaderSourcePoolResetGrowthProjectionRules.CapturedFunctionBytes
                == "55 8B EC 8B 51 08 8B C2 56 8B 71 04 2B C6 57 8B "
                    + "7D 08 C1 F8 02 3B C7 73 54 2B 31 53 BB FF FF FF "
                    + "3F C1 FE 02 8B C3 2B C6 3B C7 73 0B 68 14 EA C5 "
                    + "01 FF 15 C0 45 AC 01 2B 11 03 F7 C1 FA 02 8B C2 "
                    + "D1 E8 2B D8 3B DA 5B 73 12 33 D2 3B D6 5F 0F 42 "
                    + "D6 5E 89 55 08 5D E9 A5 03 00 00 03 D0 3B D6 5F "
                    + "0F 42 D6 5E 89 55 08 5D E9 93 03 00 00 5F 5E 5D "
                    + "C2 04 00"
            && UiShaderSourcePoolResetGrowthProjectionRules.CapturedFunctionBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x73
            && UiShaderSourcePoolResetGrowthProjectionRules.EntryRawFileOffset
                + (uint)UiShaderSourcePoolResetGrowthProjectionRules.CapturedFunctionBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
                == UiShaderSourcePoolResetGrowthProjectionRules.FunctionEndRawFileOffsetExclusive);

        check("Instruction slices preserve unsigned comparisons, independent begin reads, and bounded tail targets",
            UiShaderSourcePoolResetGrowthProjectionRules.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes,
                        instruction.DescriptorFieldOffset,
                        instruction.DiagnosticArgumentVirtualAddress,
                        instruction.TargetRawFileOffset,
                        instruction.TargetVirtualAddress))
                .SequenceEqual(new (
                    uint RawFileOffset,
                    string Bytes,
                    int? DescriptorFieldOffset,
                    uint? DiagnosticArgumentVirtualAddress,
                    uint? TargetRawFileOffset,
                    uint? TargetVirtualAddress)[]
                {
                    (0x006A5503, "8B 51 08", 8, null, null, null),
                    (0x006A5509, "8B 71 04", 4, null, null, null),
                    (0x006A550C, "2B C6", null, null, null, null),
                    (0x006A5512, "C1 F8 02", null, null, null, null),
                    (0x006A5515, "3B C7", null, null, null, null),
                    (0x006A5517, "73 54", null, null, 0x006A556D, null),
                    (0x006A5519, "2B 31", 0, null, null, null),
                    (0x006A5521, "C1 FE 02", null, null, null, null),
                    (0x006A5528, "3B C7", null, null, null, null),
                    (0x006A552A, "73 0B", null, null, 0x006A5537, null),
                    (0x006A552C, "68 14 EA C5 01", null, 0x01C5EA14, null, null),
                    (0x006A5531, "FF 15 C0 45 AC 01", null, null, null, 0x01AC45C0),
                    (0x006A5537, "2B 11", 0, null, null, null),
                    (0x006A5539, "03 F7", null, null, null, null),
                    (0x006A553B, "C1 FA 02", null, null, null, null),
                    (0x006A5540, "D1 E8", null, null, null, null),
                    (0x006A5542, "2B D8", null, null, null, null),
                    (0x006A5544, "3B DA", null, null, null, null),
                    (0x006A5547, "73 12", null, null, 0x006A555B, null),
                    (0x006A5556, "E9 A5 03 00 00", null, null, 0x006A5900, null),
                    (0x006A555B, "03 D0", null, null, null, null),
                    (0x006A555D, "3B D6", null, null, null, null),
                    (0x006A5568, "E9 93 03 00 00", null, null, 0x006A5900, null),
                    (0x006A5570, "C2 04 00", null, null, null, null)
                })
            && RelativeBranchTarget(0x006A5517, "73 54") == 0x006A556D
            && RelativeBranchTarget(0x006A552A, "73 0B") == 0x006A5537
            && RelativeBranchTarget(0x006A5547, "73 12") == 0x006A555B
            && RelativeJumpTarget(0x006A5556, "E9 A5 03 00 00") == 0x006A5900
            && RelativeJumpTarget(0x006A5568, "E9 93 03 00 00") == 0x006A5900
            && InstructionSlicesMatchCapturedBytes());

        UiShaderSourcePoolResetGrowthProjection immediateReturn =
            UiShaderSourcePoolResetGrowthProjectionRules.Project(
                new UiShaderSourcePoolResetGrowthCapture(
                    CapacityEndAtEntry: 0x200,
                    EndAtEntry: 0x1B0,
                    Request: 20));
        check("Spare-count arithmetic uses SAR's signed bit pattern then an unsigned request comparison",
            immediateReturn.SpareCount == 20
            && immediateReturn.ReturnEax == 20
            && immediateReturn.Boundary
                == UiShaderSourcePoolResetGrowthBoundary.ImmediateReturn
            && immediateReturn.UsedCount is null
            && immediateReturn.TailTransferRawFileOffset is null);

        UiShaderSourcePoolResetGrowthProjection highBitSpare =
            UiShaderSourcePoolResetGrowthProjectionRules.Project(
                new UiShaderSourcePoolResetGrowthCapture(
                    CapacityEndAtEntry: uint.MaxValue,
                    EndAtEntry: 0,
                    Request: 1));
        check("A high-bit SAR result remains an unsigned value for the spare-count branch",
            highBitSpare.SpareCount == uint.MaxValue
            && highBitSpare.Boundary
                == UiShaderSourcePoolResetGrowthBoundary.ImmediateReturn);

        UiShaderSourcePoolResetGrowthProjection growth =
            UiShaderSourcePoolResetGrowthProjectionRules.Project(
                new UiShaderSourcePoolResetGrowthCapture(
                    CapacityEndAtEntry: 0x200,
                    EndAtEntry: 0x200,
                    Request: 8,
                    BeginAtUsedCalculation: 0x100,
                    BeginAtCapacityCalculation: 0x180));
        check("Growth uses independent used and capacity begin captures and unsigned maximum selection",
            growth.SpareCount == 0
            && growth.UsedCount == 64
            && growth.RemainingCapacityLimit == 0x3FFFFFBF
            && growth.CapacityCount == 32
            && growth.RequiredCapacityCount == 72
            && growth.GrowthCandidateCount == 48
            && growth.SelectedCapacityCount == 72
            && growth.CapacityGrowthOverflowed == false
            && growth.Boundary
                == UiShaderSourcePoolResetGrowthBoundary.TailTransferToAllocatorUnmodeled
            && growth.TailTransferRawFileOffset
                == UiShaderSourcePoolResetGrowthProjectionRules.GrowthTailTransferRawFileOffset
            && growth.ReturnEax is null);

        UiShaderSourcePoolResetGrowthProjection reporterBoundary =
            UiShaderSourcePoolResetGrowthProjectionRules.Project(
                new UiShaderSourcePoolResetGrowthCapture(
                    CapacityEndAtEntry: uint.MaxValue,
                    EndAtEntry: uint.MaxValue,
                    Request: 0x40000001,
                    BeginAtUsedCalculation: 0));
        check("An insufficient 0x3FFFFFFF-used limit stops at the indirect reporter call",
            reporterBoundary.SpareCount == 0
            && reporterBoundary.UsedCount == uint.MaxValue
            && reporterBoundary.RemainingCapacityLimit == 0x40000000
            && reporterBoundary.SelectedDiagnosticArgumentVirtualAddress == 0x01C5EA14
            && reporterBoundary.Boundary
                == UiShaderSourcePoolResetGrowthBoundary.DiagnosticReporterCallUnmodeled
            && reporterBoundary.CapacityCount is null
            && reporterBoundary.GrowthCandidateCount is null
            && reporterBoundary.SelectedCapacityCount is null
            && reporterBoundary.TailTransferRawFileOffset is null);

        UiShaderSourcePoolResetGrowthProjection overflowFallback =
            UiShaderSourcePoolResetGrowthProjectionRules.Project(
                new UiShaderSourcePoolResetGrowthCapture(
                    CapacityEndAtEntry: uint.MaxValue,
                    EndAtEntry: uint.MaxValue,
                    Request: 1,
                    BeginAtUsedCalculation: 0,
                    BeginAtCapacityCalculation: 0));
        check("Overflow resets the growth proposal to zero before the unsigned required-capacity maximum",
            overflowFallback.CapacityCount == uint.MaxValue
            && overflowFallback.CapacityGrowthOverflowed == true
            && overflowFallback.RequiredCapacityCount == 0
            && overflowFallback.GrowthCandidateCount == 0
            && overflowFallback.SelectedCapacityCount == 0
            && overflowFallback.TailTransferRawFileOffset
                == UiShaderSourcePoolResetGrowthProjectionRules.OverflowFallbackTailTransferRawFileOffset);

        bool missingUsedBeginRejected = ThrowsArgumentException(() =>
            UiShaderSourcePoolResetGrowthProjectionRules.Project(
                new UiShaderSourcePoolResetGrowthCapture(
                    CapacityEndAtEntry: 0x100,
                    EndAtEntry: 0x100,
                    Request: 1)));
        bool missingGrowthBeginRejected = ThrowsArgumentException(() =>
            UiShaderSourcePoolResetGrowthProjectionRules.Project(
                new UiShaderSourcePoolResetGrowthCapture(
                    CapacityEndAtEntry: 0x200,
                    EndAtEntry: 0x200,
                    Request: 1,
                    BeginAtUsedCalculation: 0x100)));
        bool speculativeGrowthCaptureRejected = ThrowsArgumentException(() =>
            UiShaderSourcePoolResetGrowthProjectionRules.Project(
                new UiShaderSourcePoolResetGrowthCapture(
                    CapacityEndAtEntry: uint.MaxValue,
                    EndAtEntry: uint.MaxValue,
                    Request: 0x40000001,
                    BeginAtUsedCalculation: 0,
                    BeginAtCapacityCalculation: 0)));
        check("Only reached field captures are accepted and the unknown reporter continuation is not guessed",
            missingUsedBeginRejected
            && missingGrowthBeginRejected
            && speculativeGrowthCaptureRejected);
    }

    private static bool InstructionSlicesMatchCapturedBytes()
    {
        string capturedBytes = string.Concat(
            UiShaderSourcePoolResetGrowthProjectionRules.CapturedFunctionBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));

        return UiShaderSourcePoolResetGrowthProjectionRules.Instructions.All(instruction =>
        {
            if (instruction.RawFileOffset
                < UiShaderSourcePoolResetGrowthProjectionRules.EntryRawFileOffset)
            {
                return false;
            }

            string expectedSlice = string.Concat(
                instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            int start = checked((int)(instruction.RawFileOffset
                - UiShaderSourcePoolResetGrowthProjectionRules.EntryRawFileOffset) * 2);
            return start + expectedSlice.Length <= capturedBytes.Length
                && capturedBytes.Substring(start, expectedSlice.Length) == expectedSlice;
        });
    }

    private static uint RelativeBranchTarget(uint instructionRawFileOffset, string bytes)
    {
        byte displacement = Convert.ToByte(
            bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1],
            16);
        return unchecked((uint)((long)instructionRawFileOffset + 2 + (sbyte)displacement));
    }

    private static uint RelativeJumpTarget(uint instructionRawFileOffset, string bytes) =>
        Relative32Target(instructionRawFileOffset, bytes, 5);

    private static uint Relative32Target(uint instructionRawFileOffset, string bytes, uint instructionSize)
    {
        byte[] displacementBytes = bytes
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .Select(value => Convert.ToByte(value, 16))
            .ToArray();
        int displacement =
            System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(displacementBytes);
        return unchecked((uint)((long)instructionRawFileOffset + instructionSize + displacement));
    }

    private static bool ThrowsArgumentException(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentException)
        {
            return true;
        }
    }
}
