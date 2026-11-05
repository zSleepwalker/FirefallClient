using Firefall.Reconstruction.Startup;

internal static class UiOldPairCallbackGateProjectionChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("UI old-pair callback gate records its captured fields, division, ordering, and callback targets",
            UiOldPairCallbackGateProjectionRules.OldPairLowWordOffset == 0x988
            && UiOldPairCallbackGateProjectionRules.OldPairHighWordOffset == 0x98C
            && UiOldPairCallbackGateProjectionRules.Field20Offset == 0x20
            && UiOldPairCallbackGateProjectionRules.Divisor == 1000
            && UiOldPairCallbackGateProjectionRules.MinimumElapsed == 5000
            && UiOldPairCallbackGateProjectionRules.CallerAfterReadyCheckFileOffset == 0xC02778
            && UiOldPairCallbackGateProjectionRules.CallbackGateFileOffset == 0xC06670
            && UiOldPairCallbackGateProjectionRules.DivisionCallFileOffset == 0xC06686
            && UiOldPairCallbackGateProjectionRules.UnsignedDivideHelperFileOffset == 0x159DD10
            && UiOldPairCallbackGateProjectionRules.FirstStoreAfterCallbackGateFileOffset == 0xC0281C
            && UiOldPairCallbackGateProjectionRules.SecondStoreAfterCallbackGateFileOffset == 0xC02825
            && UiOldPairCallbackGateProjectionRules.Field20StoreBeforeCallbackWalksFileOffset == 0xC0669C
            && UiOldPairCallbackGateProjectionRules.FirstCallbackListOffset == 0x6B0
            && UiOldPairCallbackGateProjectionRules.SecondCallbackListOffset == 0x6D8
            && UiOldPairCallbackGateProjectionRules.CallbackListSlotOffset == 0x14
            && UiOldPairCallbackGateProjectionRules.PayloadCallbackTreeOffset == 0x5E4
            && UiOldPairCallbackGateProjectionRules.PayloadCallbackMethodFileOffset == 0xC376A0);

        var primaryTraversals = UiOldPairCallbackGateProjectionRules.PrimaryCallbackTraversals;
        check("UI old-pair primary traversals walk both trees in order and call the node receiver virtual slot without arguments",
            primaryTraversals.Count == 2
            && primaryTraversals[0] == new UiOldPairCallbackTraversalEvidence(
                0x6B0, 0, true, 0xC066B0, 0xC066B5, 0x14, 0x14, 0,
                true, 0x0D, true, 0x08, 0x00, 0x04, true, false)
            && primaryTraversals[1] == new UiOldPairCallbackTraversalEvidence(
                0x6D8, 1, true, 0xC06710, 0xC06715, 0x14, 0x14, 0,
                true, 0x0D, true, 0x08, 0x00, 0x04, true, false)
            && UiOldPairCallbackGateProjectionRules.Field20StoreBeforeCallbackWalksFileOffset
                < primaryTraversals[0].DispatchCodeStartFileOffset);

        var directTraversals = UiOldPairCallbackGateProjectionRules.DirectCallbackTraversals;
        check("UI old-pair payload traversal follows the two virtual callback trees and distinguishes its direct method target from its callsite",
            directTraversals.Count == 1
            && directTraversals[0] == new UiOldPairDirectCallbackTraversalEvidence(
                0x5E4, 2, 0xC06770, 0xC06773, 0x14, 0xC376A0));

        var nestedTraversals = UiOldPairCallbackGateProjectionRules.NestedCallbackTraversals;
        check("UI old-pair direct payload callback traverses two node receivers then a container entry through slot 0x14",
            nestedTraversals.Count == 3
            && nestedTraversals[0] == new UiOldPairNestedCallbackTraversalEvidence(
                UiOldPairNestedCallbackContainerKind.Tree, 0x0E8, 0x14, 0x14)
            && nestedTraversals[1] == new UiOldPairNestedCallbackTraversalEvidence(
                UiOldPairNestedCallbackContainerKind.Tree, 0x118, 0x14, 0x14)
            && nestedTraversals[2] == new UiOldPairNestedCallbackTraversalEvidence(
                UiOldPairNestedCallbackContainerKind.Container, 0x120, 0x08, 0x14));

        var belowThreshold = UiOldPairCallbackGateProjectionRules.Project(
            new UiOldPairCallbackGateState(4_999_000, 0, 0));
        var atThreshold = UiOldPairCallbackGateProjectionRules.Project(
            new UiOldPairCallbackGateState(5_000_000, 0, 0));
        var aboveThreshold = UiOldPairCallbackGateProjectionRules.Project(
            new UiOldPairCallbackGateState(5_001_000, 0, 0));
        check("UI old-pair callback gate skips at 4999, proceeds at 5000 and 5001, and stores only on proceed",
            belowThreshold == new UiOldPairCallbackGateProjection(4_999, 4_999, false, 0)
            && atThreshold == new UiOldPairCallbackGateProjection(5_000, 5_000, true, 5_000)
            && aboveThreshold == new UiOldPairCallbackGateProjection(5_001, 5_001, true, 5_001));

        var flooredDivision = UiOldPairCallbackGateProjectionRules.Project(
            new UiOldPairCallbackGateState(5_000_999, 0, 0));
        check("UI old-pair callback gate uses unsigned division with quotient rounded down",
            flooredDivision == new UiOldPairCallbackGateProjection(5_000, 5_000, true, 5_000));

        var highWord = UiOldPairCallbackGateProjectionRules.Project(
            new UiOldPairCallbackGateState(0, 1, 0));
        check("UI old-pair callback gate includes the high DWORD in the unsigned QWORD division",
            highWord == new UiOldPairCallbackGateProjection(4_294_967, 4_294_967, true, 4_294_967));

        ulong quotientBeyondUint = (ulong)uint.MaxValue + 2;
        ulong pairWithLargeQuotient = quotientBeyondUint * 1000 + 999;
        var lowQuotientWrap = UiOldPairCallbackGateProjectionRules.Project(
            new UiOldPairCallbackGateState(
                unchecked((uint)pairWithLargeQuotient),
                unchecked((uint)(pairWithLargeQuotient >> 32)),
                0));
        check("UI old-pair callback gate truncates the unsigned quotient to its low DWORD",
            lowQuotientWrap == new UiOldPairCallbackGateProjection(1, 1, false, 0));

        var subtractionUnderflow = UiOldPairCallbackGateProjectionRules.Project(
            new UiOldPairCallbackGateState(3_000, 0, uint.MaxValue - 1));
        var underflowPassesUnsignedGate = UiOldPairCallbackGateProjectionRules.Project(
            new UiOldPairCallbackGateState(3_000, 0, 5_001));
        check("UI old-pair callback gate computes elapsed with uint wraparound and compares it unsigned",
            subtractionUnderflow == new UiOldPairCallbackGateProjection(3, 5, false, uint.MaxValue - 1)
            && underflowPassesUnsignedGate
                == new UiOldPairCallbackGateProjection(3, unchecked(3u - 5_001u), true, 3));
    }
}
