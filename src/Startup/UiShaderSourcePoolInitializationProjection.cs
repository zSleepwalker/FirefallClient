using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UiShaderSourcePoolInitializationCapture(
    uint ThisPointer,
    uint AllocatorReturnedPointer,
    uint PriorSingletonHead);

public enum UiShaderSourcePoolInitializationActionKind
{
    CallAllocatorConfiguration,
    LoadPriorSingletonHead,
    StoreAllocatorPointer,
    StorePriorSingletonHead,
    PublishThisAsSingletonHead,
    SetInitializedFlag
}

public readonly record struct UiShaderSourcePoolInitializationAction(
    UiShaderSourcePoolInitializationActionKind Kind,
    uint InstructionRawFileOffset,
    uint? TargetVirtualAddress = null,
    uint? Value = null,
    uint? ReceiverPointer = null,
    uint? FirstArgument = null,
    uint? SecondArgument = null,
    uint? ThirdArgument = null);

public readonly record struct UiShaderSourcePoolInitializationProjection(
    UiShaderSourcePoolInitializationCapture Capture,
    ImmutableArray<UiShaderSourcePoolInitializationAction> Actions);

public readonly record struct UiShaderSourcePoolInitializationInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    int? ThisRelativeOffset = null,
    uint? TargetVirtualAddress = null);

/// <summary>
/// Projects the observed allocator-configuration branch and singleton-list publication
/// writes. The configuration call is recorded without assuming its result or overall
/// readiness; this projection does not dereference pointers or mutate memory.
/// </summary>
public static class UiShaderSourcePoolInitializationProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint InitializerRawFileOffset = 0x12CC110;
    public const uint AllocatorCallRawFileOffset = 0x12CC179;
    public const uint AllocatorResultCaptureRawFileOffset = 0x12CC17E;
    public const uint ConfigurationNullTestRawFileOffset = 0x12CC18D;
    public const uint ConfigurationNullBranchRawFileOffset = 0x12CC18F;
    public const uint ConfigurationCallRawFileOffset = 0x12CC19C;
    public const uint NullAllocatorNormalizationRawFileOffset = 0x12CC1A3;
    public const uint PriorSingletonHeadLoadRawFileOffset = 0x12CC1A5;
    public const uint AllocatorPointerStoreRawFileOffset = 0x12CC1AA;
    public const uint PriorSingletonHeadStoreRawFileOffset = 0x12CC1AD;
    public const uint SingletonHeadPublishRawFileOffset = 0x12CC1B0;
    public const uint InitializedFlagStoreRawFileOffset = 0x12CC1B6;

    public const uint AllocatorConfigurationFunctionRawFileOffset = 0x12CDB40;
    public const uint SingletonHeadGlobalVirtualAddress = 0x1E4B0A8;
    public const uint PoolPointerGlobalVirtualAddress = 0x1E00510;

    public const uint ConfigurationArgument0 = 0x810;
    public const uint ConfigurationArgument1 = 0x20;
    public const uint ConfigurationArgument2 = 1;

    public const int InitializedFlagOffset = 0x04;
    public const int PriorSingletonHeadOffset = 0x0C;
    public const int AllocatorPointerOffset = 0x10;

    public static IReadOnlyList<UiShaderSourcePoolInitializationInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourcePoolInitializationInstructionEvidence(
                "Request 0x28 bytes from the allocator helper",
                AllocatorCallRawFileOffset,
                "E8 32 BE FE FF"),
            new UiShaderSourcePoolInitializationInstructionEvidence(
                "Capture the allocator return value in EDI",
                AllocatorResultCaptureRawFileOffset,
                "8B F8"),
            new UiShaderSourcePoolInitializationInstructionEvidence(
                "Test the captured allocator pointer before configuration",
                ConfigurationNullTestRawFileOffset,
                "85 FF"),
            new UiShaderSourcePoolInitializationInstructionEvidence(
                "Skip configuration when the allocator pointer is zero",
                ConfigurationNullBranchRawFileOffset,
                "74 12"),
            new UiShaderSourcePoolInitializationInstructionEvidence(
                "Push the first observed configuration argument",
                ConfigurationCallRawFileOffset - 11,
                "6A 01"),
            new UiShaderSourcePoolInitializationInstructionEvidence(
                "Push the second observed configuration argument",
                ConfigurationCallRawFileOffset - 9,
                "6A 20"),
            new UiShaderSourcePoolInitializationInstructionEvidence(
                "Push the third observed configuration argument",
                ConfigurationCallRawFileOffset - 7,
                "68 10 08 00 00"),
            new UiShaderSourcePoolInitializationInstructionEvidence(
                "Pass the captured allocator pointer in ECX",
                ConfigurationCallRawFileOffset - 2,
                "8B CF"),
            new UiShaderSourcePoolInitializationInstructionEvidence(
                "Call the allocator configuration helper",
                ConfigurationCallRawFileOffset,
                "E8 9F 19 00 00",
                TargetVirtualAddress: AllocatorConfigurationFunctionRawFileOffset
                    + CodeVirtualAddressDeltaFromRawFileOffset),
            new UiShaderSourcePoolInitializationInstructionEvidence(
                "Normalize the null allocator result before the common continuation",
                NullAllocatorNormalizationRawFileOffset,
                "33 FF"),
            new UiShaderSourcePoolInitializationInstructionEvidence(
                "Load the prior singleton-list head from its global",
                PriorSingletonHeadLoadRawFileOffset,
                "A1 A8 B0 E4 01",
                TargetVirtualAddress: SingletonHeadGlobalVirtualAddress),
            new UiShaderSourcePoolInitializationInstructionEvidence(
                "Store the captured allocator pointer at this + 0x10",
                AllocatorPointerStoreRawFileOffset,
                "89 7E 10",
                ThisRelativeOffset: AllocatorPointerOffset),
            new UiShaderSourcePoolInitializationInstructionEvidence(
                "Link the captured prior head at this + 0x0C",
                PriorSingletonHeadStoreRawFileOffset,
                "89 46 0C",
                ThisRelativeOffset: PriorSingletonHeadOffset),
            new UiShaderSourcePoolInitializationInstructionEvidence(
                "Publish this as the new global singleton-list head",
                SingletonHeadPublishRawFileOffset,
                "89 35 A8 B0 E4 01",
                TargetVirtualAddress: SingletonHeadGlobalVirtualAddress),
            new UiShaderSourcePoolInitializationInstructionEvidence(
                "Set the byte flag at this + 0x04, including on the null-allocation continuation",
                InitializedFlagStoreRawFileOffset,
                "C6 46 04 01",
                ThisRelativeOffset: InitializedFlagOffset)
        });

    /// <summary>
    /// Projects the observed continuation after the allocator returns. A nonzero
    /// captured pointer selects the configuration call; either value then reaches
    /// the common captured-head and publication writes if preceding calls return.
    /// </summary>
    public static UiShaderSourcePoolInitializationProjection Project(
        UiShaderSourcePoolInitializationCapture capture)
    {
        var actions = ImmutableArray.CreateBuilder<UiShaderSourcePoolInitializationAction>();

        if (capture.AllocatorReturnedPointer != 0)
        {
            actions.Add(new UiShaderSourcePoolInitializationAction(
                UiShaderSourcePoolInitializationActionKind.CallAllocatorConfiguration,
                ConfigurationCallRawFileOffset,
                ReceiverPointer: capture.AllocatorReturnedPointer,
                FirstArgument: ConfigurationArgument0,
                SecondArgument: ConfigurationArgument1,
                ThirdArgument: ConfigurationArgument2));
        }

        actions.Add(new UiShaderSourcePoolInitializationAction(
            UiShaderSourcePoolInitializationActionKind.LoadPriorSingletonHead,
            PriorSingletonHeadLoadRawFileOffset,
            TargetVirtualAddress: SingletonHeadGlobalVirtualAddress,
            Value: capture.PriorSingletonHead));
        actions.Add(new UiShaderSourcePoolInitializationAction(
            UiShaderSourcePoolInitializationActionKind.StoreAllocatorPointer,
            AllocatorPointerStoreRawFileOffset,
            TargetVirtualAddress: unchecked(capture.ThisPointer + AllocatorPointerOffset),
            Value: capture.AllocatorReturnedPointer));
        actions.Add(new UiShaderSourcePoolInitializationAction(
            UiShaderSourcePoolInitializationActionKind.StorePriorSingletonHead,
            PriorSingletonHeadStoreRawFileOffset,
            TargetVirtualAddress: unchecked(capture.ThisPointer + PriorSingletonHeadOffset),
            Value: capture.PriorSingletonHead));
        actions.Add(new UiShaderSourcePoolInitializationAction(
            UiShaderSourcePoolInitializationActionKind.PublishThisAsSingletonHead,
            SingletonHeadPublishRawFileOffset,
            TargetVirtualAddress: SingletonHeadGlobalVirtualAddress,
            Value: capture.ThisPointer));
        actions.Add(new UiShaderSourcePoolInitializationAction(
            UiShaderSourcePoolInitializationActionKind.SetInitializedFlag,
            InitializedFlagStoreRawFileOffset,
            TargetVirtualAddress: unchecked(capture.ThisPointer + InitializedFlagOffset),
            Value: 1));

        return new UiShaderSourcePoolInitializationProjection(
            capture,
            actions.ToImmutable());
    }
}
