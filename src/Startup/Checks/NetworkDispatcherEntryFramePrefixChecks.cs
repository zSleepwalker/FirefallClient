using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherEntryFramePrefixChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The supplied entry prefix is bounded before PUSH EBX",
            NetworkDispatcherEntryFramePrefixProjection.EntryRawFileOffset
                == 0x00BBDDB0
            && NetworkDispatcherEntryFramePrefixProjection.SaveEntryEBPRawFileOffset
                == 0x00BBDDB0
            && NetworkDispatcherEntryFramePrefixProjection.EstablishFramePointerRawFileOffset
                == 0x00BBDDB1
            && NetworkDispatcherEntryFramePrefixProjection.PushExceptionSentinelRawFileOffset
                == 0x00BBDDB3
            && NetworkDispatcherEntryFramePrefixProjection.PushExceptionHandlerRawFileOffset
                == 0x00BBDDB5
            && NetworkDispatcherEntryFramePrefixProjection.ReadFSOffset0RawFileOffset
                == 0x00BBDDBA
            && NetworkDispatcherEntryFramePrefixProjection.PushFSOffset0RawFileOffset
                == 0x00BBDDC0
            && NetworkDispatcherEntryFramePrefixProjection.AllocateLocalsRawFileOffset
                == 0x00BBDDC1
            && NetworkDispatcherEntryFramePrefixProjection.PrefixEndRawFileOffsetExclusive
                == 0x00BBDDC4
            && Normalize(NetworkDispatcherEntryFramePrefixProjection.CapturedWindowBytes)
                == "558BEC6AFF68183CA40164A1000000005083EC0C"
            && NetworkDispatcherEntryFramePrefixProjection.ExceptionHandlerVirtualAddress
                == 0x01A43C18
            && NetworkDispatcherEntryFramePrefixProjection.FSChainHeadSegmentOffset == 0
            && NetworkDispatcherEntryFramePrefixProjection.LocalAllocationSize == 0x0C
            && !NetworkDispatcherEntryFramePrefixProjection.ExecutesNativeInstructions);

        NetworkIoCompletionClockGateCapture belowThresholdCaller = CallerCapture(
            freshClockField: 500,
            clockReturnEAX: 999);
        NetworkDispatcherEntryFramePrefixPlan syntacticTargetWithoutCall =
            NetworkDispatcherEntryFramePrefixProjection.Project(Capture(
                callerClockGateCapture: belowThresholdCaller));
        check("A syntactic dispatcher target does not admit entry unless the caller reached its actual call",
            !syntacticTargetWithoutCall.CallerDispatcherCallReached
            && !syntacticTargetWithoutCall.ExactCallerDispatcherTarget
            && syntacticTargetWithoutCall.CallerClockGatePlan.Outcome
                == NetworkIoCompletionClockGateOutcome.BelowThreshold
            && syntacticTargetWithoutCall.Outcome
                == NetworkDispatcherEntryFramePrefixOutcome.CallerDispatcherCallNotReached
            && syntacticTargetWithoutCall.StopAtRawFileOffsetExclusive
                == syntacticTargetWithoutCall.CallerClockGatePlan.StopAtRawFileOffsetExclusive
            && syntacticTargetWithoutCall.StopAtRawFileOffsetExclusive
                == NetworkIoCompletionClockGateProjection.BoundedContinuationRawFileOffset
            && syntacticTargetWithoutCall.CompletedInstructionOffsets.IsEmpty
            && syntacticTargetWithoutCall.Pushes.IsEmpty
            && syntacticTargetWithoutCall.FreshEntryESP == 0x2000
            && syntacticTargetWithoutCall.FreshEntryEBP == 0x1234
            && syntacticTargetWithoutCall.FreshFSOffset0DWORD == 0xAABBCCDD
            && syntacticTargetWithoutCall.FreshEntryESPCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.SuppliedUnused
            && syntacticTargetWithoutCall.FreshEntryEBPCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.SuppliedUnused
            && syntacticTargetWithoutCall.FreshEntryFlagsCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.Unreached
            && syntacticTargetWithoutCall.FreshFSOffset0DWORDCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.SuppliedUnused);

        NetworkDispatcherEntryFramePrefixCapture pendingCapture = Capture();
        NetworkDispatcherEntryFramePrefixPlan complete =
            NetworkDispatcherEntryFramePrefixProjection.Project(pendingCapture);
        check("A reached dispatcher call with a pending return admits the exact entry and projects only the bounded prefix",
            complete.Outcome == NetworkDispatcherEntryFramePrefixOutcome.PrefixProjected
            && complete.CallerDispatcherCallReached
            && complete.CallerDispatcherReturnPending
            && complete.ExactCallerDispatcherTarget
            && complete.CallerDispatcherTargetVirtualAddress == 0x00FBE9B0
            && complete.CallerClockGatePlan.Outcome
                == NetworkIoCompletionClockGateOutcome.DispatcherReturnStatusUnknown
            && complete.CompletedInstructionOffsets.SequenceEqual(new uint[]
            {
                0x00BBDDB0,
                0x00BBDDB1,
                0x00BBDDB3,
                0x00BBDDB5,
                0x00BBDDBA,
                0x00BBDDC0,
                0x00BBDDC1
            })
            && complete.Pushes.Select(push => push.RawFileOffset).SequenceEqual(new uint[]
            {
                0x00BBDDB0,
                0x00BBDDB3,
                0x00BBDDB5,
                0x00BBDDC0
            })
            && complete.Pushes.Select(push => push.Value).SequenceEqual(new uint[]
            {
                0x1234,
                uint.MaxValue,
                0x01A43C18,
                0xAABBCCDD
            })
            && complete.Pushes.Select(push => push.StackAddress).SequenceEqual(new uint?[]
            {
                0x1FFC,
                0x1FF8,
                0x1FF4,
                0x1FF0
            })
            && complete.FSOffset0Read is {
                RawFileOffset: 0x00BBDDBA,
                SegmentRegister: NetworkDispatcherEntryFramePrefixSegmentRegister.FS,
                SegmentOffset: 0,
                Attempted: true,
                Captured: true,
                Value: 0xAABBCCDD,
                Committed: false
            }
            && complete.FreshEntryESPCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.Captured
            && complete.FreshEntryEBPCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.Captured
            && complete.FreshEntryFlagsCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.Missing
            && complete.FreshFSOffset0DWORDCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.Captured
            && complete.EntryEBPAfterMov == 0x1FFC
            && complete.CurrentESPAtStop == 0x1FE4
            && complete.SubtractionLeft == 0x1FF0
            && complete.SubtractionRight == 0x0C
            && complete.SubtractionResult == 0x1FE4
            && complete.SubtractionFlags is not null
            && complete.FlagsAtStop == complete.SubtractionFlags
            && complete.ExactPrefixProjected
            && complete.StopAtRawFileOffsetExclusive == 0x00BBDDC4);

        NetworkIoCompletionClockGateFlags rejectedEntryFlags = new(
            Carry: true,
            Parity: false,
            Auxiliary: true,
            Zero: false,
            Sign: true,
            Overflow: false);
        NetworkDispatcherEntryFramePrefixPlan completedCaller =
            NetworkDispatcherEntryFramePrefixProjection.Project(Capture(
                entryFlags: rejectedEntryFlags,
                callerClockGateCapture: CallerCapture(
                    dispatcherReturnedNormally: true)));
        NetworkDispatcherEntryFramePrefixPlan abnormalCaller =
            NetworkDispatcherEntryFramePrefixProjection.Project(Capture(
                entryFlags: rejectedEntryFlags,
                callerClockGateCapture: CallerCapture(
                    dispatcherReturnedNormally: false)));
        check("A caller with an observed dispatcher return is not treated as an in-progress ancestor",
            completedCaller.CallerDispatcherCallReached
            && !completedCaller.CallerDispatcherReturnPending
            && completedCaller.Outcome
                == NetworkDispatcherEntryFramePrefixOutcome.CallerDispatcherReturnAlreadyObserved
            && completedCaller.CallerClockGatePlan.Outcome
                == NetworkIoCompletionClockGateOutcome.DispatcherReturnedNormally
            && completedCaller.CallerClockGateCapture.DispatcherReturnedNormally == true
            && completedCaller.CallerClockGatePlan.DispatcherReturnedNormally == true
            && completedCaller.CallerDispatcherTargetVirtualAddress == 0x00FBE9B0
            && completedCaller.StopAtRawFileOffsetExclusive
                == completedCaller.CallerClockGatePlan.StopAtRawFileOffsetExclusive
            && completedCaller.StopAtRawFileOffsetExclusive
                == NetworkIoCompletionClockGateProjection.BoundedContinuationRawFileOffset
            && completedCaller.FreshEntryESP == 0x2000
            && completedCaller.FreshEntryEBP == 0x1234
            && completedCaller.FreshEntryFlags == rejectedEntryFlags
            && completedCaller.FreshFSOffset0DWORD == 0xAABBCCDD
            && completedCaller.FreshEntryESPCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.SuppliedUnused
            && completedCaller.FreshEntryEBPCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.SuppliedUnused
            && completedCaller.FreshEntryFlagsCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.SuppliedUnused
            && completedCaller.FreshFSOffset0DWORDCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.SuppliedUnused
            && completedCaller.Pushes.IsEmpty
            && abnormalCaller.CallerDispatcherCallReached
            && !abnormalCaller.CallerDispatcherReturnPending
            && abnormalCaller.Outcome
                == NetworkDispatcherEntryFramePrefixOutcome.CallerDispatcherReturnAlreadyObserved
            && abnormalCaller.CallerClockGatePlan.Outcome
                == NetworkIoCompletionClockGateOutcome.DispatcherDidNotReturnNormally
            && abnormalCaller.CallerClockGateCapture.DispatcherReturnedNormally == false
            && abnormalCaller.CallerClockGatePlan.DispatcherReturnedNormally == false
            && abnormalCaller.CallerDispatcherTargetVirtualAddress == 0x00FBE9B0
            && abnormalCaller.StopAtRawFileOffsetExclusive
                == abnormalCaller.CallerClockGatePlan.StopAtRawFileOffsetExclusive
            && abnormalCaller.StopAtRawFileOffsetExclusive
                == NetworkIoCompletionClockGateProjection.DispatcherCallRawFileOffset
            && abnormalCaller.FreshEntryESPCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.SuppliedUnused
            && abnormalCaller.FreshEntryEBPCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.SuppliedUnused
            && abnormalCaller.FreshEntryFlagsCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.SuppliedUnused
            && abnormalCaller.FreshFSOffset0DWORDCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.SuppliedUnused
            && abnormalCaller.CompletedInstructionOffsets.IsEmpty);

        NetworkIoCompletionClockGateFlags callerFlags = new(
            Carry: true,
            Parity: false,
            Auxiliary: true,
            Zero: false,
            Sign: true,
            Overflow: false);
        NetworkIoCompletionClockGateFlags freshEntryFlags = new(
            Carry: false,
            Parity: true,
            Auxiliary: false,
            Zero: true,
            Sign: false,
            Overflow: true);
        NetworkDispatcherEntryFramePrefixPlan suppliedFlagsThenOverwritten =
            NetworkDispatcherEntryFramePrefixProjection.Project(Capture(
                entryFlags: freshEntryFlags));
        check("Fresh entry flags are captured for the prefix before SUB overwrites all six",
            suppliedFlagsThenOverwritten.FreshEntryFlags == freshEntryFlags
            && suppliedFlagsThenOverwritten.FreshEntryFlagsCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.Captured
            && suppliedFlagsThenOverwritten.SubtractionFlags is not null
            && suppliedFlagsThenOverwritten.FlagsAtStop
                == suppliedFlagsThenOverwritten.SubtractionFlags
            && suppliedFlagsThenOverwritten.FlagsAtStop != freshEntryFlags);

        NetworkDispatcherEntryFramePrefixPlan missingFS =
            NetworkDispatcherEntryFramePrefixProjection.Project(Capture(
                entryFlags: freshEntryFlags,
                fsOffset0DWORD: null,
                callerClockGateCapture: CallerCapture(entryFlags: callerFlags)));
        check("A missing FS chain-head capture stops at BA after three stores without inventing the FS value",
            missingFS.Outcome
                == NetworkDispatcherEntryFramePrefixOutcome.MissingFreshFSOffset0DWORD
            && missingFS.StopAtRawFileOffsetExclusive == 0x00BBDDBA
            && missingFS.CompletedInstructionOffsets.SequenceEqual(new uint[]
            {
                0x00BBDDB0, 0x00BBDDB1, 0x00BBDDB3, 0x00BBDDB5
            })
            && missingFS.Pushes.Length == 3
            && missingFS.FSOffset0Read is {
                SegmentRegister: NetworkDispatcherEntryFramePrefixSegmentRegister.FS,
                SegmentOffset: 0,
                Attempted: true,
                Captured: false,
                Value: null,
                Committed: false
            }
            && missingFS.EntryEBPAfterMov == 0x1FFC
            && missingFS.CurrentESPAtStop == 0x1FF4
            && missingFS.FlagsAtStop == freshEntryFlags
            && missingFS.FlagsAtStop != callerFlags
            && missingFS.CallerClockGateCapture.EntryFlags == callerFlags
            && missingFS.FreshEntryFlags == freshEntryFlags
            && missingFS.FreshEntryESPCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.Captured
            && missingFS.FreshEntryEBPCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.Captured
            && missingFS.FreshEntryFlagsCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.Captured
            && missingFS.FreshFSOffset0DWORDCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.Missing
            && !missingFS.SubtractionReached
            && missingFS.SubtractionFlags is null);

        NetworkDispatcherEntryFramePrefixPlan missingESP =
            NetworkDispatcherEntryFramePrefixProjection.Project(Capture(entryESP: null));
        NetworkDispatcherEntryFramePrefixPlan missingEBP =
            NetworkDispatcherEntryFramePrefixProjection.Project(Capture(entryEBP: null));
        check("Missing fresh entry registers stop before a fabricated prologue push or frame pointer",
            missingESP.Outcome
                == NetworkDispatcherEntryFramePrefixOutcome.MissingFreshEntryESP
            && missingESP.Pushes.IsEmpty
            && missingESP.FreshEntryESPCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.Missing
            && missingESP.FreshEntryEBPCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.SuppliedUnused
            && missingESP.FreshFSOffset0DWORDCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.SuppliedUnused
            && missingESP.EntryEBPAfterMov is null
            && missingESP.StopAtRawFileOffsetExclusive
                == NetworkDispatcherEntryFramePrefixProjection.EntryRawFileOffset
            && missingEBP.Outcome
                == NetworkDispatcherEntryFramePrefixOutcome.MissingFreshEntryEBP
            && missingEBP.Pushes.IsEmpty
            && missingEBP.CurrentESPAtStop == 0x2000
            && missingEBP.FreshEntryESPCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.Captured
            && missingEBP.FreshEntryEBPCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.Missing
            && missingEBP.FreshFSOffset0DWORDCaptureState
                == NetworkDispatcherEntryFramePrefixCaptureState.SuppliedUnused
            && missingEBP.StopAtRawFileOffsetExclusive
                == NetworkDispatcherEntryFramePrefixProjection.EntryRawFileOffset);

        NetworkDispatcherEntryFramePrefixPlan wrappedZeroInputs =
            NetworkDispatcherEntryFramePrefixProjection.Project(Capture(
                entryESP: 0,
                entryEBP: uint.MaxValue,
                fsOffset0DWORD: 0));
        NetworkDispatcherEntryFramePrefixPlan maxInputs =
            NetworkDispatcherEntryFramePrefixProjection.Project(Capture(
                entryESP: uint.MaxValue,
                entryEBP: 0,
                fsOffset0DWORD: uint.MaxValue));
        NetworkDispatcherEntryFramePrefixPlan allArithmeticFlags =
            NetworkDispatcherEntryFramePrefixProjection.Project(Capture(entryESP: 0x18));
        NetworkDispatcherEntryFramePrefixPlan zeroSubtraction =
            NetworkDispatcherEntryFramePrefixProjection.Project(Capture(entryESP: 0x1C));
        check("Entry stack arithmetic wraps as DWORDs while zero and maximum register and FS values stay captured",
            NetworkDispatcherEntryFramePrefixProjection.StackAddressAfterPush(0) == 0xFFFFFFFC
            && NetworkDispatcherEntryFramePrefixProjection.EntryEBPAfterMov(0) == 0xFFFFFFFC
            && NetworkDispatcherEntryFramePrefixProjection.SubtractLocalAllocation(0xFFFFFFF0)
                == 0xFFFFFFE4
            && wrappedZeroInputs.Pushes[0].StackAddress == 0xFFFFFFFC
            && wrappedZeroInputs.Pushes[0].Value == uint.MaxValue
            && wrappedZeroInputs.FSOffset0Read?.Value == 0
            && wrappedZeroInputs.EntryEBPAfterMov == 0xFFFFFFFC
            && wrappedZeroInputs.CurrentESPAtStop == 0xFFFFFFE4
            && maxInputs.Pushes[0].StackAddress == 0xFFFFFFFB
            && maxInputs.Pushes[0].Value == 0
            && maxInputs.FSOffset0Read?.Value == uint.MaxValue
            && maxInputs.EntryEBPAfterMov == 0xFFFFFFFB
            && maxInputs.CurrentESPAtStop == 0xFFFFFFE3
            && allArithmeticFlags.SubtractionFlags == new NetworkIoCompletionClockGateFlags(
                Carry: true,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false)
            && zeroSubtraction.SubtractionFlags == new NetworkIoCompletionClockGateFlags(
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false));

        NetworkDispatcherEntryFramePrefixPlan signedOverflow =
            NetworkDispatcherEntryFramePrefixProjection.Project(Capture(
                entryESP: 0x80000017));
        check("SUB ESP, 0Ch computes parity, auxiliary, sign, carry, zero, and signed-overflow flags from its own operands",
            signedOverflow.SubtractionLeft == 0x80000007
            && signedOverflow.SubtractionResult == 0x7FFFFFFB
            && signedOverflow.SubtractionFlags == new NetworkIoCompletionClockGateFlags(
                Carry: false,
                Parity: false,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true));

        NetworkDispatcherEntryFramePrefixPlan repeated =
            NetworkDispatcherEntryFramePrefixProjection.Project(pendingCapture);
        check("Reprojection is deterministic, immutable, ordered, and leaves the caller capture unchanged",
            pendingCapture.CallerClockGateCapture.DispatcherReturnedNormally is null
            && complete.CallerClockGateCapture == pendingCapture.CallerClockGateCapture
            && repeated.CallerClockGateCapture == pendingCapture.CallerClockGateCapture
            && complete.CallerClockGatePlan.DispatcherReturnedNormally is null
            && repeated.CompletedInstructionOffsets.SequenceEqual(
                complete.CompletedInstructionOffsets)
            && repeated.Pushes.SequenceEqual(complete.Pushes)
            && repeated.FSOffset0Read == complete.FSOffset0Read
            && repeated.SubtractionFlags == complete.SubtractionFlags
            && repeated.CompletedInstructionOffsets[^1] == 0x00BBDDC1
            && repeated.CompletedInstructionOffsets[^1]
                != NetworkDispatcherEntryFramePrefixProjection.PrefixEndRawFileOffsetExclusive);
    }

    private static NetworkDispatcherEntryFramePrefixCapture Capture(
        NetworkIoCompletionClockGateCapture? callerClockGateCapture = null,
        uint? entryESP = 0x2000,
        uint? entryEBP = 0x1234,
        NetworkIoCompletionClockGateFlags? entryFlags = null,
        uint? fsOffset0DWORD = 0xAABBCCDD) =>
        new(
            callerClockGateCapture ?? CallerCapture(),
            entryESP,
            entryEBP,
            entryFlags,
            fsOffset0DWORD);

    private static NetworkIoCompletionClockGateCapture CallerCapture(
        bool? dispatcherReturnedNormally = null,
        NetworkIoCompletionClockGateFlags? entryFlags = null,
        uint? freshClockField = 500,
        uint? clockReturnEAX = 1000) =>
        new(
            EntryValidated: true,
            EntryEBP: 0x2000,
            EntryFlags: entryFlags,
            FreshGlobalPointer: 0x1000,
            FreshClockField: freshClockField,
            CachedClockTargetVirtualAddress: 0x12345678,
            ClockReturnedNormally: true,
            ClockReturnEAX: clockReturnEAX,
            FreshSavedPointerRead: 0x2222,
            DispatcherReturnedNormally: dispatcherReturnedNormally);

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Where(Uri.IsHexDigit)).ToUpperInvariant();
}
