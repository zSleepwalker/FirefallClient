using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherEntryFsPublicationChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static NetworkDispatcherEntryFsPublicationPlan Project(
            uint? cookie = 0x12345678,
            uint? entryESP = 0x2000,
            uint? entryEBP = 0x1234,
            uint? entryEBX = 0x11111111,
            uint? entryESI = 0x22222222,
            uint? entryEDI = 0x33333333,
            uint? oldFSOffset0 = 0xAABBCCDD,
            uint? freshClockField = 500,
            uint? clockReturnEAX = 1000,
            bool forgeLegacyParentPlan = false)
        {
            var callerCapture = new NetworkIoCompletionClockGateCapture(
                EntryValidated: true,
                EntryEBP: 0x2000,
                EntryFlags: null,
                FreshGlobalPointer: 0x1000,
                FreshClockField: freshClockField,
                CachedClockTargetVirtualAddress: 0x12345678,
                ClockReturnedNormally: true,
                ClockReturnEAX: clockReturnEAX,
                FreshSavedPointerRead: 0x2222,
                DispatcherReturnedNormally: null);
            var frameCapture = new NetworkDispatcherEntryFramePrefixCapture(
                callerCapture,
                entryESP,
                entryEBP,
                new NetworkIoCompletionClockGateFlags(
                    Carry: true,
                    Parity: false,
                    Auxiliary: true,
                    Zero: false,
                    Sign: true,
                    Overflow: false),
                oldFSOffset0);
            NetworkDispatcherEntryFramePrefixPlan framePlan =
                NetworkDispatcherEntryFramePrefixProjection.Project(frameCapture);
            if (forgeLegacyParentPlan)
                framePlan = framePlan with
                {
                    StopAtRawFileOffsetExclusive = 0x00BBDDC3
                };

            var registerCapture = new NetworkDispatcherEntryRegisterSaveCapture(
                frameCapture,
                framePlan,
                entryEBX,
                entryESI,
                entryEDI);
            return NetworkDispatcherEntryFsPublicationProjection.Project(new(
                new NetworkDispatcherEntryCookiePreparationCapture(
                    registerCapture,
                    cookie)));
        }

        static string Normalize(string value) =>
            string.Concat(value.Where(Uri.IsHexDigit)).ToUpperInvariant();

        check("The FS publication window is bounded after cookie preparation and before MOV ESI",
            NetworkDispatcherEntryFsPublicationProjection.EntryRawFileOffset
                == 0x00BBDDCF
            && NetworkDispatcherEntryFsPublicationProjection.AddressCalculationRawFileOffset
                == 0x00BBDDCF
            && NetworkDispatcherEntryFsPublicationProjection.PublishFSOffset0RawFileOffset
                == 0x00BBDDD2
            && NetworkDispatcherEntryFsPublicationProjection.PrefixEndRawFileOffsetExclusive
                == 0x00BBDDD8
            && NetworkDispatcherEntryFsPublicationProjection.LocalAllocationDisplacement
                == 0x0C
            && NetworkDispatcherEntryFsPublicationProjection.FSChainHeadSegmentOffset == 0
            && Normalize(NetworkDispatcherEntryFsPublicationProjection.CapturedWindowBytes)
                == "8D45F464A3000000008BF1"
            && !NetworkDispatcherEntryFsPublicationProjection.ExecutesNativeInstructions);

        NetworkDispatcherEntryFsPublicationPlan complete = Project();
        NetworkDispatcherEntryCookiePreparationPlan cookieParent =
            complete.ParentCookiePreparationPlan;
        check("LEA precedes the logical FS:[0] write while preserving the prepared ESP and flags",
            complete.ParentCookiePreparationPlan.ExactCookiePreparationProjected
            && complete.Outcome
                == NetworkDispatcherEntryFsPublicationOutcome.FsPublicationProjected
            && complete.CompletedInstructionOffsets.SequenceEqual(new uint[]
            {
                0x00BBDDCF,
                0x00BBDDD2
            })
            && complete.AddressCalculation is
            {
                RawFileOffset: 0x00BBDDCF,
                BaseEBP: 0x1FFC,
                Displacement: 0x0C,
                ResultEAX: 0x1FF0,
                Attempted: true,
                Completed: true
            }
            && complete.PublicationWrite is
            {
                RawFileOffset: 0x00BBDDD2,
                SegmentRegister: NetworkDispatcherEntryFramePrefixSegmentRegister.FS,
                SegmentOffset: 0,
                Value: 0x1FF0,
                Attempted: true,
                Committed: true
            }
            && complete.EntryEAX == complete.PublicationWrite?.Value
            && complete.ESPBefore == cookieParent.CurrentESPAtStop
            && complete.CurrentESPAtStop == cookieParent.CurrentESPAtStop
            && complete.StackPointerUnchanged
            && complete.FlagsBefore == cookieParent.FlagsAtStop
            && complete.FlagsAtStop == cookieParent.FlagsAtStop
            && complete.FlagsUnchanged
            && complete.ExactFsPublicationProjected
            && complete.StopAtRawFileOffsetExclusive == 0x00BBDDD8
            && !complete.NativeInstructionsExecuted
            && !complete.ExceptionABIInferred
            && !complete.LinearAddressInterpretationMade);

        NetworkDispatcherEntryFsPublicationPlan wrappedZeroAddress = Project(
            cookie: 0,
            entryESP: 0x10,
            entryEBP: uint.MaxValue);
        check("The LEA address wraps as a DWORD to segment offset zero without changing ESP or flags",
            wrappedZeroAddress.EntryEBP == 0x0C
            && wrappedZeroAddress.AddressCalculation?.ResultEAX == 0
            && wrappedZeroAddress.EntryEAX == 0
            && wrappedZeroAddress.PublicationWrite is
            {
                SegmentRegister: NetworkDispatcherEntryFramePrefixSegmentRegister.FS,
                SegmentOffset: 0,
                Value: 0,
                Committed: true
            }
            && wrappedZeroAddress.StackPointerUnchanged
            && wrappedZeroAddress.FlagsUnchanged
            && wrappedZeroAddress.FlagsAtStop?.Carry == false);

        NetworkDispatcherEntryFsPublicationPlan oldValueIndependent = Project(
            cookie: 0xAABBCCDD,
            entryESP: 0x01DC5028,
            entryEDI: 0xDEADBEEF,
            oldFSOffset0: 0xCAFEBABE);
        check("The earlier FS segment-offset read remains a distinct value and is not reconciled by publication",
            oldValueIndependent.PreviousFSOffset0Read is
            {
                RawFileOffset: 0x00BBDDBA,
                SegmentRegister: NetworkDispatcherEntryFramePrefixSegmentRegister.FS,
                SegmentOffset: 0,
                Captured: true,
                Value: 0xCAFEBABE,
                Committed: false
            }
            && oldValueIndependent.ParentCookiePreparationPlan.ParentRegisterSavePlan.Pushes[^1]
                is { StackAddress: 0x01DC5000, Value: 0xDEADBEEF }
            && oldValueIndependent.ParentCookiePreparationPlan.FreshCookieRead is
            {
                Address: 0x01DC5000,
                Value: 0xAABBCCDD,
                Completed: true
            }
            && oldValueIndependent.PublicationWrite?.Value == 0x01DC5018
            && oldValueIndependent.PreviousFSOffset0Read?.Value !=
                oldValueIndependent.PublicationWrite?.Value
            && oldValueIndependent.LinearAddressInterpretationMade == false);

        NetworkDispatcherEntryFsPublicationPlan missingCookie = Project(cookie: null);
        NetworkDispatcherEntryFsPublicationPlan missingESI = Project(entryESI: null);
        NetworkDispatcherEntryFsPublicationPlan belowThreshold = Project(
            freshClockField: 500,
            clockReturnEAX: 999);
        check("A missing parent read or register and a rejected caller stop at their actual endpoints",
            missingCookie.Outcome
                == NetworkDispatcherEntryFsPublicationOutcome.ParentCookiePreparationNotAccepted
            && missingCookie.ParentCookiePreparationPlan.Outcome
                == NetworkDispatcherEntryCookiePreparationOutcome.MissingFreshCookieGlobalDWORD
            && missingCookie.StopAtRawFileOffsetExclusive == 0x00BBDDC7
            && missingCookie.AddressCalculation is null
            && missingCookie.PublicationWrite is null
            && missingESI.Outcome
                == NetworkDispatcherEntryFsPublicationOutcome.ParentCookiePreparationNotAccepted
            && missingESI.ParentCookiePreparationPlan.ParentRegisterSavePlan.Outcome
                == NetworkDispatcherEntryRegisterSaveOutcome.MissingFreshEntryESI
            && missingESI.StopAtRawFileOffsetExclusive == 0x00BBDDC5
            && missingESI.AddressCalculation is null
            && missingESI.PublicationWrite is null
            && belowThreshold.ParentCookiePreparationPlan.ParentRegisterSavePlan.ParentPlan.CallerClockGatePlan.Outcome
                == NetworkIoCompletionClockGateOutcome.BelowThreshold
            && belowThreshold.Outcome
                == NetworkDispatcherEntryFsPublicationOutcome.ParentCookiePreparationNotAccepted
            && belowThreshold.StopAtRawFileOffsetExclusive
                == NetworkIoCompletionClockGateProjection.BoundedContinuationRawFileOffset
            && belowThreshold.CompletedInstructionOffsets.IsEmpty
            && belowThreshold.PublicationWrite is null);

        NetworkDispatcherEntryFsPublicationPlan reprojectedForgery = Project(
            forgeLegacyParentPlan: true);
        check("A caller-supplied parent plan cannot forge or reject the immutable capture reprojection",
            complete.ParentCookiePreparationPlan.ParentRegisterSavePlan.ParentPlanMatchesCapture
            && reprojectedForgery.ParentCookiePreparationPlan.ParentRegisterSavePlan.ParentPlanMatchesCapture
            && complete.ParentCookiePreparationPlan.CompletedInstructionOffsets.SequenceEqual(
                reprojectedForgery.ParentCookiePreparationPlan.CompletedInstructionOffsets)
            && complete.ParentCookiePreparationPlan.CookieXor
                == reprojectedForgery.ParentCookiePreparationPlan.CookieXor
            && complete.ParentCookiePreparationPlan.CookiePush
                == reprojectedForgery.ParentCookiePreparationPlan.CookiePush
            && complete.Outcome == reprojectedForgery.Outcome
            && complete.AddressCalculation == reprojectedForgery.AddressCalculation
            && complete.PublicationWrite == reprojectedForgery.PublicationWrite
            && complete.FlagsAtStop == reprojectedForgery.FlagsAtStop
            && complete.StopAtRawFileOffsetExclusive
                == reprojectedForgery.StopAtRawFileOffsetExclusive);
    }
}
