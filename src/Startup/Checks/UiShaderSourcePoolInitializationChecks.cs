using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolInitializationChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Pool initialization anchors the pinned sample, publication fields, and configuration arguments",
            UiShaderSourcePoolInitializationProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolInitializationProjectionRules.InitializerRawFileOffset == 0x12CC110
            && UiShaderSourcePoolInitializationProjectionRules.InitializerRawFileOffset
                + UiShaderSourcePoolInitializationProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x16CCD10
            && UiShaderSourcePoolInitializationProjectionRules.AllocatorCallRawFileOffset == 0x12CC179
            && UiShaderSourcePoolInitializationProjectionRules.AllocatorConfigurationFunctionRawFileOffset
                == 0x12CDB40
            && UiShaderSourcePoolInitializationProjectionRules.ConfigurationCallRawFileOffset == 0x12CC19C
            && UiShaderSourcePoolInitializationProjectionRules.ConfigurationArgument0 == 0x810
            && UiShaderSourcePoolInitializationProjectionRules.ConfigurationArgument1 == 0x20
            && UiShaderSourcePoolInitializationProjectionRules.ConfigurationArgument2 == 1
            && UiShaderSourcePoolInitializationProjectionRules.SingletonHeadGlobalVirtualAddress == 0x1E4B0A8
            && UiShaderSourcePoolInitializationProjectionRules.PoolPointerGlobalVirtualAddress == 0x1E00510
            && UiShaderSourcePoolInitializationProjectionRules.AllocatorPointerOffset == 0x10
            && UiShaderSourcePoolInitializationProjectionRules.PriorSingletonHeadOffset == 0x0C
            && UiShaderSourcePoolInitializationProjectionRules.InitializedFlagOffset == 0x04);

        check("Pool initialization evidence preserves the exact observed instruction byte sequence",
            UiShaderSourcePoolInitializationProjectionRules.Instructions
                .Select(instruction => (instruction.RawFileOffset, instruction.Bytes))
                .SequenceEqual(new (uint RawFileOffset, string Bytes)[]
                {
                    (0x12CC179, "E8 32 BE FE FF"),
                    (0x12CC17E, "8B F8"),
                    (0x12CC18D, "85 FF"),
                    (0x12CC18F, "74 12"),
                    (0x12CC191, "6A 01"),
                    (0x12CC193, "6A 20"),
                    (0x12CC195, "68 10 08 00 00"),
                    (0x12CC19A, "8B CF"),
                    (0x12CC19C, "E8 9F 19 00 00"),
                    (0x12CC1A3, "33 FF"),
                    (0x12CC1A5, "A1 A8 B0 E4 01"),
                    (0x12CC1AA, "89 7E 10"),
                    (0x12CC1AD, "89 46 0C"),
                    (0x12CC1B0, "89 35 A8 B0 E4 01"),
                    (0x12CC1B6, "C6 46 04 01")
                })
            && UiShaderSourcePoolInitializationProjectionRules.Instructions
                .Single(instruction => instruction.RawFileOffset == 0x12CC18F)
                .RawFileOffset + 2 + 0x12 == 0x12CC1A3
            && UiShaderSourcePoolInitializationProjectionRules.Instructions
                .Where(instruction => instruction.TargetVirtualAddress.HasValue)
                .Select(instruction => instruction.TargetVirtualAddress!.Value)
                .SequenceEqual(new uint[]
                {
                    0x16CE740,
                    0x1E4B0A8,
                    0x1E4B0A8
                }));

        UiShaderSourcePoolInitializationProjection configured =
            UiShaderSourcePoolInitializationProjectionRules.Project(
                new UiShaderSourcePoolInitializationCapture(
                    ThisPointer: 0x10000000,
                    AllocatorReturnedPointer: 0x20000000,
                    PriorSingletonHead: 0x30000000));
        check("Nonzero allocator capture records the conditional call and exact common publication order",
            configured.Actions.SequenceEqual(new[]
            {
                new UiShaderSourcePoolInitializationAction(
                    UiShaderSourcePoolInitializationActionKind.CallAllocatorConfiguration,
                    0x12CC19C,
                    ReceiverPointer: 0x20000000,
                    FirstArgument: 0x810,
                    SecondArgument: 0x20,
                    ThirdArgument: 1),
                new UiShaderSourcePoolInitializationAction(
                    UiShaderSourcePoolInitializationActionKind.LoadPriorSingletonHead,
                    0x12CC1A5,
                    TargetVirtualAddress: 0x1E4B0A8,
                    Value: 0x30000000),
                new UiShaderSourcePoolInitializationAction(
                    UiShaderSourcePoolInitializationActionKind.StoreAllocatorPointer,
                    0x12CC1AA,
                    TargetVirtualAddress: 0x10000010,
                    Value: 0x20000000),
                new UiShaderSourcePoolInitializationAction(
                    UiShaderSourcePoolInitializationActionKind.StorePriorSingletonHead,
                    0x12CC1AD,
                    TargetVirtualAddress: 0x1000000C,
                    Value: 0x30000000),
                new UiShaderSourcePoolInitializationAction(
                    UiShaderSourcePoolInitializationActionKind.PublishThisAsSingletonHead,
                    0x12CC1B0,
                    TargetVirtualAddress: 0x1E4B0A8,
                    Value: 0x10000000),
                new UiShaderSourcePoolInitializationAction(
                    UiShaderSourcePoolInitializationActionKind.SetInitializedFlag,
                    0x12CC1B6,
                    TargetVirtualAddress: 0x10000004,
                    Value: 1)
            }));

        UiShaderSourcePoolInitializationProjection nullAllocation =
            UiShaderSourcePoolInitializationProjectionRules.Project(
                new UiShaderSourcePoolInitializationCapture(
                    ThisPointer: 0x12340000,
                    AllocatorReturnedPointer: 0,
                    PriorSingletonHead: 0xABCDEF00));
        check("Null allocation skips configuration but still records the common link, publication, and flag writes",
            nullAllocation.Actions.Select(action => action.Kind).SequenceEqual(new[]
            {
                UiShaderSourcePoolInitializationActionKind.LoadPriorSingletonHead,
                UiShaderSourcePoolInitializationActionKind.StoreAllocatorPointer,
                UiShaderSourcePoolInitializationActionKind.StorePriorSingletonHead,
                UiShaderSourcePoolInitializationActionKind.PublishThisAsSingletonHead,
                UiShaderSourcePoolInitializationActionKind.SetInitializedFlag
            })
            && nullAllocation.Actions[1].TargetVirtualAddress == 0x12340010
            && nullAllocation.Actions[1].Value == 0
            && nullAllocation.Actions[2].TargetVirtualAddress == 0x1234000C
            && nullAllocation.Actions[2].Value == 0xABCDEF00
            && nullAllocation.Actions[3].TargetVirtualAddress == 0x1E4B0A8
            && nullAllocation.Actions[3].Value == 0x12340000
            && nullAllocation.Actions[4].TargetVirtualAddress == 0x12340004
            && nullAllocation.Actions[4].Value == 1);

        UiShaderSourcePoolInitializationProjection wrappingThis =
            UiShaderSourcePoolInitializationProjectionRules.Project(
                new UiShaderSourcePoolInitializationCapture(
                    ThisPointer: uint.MaxValue,
                    AllocatorReturnedPointer: 0,
                    PriorSingletonHead: 0));
        check("Captured this-relative write targets preserve 32-bit address wraparound",
            wrappingThis.Actions[1].TargetVirtualAddress == 0x0F
            && wrappingThis.Actions[2].TargetVirtualAddress == 0x0B
            && wrappingThis.Actions[4].TargetVirtualAddress == 0x03);

        UiShaderSourcePoolInitializationProjection knownPoolObject =
            UiShaderSourcePoolInitializationProjectionRules.Project(
                new UiShaderSourcePoolInitializationCapture(
                    ThisPointer: 0x1E00500,
                    AllocatorReturnedPointer: 0x12345678,
                    PriorSingletonHead: 0));
        check("Captured this + 0x10 resolves to the known pool-pointer global for the 0x1E00500 instance",
            knownPoolObject.Actions[2].TargetVirtualAddress
                == UiShaderSourcePoolInitializationProjectionRules.PoolPointerGlobalVirtualAddress);
    }
}
