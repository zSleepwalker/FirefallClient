using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolReplenishmentChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Replenishment entry, allocator call, sample, and raw-to-VA mapping are exact",
            UiShaderSourcePoolReplenishmentProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolReplenishmentProjectionRules.EntryRawFileOffset == 0x12CE0F0
            && UiShaderSourcePoolReplenishmentProjectionRules.EntryVirtualAddress == 0x16CECF0
            && UiShaderSourcePoolReplenishmentProjectionRules.EntryRawFileOffset
                + UiShaderSourcePoolReplenishmentProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == UiShaderSourcePoolReplenishmentProjectionRules.EntryVirtualAddress
            && UiShaderSourcePoolReplenishmentProjectionRules.EntryEndRawFileOffsetExclusive == 0x12CE11E
            && UiShaderSourcePoolReplenishmentProjectionRules.Field14LoadRawFileOffset
                + UiShaderSourcePoolReplenishmentProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x16CED05
            && UiShaderSourcePoolReplenishmentProjectionRules.AllocatorCallRawFileOffset == 0x12CE119
            && UiShaderSourcePoolReplenishmentProjectionRules.AllocatorCallRawFileOffset
                + UiShaderSourcePoolReplenishmentProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x16CED19
            && UiShaderSourcePoolReplenishmentProjectionRules.AllocatorFunctionRawFileOffset == 0x12C8A20
            && UiShaderSourcePoolReplenishmentProjectionRules.AllocatorFunctionRawFileOffset
                + UiShaderSourcePoolReplenishmentProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x16C9620);

        check("Captured bytes preserve distinct field reads and stop at the allocator call",
            UiShaderSourcePoolReplenishmentProjectionRules.CapturedEntryBytes
                == "8B 57 14 03 57 1C 0F AF 57 20 8B 77 18 83 C6 03 "
                    + "03 F2 8B CE E8 02 A9 FF FF"
            && UiShaderSourcePoolReplenishmentProjectionRules.CapturedEntryBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x19
            && UiShaderSourcePoolReplenishmentProjectionRules.EntryEndRawFileOffsetExclusive
                == UiShaderSourcePoolReplenishmentProjectionRules.Field14LoadRawFileOffset + 0x19
            && UiShaderSourcePoolReplenishmentProjectionRules.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes, instruction.FieldOffset,
                        instruction.TargetRawFileOffset))
                .SequenceEqual(new (
                    uint RawFileOffset,
                    string Bytes,
                    int? FieldOffset,
                    uint? TargetRawFileOffset)[]
                {
                    (0x12CE105, "8B 57 14", 0x14, null),
                    (0x12CE108, "03 57 1C", 0x1C, null),
                    (0x12CE10B, "0F AF 57 20", 0x20, null),
                    (0x12CE10F, "8B 77 18", 0x18, null),
                    (0x12CE112, "83 C6 03", null, null),
                    (0x12CE115, "03 F2", null, null),
                    (0x12CE117, "8B CE", null, null),
                    (0x12CE119, "E8 02 A9 FF FF", null, 0x12C8A20)
                })
            && UiShaderSourcePoolReplenishmentProjectionRules.Instructions[^1]
                .RawFileOffset + 5
                == UiShaderSourcePoolReplenishmentProjectionRules.EntryEndRawFileOffsetExclusive
            && UiShaderSourcePoolReplenishmentProjectionRules.Instructions[^1]
                .TargetRawFileOffset == 0x12C8A20);

        UiShaderSourcePoolReplenishmentProjection ordinary =
            UiShaderSourcePoolReplenishmentProjectionRules.Project(
                new UiShaderSourcePoolReplenishmentCapture(
                    Field14Value: 2,
                    Field1CValue: 3,
                    Field20Value: 4,
                    Field18Value: 7));
        check("Ordinary request arithmetic applies the distinct captured fields in observed order",
            ordinary.Field14PlusField1C == 5
            && ordinary.ScaledTerm == 20
            && ordinary.Field18PlusThree == 10
            && ordinary.RequestBytes == 30
            && ordinary.AllocatorCallRawFileOffset == 0x12CE119
            && ordinary.AllocatorFunctionRawFileOffset == 0x12C8A20);

        UiShaderSourcePoolReplenishmentProjection additionWrap =
            UiShaderSourcePoolReplenishmentProjectionRules.Project(
                new UiShaderSourcePoolReplenishmentCapture(
                    Field14Value: uint.MaxValue,
                    Field1CValue: 1,
                    Field20Value: 9,
                    Field18Value: 1));
        check("The first 32-bit addition wraps before the multiplication",
            additionWrap.Field14PlusField1C == 0
            && additionWrap.ScaledTerm == 0
            && additionWrap.Field18PlusThree == 4
            && additionWrap.RequestBytes == 4);

        UiShaderSourcePoolReplenishmentProjection multiplicationWrap =
            UiShaderSourcePoolReplenishmentProjectionRules.Project(
                new UiShaderSourcePoolReplenishmentCapture(
                    Field14Value: 0x80000000,
                    Field1CValue: 0,
                    Field20Value: 2,
                    Field18Value: uint.MaxValue));
        check("The multiply and subsequent additions retain only their low 32 bits",
            multiplicationWrap.Field14PlusField1C == 0x80000000
            && multiplicationWrap.ScaledTerm == 0
            && multiplicationWrap.Field18PlusThree == 2
            && multiplicationWrap.RequestBytes == 2);

        UiShaderSourcePoolReplenishmentProjection finalAdditionWrap =
            UiShaderSourcePoolReplenishmentProjectionRules.Project(
                new UiShaderSourcePoolReplenishmentCapture(
                    Field14Value: 2,
                    Field1CValue: 0,
                    Field20Value: 0x7FFFFFFF,
                    Field18Value: 0));
        check("The final scaled-term addition wraps at the 32-bit request boundary",
            finalAdditionWrap.ScaledTerm == 0xFFFFFFFE
            && finalAdditionWrap.Field18PlusThree == 3
            && finalAdditionWrap.RequestBytes == 1);

        UiShaderSourcePoolReplenishmentProjection zeroBoundary =
            UiShaderSourcePoolReplenishmentProjectionRules.Project(
                new UiShaderSourcePoolReplenishmentCapture(0, 0, 0, 0));
        UiShaderSourcePoolReplenishmentProjection maximumBoundary =
            UiShaderSourcePoolReplenishmentProjectionRules.Project(
                new UiShaderSourcePoolReplenishmentCapture(
                    uint.MaxValue,
                    uint.MaxValue,
                    uint.MaxValue,
                    uint.MaxValue));
        check("Zero and maximum captured DWORD values remain bounded 32-bit requests",
            zeroBoundary.RequestBytes == 3
            && maximumBoundary.Field14PlusField1C == 0xFFFFFFFE
            && maximumBoundary.ScaledTerm == 2
            && maximumBoundary.Field18PlusThree == 2
            && maximumBoundary.RequestBytes == 4);
    }
}
