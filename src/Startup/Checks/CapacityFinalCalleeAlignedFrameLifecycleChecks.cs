using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeAlignedFrameLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The size gate and below-threshold frame window compose at their exact boundaries",
            CapacityFinalCalleeAlignedFrameLifecycleProjection.EntryRawFileOffset
                == 0x012C90F6
            && CapacityFinalCalleeAlignedFrameLifecycleProjection.EntryVirtualAddress
                == 0x016C9CF6
            && CapacityFinalCalleeAlignedFrameLifecycleProjection.BelowThresholdRawFileOffset
                == 0x012C910C
            && CapacityFinalCalleeAlignedFrameLifecycleProjection.AtOrAboveThresholdRawFileOffset
                == 0x012C919B
            && CapacityFinalCalleeAlignedFrameLifecycleProjection.OpaqueCallRawFileOffset
                == 0x012C911C
            && CapacityFinalCalleeAlignedFrameLifecycleProjection.UnequalDestinationRawFileOffset
                == 0x012C9122
            && Normalize(CapacityFinalCalleeAlignedFrameLifecycleProjection.CapturedWindowBytes)
                == "83E1F88BC103F9C1E8038945FC83F8200F838F000000"
                    + "8B42088945F08B420C8945EC3BD07506"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeAlignedFrameLifecyclePlan unvalidated =
            CapacityFinalCalleeAlignedFrameLifecycleProjection.Project(Capture(
                entryValidated: false,
                entryEcx: 0xF8,
                entryEdi: 0x100,
                entryEbp: 0x200,
                entryEdx: 0x300,
                freshEdxPlus8: 0x400,
                freshEdxPlusC: 0x500));
        check("Entry validation gates the size and every downstream frame capture",
            unvalidated.Outcome
                == CapacityFinalCalleeAlignedFrameLifecycleOutcome.EntryNotValidated
            && unvalidated.SizePlan.Outcome
                == CapacityFinalCalleeAlternateAlignedSizeOutcome.EntryNotValidated
            && !unvalidated.SizePlan.AndReached
            && !unvalidated.SizePlan.StoreReached
            && !unvalidated.FrameLoadsEntered
            && unvalidated.FrameLoadsPlan is null
            && !unvalidated.DerivedEBPCaptured
            && unvalidated.DerivedEBP is null
            && unvalidated.UnreachedFrameEntryEDXCaptureProvided
            && unvalidated.UnreachedFreshEdxPlus8CaptureProvided
            && unvalidated.UnreachedFreshEdxPlusCCaptureProvided
            && !unvalidated.ExactCleanExitAdmitted
            && unvalidated.StopAtRawFileOffsetExclusive == 0x012C90F6);

        CapacityFinalCalleeAlignedFrameLifecyclePlan missingEntryEcx =
            CapacityFinalCalleeAlignedFrameLifecycleProjection.Project(Capture(
                entryEcx: null,
                entryEdi: 0x100,
                entryEbp: 0x200,
                entryEdx: 0x300,
                freshEdxPlus8: 0x400,
                freshEdxPlusC: 0x500));
        check("Missing size ECX cannot be bypassed with later register captures",
            missingEntryEcx.Outcome
                == CapacityFinalCalleeAlignedFrameLifecycleOutcome.MissingEntryECX
            && !missingEntryEcx.SizePlan.AndReached
            && !missingEntryEcx.SizePlan.AddReached
            && !missingEntryEcx.SizePlan.StoreReached
            && !missingEntryEcx.FrameLoadsEntered
            && missingEntryEcx.FrameLoadsPlan is null
            && missingEntryEcx.UnreachedFrameEntryEDXCaptureProvided
            && missingEntryEcx.UnreachedFreshEdxPlus8CaptureProvided
            && missingEntryEcx.UnreachedFreshEdxPlusCCaptureProvided
            && !missingEntryEcx.ExactCleanExitAdmitted
            && missingEntryEcx.StopAtRawFileOffsetExclusive == 0x012C90F6);

        CapacityFinalCalleeAlignedFrameLifecyclePlan missingEntryEdi =
            CapacityFinalCalleeAlignedFrameLifecycleProjection.Project(Capture(
                entryEcx: 0xF8,
                entryEdi: null,
                entryEbp: 0x200,
                entryEdx: 0x300,
                freshEdxPlus8: 0x400,
                freshEdxPlusC: 0x500));
        check("Missing size EDI preserves only the reached size prefix",
            missingEntryEdi.Outcome
                == CapacityFinalCalleeAlignedFrameLifecycleOutcome.MissingEntryEDI
            && missingEntryEdi.SizePlan.AndReached
            && missingEntryEdi.SizePlan.MoveEAXReached
            && !missingEntryEdi.SizePlan.AddReached
            && !missingEntryEdi.SizePlan.StoreReached
            && !missingEntryEdi.FrameLoadsEntered
            && !missingEntryEdi.DerivedEBPCaptured
            && missingEntryEdi.UnreachedFrameEntryEDXCaptureProvided
            && missingEntryEdi.UnreachedFreshEdxPlus8CaptureProvided
            && missingEntryEdi.UnreachedFreshEdxPlusCCaptureProvided
            && missingEntryEdi.StopAtRawFileOffsetExclusive == 0x012C90FB);

        CapacityFinalCalleeAlignedFrameLifecyclePlan missingSizeEbp =
            CapacityFinalCalleeAlignedFrameLifecycleProjection.Project(Capture(
                entryEcx: 0xF8,
                entryEdi: 0x100,
                entryEbp: null,
                entryEdx: 0x300,
                freshEdxPlus8: 0x400,
                freshEdxPlusC: 0x500));
        check("Missing size EBP blocks its local store and makes frame-derived EBP impossible",
            missingSizeEbp.Outcome
                == CapacityFinalCalleeAlignedFrameLifecycleOutcome
                    .MissingEntryEBPForSizeStore
            && missingSizeEbp.SizePlan.AddReached
            && missingSizeEbp.SizePlan.ShiftReached
            && !missingSizeEbp.SizePlan.StoreReached
            && !missingSizeEbp.SizePlan.CompareReached
            && !missingSizeEbp.FrameLoadsEntered
            && !missingSizeEbp.DerivedEBPCaptured
            && missingSizeEbp.DerivedEBP is null
            && missingSizeEbp.UnreachedFrameEntryEDXCaptureProvided
            && missingSizeEbp.UnreachedFreshEdxPlus8CaptureProvided
            && missingSizeEbp.UnreachedFreshEdxPlusCCaptureProvided
            && missingSizeEbp.StopAtRawFileOffsetExclusive == 0x012C9100);

        CapacityFinalCalleeAlignedFrameLifecyclePlan atThreshold =
            CapacityFinalCalleeAlignedFrameLifecycleProjection.Project(Capture(
                entryEcx: 0x100,
                entryEdi: 0xFFFFFF00,
                entryEbp: 3,
                entryEdx: 0,
                freshEdxPlus8: 0x11111111,
                freshEdxPlusC: 0x22222222));
        check("A clean size of 0x20 stops at 919B and rejects all frame captures",
            atThreshold.Outcome
                == CapacityFinalCalleeAlignedFrameLifecycleOutcome
                    .SizeAtOrAboveThresholdBranchTarget
            && atThreshold.SizePlan.AlignedSize == 0x20
            && atThreshold.SizePlan.Store is
            {
                RawFileOffset: 0x012C9100,
                Address: 0xFFFFFFFF,
                Value: 0x20
            }
            && atThreshold.SizePlan.CompareReached
            && atThreshold.SizePlan.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && atThreshold.SizePlan.JnbTaken == true
            && atThreshold.SizePlan.SelectedDestinationRawFileOffset == 0x012C919B
            && atThreshold.DownstreamFrameCapturesRejected
            && !atThreshold.FrameLoadsEntered
            && atThreshold.FrameLoadsPlan is null
            && !atThreshold.DerivedEBPCaptured
            && atThreshold.DerivedEBP is null
            && atThreshold.UnreachedFrameEntryEDXCaptureProvided
            && atThreshold.UnreachedFreshEdxPlus8CaptureProvided
            && atThreshold.UnreachedFreshEdxPlusCCaptureProvided
            && !atThreshold.ExactCleanExitAdmitted
            && atThreshold.StopAtRawFileOffsetExclusive == 0x012C919B);

        CapacityFinalCalleeAlignedFrameLifecyclePlan cleanAtThreshold =
            CapacityFinalCalleeAlignedFrameLifecycleProjection.Project(Capture(
                entryEcx: 0x100,
                entryEdi: 0xFFFFFF00,
                entryEbp: 3,
                entryEdx: null,
                freshEdxPlus8: null,
                freshEdxPlusC: null));
        check("A clean 0x20 branch is admitted only when all downstream captures are absent",
            cleanAtThreshold.Outcome
                == CapacityFinalCalleeAlignedFrameLifecycleOutcome
                    .SizeAtOrAboveThresholdBranchTarget
            && cleanAtThreshold.SizePlan.Store is
            {
                RawFileOffset: 0x012C9100,
                Address: 0xFFFFFFFF,
                Value: 0x20
            }
            && cleanAtThreshold.SizePlan.ExactCleanAdmission
            && cleanAtThreshold.SizePlan.JnbTaken == true
            && cleanAtThreshold.SizePlan.SelectedDestinationRawFileOffset == 0x012C919B
            && !cleanAtThreshold.DownstreamFrameCapturesRejected
            && !cleanAtThreshold.FrameLoadsEntered
            && cleanAtThreshold.FrameLoadsPlan is null
            && !cleanAtThreshold.UnreachedFrameEntryEDXCaptureProvided
            && !cleanAtThreshold.UnreachedFreshEdxPlus8CaptureProvided
            && !cleanAtThreshold.UnreachedFreshEdxPlusCCaptureProvided
            && cleanAtThreshold.ExactCleanExitAdmitted
            && cleanAtThreshold.StopAtRawFileOffsetExclusive == 0x012C919B);

        CapacityFinalCalleeAlignedFrameLifecyclePlan missingFrameEdx =
            CapacityFinalCalleeAlignedFrameLifecycleProjection.Project(Capture(
                entryEcx: 0xF8,
                entryEdi: 0x100,
                entryEbp: 0x200,
                entryEdx: null,
                freshEdxPlus8: 0xAABBCCDD,
                freshEdxPlusC: 0x12345678));
        check("Below 0x20, missing caller EDX preserves the size store but blocks frame reads",
            missingFrameEdx.Outcome
                == CapacityFinalCalleeAlignedFrameLifecycleOutcome.MissingFrameEntryEDX
            && missingFrameEdx.SizePlan.AlignedSize == 0x1F
            && missingFrameEdx.SizePlan.Store is
            {
                RawFileOffset: 0x012C9100,
                Address: 0x1FC,
                Value: 0x1F
            }
            && missingFrameEdx.SizePlan.SelectedDestinationRawFileOffset == 0x012C910C
            && missingFrameEdx.FrameLoadsEntered
            && missingFrameEdx.FrameLoadsPlan is
            {
                Outcome: CapacityFinalCalleeAlternateFrameLoadsOutcome.MissingEntryEDX,
                FirstReadReached: true,
                EntryEDXCaptured: false,
                FirstWriteReached: false,
                SecondReadReached: false
            }
            && !missingFrameEdx.DerivedEBPCaptured
            && missingFrameEdx.DerivedEBP is null
            && missingFrameEdx.FrameLoadsPlan.UnreachedFreshEdxPlus8CaptureProvided
            && missingFrameEdx.FrameLoadsPlan.UnreachedFreshEdxPlusCCaptureProvided
            && !missingFrameEdx.ExactCleanExitAdmitted
            && missingFrameEdx.StopAtRawFileOffsetExclusive == 0x012C910C);

        CapacityFinalCalleeAlignedFrameLifecyclePlan missingFreshPlus8 =
            CapacityFinalCalleeAlignedFrameLifecycleProjection.Project(Capture(
                entryEcx: 0xF8,
                entryEdi: 0x100,
                entryEbp: 0x200,
                entryEdx: 0x100,
                freshEdxPlus8: null,
                freshEdxPlusC: 0x12345678));
        check("A missing fresh EDX+8 does not fabricate or hand off the unused derived EBP",
            missingFreshPlus8.Outcome
                == CapacityFinalCalleeAlignedFrameLifecycleOutcome.MissingFreshEdxPlus8
            && missingFreshPlus8.SizePlan.Store is
            {
                Address: 0x1FC,
                Value: 0x1F
            }
            && missingFreshPlus8.FrameLoadsPlan is
            {
                Outcome: CapacityFinalCalleeAlternateFrameLoadsOutcome.MissingFreshEdxPlus8,
                FreshEdxPlus8Address: 0x108,
                FreshEdxPlus8Captured: false,
                FirstWriteReached: false,
                SecondReadReached: false,
                UnreachedEntryEBPCaptureProvided: false,
                UnreachedFreshEdxPlusCCaptureProvided: true
            }
            && !missingFreshPlus8.DerivedEBPCaptured
            && missingFreshPlus8.DerivedEBP is null
            && !missingFreshPlus8.ExactCleanExitAdmitted
            && missingFreshPlus8.StopAtRawFileOffsetExclusive == 0x012C910C);

        CapacityFinalCalleeAlignedFrameLifecyclePlan missingFreshPlusC =
            CapacityFinalCalleeAlignedFrameLifecycleProjection.Project(Capture(
                entryEcx: 0xF8,
                entryEdi: 0x100,
                entryEbp: 0x200,
                entryEdx: 0x100,
                freshEdxPlus8: 0xAABBCCDD,
                freshEdxPlusC: null));
        check("Missing fresh EDX+C retains the size store and first frame store",
            missingFreshPlusC.Outcome
                == CapacityFinalCalleeAlignedFrameLifecycleOutcome.MissingFreshEdxPlusC
            && missingFreshPlusC.SizePlan.Store is
            {
                RawFileOffset: 0x012C9100,
                Address: 0x1FC,
                Value: 0x1F
            }
            && missingFreshPlusC.DerivedEBPCaptured
            && missingFreshPlusC.DerivedEBP == 0x200
            && missingFreshPlusC.FrameLoadsPlan is
            {
                Outcome: CapacityFinalCalleeAlternateFrameLoadsOutcome.MissingFreshEdxPlusC,
                FirstWriteReached: true,
                FirstWriteAddress: 0x1F0,
                FirstWriteValue: 0xAABBCCDD,
                SecondReadReached: true,
                FreshEdxPlusCAddress: 0x10C,
                FreshEdxPlusCCaptured: false,
                SecondWriteReached: false,
                CompareReached: false,
                ExactCleanExitAdmitted: false
            }
            && missingFreshPlusC.FrameLoadsPlan.Writes.Length == 1
            && missingFreshPlusC.FrameLoadsPlan.Writes[0]
                == new CapacityFinalCalleeAlternateFrameLoadsWrite(
                    0x012C910F, 0x1F0, 0xAABBCCDD)
            && missingFreshPlusC.StopAtRawFileOffsetExclusive == 0x012C9112);

        CapacityFinalCalleeAlignedFrameLifecyclePlan equal =
            CapacityFinalCalleeAlignedFrameLifecycleProjection.Project(Capture(
                entryEcx: 0xF8,
                entryEdi: 0x100,
                entryEbp: 0x200,
                entryEdx: 0,
                freshEdxPlus8: 0x55,
                freshEdxPlusC: 0));
        check("A below-threshold equal compare retains both stores and stops before opaque CALL 911C",
            equal.Outcome
                == CapacityFinalCalleeAlignedFrameLifecycleOutcome
                        .CompareEqualToOpaqueCallBoundary
                && equal.SizePlan.Store is { Value: 0x1F }
                && equal.SizePlan.Outcome
                == CapacityFinalCalleeAlternateAlignedSizeOutcome.BelowThresholdFallthrough
            && equal.SizePlan.ExactCleanAdmission
            && equal.SizePlan.JnbReached
            && equal.SizePlan.JnbTaken == false
            && equal.SizePlan.SelectedDestinationRawFileOffset == 0x012C910C
            && equal.SizePlan.StopAtRawFileOffsetExclusive == 0x012C910C
            && !equal.SizePlan.UnreachedEntryECXCaptureProvided
            && !equal.SizePlan.UnreachedEntryEDICaptureProvided
            && !equal.SizePlan.UnreachedEntryEBPCaptureProvided
            && equal.DerivedEBPCaptured
            && equal.DerivedEBP == 0x200
            && equal.FrameLoadsPlan is
            {
                Outcome: CapacityFinalCalleeAlternateFrameLoadsOutcome
                    .CompareEqualToOpaqueCallBoundary,
                FirstWriteReached: true,
                SecondWriteReached: true,
                CompareReached: true,
                CompareLeft: 0,
                CompareRight: 0,
                CompareResult: 0,
                CompareFlags: {
                    Carry: false,
                    Parity: true,
                    Auxiliary: false,
                    Zero: true,
                    Sign: false,
                    Overflow: false
                },
                BranchReached: true,
                BranchTaken: false,
                BranchSelectedDestinationRawFileOffset: 0x012C911C,
                OpaqueCallReached: false,
                ExactCleanExitAdmitted: true,
                StopAtRawFileOffsetExclusive: 0x012C911C
            }
            && equal.FrameLoadsPlan.Writes.Length == 2
            && equal.ExactCleanExitAdmitted);

        CapacityFinalCalleeAlignedFrameLifecyclePlan unequal =
            CapacityFinalCalleeAlignedFrameLifecycleProjection.Project(Capture(
                entryEcx: 0xF8,
                entryEdi: 0x100,
                entryEbp: 0x200,
                entryEdx: 0,
                freshEdxPlus8: 0x55,
                freshEdxPlusC: 1));
        check("A below-threshold unequal compare preserves both stores and stops at 9122",
            unequal.Outcome
                == CapacityFinalCalleeAlignedFrameLifecycleOutcome
                    .CompareUnequalToBranchTarget
            && unequal.SizePlan.Store is { Value: 0x1F }
            && unequal.FrameLoadsPlan is
            {
                CompareReached: true,
                CompareLeft: 0,
                CompareRight: 1,
                CompareResult: 0xFFFFFFFF,
                CompareFlags: {
                    Carry: true,
                    Parity: true,
                    Auxiliary: true,
                    Zero: false,
                    Sign: true,
                    Overflow: false
                },
                BranchReached: true,
                BranchTaken: true,
                BranchSelectedDestinationRawFileOffset: 0x012C9122,
                OpaqueCallReached: false,
                ExactCleanExitAdmitted: true,
                StopAtRawFileOffsetExclusive: 0x012C9122
            }
            && unequal.FrameLoadsPlan.Writes.Length == 2
            && unequal.ExactCleanExitAdmitted);

        CapacityFinalCalleeAlignedFrameLifecyclePlan wrappedAliasedReload =
            CapacityFinalCalleeAlignedFrameLifecycleProjection.Project(Capture(
                entryEcx: 0xF8,
                entryEdi: 0x100,
                entryEbp: 0x14,
                entryEdx: 0xFFFFFFF8,
                freshEdxPlus8: 0xA1,
                freshEdxPlusC: 0xB2));
        check("Wrapped aliased frame addresses still use two independently captured fresh values",
            wrappedAliasedReload.SizePlan.Store is
            {
                Address: 0x10,
                Value: 0x1F
            }
            && wrappedAliasedReload.DerivedEBPCaptured
            && wrappedAliasedReload.DerivedEBP == 0x14
            && wrappedAliasedReload.FrameLoadsPlan is
            {
                FirstWriteAddress: 4,
                FirstWriteValue: 0xA1,
                FreshEdxPlusCAddress: 4,
                FreshEdxPlusCCaptured: true,
                FreshEdxPlusC: 0xB2,
                SecondWriteAddress: 0,
                SecondWriteValue: 0xB2,
                CompareLeft: 0xFFFFFFF8,
                CompareRight: 0xB2,
                BranchSelectedDestinationRawFileOffset: 0x012C9122
            }
            && wrappedAliasedReload.FrameLoadsPlan.Writes.Select(write => write.Address)
                .SequenceEqual(new uint[] { 4, 0 })
            && wrappedAliasedReload.FrameLoadsPlan.Writes.Select(write => write.Value)
                .SequenceEqual(new uint[] { 0xA1, 0xB2 })
            && wrappedAliasedReload.FrameLoadsPlan.Reads.Select(read => read.Address)
                .SequenceEqual(new uint?[] { 0, 4 }));
    }

    private static CapacityFinalCalleeAlignedFrameLifecycleCapture Capture(
        bool entryValidated = true,
        uint? entryEcx = 0xF8,
        uint? entryEdi = 0x100,
        uint? entryEbp = 0x200,
        uint? entryEdx = 0x1000,
        uint? freshEdxPlus8 = 0x3000,
        uint? freshEdxPlusC = 0x4000) =>
        new(
            entryValidated,
            entryEcx,
            entryEdi,
            entryEbp,
            entryEdx,
            freshEdxPlus8,
            freshEdxPlusC);

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw)[] {
            (0x012C90F6, "83 E1 F8", null),
            (0x012C90F9, "8B C1", null),
            (0x012C90FB, "03 F9", null),
            (0x012C90FD, "C1 E8 03", null),
            (0x012C9100, "89 45 FC", null),
            (0x012C9103, "83 F8 20", null),
            (0x012C9106, "0F 83 8F 00 00 00", 0x012C919B),
            (0x012C910C, "8B 42 08", null),
            (0x012C910F, "89 45 F0", null),
            (0x012C9112, "8B 42 0C", null),
            (0x012C9115, "89 45 EC", null),
            (0x012C9118, "3B D0", null),
            (0x012C911A, "75 06", 0x012C9122)
        };
        uint nextRawOffset =
            CapacityFinalCalleeAlignedFrameLifecycleProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeAlignedFrameLifecycleInstructionEvidence instruction
            in CapacityFinalCalleeAlignedFrameLifecycleProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeAlternateAlignedSizeProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || instruction.BranchTargetRawFileOffset
                    != expected.First(item => item.Raw == instruction.RawFileOffset)
                        .BranchTargetRaw
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeAlternateAlignedSizeProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeAlignedFrameLifecycleProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeAlternateFrameLoadsProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeAlignedFrameLifecycleProjection.CapturedWindowBytes)
            && CapacityFinalCalleeAlignedFrameLifecycleProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes,
                        instruction.BranchTargetRawFileOffset))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
