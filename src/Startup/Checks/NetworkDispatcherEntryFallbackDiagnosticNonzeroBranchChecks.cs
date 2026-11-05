using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCapture Capture(
            uint? dwordAtESIPlusA4 = 0x6000,
            uint? arrayEntryDWORD = 0xCAFEBABE,
            bool? parentCallReturnedNormally = true,
            uint? parentA8DWORD = 0x80000003,
            uint? parentA4DWORD = 0x7FFFFFFF,
            uint? comparedDword = 0) =>
            new(
                new NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCapture(
                    CreateReturnCapture(parentCallReturnedNormally, comparedDword),
                    FreshPostCallESI: 0x5000,
                    FreshDWORDAtESIPlusA8: parentA8DWORD,
                    FreshDWORDAtESIPlusA4: parentA4DWORD),
                dwordAtESIPlusA4,
                arrayEntryDWORD);

        static NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchPlan Project(
            uint? dwordAtESIPlusA4 = 0x6000,
            uint? arrayEntryDWORD = 0xCAFEBABE,
            bool? parentCallReturnedNormally = true,
            uint? parentA8DWORD = 0x80000003,
            uint? parentA4DWORD = 0x7FFFFFFF,
            uint? comparedDword = 0) =>
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection.Project(
                Capture(
                    dwordAtESIPlusA4,
                    arrayEntryDWORD,
                    parentCallReturnedNormally,
                    parentA8DWORD,
                    parentA4DWORD,
                    comparedDword));

        static string Normalize(string value) =>
            string.Concat(value.Where(Uri.IsHexDigit)).ToUpperInvariant();

        check("Coordinator byte anchor and shared raw-to-VA mapping identify the fallthrough window",
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                .BytesIndependentlyVerified
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                .CoordinatorProvidedByteLength == 13
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                .CoordinatorProvidedBytes) == "8B86A40000008B3C9885FF7519"
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                .CoordinatorProvidedBytes).Length / 2
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                    .CoordinatorProvidedByteLength
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                .MoveEaxInstructionBytes) == "8B86A4000000"
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                .MoveEdiInstructionBytes) == "8B3C98"
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                .TestEdiInstructionBytes) == "85FF"
            && Normalize(NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                .BranchInstructionBytes) == "7519"
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection.EntryRawFileOffset
                == 0x00BBDE40
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                .MoveEaxRawFileOffset == 0x00BBDE40
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                .MoveEdiRawFileOffset == 0x00BBDE46
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                .TestEdiRawFileOffset == 0x00BBDE49
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                .BranchRawFileOffset == 0x00BBDE4B
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                .BranchNextRawFileOffset == 0x00BBDE4D
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                .BranchTargetRawFileOffset == 0x00BBDE66
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BBDE40) == 0x00FBEA40
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BBDE4B) == 0x00FBEA4B
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BBDE40)
                - 0x00BBDE40
                == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .RawFileOffsetToCodeVirtualAddress(0)
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                .BranchRawFileOffset
                + NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                    .BranchInstructionByteLength
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                    .BranchNextRawFileOffset
            && unchecked(
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                    .BranchNextRawFileOffset
                + 0x19)
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                    .BranchTargetRawFileOffset
            && NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .BranchRawFileOffset
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                    .EntryJzRawFileOffset
            && NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .BranchNextRawFileOffset
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                    .EntryRawFileOffset
            && !NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                .ExecutesNativeInstructions);

        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchPlan success = Project();
        check("Nonzero parent TEST result admits the fallthrough and stops before JNZ",
            success.ParentPlan.ExactCleanCompletion
            && success.ParentPlan.EAX == 1
            && success.ParentPlan.FlagsAtStop is { Zero: false }
            && success.BranchNotTakenAccepted
            && success.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchOutcome.BoundedBeforeJnz
            && success.ExactBoundedCompletion
            && success.ExactCleanCompletion
            && success.ProjectionEntered
            && success.ESI == 0x5000
            && success.EAX == 0x6000
            && success.EBX == 0
            && success.EDI == 0xCAFEBABE
            && success.Reads.Length == 2
            && success.Reads[0] is {
                InstructionRawFileOffset: 0x00BBDE40,
                Address: 0x50A4,
                Width: 4,
                Attempted: true,
                Captured: true,
                Value: 0x6000
            }
            && success.Reads[1] is {
                InstructionRawFileOffset: 0x00BBDE46,
                Address: 0x6000,
                Width: 4,
                Attempted: true,
                Captured: true,
                Value: 0xCAFEBABE
            }
            && success.Instructions.Length == 3
            && success.Instructions[0] is {
                InstructionRawFileOffset: 0x00BBDE40,
                InstructionCodeVirtualAddress: 0x00FBEA40,
                InstructionByteLength: 6,
                InstructionBytes: "8B86A4000000",
                Kind: NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstructionKind
                    .MoveEaxFromEsiPlusA4,
                EAXBefore: 1,
                EAXAfter: 0x6000,
                EBXBefore: 0,
                EBXAfter: 0,
                EDIAfter: null,
                Committed: true
            }
            && success.Instructions[0].FlagsBefore == success.ParentPlan.FlagsAtStop
            && success.Instructions[0].FlagsAfter == success.ParentPlan.FlagsAtStop
            && success.Instructions[1] is {
                InstructionRawFileOffset: 0x00BBDE46,
                InstructionByteLength: 3,
                InstructionBytes: "8B3C98",
                Kind: NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstructionKind
                    .MoveEdiFromEaxPlusScaledEbx,
                EAXBefore: 0x6000,
                EAXAfter: 0x6000,
                EBXBefore: 0,
                EBXAfter: 0,
                EDIBefore: null,
                EDIAfter: 0xCAFEBABE,
                Committed: true
            }
            && success.Instructions[1].FlagsBefore == success.ParentPlan.FlagsAtStop
            && success.Instructions[1].FlagsAfter == success.ParentPlan.FlagsAtStop
            && success.Instructions[2] is {
                InstructionRawFileOffset: 0x00BBDE49,
                InstructionByteLength: 2,
                InstructionBytes: "85FF",
                Kind: NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstructionKind
                    .TestEdiEdi,
                EAXBefore: 0x6000,
                EAXAfter: 0x6000,
                EBXBefore: 0,
                EBXAfter: 0,
                EDIBefore: 0xCAFEBABE,
                EDIAfter: 0xCAFEBABE,
                Committed: true
            }
            && success.FlagsAtStop is {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && success.CompletedInstructionOffsets.SequenceEqual(
                success.ParentPlan.CompletedInstructionOffsets
                    .Add(0x00BBDE40)
                    .Add(0x00BBDE46)
                    .Add(0x00BBDE49))
            && success.CompletedInstructionOffsets[^1] == 0x00BBDE49
            && success.StopAtRawFileOffsetExclusive == 0x00BBDE4B
            && success.JnzBoundaryReached
            && !success.ConditionalBranchProjected
            && !success.NativeInstructionsExecuted
            && !success.NativeMemoryReadsExecuted
            && !success.NativeMemoryWritesExecuted
            && !success.ParentInstructionsExecuted);

        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchPlan missingFirstDWORD =
            Project(dwordAtESIPlusA4: null);
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchPlan missingArrayEntry =
            Project(arrayEntryDWORD: null);
        check("Missing DWORD captures stop at their first use and leave later captures unused",
            missingFirstDWORD.ParentPlan.ExactCleanCompletion
            && missingArrayEntry.ParentPlan.ExactCleanCompletion
            && missingFirstDWORD.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchOutcome
                    .MissingFreshDWORDAtESIPlusA4
            && missingArrayEntry.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchOutcome
                    .MissingFreshArrayEntryDWORD
            && missingFirstDWORD.DWORDAtA4CaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState.Missing
            && missingFirstDWORD.ArrayEntryCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState
                    .SuppliedUnused
            && missingFirstDWORD.Reads is [
                { InstructionRawFileOffset: 0x00BBDE40, Address: 0x50A4, Captured: false }
            ]
            && missingFirstDWORD.Instructions is [
                { InstructionRawFileOffset: 0x00BBDE40, Committed: false }
            ]
            && missingFirstDWORD.CompletedInstructionOffsets.SequenceEqual(
                missingFirstDWORD.ParentPlan.CompletedInstructionOffsets)
            && missingFirstDWORD.StopAtRawFileOffsetExclusive == 0x00BBDE40
            && missingFirstDWORD.FlagsAtStop == missingFirstDWORD.ParentPlan.FlagsAtStop
            && missingArrayEntry.DWORDAtA4CaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState.Captured
            && missingArrayEntry.ArrayEntryCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState.Missing
            && missingArrayEntry.Reads is [
                { InstructionRawFileOffset: 0x00BBDE40, Address: 0x50A4, Captured: true },
                { InstructionRawFileOffset: 0x00BBDE46, Address: 0x6000, Captured: false }
            ]
            && missingArrayEntry.Instructions is [
                { InstructionRawFileOffset: 0x00BBDE40, Committed: true },
                { InstructionRawFileOffset: 0x00BBDE46, Committed: false }
            ]
            && missingArrayEntry.CompletedInstructionOffsets.SequenceEqual(
                missingArrayEntry.ParentPlan.CompletedInstructionOffsets.Add(0x00BBDE40))
            && missingArrayEntry.StopAtRawFileOffsetExclusive == 0x00BBDE46
            && missingArrayEntry.FlagsAtStop == missingArrayEntry.ParentPlan.FlagsAtStop
            && !missingFirstDWORD.ExactCleanCompletion
            && !missingArrayEntry.ExactCleanCompletion);

        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchPlan jzTaken =
            Project(parentA8DWORD: 0x100, parentA4DWORD: 0x100);
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchPlan rejectedParent =
            Project(parentCallReturnedNormally: false);
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchPlan pendingParent =
            Project(parentCallReturnedNormally: null);
        check("JZ-taken, pending, and rejected parents preserve bounds and child capture states",
            jzTaken.ParentPlan.ExactCleanCompletion
            && jzTaken.ParentPlan.EAX == 0
            && jzTaken.ParentPlan.FlagsAtStop is { Zero: true }
            && jzTaken.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchOutcome.JzTaken
            && rejectedParent.ParentPlan.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome
                    .ParentCallReturnNotAccepted
            && rejectedParent.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchOutcome
                    .ParentCallTargetPrefixNotAccepted
            && pendingParent.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchOutcome
                    .ParentCallTargetPrefixNotAccepted
            && !jzTaken.ProjectionEntered
            && !rejectedParent.ProjectionEntered
            && !pendingParent.ProjectionEntered
            && jzTaken.Reads.IsEmpty
            && rejectedParent.Reads.IsEmpty
            && pendingParent.Reads.IsEmpty
            && jzTaken.CompletedInstructionOffsets.SequenceEqual(
                jzTaken.ParentPlan.CompletedInstructionOffsets)
            && rejectedParent.CompletedInstructionOffsets.SequenceEqual(
                rejectedParent.ParentPlan.CompletedInstructionOffsets)
            && pendingParent.CompletedInstructionOffsets.SequenceEqual(
                pendingParent.ParentPlan.CompletedInstructionOffsets)
            && jzTaken.StopAtRawFileOffsetExclusive
                == jzTaken.ParentPlan.StopAtRawFileOffsetExclusive
            && rejectedParent.StopAtRawFileOffsetExclusive
                == rejectedParent.ParentPlan.StopAtRawFileOffsetExclusive
            && pendingParent.StopAtRawFileOffsetExclusive
                == pendingParent.ParentPlan.StopAtRawFileOffsetExclusive
            && jzTaken.FlagsAtStop == jzTaken.ParentPlan.FlagsAtStop
            && rejectedParent.FlagsAtStop == rejectedParent.ParentPlan.FlagsAtStop
            && pendingParent.FlagsAtStop == pendingParent.ParentPlan.FlagsAtStop
            && jzTaken.StopAtRawFileOffsetExclusive == 0x00BBDE3E
            && rejectedParent.StopAtRawFileOffsetExclusive == 0x00BBDE26
            && pendingParent.StopAtRawFileOffsetExclusive
                == pendingParent.ParentPlan.StopAtRawFileOffsetExclusive
            && jzTaken.DWORDAtA4CaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState
                    .SuppliedUnused
            && jzTaken.ArrayEntryCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState
                    .SuppliedUnused
            && rejectedParent.DWORDAtA4CaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState
                    .SuppliedUnused
            && rejectedParent.ArrayEntryCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState
                    .SuppliedUnused
            && pendingParent.DWORDAtA4CaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState
                    .SuppliedUnused
            && pendingParent.ArrayEntryCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState
                    .SuppliedUnused);

        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCapture immutableCapture =
            Capture();
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchPlan first =
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection.Project(
                immutableCapture);
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchPlan replay =
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection.Project(
                immutableCapture);
        check("Fresh reprojection preserves the parent branch and ordered fallthrough effects",
            first.Capture == replay.Capture
            && first.ParentPlan.Capture == replay.ParentPlan.Capture
            && first.ParentPlan.Instructions.SequenceEqual(replay.ParentPlan.Instructions)
            && first.ParentPlan.CompletedInstructionOffsets.SequenceEqual(
                replay.ParentPlan.CompletedInstructionOffsets)
            && first.Reads.SequenceEqual(replay.Reads)
            && first.Instructions.SequenceEqual(replay.Instructions)
            && first.CompletedInstructionOffsets.SequenceEqual(
                replay.CompletedInstructionOffsets)
            && first.StopAtRawFileOffsetExclusive == replay.StopAtRawFileOffsetExclusive
            && first.ESI == replay.ESI
            && first.EAX == replay.EAX
            && first.EBX == replay.EBX
            && first.EDI == replay.EDI
            && first.FlagsAtStop == replay.FlagsAtStop
            && first.OrderedSemanticPrefixPreserved
            && replay.OrderedSemanticPrefixPreserved
            && first.ExactCleanCompletion
            && replay.ExactCleanCompletion);

        static NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationPlan
            ProjectContinuation(
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCapture parentCapture,
                bool? jnzTaken,
                uint? entryESP,
                uint? dwordAtEDI) =>
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection.Project(
                new(
                    parentCapture,
                    jnzTaken,
                    entryESP,
                    dwordAtEDI));

        static string NormalizeContinuationBytes(string value) =>
            string.Concat(value.Where(Uri.IsHexDigit)).ToUpperInvariant();

        check("JNZ bytes, both raw targets, call boundaries, and raw-to-VA mapping are anchored",
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                .BytesIndependentlyVerified
            && NormalizeContinuationBytes(
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                    .JnzInstructionBytes) == "7519"
            && NormalizeContinuationBytes(
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                    .FallthroughPushEdiInstructionBytes) == "57"
            && NormalizeContinuationBytes(
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                    .FallthroughPushArgumentInstructionBytes) == "683B020000"
            && NormalizeContinuationBytes(
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                    .FallthroughPushFirstPointerInstructionBytes) == "687F71B501"
            && NormalizeContinuationBytes(
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                    .FallthroughPushSecondPointerInstructionBytes) == "687E71B501"
            && NormalizeContinuationBytes(
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                    .FallthroughCallInstructionBytes) == "FF15A05EC101"
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                .FallthroughCallInstructionByteLength == 6
            && NormalizeContinuationBytes(
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                    .TargetMoveEaxInstructionBytes) == "8B07"
            && NormalizeContinuationBytes(
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                    .TargetMoveEcxInstructionBytes) == "8BCF"
            && NormalizeContinuationBytes(
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                    .TargetPushOneInstructionBytes) == "6A01"
            && NormalizeContinuationBytes(
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                    .TargetCallInstructionBytes) == "FF5010"
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                .TargetCallInstructionByteLength == 3
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                .JnzRawFileOffset == 0x00BBDE4B
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                .FallthroughRawFileOffset == 0x00BBDE4D
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                .TakenTargetRawFileOffset == 0x00BBDE66
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                .FallthroughCallRawFileOffset == 0x00BBDE5D
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                .TargetCallRawFileOffset == 0x00BBDE6C
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BBDE4B) == 0x00FBEA4B
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BBDE4D) == 0x00FBEA4D
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BBDE66) == 0x00FBEA66
            && unchecked(
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                    .JnzRawFileOffset
                + NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                    .JnzInstructionByteLength
                + NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                    .BranchDisplacement)
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                    .TakenTargetRawFileOffset
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                .FallthroughRawFileOffset
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                    .BranchNextRawFileOffset
            && !NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                .ExecutesNativeInstructions);

        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationPlan fallthrough =
            ProjectContinuation(Capture(arrayEntryDWORD: 0), false, 0x9000, 0x11111111);
        check("JNZ-not-taken projects all four pushes and preserves registers and TEST flags",
            fallthrough.ParentPlan.ExactCleanCompletion
            && fallthrough.ParentPlan.EDI == 0
            && fallthrough.ParentPlan.FlagsAtStop is { Zero: true }
            && fallthrough.BranchObservationAccepted
            && fallthrough.BranchInstructionProjected
            && fallthrough.ConditionalBranchTaken == false
            && fallthrough.SelectedBranchTargetRawFileOffset == 0x00BBDE4D
            && fallthrough.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationOutcome
                    .BoundedBeforeFallthroughCall
            && fallthrough.ExactBoundedCompletion
            && fallthrough.ExactCleanCompletion
            && fallthrough.EAX == fallthrough.ParentPlan.EAX
            && fallthrough.EBX == fallthrough.ParentPlan.EBX
            && fallthrough.EDI == 0
            && fallthrough.ECX is null
            && fallthrough.ESP == 0x8FF0
            && fallthrough.FlagsAtStop == fallthrough.ParentPlan.FlagsAtStop
            && fallthrough.Instructions.Length == 5
            && fallthrough.Instructions[0] is {
                InstructionRawFileOffset: 0x00BBDE4B,
                InstructionCodeVirtualAddress: 0x00FBEA4B,
                InstructionByteLength: 2,
                InstructionBytes: "7519",
                Kind: NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstructionKind
                    .Jnz,
                BranchTaken: false,
                BranchTargetRawFileOffset: 0x00BBDE4D,
                Committed: true
            }
            && fallthrough.Instructions[0].FlagsBefore == fallthrough.ParentPlan.FlagsAtStop
            && fallthrough.Instructions[0].FlagsAfter == fallthrough.ParentPlan.FlagsAtStop
            && fallthrough.Instructions[1] is {
                InstructionRawFileOffset: 0x00BBDE4D,
                InstructionBytes: "57",
                StackWriteValue: 0,
                ESPBefore: 0x9000,
                ESPAfter: 0x8FFC,
                Committed: true
            }
            && fallthrough.Instructions[2] is {
                InstructionRawFileOffset: 0x00BBDE4E,
                InstructionBytes: "683B020000",
                StackWriteValue: 0x23B,
                ESPAfter: 0x8FF8,
                Committed: true
            }
            && fallthrough.Instructions[3] is {
                InstructionRawFileOffset: 0x00BBDE53,
                InstructionBytes: "687F71B501",
                StackWriteValue: 0x01B5717F,
                ESPAfter: 0x8FF4,
                Committed: true
            }
            && fallthrough.Instructions[4] is {
                InstructionRawFileOffset: 0x00BBDE58,
                InstructionBytes: "687E71B501",
                StackWriteValue: 0x01B5717E,
                ESPAfter: 0x8FF0,
                Committed: true
            }
            && fallthrough.EDIReadCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                    .SuppliedUnused
            && fallthrough.StackWrites is [
                {
                    InstructionRawFileOffset: 0x00BBDE4D,
                    Address: 0x8FFC,
                    Value: 0
                },
                {
                    InstructionRawFileOffset: 0x00BBDE4E,
                    Address: 0x8FF8,
                    Value: 0x23B
                },
                {
                    InstructionRawFileOffset: 0x00BBDE53,
                    Address: 0x8FF4,
                    Value: 0x01B5717F
                },
                {
                    InstructionRawFileOffset: 0x00BBDE58,
                    Address: 0x8FF0,
                    Value: 0x01B5717E
                }
            ]
            && fallthrough.CompletedInstructionOffsets.SequenceEqual(
                fallthrough.ParentPlan.CompletedInstructionOffsets
                    .Add(0x00BBDE4B)
                    .Add(0x00BBDE4D)
                    .Add(0x00BBDE4E)
                    .Add(0x00BBDE53)
                    .Add(0x00BBDE58))
            && fallthrough.StopAtRawFileOffsetExclusive == 0x00BBDE5D
            && !fallthrough.NativeInstructionsExecuted
            && !fallthrough.NativeMemoryReadsExecuted
            && !fallthrough.NativeMemoryWritesExecuted);

        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationPlan taken =
            ProjectContinuation(Capture(), true, 0x9000, 0x11223344);
        check("JNZ-taken projects target loads and push with exact register and flag effects",
            taken.ParentPlan.ExactCleanCompletion
            && taken.ParentPlan.EDI == 0xCAFEBABE
            && taken.ParentPlan.FlagsAtStop is { Zero: false }
            && taken.BranchObservationAccepted
            && taken.BranchInstructionProjected
            && taken.ConditionalBranchTaken == true
            && taken.SelectedBranchTargetRawFileOffset == 0x00BBDE66
            && taken.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationOutcome
                    .BoundedBeforeTargetCall
            && taken.ExactBoundedCompletion
            && taken.ExactCleanCompletion
            && taken.EAX == 0x11223344
            && taken.EBX == taken.ParentPlan.EBX
            && taken.ECX == 0xCAFEBABE
            && taken.EDI == taken.ParentPlan.EDI
            && taken.ESP == 0x8FFC
            && taken.FlagsAtStop == taken.ParentPlan.FlagsAtStop
            && taken.Reads is [{
                InstructionRawFileOffset: 0x00BBDE66,
                Address: 0xCAFEBABE,
                Width: 4,
                Attempted: true,
                Captured: true,
                Value: 0x11223344
            }]
            && taken.Instructions is [
                {
                    InstructionRawFileOffset: 0x00BBDE4B,
                    BranchTaken: true,
                    BranchTargetRawFileOffset: 0x00BBDE66,
                    Committed: true
                },
                {
                    InstructionRawFileOffset: 0x00BBDE66,
                    InstructionBytes: "8B07",
                    EAXBefore: 0x6000,
                    EAXAfter: 0x11223344,
                    Committed: true
                },
                {
                    InstructionRawFileOffset: 0x00BBDE68,
                    InstructionBytes: "8BCF",
                    ECXAfter: 0xCAFEBABE,
                    Committed: true
                },
                {
                    InstructionRawFileOffset: 0x00BBDE6A,
                    InstructionBytes: "6A01",
                    StackWriteValue: 1,
                    ESPBefore: 0x9000,
                    ESPAfter: 0x8FFC,
                    Committed: true
                }
            ]
            && taken.Instructions[0].FlagsBefore == taken.ParentPlan.FlagsAtStop
            && taken.Instructions[0].FlagsAfter == taken.ParentPlan.FlagsAtStop
            && taken.StackWrites is [{
                InstructionRawFileOffset: 0x00BBDE6A,
                Address: 0x8FFC,
                Value: 1
            }]
            && taken.CompletedInstructionOffsets.SequenceEqual(
                taken.ParentPlan.CompletedInstructionOffsets
                    .Add(0x00BBDE4B)
                    .Add(0x00BBDE66)
                    .Add(0x00BBDE68)
                    .Add(0x00BBDE6A))
            && taken.StopAtRawFileOffsetExclusive == 0x00BBDE6C);

        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationPlan missingBranch =
            ProjectContinuation(Capture(), null, 0x9000, 0x11223344);
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationPlan mismatchedBranch =
            ProjectContinuation(Capture(), false, 0x9000, 0x11223344);
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationPlan missingTargetRead =
            ProjectContinuation(Capture(), true, 0x9000, null);
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationPlan missingFallthroughESP =
            ProjectContinuation(Capture(arrayEntryDWORD: 0), false, null, null);
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationPlan missingTargetESP =
            ProjectContinuation(Capture(), true, null, 0x11223344);
        check("Missing or inconsistent branch and path captures stop exactly at their first use",
            missingBranch.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationOutcome
                    .MissingJnzObservation
            && missingBranch.BranchCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                    .Missing
            && missingBranch.ESPCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                    .SuppliedUnused
            && missingBranch.EDIReadCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                    .SuppliedUnused
            && missingBranch.CompletedInstructionOffsets.SequenceEqual(
                missingBranch.ParentPlan.CompletedInstructionOffsets)
            && missingBranch.StopAtRawFileOffsetExclusive == 0x00BBDE4B
            && !missingBranch.BranchInstructionProjected
            && mismatchedBranch.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationOutcome
                    .JnzObservationMismatch
            && mismatchedBranch.BranchCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                    .Captured
            && !mismatchedBranch.BranchObservationAccepted
            && mismatchedBranch.SelectedBranchTargetRawFileOffset is null
            && mismatchedBranch.CompletedInstructionOffsets.SequenceEqual(
                mismatchedBranch.ParentPlan.CompletedInstructionOffsets)
            && mismatchedBranch.StopAtRawFileOffsetExclusive == 0x00BBDE4B
            && missingTargetRead.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationOutcome
                    .MissingFreshDWORDAtEDI
            && missingTargetRead.EDIReadCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                    .Missing
            && missingTargetRead.ESPCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                    .SuppliedUnused
            && missingTargetRead.CompletedInstructionOffsets.SequenceEqual(
                missingTargetRead.ParentPlan.CompletedInstructionOffsets.Add(0x00BBDE4B))
            && missingTargetRead.StopAtRawFileOffsetExclusive == 0x00BBDE66
            && missingFallthroughESP.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationOutcome
                    .MissingFreshEntryESPAtFallthroughPush
            && missingFallthroughESP.ESPCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                    .Missing
            && missingFallthroughESP.EDIReadCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                    .Unreached
            && missingFallthroughESP.StopAtRawFileOffsetExclusive == 0x00BBDE4D
            && missingTargetESP.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationOutcome
                    .MissingFreshEntryESPAtTargetPush
            && missingTargetESP.ESPCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                    .Missing
            && missingTargetESP.EAX == 0x11223344
            && missingTargetESP.ECX == 0xCAFEBABE
            && missingTargetESP.StopAtRawFileOffsetExclusive == 0x00BBDE6A);

        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCapture validParentCapture =
            Capture();
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationPlan validReplay =
            ProjectContinuation(validParentCapture, true, 0x9000, 0x11223344);
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCapture changedParentCapture =
            validParentCapture with
            {
                ParentCapture = validParentCapture.ParentCapture with
                {
                    FreshDWORDAtESIPlusA8 = 0x100,
                    FreshDWORDAtESIPlusA4 = 0x100
                }
            };
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationPlan rejectedReplay =
            ProjectContinuation(changedParentCapture, true, 0x9000, 0x11223344);
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationPlan validReplayAgain =
            ProjectContinuation(validParentCapture, true, 0x9000, 0x11223344);
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationPlan
            fallthroughReplayAgain =
            ProjectContinuation(Capture(arrayEntryDWORD: 0), false, 0x9000, 0x11111111);
        check("A changed parent capture is freshly replayed and rejected without trusting an old plan",
            validReplay.ParentPlan.ExactCleanCompletion
            && rejectedReplay.ParentPlan.Capture != validReplay.ParentPlan.Capture
            && rejectedReplay.ParentPlan.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchOutcome.JzTaken
            && rejectedReplay.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationOutcome
                    .ParentNotAccepted
            && !rejectedReplay.ProjectionEntered
            && rejectedReplay.BranchCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                    .SuppliedUnused
            && rejectedReplay.ESPCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                    .SuppliedUnused
            && rejectedReplay.EDIReadCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                    .SuppliedUnused
            && rejectedReplay.StopAtRawFileOffsetExclusive
                == rejectedReplay.ParentPlan.StopAtRawFileOffsetExclusive
            && rejectedReplay.OrderedSemanticPrefixPreserved);
        check("Fresh continuation reprojection is deterministic for both JNZ choices",
            validReplay.Capture == validReplayAgain.Capture
            && validReplay.ParentPlan.Capture == validReplayAgain.ParentPlan.Capture
            && validReplay.Outcome == validReplayAgain.Outcome
            && validReplay.Instructions.SequenceEqual(validReplayAgain.Instructions)
            && validReplay.Reads.SequenceEqual(validReplayAgain.Reads)
            && validReplay.StackWrites.SequenceEqual(validReplayAgain.StackWrites)
            && validReplay.CompletedInstructionOffsets.SequenceEqual(
                validReplayAgain.CompletedInstructionOffsets)
            && validReplay.SelectedBranchTargetRawFileOffset
                == validReplayAgain.SelectedBranchTargetRawFileOffset
            && validReplay.StopAtRawFileOffsetExclusive
                == validReplayAgain.StopAtRawFileOffsetExclusive
            && validReplay.ESI == validReplayAgain.ESI
            && validReplay.EAX == validReplayAgain.EAX
            && validReplay.EBX == validReplayAgain.EBX
            && validReplay.ECX == validReplayAgain.ECX
            && validReplay.EDI == validReplayAgain.EDI
            && validReplay.ESP == validReplayAgain.ESP
            && validReplay.FlagsAtStop == validReplayAgain.FlagsAtStop
            && validReplay.OrderedSemanticPrefixPreserved
            && validReplay.ExactCleanCompletion
            && fallthrough.OrderedSemanticPrefixPreserved
            && fallthrough.ExactCleanCompletion
            && fallthrough.Capture == fallthroughReplayAgain.Capture
            && fallthrough.Instructions.SequenceEqual(fallthroughReplayAgain.Instructions)
            && fallthrough.StackWrites.SequenceEqual(fallthroughReplayAgain.StackWrites)
            && fallthrough.CompletedInstructionOffsets.SequenceEqual(
                fallthroughReplayAgain.CompletedInstructionOffsets)
            && fallthrough.SelectedBranchTargetRawFileOffset
                == fallthroughReplayAgain.SelectedBranchTargetRawFileOffset
            && fallthrough.StopAtRawFileOffsetExclusive
                == fallthroughReplayAgain.StopAtRawFileOffsetExclusive
            && fallthrough.FlagsAtStop == fallthroughReplayAgain.FlagsAtStop
            && fallthroughReplayAgain.OrderedSemanticPrefixPreserved
            && fallthroughReplayAgain.ExactCleanCompletion);

        static NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnPlan
            ProjectCallReturn(
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCapture
                    parentCapture,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite callSite,
                bool? normalReturnObserved) =>
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnProjection.Project(
                new(parentCapture, callSite, normalReturnObserved));

        check("Both opaque CALL sites and immediate return boundaries have exact byte anchors",
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnProjection
                .BytesIndependentlyVerified
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnProjection
                .CallsAreIndirect
            && NormalizeContinuationBytes(
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnProjection
                    .FallthroughCallInstructionBytes) == "FF15A05EC101"
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnProjection
                .FallthroughCallInstructionByteLength == 6
            && NormalizeContinuationBytes(
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnProjection
                    .FallthroughNextInstructionBytes) == "83C410"
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnProjection
                .FallthroughCallRawFileOffset == 0x00BBDE5D
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnProjection
                .FallthroughNextInstructionRawFileOffset == 0x00BBDE63
            && NormalizeContinuationBytes(
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnProjection
                    .TargetCallInstructionBytes) == "FF5010"
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnProjection
                .TargetCallInstructionByteLength == 3
            && NormalizeContinuationBytes(
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnProjection
                    .TargetNextInstructionBytes) == "8B86A8000000"
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnProjection
                .TargetCallRawFileOffset == 0x00BBDE6C
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnProjection
                .TargetNextInstructionRawFileOffset == 0x00BBDE6F
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BBDE5D) == 0x00FBEA5D
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BBDE63) == 0x00FBEA63
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BBDE6C) == 0x00FBEA6C
            && NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BBDE6F) == 0x00FBEA6F);

        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCapture
            fallthroughCallParentCapture =
            new(Capture(arrayEntryDWORD: 0), false, 0x9000, null);
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCapture
            targetCallParentCapture =
            new(Capture(), true, 0x9000, 0x11223344);
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnPlan
            fallthroughNormalReturn =
            ProjectCallReturn(
                fallthroughCallParentCapture,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite
                    .FallthroughAtBBDE5D,
                true);
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnPlan targetNormalReturn =
            ProjectCallReturn(
                targetCallParentCapture,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite
                    .TakenTargetAtBBDE6C,
                true);
        check("Normal return at BBDE5D completes only that CALL and stops before ADD ESP",
            fallthroughNormalReturn.ParentPlan.ExactCleanCompletion
            && fallthroughNormalReturn.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnOutcome
                    .BoundedAfterFallthroughCall
            && fallthroughNormalReturn.Call is {
                CallSite: NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite
                    .FallthroughAtBBDE5D,
                InstructionRawFileOffset: 0x00BBDE5D,
                InstructionCodeVirtualAddress: 0x00FBEA5D,
                InstructionByteLength: 6,
                InstructionBytes: "FF15A05EC101",
                InstructionReached: true,
                Attempted: true,
                ReturnedNormally: true,
                Completed: true
            }
            && fallthroughNormalReturn.CompletedInstructionOffsets.SequenceEqual(
                fallthroughNormalReturn.ParentPlan.CompletedInstructionOffsets
                    .Add(0x00BBDE5D))
            && fallthroughNormalReturn.StopAtRawFileOffsetExclusive == 0x00BBDE63
            && fallthroughNormalReturn.FlagsAtStop is {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && fallthroughNormalReturn.NextInstructionBoundaryReached
            && !fallthroughNormalReturn.PostCallRegistersForwarded
            && !fallthroughNormalReturn.PostCallFlagsForwarded
            && !fallthroughNormalReturn.NativeCallExecuted
            && targetNormalReturn.ParentPlan.ExactCleanCompletion
            && targetNormalReturn.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnOutcome
                    .BoundedAfterTargetCall
            && targetNormalReturn.Call is {
                CallSite: NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite
                    .TakenTargetAtBBDE6C,
                InstructionRawFileOffset: 0x00BBDE6C,
                InstructionCodeVirtualAddress: 0x00FBEA6C,
                InstructionByteLength: 3,
                InstructionBytes: "FF5010",
                InstructionReached: true,
                Attempted: true,
                ReturnedNormally: true,
                Completed: true
            }
            && targetNormalReturn.CompletedInstructionOffsets.SequenceEqual(
                targetNormalReturn.ParentPlan.CompletedInstructionOffsets
                    .Add(0x00BBDE6C))
            && targetNormalReturn.StopAtRawFileOffsetExclusive == 0x00BBDE6F
            && targetNormalReturn.FlagsAtStop is {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && targetNormalReturn.NextInstructionBoundaryReached
            && !targetNormalReturn.PostCallRegistersForwarded
            && !targetNormalReturn.PostCallFlagsForwarded
            && !targetNormalReturn.NativeCallExecuted);

        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnPlan
            fallthroughPendingReturn =
            ProjectCallReturn(
                fallthroughCallParentCapture,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite
                    .FallthroughAtBBDE5D,
                null);
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnPlan
            fallthroughNonNormalReturn =
            ProjectCallReturn(
                fallthroughCallParentCapture,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite
                    .FallthroughAtBBDE5D,
                false);
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnPlan targetPendingReturn =
            ProjectCallReturn(
                targetCallParentCapture,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite
                    .TakenTargetAtBBDE6C,
                null);
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnPlan
            targetNonNormalReturn =
            ProjectCallReturn(
                targetCallParentCapture,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite
                    .TakenTargetAtBBDE6C,
                false);
        check("Pending and non-normal observations preserve each pre-call bound and flags",
            fallthroughPendingReturn.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnOutcome
                    .NormalReturnNotObserved
            && fallthroughPendingReturn.NormalReturnCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnCaptureState
                    .Pending
            && fallthroughPendingReturn.CompletedInstructionOffsets.SequenceEqual(
                fallthroughPendingReturn.ParentPlan.CompletedInstructionOffsets)
            && fallthroughPendingReturn.StopAtRawFileOffsetExclusive == 0x00BBDE5D
            && fallthroughPendingReturn.FlagsAtStop
                == fallthroughPendingReturn.ParentPlan.FlagsAtStop
            && fallthroughPendingReturn.Call is {
                ReturnedNormally: null,
                Completed: false
            }
            && fallthroughNonNormalReturn.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnOutcome
                    .CallDidNotReturnNormally
            && fallthroughNonNormalReturn.NormalReturnCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnCaptureState
                    .Captured
            && fallthroughNonNormalReturn.CompletedInstructionOffsets.SequenceEqual(
                fallthroughNonNormalReturn.ParentPlan.CompletedInstructionOffsets)
            && fallthroughNonNormalReturn.StopAtRawFileOffsetExclusive == 0x00BBDE5D
            && fallthroughNonNormalReturn.FlagsAtStop
                == fallthroughNonNormalReturn.ParentPlan.FlagsAtStop
            && fallthroughNonNormalReturn.Call is {
                ReturnedNormally: false,
                Completed: false
            }
            && targetPendingReturn.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnOutcome
                    .NormalReturnNotObserved
            && targetPendingReturn.NormalReturnCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnCaptureState
                    .Pending
            && targetPendingReturn.CompletedInstructionOffsets.SequenceEqual(
                targetPendingReturn.ParentPlan.CompletedInstructionOffsets)
            && targetPendingReturn.StopAtRawFileOffsetExclusive == 0x00BBDE6C
            && targetPendingReturn.FlagsAtStop == targetPendingReturn.ParentPlan.FlagsAtStop
            && targetPendingReturn.Call is {
                ReturnedNormally: null,
                Completed: false
            }
            && targetNonNormalReturn.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnOutcome
                    .CallDidNotReturnNormally
            && targetNonNormalReturn.NormalReturnCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnCaptureState
                    .Captured
            && targetNonNormalReturn.CompletedInstructionOffsets.SequenceEqual(
                targetNonNormalReturn.ParentPlan.CompletedInstructionOffsets)
            && targetNonNormalReturn.StopAtRawFileOffsetExclusive == 0x00BBDE6C
            && targetNonNormalReturn.FlagsAtStop == targetNonNormalReturn.ParentPlan.FlagsAtStop
            && targetNonNormalReturn.Call is {
                ReturnedNormally: false,
                Completed: false
            });

        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnPlan
            rejectedCallParent =
            ProjectCallReturn(
                new(
                    Capture(parentCallReturnedNormally: false),
                    true,
                    0x9000,
                    0x11223344),
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite
                    .TakenTargetAtBBDE6C,
                true);
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnPlan
            wrongPathCallSite =
            ProjectCallReturn(
                targetCallParentCapture,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite
                    .FallthroughAtBBDE5D,
                true);
        check("Rejected parents and unreachable call sites inherit their parent bounds",
            rejectedCallParent.ParentPlan.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationOutcome
                    .ParentNotAccepted
            && rejectedCallParent.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnOutcome
                    .ParentNotAccepted
            && rejectedCallParent.Call is null
            && rejectedCallParent.NormalReturnCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnCaptureState
                    .SuppliedUnused
            && rejectedCallParent.CompletedInstructionOffsets.SequenceEqual(
                rejectedCallParent.ParentPlan.CompletedInstructionOffsets)
            && rejectedCallParent.StopAtRawFileOffsetExclusive
                == rejectedCallParent.ParentPlan.StopAtRawFileOffsetExclusive
            && rejectedCallParent.FlagsAtStop == rejectedCallParent.ParentPlan.FlagsAtStop
            && wrongPathCallSite.ParentPlan.ExactCleanCompletion
            && wrongPathCallSite.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnOutcome
                    .CallSiteNotReached
            && wrongPathCallSite.Call is null
            && wrongPathCallSite.NormalReturnCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnCaptureState
                    .SuppliedUnused
            && wrongPathCallSite.CompletedInstructionOffsets.SequenceEqual(
                wrongPathCallSite.ParentPlan.CompletedInstructionOffsets)
            && wrongPathCallSite.StopAtRawFileOffsetExclusive == 0x00BBDE6C
            && wrongPathCallSite.FlagsAtStop == wrongPathCallSite.ParentPlan.FlagsAtStop);

        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnPlan
            fallthroughCallReplay =
            ProjectCallReturn(
                fallthroughCallParentCapture,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite
                    .FallthroughAtBBDE5D,
                true);
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnPlan targetCallReplay =
            ProjectCallReturn(
                targetCallParentCapture,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite
                    .TakenTargetAtBBDE6C,
                true);
        check("Fresh call-boundary reprojection is deterministic for both paths",
            fallthroughNormalReturn.Capture == fallthroughCallReplay.Capture
            && fallthroughNormalReturn.ParentPlan.Capture == fallthroughCallReplay.ParentPlan.Capture
            && fallthroughNormalReturn.Call == fallthroughCallReplay.Call
            && fallthroughNormalReturn.Outcome == fallthroughCallReplay.Outcome
            && fallthroughNormalReturn.CompletedInstructionOffsets.SequenceEqual(
                fallthroughCallReplay.CompletedInstructionOffsets)
            && fallthroughNormalReturn.StopAtRawFileOffsetExclusive
                == fallthroughCallReplay.StopAtRawFileOffsetExclusive
            && fallthroughNormalReturn.FlagsAtStop == fallthroughCallReplay.FlagsAtStop
            && fallthroughNormalReturn.OrderedSemanticPrefixPreserved
            && fallthroughCallReplay.OrderedSemanticPrefixPreserved
            && fallthroughNormalReturn.ExactCleanCompletion
            && fallthroughCallReplay.ExactCleanCompletion
            && targetNormalReturn.Capture == targetCallReplay.Capture
            && targetNormalReturn.ParentPlan.Capture == targetCallReplay.ParentPlan.Capture
            && targetNormalReturn.Call == targetCallReplay.Call
            && targetNormalReturn.Outcome == targetCallReplay.Outcome
            && targetNormalReturn.CompletedInstructionOffsets.SequenceEqual(
                targetCallReplay.CompletedInstructionOffsets)
            && targetNormalReturn.StopAtRawFileOffsetExclusive
                == targetCallReplay.StopAtRawFileOffsetExclusive
            && targetNormalReturn.FlagsAtStop == targetCallReplay.FlagsAtStop
            && targetNormalReturn.OrderedSemanticPrefixPreserved
            && targetCallReplay.OrderedSemanticPrefixPreserved
            && targetNormalReturn.ExactCleanCompletion
            && targetCallReplay.ExactCleanCompletion);
    }

    private static NetworkDispatcherEntryFallbackDiagnosticReturnCapture
        CreateReturnCapture(bool? parentCallReturnedNormally, uint? comparedDword) =>
        new(
            CreatePreparationCapture(comparedDword),
            parentCallReturnedNormally);

    private static NetworkDispatcherEntryFallbackDiagnosticPreparationCapture
        CreatePreparationCapture(uint? comparedDword)
    {
        var callerCapture = new NetworkIoCompletionClockGateCapture(
            EntryValidated: true,
            EntryEBP: 0x2000,
            EntryFlags: null,
            FreshGlobalPointer: 0x1000,
            FreshClockField: 500,
            CachedClockTargetVirtualAddress: 0x12345678,
            ClockReturnedNormally: true,
            ClockReturnEAX: 1000,
            FreshSavedPointerRead: 0x2222,
            DispatcherReturnedNormally: null);
        var frameCapture = new NetworkDispatcherEntryFramePrefixCapture(
            callerCapture,
            0x2000,
            0x2000,
            new NetworkIoCompletionClockGateFlags(
                Carry: true,
                Parity: false,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false),
            0xAABBCCDD);
        NetworkDispatcherEntryFramePrefixPlan framePlan =
            NetworkDispatcherEntryFramePrefixProjection.Project(frameCapture);
        var registerCapture = new NetworkDispatcherEntryRegisterSaveCapture(
            frameCapture,
            framePlan,
            FreshEntryEBX: 0x11111111,
            FreshEntryESI: 0x123,
            FreshEntryEDI: 0x33333333);
        var cookieCapture = new NetworkDispatcherEntryCookiePreparationCapture(
            registerCapture,
            FreshCookieGlobalDWORD: 0);
        var lockCapture = new NetworkDispatcherEntryLockPreparationCapture(
            new NetworkDispatcherEntryFsPublicationCapture(cookieCapture),
            FreshEntryECX: 0x123);
        var entryCallCapture = new NetworkDispatcherEntryImportReturnCapture(
            lockCapture,
            FreshOperationTargetDWORD: 0x12345678,
            NormalReturnObserved: true,
            FreshReturnedEAX: 0);
        var fallbackPushCapture = new NetworkDispatcherEntryFallbackPushCapture(
            new NetworkDispatcherEntryReturnedTestBranchCapture(entryCallCapture),
            FreshPostCallESP: 0x5000,
            FreshPostCallEBX: 0xA5A5);
        var fallbackImportCapture = new NetworkDispatcherEntryFallbackImportReturnCapture(
            fallbackPushCapture,
            FreshOperationTargetDWORD: 0x12345678,
            NormalReturnObserved: true);
        var fallbackCounterCapture = new NetworkDispatcherEntryFallbackCounterCapture(
            fallbackImportCapture,
            FreshPostFallbackEBX: 0x1000,
            FreshCounterDword: 0);
        var argumentCompareCapture = new NetworkDispatcherEntryFallbackArgumentCompareCapture(
            fallbackCounterCapture,
            FreshPostFallbackEBP: 0x2000,
            FreshArgumentByte: 0x80);
        var clearStoresCapture = new NetworkDispatcherEntryFallbackClearStoresCapture(
            argumentCompareCapture,
            FreshPostFallbackESI: 0x1000);
        var zeroBranchCapture = new NetworkDispatcherEntryFallbackZeroBranchCapture(
            clearStoresCapture);
        return new(
            new NetworkDispatcherEntryFallbackSelectorFallthroughCapture(
                new NetworkDispatcherEntryFallbackSelectorBranchCapture(
                    new NetworkDispatcherEntryFallbackSelectorCompareCapture(
                        zeroBranchCapture,
                        comparedDword)),
                0x5008,
                0x12345678));
    }
}
