using Firefall.Reconstruction.Startup;

internal static class FrameHandoffEmptySlotCallChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned PE range, raw/VA mapping, exact bytes, and per-instruction evidence agree",
            FrameHandoffEmptySlotCallProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && FrameHandoffEmptySlotCallProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && FrameHandoffEmptySlotCallProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && FrameHandoffEmptySlotCallProjection.EntryRawFileOffset == 0x00BE64F2
            && FrameHandoffEmptySlotCallProjection.RawFileOffsetToVirtualAddress(0x00BE64F2)
                == 0x00FE70F2
            && FrameHandoffEmptySlotCallProjection.DirectCallRawFileOffset == 0x00BE64F4
            && FrameHandoffEmptySlotCallProjection.DirectCallNormalReturnRawFileOffset
                == 0x00BE64F9
            && FrameHandoffEmptySlotCallProjection.ZeroRouteIndirectCallRawFileOffset
                == 0x00BE6502
            && FrameHandoffEmptySlotCallProjection.NormalBoundaryRawFileOffset == 0x00BE6508
            && FrameHandoffEmptySlotCallProjection.RawFileOffsetToVirtualAddress(0x00BE6508)
                == 0x00FE7108
            && FrameHandoffEmptySlotCallProjection.DirectCallTargetRawFileOffset == 0x012B7FB0
            && FrameHandoffEmptySlotCallProjection.DirectCallTargetVirtualAddress == 0x016B8BB0
            && FrameHandoffEmptySlotCallProjection.IndirectCallCellVirtualAddress == 0x01AC4424
            && FrameHandoffEmptySlotCallProjection.CapturedWindowBytes
                == "6A10E8B71A6D008BC883C40485C97506FF152444AC01"
            && FrameHandoffEmptySlotCallProjection.CapturedWindowBytes.Length == 0x2C
            && EvidenceBytesMatchCapturedWindow()
            && InstructionEvidenceMatches());

        check("The call displacement and ECX branch resolve to their expected destinations",
            FrameHandoffEmptySlotCallProjection.RelativeCallTarget(
                FrameHandoffEmptySlotCallProjection.DirectCallRawFileOffset,
                0x006D1AB7)
                == FrameHandoffEmptySlotCallProjection.VirtualAddressToRawFileOffset(
                    FrameHandoffEmptySlotCallProjection.DirectCallTargetVirtualAddress)
            && FrameHandoffEmptySlotCallProjection.RelativeCallTarget(
                FrameHandoffEmptySlotCallProjection.DirectCallRawFileOffset,
                0x006D1AB7)
                == FrameHandoffEmptySlotCallProjection.DirectCallTargetRawFileOffset
            && FrameHandoffEmptySlotCallProjection.ShortBranchTarget(
                FrameHandoffEmptySlotCallProjection.ZeroRouteBranchRawFileOffset,
                0x06) == FrameHandoffEmptySlotCallProjection.NormalBoundaryRawFileOffset
            && FrameHandoffEmptySlotCallProjection.InstructionsInOrder.SequenceEqual(
                new[]
                {
                    FrameHandoffEmptySlotCallInstruction.PushTen,
                    FrameHandoffEmptySlotCallInstruction.DirectCall,
                    FrameHandoffEmptySlotCallInstruction.CopyEaxToEcx,
                    FrameHandoffEmptySlotCallInstruction.CleanupFourStackBytes,
                    FrameHandoffEmptySlotCallInstruction.TestEcx,
                    FrameHandoffEmptySlotCallInstruction.BranchIfEcxNonzero,
                    FrameHandoffEmptySlotCallInstruction.ZeroRouteIndirectCall
                }));

        FrameHandoffEmptySlotCallPlan unvalidated =
            FrameHandoffEmptySlotCallProjection.Project(
                new(false, true, 0, true, 0x12345678));
        check("An unvalidated entry stops before push or either call and marks every later capture unreached",
            unvalidated.Outcome == FrameHandoffEmptySlotCallOutcome.EntryNotValidated
            && !unvalidated.PushTenReached
            && !unvalidated.DirectCallReached
            && !unvalidated.EaxCopiedToEcx
            && !unvalidated.StackCleanupReached
            && !unvalidated.ZeroRouteIndirectCallReached
            && unvalidated.UnreachedDirectCallReturnCaptureProvided
            && unvalidated.UnreachedDirectCallReturnEaxCaptureProvided
            && unvalidated.UnreachedZeroRouteIndirectCallReturnCaptureProvided
            && unvalidated.UnreachedZeroRoutePostCallEcxCaptureProvided
            && unvalidated.InstructionsExecuted.IsEmpty
            && unvalidated.StopAtRawFileOffsetExclusive
                == FrameHandoffEmptySlotCallProjection.EntryRawFileOffset);

        FrameHandoffEmptySlotCallPlan missingAll =
            FrameHandoffEmptySlotCallProjection.Project(new(true, null, null, null, null));
        FrameHandoffEmptySlotCallPlan directDidNotReturn =
            FrameHandoffEmptySlotCallProjection.Project(
                new(true, false, 0xAABBCCDD, true, 0x12345678));
        check("Missing or nonreturning direct-call evidence stops at its callsite and never performs cleanup",
            missingAll.Outcome
                == FrameHandoffEmptySlotCallOutcome.MissingDirectCallReturnCapture
            && missingAll.PushTenReached
            && missingAll.StackBytesPushed == 4
            && missingAll.ExplicitPushValue == 0x10
            && missingAll.DirectCallReached
            && missingAll.DirectCallReturnedNormally is null
            && !missingAll.EaxCopiedToEcx
            && !missingAll.StackCleanupReached
            && missingAll.StopAtRawFileOffsetExclusive
                == FrameHandoffEmptySlotCallProjection.DirectCallRawFileOffset
            && directDidNotReturn.Outcome
                == FrameHandoffEmptySlotCallOutcome.DirectCallDidNotReturnNormally
            && directDidNotReturn.DirectCallReturnedNormally == false
            && directDidNotReturn.UnreachedDirectCallReturnEaxCaptureProvided
            && directDidNotReturn.UnreachedZeroRouteIndirectCallReturnCaptureProvided
            && directDidNotReturn.UnreachedZeroRoutePostCallEcxCaptureProvided
            && directDidNotReturn.CapturedDirectCallReturnEax is null
            && !directDidNotReturn.StackCleanupReached
            && !directDidNotReturn.ZeroRouteIndirectCallReached
            && directDidNotReturn.StopAtRawFileOffsetExclusive
                == FrameHandoffEmptySlotCallProjection.DirectCallRawFileOffset);

        FrameHandoffEmptySlotCallPlan missingEax =
            FrameHandoffEmptySlotCallProjection.Project(new(true, true, null, null, null));
        FrameHandoffEmptySlotCallPlan directNonzero =
            FrameHandoffEmptySlotCallProjection.Project(
                new(true, true, 0xAABBCCDD, true, 0x12345678));
        check("EAX is copied intact to ECX, four pushed bytes are cleaned, and nonzero ECX skips the indirect call",
            missingEax.Outcome
                == FrameHandoffEmptySlotCallOutcome.MissingDirectCallReturnEaxCapture
            && missingEax.DirectCallReturnedNormally == true
                && !missingEax.EaxCopiedToEcx
            && missingEax.EcxAfterDirectCall is null
                && !missingEax.StackCleanupReached
                && missingEax.StackCleanupBytes is null
                && !missingEax.EcxTestReached
            && missingEax.JnzTaken is null
            && !missingEax.UnreachedZeroRouteIndirectCallReturnCaptureProvided
            && !missingEax.UnreachedZeroRoutePostCallEcxCaptureProvided
            && missingEax.StopAtRawFileOffsetExclusive
                    == FrameHandoffEmptySlotCallProjection.EaxToEcxRawFileOffset
                && missingEax.InstructionsExecuted.SequenceEqual(
                    FrameHandoffEmptySlotCallProjection.InstructionsInOrder.Take(2))
            && directNonzero.Outcome
                == FrameHandoffEmptySlotCallOutcome.DirectNonzeroRouteReturnedNormally
            && directNonzero.CapturedDirectCallReturnEax == 0xAABBCCDD
            && directNonzero.EaxCopiedToEcx
            && directNonzero.EcxAfterDirectCall == 0xAABBCCDD
            && directNonzero.StackCleanupReached
            && directNonzero.StackCleanupBytes == 4
            && directNonzero.EcxTestReached
            && directNonzero.JnzTaken == true
            && !directNonzero.ZeroRouteIndirectCallReached
            && directNonzero.UnreachedZeroRouteIndirectCallReturnCaptureProvided
            && directNonzero.UnreachedZeroRoutePostCallEcxCaptureProvided
            && directNonzero.NormalBoundaryReached
            && directNonzero.StopAtRawFileOffsetExclusive
                == FrameHandoffEmptySlotCallProjection.NormalBoundaryRawFileOffset);

        FrameHandoffEmptySlotCallPlan missingIndirectReturn =
            FrameHandoffEmptySlotCallProjection.Project(
                new(true, true, 0, null, 0xDEADBEEF));
        FrameHandoffEmptySlotCallPlan indirectDidNotReturn =
            FrameHandoffEmptySlotCallProjection.Project(
                new(true, true, 0, false, 0xDEADBEEF));
        check("The zero route requires independent indirect-call return evidence and stops at that callsite otherwise",
            missingIndirectReturn.Outcome
                == FrameHandoffEmptySlotCallOutcome.MissingZeroRouteIndirectCallReturnCapture
            && missingIndirectReturn.EcxAfterDirectCall == 0
            && missingIndirectReturn.JnzTaken == false
            && missingIndirectReturn.ZeroRouteIndirectCallReached
            && missingIndirectReturn.ZeroRouteIndirectCallReturnedNormally is null
            && missingIndirectReturn.UnreachedZeroRoutePostCallEcxCaptureProvided
            && missingIndirectReturn.StopAtRawFileOffsetExclusive
                == FrameHandoffEmptySlotCallProjection.ZeroRouteIndirectCallRawFileOffset
            && indirectDidNotReturn.Outcome
                == FrameHandoffEmptySlotCallOutcome.ZeroRouteIndirectCallDidNotReturnNormally
            && indirectDidNotReturn.ZeroRouteIndirectCallReturnedNormally == false
            && indirectDidNotReturn.UnreachedZeroRoutePostCallEcxCaptureProvided
            && !indirectDidNotReturn.NormalBoundaryReached
            && indirectDidNotReturn.StopAtRawFileOffsetExclusive
                == FrameHandoffEmptySlotCallProjection.ZeroRouteIndirectCallRawFileOffset);

        FrameHandoffEmptySlotCallPlan missingPostCallEcx =
            FrameHandoffEmptySlotCallProjection.Project(new(true, true, 0, true, null));
        FrameHandoffEmptySlotCallPlan zeroRoute =
            FrameHandoffEmptySlotCallProjection.Project(
                new(true, true, 0, true, 0xDEADBEEF));
        check("Zero-route success requires post-call ECX capture without assuming its value is zero",
            missingPostCallEcx.Outcome
                == FrameHandoffEmptySlotCallOutcome.MissingZeroRoutePostCallEcxCapture
            && missingPostCallEcx.ZeroRouteIndirectCallReturnedNormally == true
            && !missingPostCallEcx.ZeroRoutePostCallEcxCaptured
            && missingPostCallEcx.NormalBoundaryReached
            && missingPostCallEcx.StopAtRawFileOffsetExclusive
                == FrameHandoffEmptySlotCallProjection.NormalBoundaryRawFileOffset
            && zeroRoute.Outcome == FrameHandoffEmptySlotCallOutcome.ZeroRouteReturnedNormally
            && zeroRoute.ZeroRoutePostCallEcxCaptured
            && zeroRoute.CapturedZeroRoutePostCallEcx == 0xDEADBEEF
            && zeroRoute.NormalBoundaryReached
            && zeroRoute.StopAtRawFileOffsetExclusive
                == FrameHandoffEmptySlotCallProjection.NormalBoundaryRawFileOffset);

        check("Raw/VA and relative-target arithmetic wraps at DWORD width",
            FrameHandoffEmptySlotCallProjection.RawFileOffsetToVirtualAddress(uint.MaxValue)
                == 0x00400BFF
            && FrameHandoffEmptySlotCallProjection.VirtualAddressToRawFileOffset(0)
                == 0xFFBFF400
            && FrameHandoffEmptySlotCallProjection.RelativeCallTarget(uint.MaxValue - 3, 0) == 1
            && FrameHandoffEmptySlotCallProjection.ShortBranchTarget(uint.MaxValue, 0) == 1);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected =
            new (
                uint Raw,
                uint Virtual,
                string Bytes,
                uint? TargetRaw,
                uint? TargetVirtual,
                uint? IndirectCell)[]
            {
                (0x00BE64F2, 0x00FE70F2, "6A 10", null, null, null),
                (0x00BE64F4, 0x00FE70F4, "E8 B7 1A 6D 00", 0x012B7FB0, 0x016B8BB0, null),
                (0x00BE64F9, 0x00FE70F9, "8B C8", null, null, null),
                (0x00BE64FB, 0x00FE70FB, "83 C4 04", null, null, null),
                (0x00BE64FE, 0x00FE70FE, "85 C9", null, null, null),
                (0x00BE6500, 0x00FE7100, "75 06", 0x00BE6508, 0x00FE7108, null),
                (0x00BE6502, 0x00FE7102, "FF 15 24 44 AC 01", null, null, 0x01AC4424)
            };
        return FrameHandoffEmptySlotCallProjection.Evidence
            .Select(instruction =>
                (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes,
                    instruction.BranchTargetRawFileOffset, instruction.BranchTargetVirtualAddress,
                    instruction.IndirectCallCellVirtualAddress))
            .SequenceEqual(expected);
    }

    private static bool EvidenceBytesMatchCapturedWindow() =>
        string.Concat(FrameHandoffEmptySlotCallProjection.Evidence.Select(
            instruction => string.Concat(instruction.Bytes.Where(character =>
                !char.IsWhiteSpace(character)))))
        == FrameHandoffEmptySlotCallProjection.CapturedWindowBytes;
}
