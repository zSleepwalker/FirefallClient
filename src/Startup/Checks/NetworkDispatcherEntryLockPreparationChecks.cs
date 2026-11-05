using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherEntryLockPreparationChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static NetworkDispatcherEntryLockPreparationPlan Project(
            uint? entryECX = 0x123,
            uint? entryESP = 0x2000,
            uint? entryEBP = 0x1234,
            uint? entryEBX = 0x11111111,
            uint? entryESI = 0x22222222,
            uint? entryEDI = 0x33333333,
            uint? oldFSOffset0 = 0xAABBCCDD,
            uint? cookie = 0,
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
            var cookieCapture = new NetworkDispatcherEntryCookiePreparationCapture(
                registerCapture,
                cookie);
            return NetworkDispatcherEntryLockPreparationProjection.Project(new(
                new NetworkDispatcherEntryFsPublicationCapture(cookieCapture),
                entryECX));
        }

        static string Normalize(string value) =>
            string.Concat(value.Where(Uri.IsHexDigit)).ToUpperInvariant();

        check("The lock-preparation projection is bounded before the opaque import call",
            NetworkDispatcherEntryLockPreparationProjection.EntryRawFileOffset
                == 0x00BBDDD8
            && NetworkDispatcherEntryLockPreparationProjection.MoveEntryECXToESIRawFileOffset
                == 0x00BBDDD8
            && NetworkDispatcherEntryLockPreparationProjection.CalculateEntryESIPlusDisplacementToEBXRawFileOffset
                == 0x00BBDDDA
            && NetworkDispatcherEntryLockPreparationProjection.WriteByteLocalOneRawFileOffset
                == 0x00BBDDDD
            && NetworkDispatcherEntryLockPreparationProjection.PushEBXRawFileOffset
                == 0x00BBDDE1
            && NetworkDispatcherEntryLockPreparationProjection.WriteDwordLocalEBXRawFileOffset
                == 0x00BBDDE2
            && NetworkDispatcherEntryLockPreparationProjection.OpaqueImportCallRawFileOffset
                == 0x00BBDDE5
            && NetworkDispatcherEntryLockPreparationProjection.PrefixEndRawFileOffsetExclusive
                == 0x00BBDDE5
            && NetworkDispatcherEntryLockPreparationProjection.EBXDisplacement == 0x6C
            && NetworkDispatcherEntryLockPreparationProjection.ByteLocalDisplacement == 0x14
            && NetworkDispatcherEntryLockPreparationProjection.DwordLocalDisplacement == 0x18
            && Normalize(NetworkDispatcherEntryLockPreparationProjection.CapturedWindowBytes)
                == "8BF18D5E6CC645EC0153895DE8FF158442AC01"
            && !NetworkDispatcherEntryLockPreparationProjection.ExecutesNativeInstructions);

        NetworkDispatcherEntryLockPreparationPlan complete = Project();
        NetworkDispatcherEntryFsPublicationPlan parent = complete.ParentFsPublicationPlan;
        check("Fresh ECX is moved to ESI before LEA, the local byte write, push, and DWORD write",
            parent.ExactFsPublicationProjected
            && complete.Outcome
                == NetworkDispatcherEntryLockPreparationOutcome.LockPreparationProjected
            && complete.FreshEntryECXCaptureState
                == NetworkDispatcherEntryLockPreparationCaptureState.Captured
            && complete.EntryECX == 0x123
            && complete.InitialESI == 0x22222222
            && complete.CurrentESIAtStop == 0x123
            && complete.InitialEBX == 0x11111111
            && complete.CurrentEBXAtStop == 0x18F
            && complete.EntryEBP == 0x1FFC
            && complete.CompletedInstructionOffsets.SequenceEqual(new uint[]
            {
                0x00BBDDD8,
                0x00BBDDDA,
                0x00BBDDDD,
                0x00BBDDE1,
                0x00BBDDE2
            })
            && complete.InstructionOrder.Select(instruction => instruction.Kind).SequenceEqual(
                new[]
                {
                    NetworkDispatcherEntryLockPreparationInstructionKind.MoveEntryECXToESI,
                    NetworkDispatcherEntryLockPreparationInstructionKind.CalculateEntryESIPlusDisplacementToEBX,
                    NetworkDispatcherEntryLockPreparationInstructionKind.WriteByteLocalOne,
                    NetworkDispatcherEntryLockPreparationInstructionKind.PushEBX,
                    NetworkDispatcherEntryLockPreparationInstructionKind.WriteDwordLocalEBX
                })
            && complete.MoveECXToESI is
            {
                RawFileOffset: 0x00BBDDD8,
                SourceECX: 0x123,
                ResultESI: 0x123,
                Attempted: true,
                Completed: true
            }
            && complete.AddressCalculation is
            {
                RawFileOffset: 0x00BBDDDA,
                BaseESI: 0x123,
                Displacement: 0x6C,
                ResultEBX: 0x18F,
                Attempted: true,
                Completed: true
            }
            && complete.ByteWrite is
            {
                RawFileOffset: 0x00BBDDDD,
                BaseEBP: 0x1FFC,
                Displacement: 0x14,
                Address: 0x1FE8,
                Width: 1,
                Value: 1,
                Attempted: true,
                Committed: true
            }
            && complete.EBXPush is
            {
                RawFileOffset: 0x00BBDDE1,
                StackPointerBefore: not null,
                StackPointerAfter: not null,
                Value: 0x18F,
                Attempted: true,
                Committed: true
            }
            && complete.ESPBefore == parent.CurrentESPAtStop
            && complete.EBXPush?.StackPointerAfter
                == NetworkDispatcherEntryLockPreparationProjection.StackAddressAfterPush(
                    complete.ESPBefore!.Value)
            && complete.CurrentESPAtStop == complete.EBXPush?.StackPointerAfter
            && complete.StackPointerChangedByEBXPush
            && complete.DwordWrite is
            {
                RawFileOffset: 0x00BBDDE2,
                BaseEBP: 0x1FFC,
                Displacement: 0x18,
                Address: 0x1FE4,
                Width: 4,
                Value: 0x18F,
                Attempted: true,
                Committed: true
            }
            && complete.FlagsBefore == parent.FlagsAtStop
            && complete.FlagsAtStop == parent.FlagsAtStop
            && complete.FlagsUnchanged
            && complete.ExactLockPreparationProjected
            && complete.StopAtRawFileOffsetExclusive == 0x00BBDDE5
            && !complete.NativeInstructionsExecuted
            && !complete.NativeMemoryWritesExecuted
            && !complete.MemoryWriteAliasingReconciled
            && !complete.OpaqueImportCallReached
            && !complete.CallerECXForwardedToOpaqueImport
            && !complete.OpaqueImportSemanticsInferred);

        NetworkDispatcherEntryLockPreparationPlan zeroECX = Project(entryECX: 0);
        NetworkDispatcherEntryLockPreparationPlan maximumECX =
            Project(entryECX: uint.MaxValue);
        check("LEA uses wrapping DWORD addition for zero and maximum fresh ECX values",
            zeroECX.AddressCalculation?.ResultEBX == 0x6C
            && zeroECX.CurrentEBXAtStop == 0x6C
            && zeroECX.EBXPush?.Value == 0x6C
            && zeroECX.DwordWrite?.Value == 0x6C
            && maximumECX.AddressCalculation?.ResultEBX == 0x6B
            && maximumECX.CurrentEBXAtStop == 0x6B
            && maximumECX.EBXPush?.Value == 0x6B
            && maximumECX.DwordWrite?.Value == 0x6B);

        NetworkDispatcherEntryLockPreparationPlan wrappedLocals = Project(entryESP: 0x14);
        NetworkDispatcherEntryLockPreparationPlan wrappedPush = Project(entryESP: 0x2C);
        check("Wrapped local and push addresses retain their independent widths and values",
            wrappedLocals.EntryEBP == 0x10
            && wrappedLocals.ByteWrite is
            {
                Address: 0xFFFFFFFC,
                Width: 1,
                Value: 1,
                Committed: true
            }
            && wrappedLocals.DwordWrite is
            {
                Address: 0xFFFFFFF8,
                Width: 4,
                Value: 0x18F,
                Committed: true
            }
            && wrappedLocals.EBXPush is
            {
                StackPointerBefore: 0xFFFFFFE8,
                StackPointerAfter: 0xFFFFFFE4,
                Value: 0x18F,
                Committed: true
            }
            && wrappedPush.EntryEBP == 0x28
            && wrappedPush.EBXPush is
            {
                StackPointerBefore: 0,
                StackPointerAfter: 0xFFFFFFFC,
                Value: 0x18F,
                Committed: true
            }
            && wrappedPush.ByteWrite?.Address == 0x14
            && wrappedPush.DwordWrite?.Address == 0x10
            && !wrappedLocals.MemoryWriteAliasingReconciled
            && !wrappedPush.MemoryWriteAliasingReconciled);

        NetworkDispatcherEntryLockPreparationPlan missingECX = Project(entryECX: null);
        check("Missing fresh ECX stops at MOV without changing the parent ESP, EBP, flags, or later effects",
            missingECX.ParentFsPublicationPlan.ExactFsPublicationProjected
            && missingECX.Outcome
                == NetworkDispatcherEntryLockPreparationOutcome.MissingFreshEntryECX
            && missingECX.FreshEntryECXCaptureState
                == NetworkDispatcherEntryLockPreparationCaptureState.Missing
            && missingECX.MoveECXToESI is
            {
                RawFileOffset: 0x00BBDDD8,
                SourceECX: null,
                ResultESI: null,
                Attempted: true,
                Completed: false
            }
            && missingECX.CompletedInstructionOffsets.IsEmpty
            && missingECX.InstructionOrder.IsEmpty
            && missingECX.AddressCalculation is null
            && missingECX.ByteWrite is null
            && missingECX.EBXPush is null
            && missingECX.DwordWrite is null
            && missingECX.EntryECX is null
            && missingECX.CurrentESIAtStop == missingECX.InitialESI
            && missingECX.CurrentEBXAtStop == missingECX.InitialEBX
            && missingECX.EntryEBP == missingECX.ParentFsPublicationPlan.EntryEBP
            && missingECX.ESPBefore == missingECX.ParentFsPublicationPlan.CurrentESPAtStop
            && missingECX.CurrentESPAtStop == missingECX.ESPBefore
            && missingECX.StackPointerUnchangedBeforeMissingECX
            && missingECX.FlagsBefore == missingECX.ParentFsPublicationPlan.FlagsAtStop
            && missingECX.FlagsAtStop == missingECX.FlagsBefore
            && missingECX.FlagsUnchanged
            && missingECX.StopAtRawFileOffsetExclusive == 0x00BBDDD8);

        NetworkDispatcherEntryLockPreparationPlan missingParentRegister =
            Project(entryESI: null, entryECX: 0x456);
        NetworkDispatcherEntryLockPreparationPlan rejectedCaller = Project(
            entryECX: 0x456,
            freshClockField: 500,
            clockReturnEAX: 999);
        check("Rejected parents preserve their own endpoints and mark supplied ECX unused",
            missingParentRegister.Outcome
                == NetworkDispatcherEntryLockPreparationOutcome.ParentFsPublicationNotAccepted
            && missingParentRegister.ParentFsPublicationPlan.ParentCookiePreparationPlan
                .ParentRegisterSavePlan.Outcome
                == NetworkDispatcherEntryRegisterSaveOutcome.MissingFreshEntryESI
            && missingParentRegister.StopAtRawFileOffsetExclusive == 0x00BBDDC5
            && missingParentRegister.FreshEntryECXCaptureState
                == NetworkDispatcherEntryLockPreparationCaptureState.SuppliedUnused
            && missingParentRegister.EntryECX is null
            && missingParentRegister.CompletedInstructionOffsets.IsEmpty
            && missingParentRegister.MoveECXToESI is null
            && rejectedCaller.Outcome
                == NetworkDispatcherEntryLockPreparationOutcome.ParentFsPublicationNotAccepted
            && rejectedCaller.StopAtRawFileOffsetExclusive
                == rejectedCaller.ParentFsPublicationPlan.StopAtRawFileOffsetExclusive
            && rejectedCaller.StopAtRawFileOffsetExclusive
                != NetworkDispatcherEntryLockPreparationProjection.MoveEntryECXToESIRawFileOffset
            && rejectedCaller.FreshEntryECXCaptureState
                == NetworkDispatcherEntryLockPreparationCaptureState.SuppliedUnused
            && rejectedCaller.EntryECX is null
            && rejectedCaller.CompletedInstructionOffsets.IsEmpty);

        NetworkDispatcherEntryLockPreparationPlan reprojectedForgery =
            Project(forgeLegacyParentPlan: true);
        check("A supplied legacy parent plan cannot forge or reject immutable capture reprojection",
            complete.ParentFsPublicationPlan.ParentCookiePreparationPlan
                .ParentRegisterSavePlan.ParentPlanMatchesCapture
            && reprojectedForgery.ParentFsPublicationPlan.ParentCookiePreparationPlan
                .ParentRegisterSavePlan.ParentPlanMatchesCapture
            && complete.ParentFsPublicationPlan.CompletedInstructionOffsets.SequenceEqual(
                reprojectedForgery.ParentFsPublicationPlan.CompletedInstructionOffsets)
            && complete.Outcome == reprojectedForgery.Outcome
            && complete.CompletedInstructionOffsets.SequenceEqual(
                reprojectedForgery.CompletedInstructionOffsets)
            && complete.MoveECXToESI == reprojectedForgery.MoveECXToESI
            && complete.AddressCalculation == reprojectedForgery.AddressCalculation
            && complete.ByteWrite == reprojectedForgery.ByteWrite
            && complete.EBXPush == reprojectedForgery.EBXPush
            && complete.DwordWrite == reprojectedForgery.DwordWrite
            && complete.FlagsAtStop == reprojectedForgery.FlagsAtStop
            && complete.StopAtRawFileOffsetExclusive
                == reprojectedForgery.StopAtRawFileOffsetExclusive);
    }
}
