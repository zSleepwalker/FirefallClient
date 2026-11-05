namespace Firefall.Reconstruction.Startup;

public enum UiPayloadProducerPreworkDecision
{
    RejectedByByte300,
    RejectedByThreshold,
    PassedPreworkGates
}

public enum UiPayloadProducerAppendDecision
{
    NotReached,
    RejectedByByte72C,
    RejectedByNullDescriptor,
    AppendGatesPassed
}

public readonly record struct UiPayloadProducerGateInput(
    byte CapturedByte300,
    float CapturedField2EC,
    float CapturedGlobalThreshold,
    byte CapturedObjectByte72C,
    bool CapturedDescriptorPointerPresent);

public readonly record struct UiPayloadProducerGateProjection(
    UiPayloadProducerPreworkDecision Prework,
    UiPayloadProducerAppendDecision Append);

/// <summary>
/// Projects the observed early-return and append-admission tests from captured values.
/// It does not execute the derived-state helper, callbacks, or descriptor append.
/// </summary>
public static class UiPayloadProducerGateProjectionRules
{
    public const uint ProducerMethodFileOffset = 0xC95400;
    public const uint ProducerMethodVirtualAddress = 0x1096000;
    public const uint DerivedStateMethodFileOffset = 0xCCA190;
    public const uint FirstVirtualCallSiteFileOffset = 0xC9544D;
    public const int FirstVirtualCallVtableOffset = 0x34;
    public const uint ZeroByteReturnFileOffset = 0xC95456;
    public const uint ThresholdCompareBeginFileOffset = 0xC9545E;
    public const uint ThresholdCompareEndFileOffset = 0xC9546D;
    public const int DerivedOutputFirstFloatOffset = 0x2DC;
    public const int DerivedOutputFloatCount = 9;
    public const int CapturedByte300Offset = 0x300;
    public const int ComparedField2ECOffset = 0x2EC;
    public const int ObjectByte72COffset = 0x72C;
    public const int UiManagerDescriptorPointerOffset = 0xA08;
    public const int SkippedOnPreworkRejectVirtualCall20Offset = 0x20;
    public const int SkippedOnPreworkRejectVirtualCall28Offset = 0x28;
    public const int SkippedOnPreworkRejectConditionalVirtualCall74Offset = 0x74;

    public static UiPayloadProducerGateProjection Project(UiPayloadProducerGateInput input)
    {
        if (input.CapturedByte300 == 0)
            return new(
                UiPayloadProducerPreworkDecision.RejectedByByte300,
                UiPayloadProducerAppendDecision.NotReached);

        if (input.CapturedGlobalThreshold > input.CapturedField2EC)
            return new(
                UiPayloadProducerPreworkDecision.RejectedByThreshold,
                UiPayloadProducerAppendDecision.NotReached);

        UiPayloadProducerAppendDecision appendDecision =
            input.CapturedObjectByte72C == 0
                ? UiPayloadProducerAppendDecision.RejectedByByte72C
                : input.CapturedDescriptorPointerPresent
                    ? UiPayloadProducerAppendDecision.AppendGatesPassed
                    : UiPayloadProducerAppendDecision.RejectedByNullDescriptor;

        return new(UiPayloadProducerPreworkDecision.PassedPreworkGates, appendDecision);
    }
}
