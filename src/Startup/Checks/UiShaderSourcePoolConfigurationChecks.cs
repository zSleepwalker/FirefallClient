using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolConfigurationChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Configuration initializer anchors match the pinned sample and requested raw bounds",
            UiShaderSourcePoolConfigurationProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolConfigurationProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UiShaderSourcePoolConfigurationProjectionRules.EntryRawFileOffset == 0x12CDB40
            && UiShaderSourcePoolConfigurationProjectionRules.EntryVirtualAddress == 0x16CE740
            && UiShaderSourcePoolConfigurationProjectionRules.EntryRawFileOffset
                + UiShaderSourcePoolConfigurationProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == UiShaderSourcePoolConfigurationProjectionRules.EntryVirtualAddress
            && UiShaderSourcePoolConfigurationProjectionRules.DiagnosticAndPaddingEndRawFileOffsetExclusive
                == 0x12CDC1B
            && UiShaderSourcePoolConfigurationProjectionRules.DiagnosticAndPaddingEndVirtualAddressExclusive
                == 0x16CE81B
            && UiShaderSourcePoolConfigurationProjectionRules.NormalReturnInstructionRawFileOffset
                == 0x12CDC1B);

        check("Captured initializer bytes match the verified 0xDB-byte raw interval",
            UiShaderSourcePoolConfigurationProjectionRules.CapturedInitializerBytes
                == "55 8B EC 8B 45 10 56 8B F1 8B 4D 08 C7 06 00 00 00 00 "
                    + "C7 46 04 00 00 00 00 C7 46 08 00 00 00 00 C7 46 0C 00 00 00 00 "
                    + "C7 46 10 00 00 00 00 89 46 18 8B 45 0C 89 4E 14 C7 46 1C 00 00 00 00 "
                    + "89 46 20 85 C9 75 19 51 68 F5 01 00 00 68 5F 41 B7 01 68 83 41 B7 01 "
                    + "FF 15 A0 5E C1 01 83 C4 10 83 7E 20 00 77 1A 6A 00 68 F6 01 00 00 "
                    + "68 77 41 B7 01 68 FB 41 B7 01 FF 15 A0 5E C1 01 83 C4 10 "
                    + "83 7E 18 00 77 1A 6A 00 68 F7 01 00 00 68 EF 41 B7 01 68 13 42 B7 01 "
                    + "FF 15 A0 5E C1 01 83 C4 10 8B 4E 18 8D 41 FF 85 C1 74 1A "
                    + "6A 00 68 FD 01 00 00 68 07 42 B7 01 68 CB 41 B7 01 FF 15 A0 5E C1 01 "
                    + "83 C4 10 8B 4E 14 8B 46 18 8D 51 FF 03 D0 48 F7 D0 23 D0 8B C6 "
                    + "2B D1 89 56 1C 5E 5D"
            && UiShaderSourcePoolConfigurationProjectionRules.CapturedInitializerBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0xDB
            && UiShaderSourcePoolConfigurationProjectionRules.EntryRawFileOffset
                + (uint)UiShaderSourcePoolConfigurationProjectionRules.CapturedInitializerBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
                == UiShaderSourcePoolConfigurationProjectionRules.DiagnosticAndPaddingEndRawFileOffsetExclusive
            && UiShaderSourcePoolConfigurationProjectionRules.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes,
                        instruction.FieldOffset, instruction.DiagnosticId,
                        instruction.BranchTargetRawFileOffset))
                .SequenceEqual(new (
                    uint RawFileOffset,
                    string Bytes,
                    int? FieldOffset,
                    uint? DiagnosticId,
                    uint? BranchTargetRawFileOffset)[]
                {
                    (0x12CDB4C, "C7 06 00 00 00 00", 0x00, null, null),
                    (0x12CDB52, "C7 46 04 00 00 00 00", 0x04, null, null),
                    (0x12CDB59, "C7 46 08 00 00 00 00", 0x08, null, null),
                    (0x12CDB60, "C7 46 0C 00 00 00 00", 0x0C, null, null),
                    (0x12CDB67, "C7 46 10 00 00 00 00", 0x10, null, null),
                    (0x12CDB6E, "89 46 18", 0x18, null, null),
                    (0x12CDB74, "89 4E 14", 0x14, null, null),
                    (0x12CDB77, "C7 46 1C 00 00 00 00", 0x1C, null, null),
                    (0x12CDB7E, "89 46 20", 0x20, null, null),
                    (0x12CDB81, "85 C9", null, null, null),
                    (0x12CDB83, "75 19", null, null, 0x12CDB9E),
                    (0x12CDB86, "68 F5 01 00 00", null, 501, null),
                    (0x12CDB95, "FF 15 A0 5E C1 01", null, null, null),
                    (0x12CDB9E, "83 7E 20 00", 0x20, null, null),
                    (0x12CDBA2, "77 1A", null, null, 0x12CDBBE),
                    (0x12CDBA6, "68 F6 01 00 00", null, 502, null),
                    (0x12CDBB5, "FF 15 A0 5E C1 01", null, null, null),
                    (0x12CDBBE, "83 7E 18 00", 0x18, null, null),
                    (0x12CDBC2, "77 1A", null, null, 0x12CDBDE),
                    (0x12CDBC6, "68 F7 01 00 00", null, 503, null),
                    (0x12CDBD5, "FF 15 A0 5E C1 01", null, null, null),
                    (0x12CDBDE, "8B 4E 18 8D 41 FF", 0x18, null, null),
                    (0x12CDBE4, "85 C1", null, null, null),
                    (0x12CDBE6, "74 1A", null, null, 0x12CDC02),
                    (0x12CDBEA, "68 FD 01 00 00", null, 509, null),
                    (0x12CDBF9, "FF 15 A0 5E C1 01", null, null, null),
                    (0x12CDC02, "8B 4E 14 8B 46 18", null, null, null),
                    (0x12CDC08, "8D 51 FF 03 D0", null, null, null),
                    (0x12CDC0D, "48 F7 D0 23 D0", null, null, null),
                    (0x12CDC14, "2B D1 89 56 1C", 0x1C, null, null)
                }));

        string capturedBytes = string.Concat(
            UiShaderSourcePoolConfigurationProjectionRules.CapturedInitializerBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));
        check("Each instruction evidence slice matches its exact offset in the captured byte range",
            UiShaderSourcePoolConfigurationProjectionRules.Instructions.All(instruction =>
            {
                if (instruction.RawFileOffset
                    < UiShaderSourcePoolConfigurationProjectionRules.EntryRawFileOffset)
                {
                    return false;
                }

                string expectedSlice = string.Concat(
                    instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
                int start = checked((int)(instruction.RawFileOffset
                    - UiShaderSourcePoolConfigurationProjectionRules.EntryRawFileOffset) * 2);
                return start + expectedSlice.Length <= capturedBytes.Length
                    && capturedBytes.Substring(start, expectedSlice.Length) == expectedSlice;
            }));

        UiShaderSourcePoolConfigurationProjection known =
            UiShaderSourcePoolConfigurationProjectionRules.Project(
                CaptureRepeatedValues(size: 0x810, count: 0x20, alignment: 1));
        check("Known size/count/alignment arguments preserve ordered writes and zero padding",
            known.FieldWrites.Select(write => (write.FieldOffset, write.Value,
                    write.InstructionRawFileOffset))
                .SequenceEqual(new (int FieldOffset, uint Value, uint InstructionRawFileOffset)[]
                {
                    (0x00, 0, 0x12CDB4C),
                    (0x04, 0, 0x12CDB52),
                    (0x08, 0, 0x12CDB59),
                    (0x0C, 0, 0x12CDB60),
                    (0x10, 0, 0x12CDB67),
                    (0x18, 1, 0x12CDB6E),
                    (0x14, 0x810, 0x12CDB74),
                    (0x1C, 0, 0x12CDB77),
                    (0x20, 0x20, 0x12CDB7E),
                    (0x1C, 0, 0x12CDC16)
                })
            && known.DiagnosticIds.IsEmpty
            && known.PaddingBytes == 0
            && known.Boundary
                == UiShaderSourcePoolConfigurationBoundary.SelectedDiagnosticsReturnedNormally);

        UiShaderSourcePoolConfigurationProjection zeroArguments =
            UiShaderSourcePoolConfigurationProjectionRules.Project(
                CaptureRepeatedValues(0, 0, 0));
        check("Zero size/count/alignment select 501, 502, and 503 in observed order",
            zeroArguments.DiagnosticIds.SequenceEqual(new uint[] { 501, 502, 503 })
            && zeroArguments.Boundary
                == UiShaderSourcePoolConfigurationBoundary.SelectedDiagnosticsReturnedNormally);

        UiShaderSourcePoolConfigurationProjection invalidAlignment =
            UiShaderSourcePoolConfigurationProjectionRules.Project(
                CaptureRepeatedValues(0, 0, 3));
        check("Nonzero non-power-of-two alignment selects 509 after earlier diagnostics",
            invalidAlignment.DiagnosticIds.SequenceEqual(new uint[] { 501, 502, 509 })
            && invalidAlignment.PaddingBytes == 0
            && invalidAlignment.Boundary
                == UiShaderSourcePoolConfigurationBoundary.SelectedDiagnosticsReturnedNormally);

        UiShaderSourcePoolConfigurationProjection nonzeroInvalidAlignment =
            UiShaderSourcePoolConfigurationProjectionRules.Project(
                CaptureRepeatedValues(0x812, 7, 3));
        check("Invalid nonzero alignment still reaches the captured DWORD padding arithmetic",
            nonzeroInvalidAlignment.DiagnosticIds.SequenceEqual(new uint[] { 509 })
            && nonzeroInvalidAlignment.PaddingBytes == 2);

        UiShaderSourcePoolConfigurationProjection zeroAlignmentArithmetic =
            UiShaderSourcePoolConfigurationProjectionRules.Project(
                CaptureRepeatedValues(0x810, 0x20, 0));
        check("Zero alignment reports 503 but does not select 509 and arithmetic remains DWORD-wrapped",
            zeroAlignmentArithmetic.DiagnosticIds.SequenceEqual(new uint[] { 503 })
            && zeroAlignmentArithmetic.PaddingBytes == unchecked(0u - 0x810u));

        UiShaderSourcePoolConfigurationProjection powerOfTwo =
            UiShaderSourcePoolConfigurationProjectionRules.Project(
                CaptureRepeatedValues(0x810, 0x20, 0x10));
        check("Power-of-two alignment values bypass diagnostic 509",
            powerOfTwo.DiagnosticIds.IsEmpty
            && powerOfTwo.PaddingBytes == 0);

        UiShaderSourcePoolConfigurationProjection changingCaptures =
            UiShaderSourcePoolConfigurationProjectionRules.Project(
                new UiShaderSourcePoolConfigurationCapture(
                    SizeArgument: 0,
                    CountArgument: 7,
                    AlignmentArgument: 8,
                    CountAt9E: 0,
                    AlignmentAtBE: 0,
                    AlignmentAtDE: 3,
                    PaddingSizeAtC02: 0x812,
                    PaddingAlignmentAtC05: 3));
        check("Callback-visible later reads stay independent of the original argument stores",
            changingCaptures.DiagnosticIds.SequenceEqual(new uint[] { 501, 502, 503, 509 })
            && changingCaptures.PaddingBytes == 2
            && changingCaptures.FieldWrites[5].FieldOffset == 0x18
            && changingCaptures.FieldWrites[5].Value == 8
            && changingCaptures.FieldWrites[6].FieldOffset == 0x14
            && changingCaptures.FieldWrites[6].Value == 0
            && changingCaptures.FieldWrites[8].FieldOffset == 0x20
            && changingCaptures.FieldWrites[8].Value == 7
            && changingCaptures.FieldWrites[9].FieldOffset == 0x1C
            && changingCaptures.FieldWrites[9].Value == 2
            && changingCaptures.Capture.CountAt9E == 0
            && changingCaptures.Capture.AlignmentAtBE == 0
            && changingCaptures.Capture.AlignmentAtDE == 3
            && changingCaptures.Capture.PaddingSizeAtC02 == 0x812
            && changingCaptures.Capture.PaddingAlignmentAtC05 == 3);
    }

    private static UiShaderSourcePoolConfigurationCapture CaptureRepeatedValues(
        uint size,
        uint count,
        uint alignment) =>
        new(
            size,
            count,
            alignment,
            count,
            alignment,
            alignment,
            size,
            alignment);
}
