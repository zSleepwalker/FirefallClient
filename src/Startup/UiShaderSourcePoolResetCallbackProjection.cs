using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public enum UiShaderSourcePoolResetCallbackOutcome
{
    IndexOutsideBound,
    ExistingCandidateMatched,
    CallbackCallReached
}

public readonly record struct UiShaderSourcePoolResetCallbackProjection(
    uint Index,
    uint LoopBound,
    uint Field1CValue,
    uint Field14Value,
    uint AlignedBase,
    uint CallbackPointer,
    ImmutableArray<uint> CapturedScanValues,
    uint? Candidate,
    int? FirstMatchingScanValueIndex,
    UiShaderSourcePoolResetCallbackOutcome Outcome,
    uint? BranchTargetRawFileOffset,
    bool CallbackCallReached,
    uint? CallbackArgumentPointer,
    uint? CallbackTargetPointer);

/// <summary>
/// Projects the bounded candidate calculation, equality scan over already
/// captured DWORDs, and callback-call branch. Index and loop bound represent
/// this captured iteration-entry predicate: native code zeroes EDI at 0x12CDDA8
/// and checks the bound at 0x12CDDAA. The distinct post-iteration check at
/// 0x12CDDE1 is not modeled as a later traversal. This does not claim another
/// bound read per candidate. For an equality, it stops at the branch to the
/// later path without asserting whether that path eventually reaches another
/// callback. It also does not model target memory, callback behavior, a
/// receiver, or a callback return value.
/// </summary>
public static class UiShaderSourcePoolResetCallbackProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint FunctionRawFileOffset = 0x12CDC50;
    public const uint IterationStartRawFileOffset = 0x12CDDB2;
    public const uint IterationEndRawFileOffsetExclusive = 0x12CDDE6;
    public const uint InitialIndexZeroRawFileOffset = 0x12CDDA8;
    public const uint BoundCompareRawFileOffset = 0x12CDDAA;
    public const uint BoundBranchRawFileOffset = 0x12CDDAD;
    public const uint BoundBranchTargetRawFileOffset = 0x12CDDE6;
    public const uint PaddingLoadRawFileOffset = 0x12CDDB2;
    public const uint SizeAddRawFileOffset = 0x12CDDB7;
    public const uint IndexMultiplyRawFileOffset = 0x12CDDBA;
    public const uint AlignedBaseAddRawFileOffset = 0x12CDDC1;
    public const uint ScanRangeEmptyCompareRawFileOffset = 0x12CDDC4;
    public const uint MatchCandidateCompareRawFileOffset = 0x12CDDC8;
    public const uint MatchBranchRawFileOffset = 0x12CDDCA;
    public const uint MatchBranchTargetRawFileOffset = 0x12CDE44;
    public const uint CallbackArgumentPushRawFileOffset = 0x12CDDD3;
    public const uint CallbackCallRawFileOffset = 0x12CDDD4;
    public const uint IterationTailBoundCompareRawFileOffset = 0x12CDDE1;
    public const uint IterationTailBoundBranchRawFileOffset = 0x12CDDE4;
    public const uint CallbackCallArgumentCount = 1;
    public const string CallbackCallingConvention = "cdecl";
    public const bool CallbackSuppliesThis = false;
    public const bool CallbackReturnValueUsed = false;

    public const int Field14Offset = 0x14;
    public const int Field1COffset = 0x1C;
    public const int LoopBoundOffset = 0x20;
    public const int AlignedBaseStackOffset = -0x1C;
    public const int ScanBeginStackOffset = -0x34;

    public const string CapturedDispatchBytes =
        "33 FF 39 7B 20 76 37 8B 55 CC 8B 4B 1C 8B C2 03 " +
        "4B 14 0F AF CF C6 45 F3 00 03 4D E4 3B D6 74 0B " +
        "39 08 74 78 83 C0 04 3B C6 75 F5 51 FF 55 08 8B " +
        "55 CC 83 C4 04 FF 45 E8 47 3B 7B 20 72 CC";

    /// <summary>
    /// Copies the supplied scan values before projecting one candidate. Index
    /// and loop bound are captured iteration-entry values; the native zero-index
    /// setup and later tail check are outside this projection's traversal.
    /// The captured list represents the DWORD range already read by the caller;
    /// this method performs no pointer arithmetic or memory reads.
    /// </summary>
    public static UiShaderSourcePoolResetCallbackProjection Project(
        uint index,
        uint loopBound,
        uint field1CValue,
        uint field14Value,
        uint alignedBase,
        uint callbackPointer,
        IReadOnlyList<uint> capturedScanValues)
    {
        ArgumentNullException.ThrowIfNull(capturedScanValues);
        ImmutableArray<uint> scanValues = CopyScanValues(capturedScanValues);

        if (index >= loopBound)
        {
            return new UiShaderSourcePoolResetCallbackProjection(
                index,
                loopBound,
                field1CValue,
                field14Value,
                alignedBase,
                callbackPointer,
                scanValues,
                null,
                null,
                UiShaderSourcePoolResetCallbackOutcome.IndexOutsideBound,
                BoundBranchTargetRawFileOffset,
                false,
                null,
                null);
        }

        uint stride = unchecked(field1CValue + field14Value);
        uint candidate = unchecked(alignedBase + unchecked(index * stride));

        for (int scanIndex = 0; scanIndex < scanValues.Length; scanIndex++)
        {
            if (scanValues[scanIndex] == candidate)
            {
                return new UiShaderSourcePoolResetCallbackProjection(
                    index,
                    loopBound,
                    field1CValue,
                    field14Value,
                    alignedBase,
                    callbackPointer,
                    scanValues,
                    candidate,
                    scanIndex,
                    UiShaderSourcePoolResetCallbackOutcome.ExistingCandidateMatched,
                    MatchBranchTargetRawFileOffset,
                    false,
                    null,
                    null);
            }
        }

        return new UiShaderSourcePoolResetCallbackProjection(
            index,
            loopBound,
            field1CValue,
            field14Value,
            alignedBase,
            callbackPointer,
            scanValues,
            candidate,
            null,
            UiShaderSourcePoolResetCallbackOutcome.CallbackCallReached,
            null,
            true,
            candidate,
            callbackPointer);
    }

    private static ImmutableArray<uint> CopyScanValues(
        IReadOnlyList<uint> capturedScanValues)
    {
        var copy = ImmutableArray.CreateBuilder<uint>(capturedScanValues.Count);
        for (int index = 0; index < capturedScanValues.Count; index++)
        {
            copy.Add(capturedScanValues[index]);
        }

        return copy.MoveToImmutable();
    }
}
