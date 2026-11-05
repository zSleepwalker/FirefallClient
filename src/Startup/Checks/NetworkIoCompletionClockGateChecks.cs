using Firefall.Reconstruction.Startup;

internal static class NetworkIoCompletionClockGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The raw instruction sequence, dispatcher raw target, and PE RVA/VA conversion agree",
            NetworkIoCompletionClockGateProjection.EntryRawFileOffset == 0x00BBC8B0
            && NetworkIoCompletionClockGateProjection.ThresholdLoadRawFileOffset == 0x00BBC8B6
            && NetworkIoCompletionClockGateProjection.SavePointerRawFileOffset == 0x00BBC8BB
            && NetworkIoCompletionClockGateProjection.FirstFenceRawFileOffset == 0x00BBC8BE
            && NetworkIoCompletionClockGateProjection.FreshClockFieldRawFileOffset == 0x00BBC8C1
            && NetworkIoCompletionClockGateProjection.SecondFenceRawFileOffset == 0x00BBC8C4
            && NetworkIoCompletionClockGateProjection.ClockCallRawFileOffset == 0x00BBC8C7
            && NetworkIoCompletionClockGateProjection.SubtractRawFileOffset == 0x00BBC8C9
            && NetworkIoCompletionClockGateProjection.RestorePointerRawFileOffset == 0x00BBC8CB
            && NetworkIoCompletionClockGateProjection.CompareRawFileOffset == 0x00BBC8CE
            && NetworkIoCompletionClockGateProjection.BelowBranchRawFileOffset == 0x00BBC8D0
            && NetworkIoCompletionClockGateProjection.DispatcherPushRawFileOffset == 0x00BBC8D2
            && NetworkIoCompletionClockGateProjection.DispatcherArgumentRawFileOffset == 0x00BBC8D4
            && NetworkIoCompletionClockGateProjection.DispatcherCallRawFileOffset == 0x00BBC8D6
            && NetworkIoCompletionClockGateProjection.BoundedContinuationRawFileOffset == 0x00BBC8DB
            && Normalize(NetworkIoCompletionClockGateProjection.CapturedWindowBytes)
                == "8B3570FFDF01BBF40100008975F00FAEE88B76680FAEE8FFD02BC6"
                    + "8B75F03BC372096A018BCEE8D5140000"
            && NetworkIoCompletionClockGateProjection.FreshGlobalPointerVirtualAddress
                == 0x01DFFF70
            && NetworkIoCompletionClockGateProjection.FreshClockFieldOffset == 0x68
            && NetworkIoCompletionClockGateProjection.SavedPointerStackOffset == 0xFFFFFFF0
            && NetworkIoCompletionClockGateProjection.ThresholdMilliseconds == 500
            && NetworkIoCompletionClockGateProjection.ClockIatVirtualAddress == 0x01AC4D24
            && NetworkIoCompletionClockGateProjection.ClockIatRawFileOffset == 0x016C3924
            && NetworkIoCompletionClockGateProjection.DispatcherRawFileOffset == 0x00BBDDB0
            && NetworkIoCompletionClockGateProjection.DispatcherRva == 0x00BBE9B0
            && NetworkIoCompletionClockGateProjection.DispatcherVirtualAddress == 0x00FBE9B0
            && unchecked(NetworkIoCompletionClockGateProjection.DispatcherCallRawFileOffset
                + 5 + NetworkIoCompletionClockGateProjection.DispatcherCallRelativeDisplacement)
                == NetworkIoCompletionClockGateProjection.DispatcherRawFileOffset
            && unchecked(NetworkIoCompletionClockGateProjection.DispatcherRva
                + NetworkIoCompletionClockGateProjection.ImageBase)
                == NetworkIoCompletionClockGateProjection.DispatcherVirtualAddress
            && !NetworkIoCompletionClockGateProjection.ExecutesNativeInstructions);

        NetworkIoCompletionClockGatePlan invalidEntry =
            NetworkIoCompletionClockGateProjection.Project(Capture(entryValidated: false));
        check("An invalid internal gate entry is stopped before reads, stores, or fences",
            invalidEntry.Outcome == NetworkIoCompletionClockGateOutcome.EntryNotValidated
            && invalidEntry.Reads.IsEmpty
            && invalidEntry.Writes.IsEmpty
            && invalidEntry.FencesRecorded.IsEmpty
            && !invalidEntry.ThresholdLoaded
            && !invalidEntry.ClockCallReached
            && invalidEntry.FlagsAtStop is null
            && invalidEntry.StopAtRawFileOffsetExclusive
                == NetworkIoCompletionClockGateProjection.EntryRawFileOffset);

        NetworkIoCompletionClockGateFlags entryFlags = new(
            Carry: true,
            Parity: null,
            Auxiliary: false,
            Zero: null,
            Sign: true,
            Overflow: false);
        NetworkIoCompletionClockGatePlan missingGlobal =
            NetworkIoCompletionClockGateProjection.Project(Capture(
                freshGlobalPointer: null,
                entryFlags: entryFlags));
        check("A missing fresh global read preserves mixed incoming flags and records an uncommitted attempt",
            missingGlobal.Outcome
                == NetworkIoCompletionClockGateOutcome.MissingFreshGlobalPointer
            && missingGlobal.Reads is [{ RawFileOffset: 0x00BBC8B0,
                Kind: NetworkIoCompletionClockGateReadKind.FreshGlobalPointer,
                Address: 0x01DFFF70, Attempted: true, Captured: false,
                Value: null, Committed: false }]
            && missingGlobal.Writes.IsEmpty
            && missingGlobal.FencesRecorded.IsEmpty
            && missingGlobal.FlagsAtStop == entryFlags
            && missingGlobal.UnreachedEntryEBPCaptureProvided
            && missingGlobal.UnreachedFreshClockFieldCaptureProvided);

        NetworkIoCompletionClockGatePlan missingEBP =
            NetworkIoCompletionClockGateProjection.Project(Capture(
                entryEBP: null,
                entryFlags: entryFlags));
        check("Missing EBP stops before the save at BB after the global read and threshold load",
            missingEBP.Outcome == NetworkIoCompletionClockGateOutcome.MissingEntryEBP
            && missingEBP.ThresholdLoaded
            && missingEBP.Threshold == 500
            && missingEBP.Reads.Length == 1
            && missingEBP.Reads[0].Kind
                == NetworkIoCompletionClockGateReadKind.FreshGlobalPointer
            && missingEBP.Writes.IsEmpty
            && missingEBP.FencesRecorded.IsEmpty
            && missingEBP.SavedPointerAddress is null
            && missingEBP.UnreachedFreshSavedPointerCaptureProvided
            && missingEBP.FlagsAtStop == entryFlags
            && missingEBP.StopAtRawFileOffsetExclusive
                == NetworkIoCompletionClockGateProjection.SavePointerRawFileOffset);

        NetworkIoCompletionClockGatePlan at499 =
            NetworkIoCompletionClockGateProjection.Project(Capture(
                freshClockField: 500,
                clockReturnEAX: 999,
                freshSavedPointerRead: 0x2222,
                dispatcherReturnedNormally: true));
        NetworkIoCompletionClockGatePlan exact499 =
            NetworkIoCompletionClockGateProjection.Project(Capture(
                freshClockField: 500,
                clockReturnEAX: 999,
                dispatcherReturnedNormally: null));
        check("Unsigned elapsed 499 selects bounded completion despite unused dispatcher evidence",
            at499.Outcome == NetworkIoCompletionClockGateOutcome.BelowThreshold
            && at499.SubtractionResult == 499
            && at499.CompareLeft == 499
            && at499.CompareRight == 500
            && at499.CompareResult == uint.MaxValue
            && at499.BelowBranchReached
            && at499.BelowBranchTaken == true
            && at499.BoundedBranchSelected
            && at499.SelectedDestinationRawFileOffset
                == NetworkIoCompletionClockGateProjection.BoundedContinuationRawFileOffset
            && at499.BoundedCompletionBoundaryReached
            && !at499.BoundedContinuationEntered
            && !at499.DispatcherCallReached
            && at499.DispatcherReturnStatusProvidedButUnused
            && at499.UnreachedDispatcherReturnStatusProvided
            && !at499.ExactCleanExitAdmitted
            && exact499.BoundedCompletionBoundaryReached
            && exact499.ExactCleanExitAdmitted
            && at499.FlagsAtStop == at499.CompareFlags
            && at499.StopAtRawFileOffsetExclusive
                == NetworkIoCompletionClockGateProjection.BoundedContinuationRawFileOffset);

        NetworkIoCompletionClockGatePlan at500 =
            NetworkIoCompletionClockGateProjection.Project(Capture(
                freshClockField: 500,
                clockReturnEAX: 1000,
                freshSavedPointerRead: 0x2222));
        check("Elapsed 500 takes the fallthrough dispatcher branch and completes at DB only after normal return",
            at500.Outcome
                == NetworkIoCompletionClockGateOutcome.DispatcherReturnedNormally
            && at500.SubtractionResult == 500
            && at500.BelowBranchTaken == false
            && at500.SelectedDestinationRawFileOffset
                == NetworkIoCompletionClockGateProjection.DispatcherPushRawFileOffset
            && at500.Pushes is [{ RawFileOffset: 0x00BBC8D2,
                StackAddress: null, Value: 1, StackAddressKnown: false,
                Attempted: true, Committed: true }]
            && at500.Writes[^1] is {
                Kind: NetworkIoCompletionClockGateWriteKind.DispatcherPush,
                Address: null, Value: 1, AddressKnown: false,
                Attempted: true, Committed: true
            }
            && at500.DispatcherArgumentReached
            && at500.DispatcherArgumentECX == 0x2222
            && at500.DispatcherTargetVirtualAddress == 0x00FBE9B0
            && at500.DispatcherReturnedNormally == true
            && at500.FlagsAtStop is null
            && at500.SubtractionFlags is not null
            && at500.CompareFlags is not null
            && at500.BoundedCompletionBoundaryReached
            && at500.StopAtRawFileOffsetExclusive
                == NetworkIoCompletionClockGateProjection.BoundedContinuationRawFileOffset
            && at500.Reads.All(read => !read.Committed)
            && at500.ExactCleanExitAdmitted);

        NetworkIoCompletionClockGatePlan at501 =
            NetworkIoCompletionClockGateProjection.Project(Capture(
                freshClockField: 500,
                clockReturnEAX: 1001,
                dispatcherReturnedNormally: false));
        check("Elapsed 501 takes the dispatcher path and a nonnormal dispatcher status preserves CMP flags",
            at501.Outcome
                == NetworkIoCompletionClockGateOutcome.DispatcherDidNotReturnNormally
            && at501.SubtractionResult == 501
            && at501.BelowBranchTaken == false
            && at501.DispatcherCallReached
            && at501.DispatcherReturnedNormally == false
            && at501.FlagsAtStop == at501.CompareFlags
            && at501.SubtractionFlags is not null
            && at501.CompareFlags is not null
            && !at501.BoundedCompletionBoundaryReached);

        NetworkIoCompletionClockGatePlan wrappedSubtraction =
            NetworkIoCompletionClockGateProjection.Project(Capture(
                freshClockField: 0xFFFFFFF0,
                clockReturnEAX: 0x10));
        check("SUB arithmetic wraps as a DWORD before the unsigned threshold comparison",
            NetworkIoCompletionClockGateProjection.Subtract(0x10, 0xFFFFFFF0) == 0x20
            && wrappedSubtraction.SubtractionResult == 0x20
            && wrappedSubtraction.BelowBranchTaken == true
            && wrappedSubtraction.CompareLeft == 0x20);

        NetworkIoCompletionClockGatePlan wrappedFieldAndStackAddresses =
            NetworkIoCompletionClockGateProjection.Project(Capture(
                entryEBP: 8,
                freshGlobalPointer: 0xFFFFFFF0,
                freshClockField: 0xFFFFFFFE,
                clockReturnEAX: 0xFFFFFFFF,
                freshSavedPointerRead: 0));
        check("Fresh field and saved-stack addresses wrap independently without forwarding stored pointer values",
            NetworkIoCompletionClockGateProjection.FreshClockFieldAddress(0xFFFFFFF0) == 0x58
            && NetworkIoCompletionClockGateProjection.SavedPointerAddress(8) == 0xFFFFFFF8
            && wrappedFieldAndStackAddresses.FreshClockFieldAddress == 0x58
            && wrappedFieldAndStackAddresses.SavedPointerAddress == 0xFFFFFFF8
            && wrappedFieldAndStackAddresses.Reads.Length == 3
            && wrappedFieldAndStackAddresses.Reads.Select(read => read.Kind).SequenceEqual(new[]
            {
                NetworkIoCompletionClockGateReadKind.FreshGlobalPointer,
                NetworkIoCompletionClockGateReadKind.FreshClockField,
                NetworkIoCompletionClockGateReadKind.FreshSavedPointer
            })
            && wrappedFieldAndStackAddresses.Reads.Select(read => read.Committed).All(value => !value)
            && wrappedFieldAndStackAddresses.Reads[1].Address == 0x58
            && wrappedFieldAndStackAddresses.Reads[2].Address == 0xFFFFFFF8
            && wrappedFieldAndStackAddresses.SavedPointerValue == 0xFFFFFFF0
            && wrappedFieldAndStackAddresses.RestoredPointer == 0
            && wrappedFieldAndStackAddresses.SubtractionResult == 1
            && wrappedFieldAndStackAddresses.BelowBranchTaken == true
            && wrappedFieldAndStackAddresses.Writes[0] is {
                Address: 0xFFFFFFF8, Value: 0xFFFFFFF0,
                AddressKnown: true, Attempted: true, Committed: true
            });

        NetworkIoCompletionClockGatePlan zeroValues =
            NetworkIoCompletionClockGateProjection.Project(Capture(
                entryEBP: 0,
                freshGlobalPointer: 0,
                freshClockField: 0,
                clockReturnEAX: 0,
                freshSavedPointerRead: 0));
        check("Zero global, EBP, field, and restored-pointer values remain captured values rather than missing inputs",
            zeroValues.Reads.Length == 3
            && zeroValues.Reads.All(read => read.Captured && read.Value == 0)
            && zeroValues.Reads[1].Address == 0x68
            && zeroValues.Reads[2].Address == 0xFFFFFFF0
            && zeroValues.SavedPointerAddress == 0xFFFFFFF0
            && zeroValues.SavedPointerValue == 0
            && zeroValues.RestoredPointer == 0
            && zeroValues.Outcome == NetworkIoCompletionClockGateOutcome.BelowThreshold);

        NetworkIoCompletionClockGatePlan missingField =
            NetworkIoCompletionClockGateProjection.Project(Capture(
                freshClockField: null,
                entryFlags: entryFlags));
        check("A missing field read is attempted at its known address with its capture uncommitted",
            missingField.Outcome == NetworkIoCompletionClockGateOutcome.MissingFreshClockField
            && missingField.Reads.Length == 2
            && missingField.Reads[1] is {
                RawFileOffset: 0x00BBC8C1,
                Address: 0x1068,
                Attempted: true,
                Captured: false,
                Value: null,
                Committed: false
            }
            && missingField.Writes is [{ RawFileOffset: 0x00BBC8BB,
                Address: 0x1FF0, Value: 0x1000,
                AddressKnown: true, Attempted: true, Committed: true }]
            && missingField.FencesRecorded is [{ RawFileOffset: 0x00BBC8BE }]
            && missingField.FlagsAtStop == entryFlags
            && !missingField.ClockCallReached
            && missingField.StopAtRawFileOffsetExclusive
                == NetworkIoCompletionClockGateProjection.FreshClockFieldRawFileOffset);

        NetworkIoCompletionClockGatePlan missingTarget =
            NetworkIoCompletionClockGateProjection.Project(Capture(
                cachedClockTargetVirtualAddress: null));
        check("A missing cached EAX target stops before CALL without rereading the clock IAT",
            missingTarget.Outcome
                == NetworkIoCompletionClockGateOutcome.MissingCachedClockTarget
            && !missingTarget.ClockCallReached
            && missingTarget.Reads.Select(read => read.Kind).SequenceEqual(new[]
            {
                NetworkIoCompletionClockGateReadKind.FreshGlobalPointer,
                NetworkIoCompletionClockGateReadKind.FreshClockField
            })
            && missingTarget.UnreachedClockReturnStatusProvided
            && missingTarget.StopAtRawFileOffsetExclusive
                == NetworkIoCompletionClockGateProjection.ClockCallRawFileOffset);

        NetworkIoCompletionClockGatePlan unknownClockReturn =
            NetworkIoCompletionClockGateProjection.Project(Capture(
                clockReturnedNormally: null,
                entryFlags: entryFlags));
        NetworkIoCompletionClockGatePlan clockDidNotReturn =
            NetworkIoCompletionClockGateProjection.Project(Capture(
                clockReturnedNormally: false,
                entryFlags: entryFlags));
        check("Unknown and nonreturning clock status preserve the captured incoming flags and stop before SUB",
            unknownClockReturn.Outcome
                == NetworkIoCompletionClockGateOutcome.ClockReturnStatusUnknown
            && unknownClockReturn.ClockCallReached
            && unknownClockReturn.ClockReturnedNormally is null
            && unknownClockReturn.UnreachedClockReturnEAXCaptureProvided
            && unknownClockReturn.FlagsAtStop == entryFlags
            && clockDidNotReturn.Outcome
                == NetworkIoCompletionClockGateOutcome.ClockDidNotReturnNormally
            && clockDidNotReturn.ClockReturnedNormally == false
            && clockDidNotReturn.UnreachedClockReturnEAXCaptureProvided
            && clockDidNotReturn.FlagsAtStop == entryFlags
            && !clockDidNotReturn.SubtractionReached);

        NetworkIoCompletionClockGatePlan missingClockReturnValue =
            NetworkIoCompletionClockGateProjection.Project(Capture(
                clockReturnEAX: null,
                entryFlags: entryFlags));
        check("A normal clock return invalidates incoming flags before a missing-EAX SUB stop",
            missingClockReturnValue.Outcome
                == NetworkIoCompletionClockGateOutcome.MissingClockReturnEAX
            && missingClockReturnValue.ClockReturnedNormally == true
            && missingClockReturnValue.ClockCallInvalidatedArithmeticFlags
            && missingClockReturnValue.SubtractionFlags is null
            && missingClockReturnValue.FlagsAtStop is null
            && missingClockReturnValue.StopAtRawFileOffsetExclusive
                == NetworkIoCompletionClockGateProjection.SubtractRawFileOffset);

        NetworkIoCompletionClockGatePlan missingRestoredPointer =
            NetworkIoCompletionClockGateProjection.Project(Capture(
                freshSavedPointerRead: null));
        check("A missing fresh stack read at CB stops after SUB without entering CMP",
            missingRestoredPointer.Outcome
                == NetworkIoCompletionClockGateOutcome.MissingFreshSavedPointer
            && missingRestoredPointer.Reads[^1] is {
                RawFileOffset: 0x00BBC8CB,
                Address: 0x1FF0,
                Attempted: true,
                Captured: false,
                Value: null,
                Committed: false
            }
            && missingRestoredPointer.SubtractionReached
            && !missingRestoredPointer.CompareReached
            && missingRestoredPointer.FlagsAtStop
                == missingRestoredPointer.SubtractionFlags
            && missingRestoredPointer.StopAtRawFileOffsetExclusive
                == NetworkIoCompletionClockGateProjection.RestorePointerRawFileOffset);

        NetworkIoCompletionClockGatePlan unknownDispatcher =
            NetworkIoCompletionClockGateProjection.Project(Capture(
                freshClockField: 500,
                clockReturnEAX: 1000,
                freshSavedPointerRead: 0xAABBCCDD,
                dispatcherReturnedNormally: null));
        NetworkIoCompletionClockGatePlan dispatcherDidNotReturn =
            NetworkIoCompletionClockGateProjection.Project(Capture(
                freshClockField: 500,
                clockReturnEAX: 1000,
                dispatcherReturnedNormally: false));
        check("Pending and nonnormal dispatcher outcomes preserve CMP flags; only normal return invalidates them",
            unknownDispatcher.Outcome
                == NetworkIoCompletionClockGateOutcome.DispatcherReturnStatusUnknown
            && unknownDispatcher.DispatcherArgumentECX == 0xAABBCCDD
            && unknownDispatcher.DispatcherCallReached
            && unknownDispatcher.DispatcherReturnedNormally is null
            && unknownDispatcher.FlagsAtStop == unknownDispatcher.CompareFlags
            && dispatcherDidNotReturn.Outcome
                == NetworkIoCompletionClockGateOutcome.DispatcherDidNotReturnNormally
            && dispatcherDidNotReturn.DispatcherReturnedNormally == false
            && dispatcherDidNotReturn.FlagsAtStop == dispatcherDidNotReturn.CompareFlags
            && at500.DispatcherReturnedNormally == true
            && at500.FlagsAtStop is null);

        NetworkIoCompletionClockGatePlan allArithmeticFlags =
            NetworkIoCompletionClockGateProjection.Project(Capture(
                freshClockField: 1,
                clockReturnEAX: 0x80000000,
                dispatcherReturnedNormally: null));
        check("SUB and CMP preserve all six independently computed arithmetic flags",
            allArithmeticFlags.SubtractionResult == 0x7FFFFFFF
            && allArithmeticFlags.SubtractionFlags == new NetworkIoCompletionClockGateFlags(
                Carry: false,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true)
            && allArithmeticFlags.CompareResult == 0x7FFFFE0B
            && allArithmeticFlags.CompareFlags == new NetworkIoCompletionClockGateFlags(
                Carry: false,
                Parity: false,
                Auxiliary: false,
                Zero: false,
                Sign: false,
                Overflow: false)
            && allArithmeticFlags.FlagsAtStop == allArithmeticFlags.CompareFlags);
    }

    private static NetworkIoCompletionClockGateCapture Capture(
        bool entryValidated = true,
        uint? entryEBP = 0x2000,
        NetworkIoCompletionClockGateFlags? entryFlags = null,
        uint? freshGlobalPointer = 0x1000,
        uint? freshClockField = 500,
        uint? cachedClockTargetVirtualAddress = 0x12345678,
        bool? clockReturnedNormally = true,
        uint? clockReturnEAX = 1000,
        uint? freshSavedPointerRead = 0x1000,
        bool? dispatcherReturnedNormally = true) =>
        new(
            entryValidated,
            entryEBP,
            entryFlags,
            freshGlobalPointer,
            freshClockField,
            cachedClockTargetVirtualAddress,
            clockReturnedNormally,
            clockReturnEAX,
            freshSavedPointerRead,
            dispatcherReturnedNormally);

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Where(Uri.IsHexDigit)).ToUpperInvariant();
}
