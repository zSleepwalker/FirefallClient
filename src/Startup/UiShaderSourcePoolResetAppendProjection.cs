namespace Firefall.Reconstruction.Startup;

public enum UiShaderSourcePoolResetAppendSourceKind
{
    AliasedDescriptorElement,
    ExternalSavedLocalValue
}

public enum UiShaderSourcePoolResetAppendBoundary
{
    GrowthCallBoundary,
    EndAdvanced
}

public readonly record struct UiShaderSourcePoolResetAppendGrowthReturnCapture(
    uint DescriptorEndPointer,
    uint DescriptorStorageEndPointer,
    uint? DescriptorBeginPointer,
    uint? AliasedSourceValue);

public readonly record struct UiShaderSourcePoolResetAppendCapture(
    uint DescriptorAddress,
    uint LocalSourceAddress,
    uint SavedLocalPrimaryPopValue,
    uint DescriptorBeginPointer,
    uint DescriptorEndPointer,
    uint DescriptorStorageEndPointer,
    uint? AliasedSourceValueBeforeGrowth = null,
    UiShaderSourcePoolResetAppendGrowthReturnCapture? GrowthReturn = null);

public readonly record struct UiShaderSourcePoolResetAppendProjection(
    UiShaderSourcePoolResetAppendCapture Capture,
    UiShaderSourcePoolResetAppendSourceKind SourceKind,
    UiShaderSourcePoolResetAppendBoundary Boundary,
    uint? AliasedElementIndex,
    bool GrowthCallReached,
    uint? GrowthCallDescriptorAddress,
    uint? GrowthCallArgument,
    uint? AppendDestinationEndPointer,
    uint? AliasedSourceAddressRead,
    uint? StoredValue,
    bool AppendStoreAttempted,
    uint? AdvancedDescriptorEndPointer);

/// <summary>
/// Projects the captured nonzero-primary-pop append branch only. The local
/// source address and initial descriptor bounds are independent inputs. A full
/// descriptor reaches the native growth call; any later descriptor pointers
/// and aliased value are separate return captures, not simulated growth.
/// </summary>
public static class UiShaderSourcePoolResetAppendProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint ResetFunctionRawFileOffset = 0x12CDC50;
    public const uint AppendBranchStartRawFileOffset = 0x12CDCE6;
    public const uint CapturedBytesStartRawFileOffset = 0x12CDCE3;
    public const uint CapturedBytesEndRawFileOffsetExclusive = 0x12CDD42;
    public const uint AppendEndAdvanceRawFileOffset = 0x12CDD3C;
    public const uint AppendEndStoreRawFileOffset = 0x12CDD3F;

    public const int DescriptorBeginStackOffset = -0x34;
    public const int DescriptorEndStackOffset = -0x30;
    public const int DescriptorStorageEndStackOffset = -0x2C;
    public const int SavedLocalPrimaryPopStackOffset = -0x14;

    public const uint SourceAddressCaptureRawFileOffset = 0x12CDCE3;
    public const uint SavePrimaryPopValueRawFileOffset = 0x12CDCE6;
    public const uint SourceAtOrAboveEndCompareRawFileOffset = 0x12CDCE9;
    public const uint SourceAtOrAboveEndBranchRawFileOffset = 0x12CDCEB;
    public const uint BeginAboveSourceCompareRawFileOffset = 0x12CDCF2;
    public const uint BeginAboveSourceBranchRawFileOffset = 0x12CDCF4;
    public const uint AliasIndexComputeRawFileOffset = 0x12CDCF6;
    public const uint EndVersusStorageEndCompareRawFileOffset = 0x12CDCFD;
    public const uint AliasGrowthCallRawFileOffset = 0x12CDD06;
    public const uint ExternalGrowthCallRawFileOffset = 0x12CDD28;
    public const uint GrowthCallTargetRawFileOffset = 0x006A5500;
    public const uint GrowthCallArgument = 1;
    public const uint AliasSourceLoadRawFileOffset = 0x12CDD18;
    public const uint AliasSourceStoreRawFileOffset = 0x12CDD1B;
    public const uint ExternalSourceStoreRawFileOffset = 0x12CDD37;
    public const uint EndPointerAdvanceRawFileOffset = 0x12CDD3C;
    public const uint EndPointerStoreRawFileOffset = 0x12CDD3F;

    public const string CapturedAppendBytes =
        "8D 45 EC 89 7D EC 3B C6 73 32 8B C8 8B 45 CC 3B " +
        "C1 77 29 8B F9 2B F8 C1 FF 02 3B F3 75 13 6A 01 " +
        "8D 4D CC E8 F5 77 3D FF 8B 5D D4 8B 75 D0 8B 45 " +
        "CC 85 F6 74 21 8B 04 B8 89 06 EB 1A 3B F3 75 10 " +
        "6A 01 8D 4D CC E8 D3 77 3D FF 8B 5D D4 8B 75 D0 " +
        "85 F6 74 02 89 3E 8B 4D E4 83 C6 04 89 75 D0";

    public static UiShaderSourcePoolResetAppendProjection Project(
        UiShaderSourcePoolResetAppendCapture capture)
    {
        if (capture.SavedLocalPrimaryPopValue == 0)
        {
            throw new ArgumentException(
                "The bounded append branch requires the captured nonzero primary pop value.",
                nameof(capture));
        }

        bool aliasesDescriptor =
            capture.LocalSourceAddress < capture.DescriptorEndPointer
            && capture.DescriptorBeginPointer <= capture.LocalSourceAddress;

        UiShaderSourcePoolResetAppendSourceKind sourceKind = aliasesDescriptor
            ? UiShaderSourcePoolResetAppendSourceKind.AliasedDescriptorElement
            : UiShaderSourcePoolResetAppendSourceKind.ExternalSavedLocalValue;

        uint? aliasIndex = aliasesDescriptor
            ? GetAliasedElementIndex(
                capture.LocalSourceAddress,
                capture.DescriptorBeginPointer)
            : null;

        bool growthCallReached =
            capture.DescriptorEndPointer == capture.DescriptorStorageEndPointer;
        bool aliasedSourceReadBeforeGrowthReached =
            aliasesDescriptor
            && !growthCallReached
            && capture.DescriptorEndPointer != 0;

        if (aliasedSourceReadBeforeGrowthReached
            && !capture.AliasedSourceValueBeforeGrowth.HasValue)
        {
            throw new ArgumentException(
                "The nonfull alias path requires its captured source value before growth.",
                nameof(capture));
        }

        if (!aliasedSourceReadBeforeGrowthReached
            && capture.AliasedSourceValueBeforeGrowth.HasValue)
        {
            throw new ArgumentException(
                "The pre-growth aliased source value is unreachable on this path.",
                nameof(capture));
        }

        if (!growthCallReached && capture.GrowthReturn.HasValue)
        {
            throw new ArgumentException(
                "Post-growth captures are unreachable when the initial descriptor is not full.",
                nameof(capture));
        }

        if (!growthCallReached)
        {
            return ProjectAppendContinuation(
                capture,
                sourceKind,
                aliasIndex,
                growthCallReached: false,
                endingPointer: capture.DescriptorEndPointer,
                beginPointer: capture.DescriptorBeginPointer,
                growthAliasedValue: null);
        }

        if (!capture.GrowthReturn.HasValue)
        {
            return new UiShaderSourcePoolResetAppendProjection(
                capture,
                sourceKind,
                UiShaderSourcePoolResetAppendBoundary.GrowthCallBoundary,
                aliasIndex,
                GrowthCallReached: true,
                capture.DescriptorAddress,
                GrowthCallArgument,
                null,
                null,
                null,
                AppendStoreAttempted: false,
                null);
        }

        UiShaderSourcePoolResetAppendGrowthReturnCapture growth =
            capture.GrowthReturn.Value;
        if (aliasesDescriptor)
        {
            if (!growth.DescriptorBeginPointer.HasValue)
            {
                throw new ArgumentException(
                    "The alias path reloads the descriptor begin pointer after the growth call.",
                    nameof(capture));
            }

            if (growth.DescriptorEndPointer == 0
                ? growth.AliasedSourceValue.HasValue
                : !growth.AliasedSourceValue.HasValue)
            {
                throw new ArgumentException(
                    "The post-growth aliased source value must match the end-pointer null gate.",
                    nameof(capture));
            }
        }
        else if (growth.DescriptorBeginPointer.HasValue
            || growth.AliasedSourceValue.HasValue)
        {
            throw new ArgumentException(
                "The external path does not reload the begin pointer or read an aliased source.",
                nameof(capture));
        }

        return ProjectAppendContinuation(
            capture,
            sourceKind,
            aliasIndex,
            growthCallReached: true,
            endingPointer: growth.DescriptorEndPointer,
            beginPointer: growth.DescriptorBeginPointer,
            growthAliasedValue: growth.AliasedSourceValue);
    }

    private static UiShaderSourcePoolResetAppendProjection ProjectAppendContinuation(
        UiShaderSourcePoolResetAppendCapture capture,
        UiShaderSourcePoolResetAppendSourceKind sourceKind,
        uint? aliasIndex,
        bool growthCallReached,
        uint endingPointer,
        uint? beginPointer,
        uint? growthAliasedValue)
    {
        uint? aliasedSourceAddress = null;
        uint? storedValue = null;
        bool storeAttempted = endingPointer != 0;

        if (storeAttempted)
        {
            if (sourceKind == UiShaderSourcePoolResetAppendSourceKind.AliasedDescriptorElement)
            {
                uint selectedBegin = beginPointer
                    ?? throw new ArgumentException(
                        "The aliased source read requires its independently captured begin pointer.",
                        nameof(capture));
                aliasedSourceAddress = unchecked(
                    selectedBegin + unchecked(aliasIndex!.Value * 4));
                storedValue = growthCallReached
                    ? growthAliasedValue
                    : capture.AliasedSourceValueBeforeGrowth;

                if (!storedValue.HasValue)
                {
                    throw new ArgumentException(
                        "The aliased source value must be captured when the end pointer is nonzero.",
                        nameof(capture));
                }
            }
            else
            {
                storedValue = capture.SavedLocalPrimaryPopValue;
            }
        }
        return new UiShaderSourcePoolResetAppendProjection(
            capture,
            sourceKind,
            UiShaderSourcePoolResetAppendBoundary.EndAdvanced,
            aliasIndex,
            growthCallReached,
            growthCallReached ? capture.DescriptorAddress : null,
            growthCallReached ? GrowthCallArgument : null,
            endingPointer,
            aliasedSourceAddress,
            storedValue,
            storeAttempted,
            unchecked(endingPointer + 4));
    }

    private static uint GetAliasedElementIndex(uint sourceAddress, uint beginPointer)
    {
        uint byteOffset = unchecked(sourceAddress - beginPointer);
        return unchecked((uint)(unchecked((int)byteOffset) >> 2));
    }
}
