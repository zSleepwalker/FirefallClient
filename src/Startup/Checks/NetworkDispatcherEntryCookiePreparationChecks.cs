using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherEntryCookiePreparationChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static NetworkDispatcherEntryCookiePreparationPlan Project(
            uint? cookie = 0,
            uint? entryESP = 0x2000,
            uint? entryEBP = 0x1234,
            uint? entryEBX = 0x11111111,
            uint? entryESI = 0x22222222,
            uint? entryEDI = 0x33333333,
            bool forgeLegacyParentPlan = false)
        {
            NetworkDispatcherEntryFramePrefixCapture frameCapture = FrameCapture(
                entryESP, entryEBP);
            NetworkDispatcherEntryFramePrefixPlan framePlan =
                NetworkDispatcherEntryFramePrefixProjection.Project(frameCapture);
            if (forgeLegacyParentPlan)
                framePlan = framePlan with
                {
                    StopAtRawFileOffsetExclusive = 0x00BBDDC3
                };

            var parentCapture = new NetworkDispatcherEntryRegisterSaveCapture(
                frameCapture,
                framePlan,
                entryEBX,
                entryESI,
                entryEDI);
            return NetworkDispatcherEntryCookiePreparationProjection.Project(new(
                parentCapture,
                cookie));
        }

        static string Normalize(string value) =>
            string.Concat(value.Where(Uri.IsHexDigit)).ToUpperInvariant();

        check("The cookie preparation window is anchored through the push and stops before LEA",
            NetworkDispatcherEntryCookiePreparationProjection.EntryRawFileOffset
                == 0x00BBDDC7
            && NetworkDispatcherEntryCookiePreparationProjection.FreshCookieGlobalReadRawFileOffset
                == 0x00BBDDC7
            && NetworkDispatcherEntryCookiePreparationProjection.FreshCookieGlobalVirtualAddress
                == 0x01DC5000
            && NetworkDispatcherEntryCookiePreparationProjection.CookieXorEntryEBPRawFileOffset
                == 0x00BBDDCC
            && NetworkDispatcherEntryCookiePreparationProjection.PushCookieRawFileOffset
                == 0x00BBDDCE
            && NetworkDispatcherEntryCookiePreparationProjection.PrefixEndRawFileOffsetExclusive
                == 0x00BBDDCF
            && Normalize(NetworkDispatcherEntryCookiePreparationProjection.CapturedWindowBytes)
                == "558BEC6AFF68183CA40164A1000000005083EC0C535657A10050DC0133C5508D45F464A3000000008BF18D5E6CC645EC0153895DE8FF158442AC01"
            && !NetworkDispatcherEntryCookiePreparationProjection.ExecutesNativeInstructions);

        NetworkDispatcherEntryCookiePreparationPlan zero = Project(
            cookie: 0,
            entryESP: 4);
        check("A zero global DWORD is freshly read and XOR updates flags before the committed push",
            zero.ParentRegisterSavePlan.ExactRegisterSavesProjected
            && zero.Outcome
                == NetworkDispatcherEntryCookiePreparationOutcome.CookiePreparationProjected
            && zero.FreshCookieRead is
            {
                RawFileOffset: 0x00BBDDC7,
                Address: 0x01DC5000,
                Width: 4,
                Attempted: true,
                Captured: true,
                Value: 0,
                Completed: true
            }
            && zero.CookieXor is
            {
                RawFileOffset: 0x00BBDDCC,
                LeftEAX: 0,
                EntryFrameEBP: 0,
                ResultEAX: 0,
                Attempted: true,
                Completed: true,
                FlagsBefore: not null,
                FlagsAfter: {
                    Carry: false,
                    Parity: true,
                    Auxiliary: null,
                    Zero: true,
                    Sign: false,
                    Overflow: false
                }
            }
            && zero.CookieXor?.FlagsBefore == zero.ParentRegisterSavePlan.FlagsAtStop
            && zero.CookiePush is
            {
                RawFileOffset: 0x00BBDDCE,
                StackPointerBefore: 0xFFFFFFDC,
                StackPointerAfter: 0xFFFFFFD8,
                Value: 0,
                Attempted: true,
                Committed: true
            }
            && zero.FlagsAtStop == zero.CookieXor?.FlagsAfter
            && zero.CurrentESPAtStop == 0xFFFFFFD8
            && zero.CompletedInstructionOffsets.SequenceEqual(new uint[]
            {
                0x00BBDDC7, 0x00BBDDCC, 0x00BBDDCE
            })
            && zero.StopAtRawFileOffsetExclusive == 0x00BBDDCF
            && zero.ExactCookiePreparationProjected
            && !zero.NativeCookieValidityChecked
            && !zero.CookieABIInferred
            && !zero.LinearFSInterpretationMade);

        NetworkDispatcherEntryCookiePreparationPlan maximum = Project(
            cookie: uint.MaxValue,
            entryESP: 0x80000003);
        NetworkDispatcherEntryCookiePreparationPlan negativeOddParity = Project(
            cookie: 0,
            entryESP: 0x80000005);
        check("Maximum and negative XOR values compute DWORD sign, zero, and low-byte parity",
            maximum.EntryEAX == 0x80000000
            && maximum.CookieXor?.ResultEAX == 0x80000000
            && maximum.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && maximum.CookiePush?.Value == 0x80000000
            && negativeOddParity.EntryEAX == 0x80000001
            && negativeOddParity.FlagsAtStop is
            {
                Parity: false,
                Zero: false,
                Sign: true,
                Carry: false,
                Auxiliary: null,
                Overflow: false
            });

        NetworkDispatcherEntryCookiePreparationPlan missingRead = Project(
            cookie: null,
            entryESP: 0x40,
            entryEBP: 0x87654321);
        check("A missing fresh global read retains parent flags and ESP without attempting XOR or push",
            missingRead.Outcome
                == NetworkDispatcherEntryCookiePreparationOutcome.MissingFreshCookieGlobalDWORD
            && missingRead.FreshCookieRead is
            {
                Address: 0x01DC5000,
                Attempted: true,
                Captured: false,
                Value: null,
                Completed: false
            }
            && missingRead.CookieXor is null
            && missingRead.CookiePush is null
            && missingRead.EntryEAX is null
            && missingRead.FlagsAtStop == missingRead.ParentRegisterSavePlan.FlagsAtStop
            && missingRead.CurrentESPAtStop
                == missingRead.ParentRegisterSavePlan.CurrentESPAtStop
            && missingRead.CompletedInstructionOffsets.IsEmpty
            && missingRead.StopAtRawFileOffsetExclusive == 0x00BBDDC7
            && missingRead.FreshCookieGlobalDWORDCaptureState
                == NetworkDispatcherEntryCookiePreparationCaptureState.Missing);

        NetworkDispatcherEntryCookiePreparationPlan wrappedESP = Project(
            cookie: 0x24,
            entryESP: 0x28);
        check("The cookie push derives its stack address after prior saves and wraps at DWORD zero",
            wrappedESP.ParentRegisterSavePlan.CurrentESPAtStop == 0
            && wrappedESP.CookieXor?.ResultEAX == 0
            && wrappedESP.CookiePush is
            {
                StackPointerBefore: 0,
                StackPointerAfter: 0xFFFFFFFC,
                Value: 0,
                Committed: true
            }
            && wrappedESP.CurrentESPAtStop == 0xFFFFFFFC
            && NetworkDispatcherEntryCookiePreparationProjection.StackAddressAfterPush(0)
                == 0xFFFFFFFC);

        NetworkDispatcherEntryCookiePreparationPlan missingParentESI = Project(
            cookie: 0xAABBCCDD,
            entryESP: 0x2000,
            entryEBX: 0x10,
            entryESI: null,
            entryEDI: 0x30);
        check("A rejected parent with a missing ESI preserves its partial ESP, flags, and actual endpoint",
            missingParentESI.Outcome
                == NetworkDispatcherEntryCookiePreparationOutcome.ParentRegisterSavesNotAccepted
            && missingParentESI.ParentRegisterSavePlan.Outcome
                == NetworkDispatcherEntryRegisterSaveOutcome.MissingFreshEntryESI
            && missingParentESI.ParentRegisterSavePlan.CurrentESPAtStop == 0x1FE0
            && missingParentESI.CurrentESPAtStop == 0x1FE0
            && missingParentESI.FlagsAtStop
                == missingParentESI.ParentRegisterSavePlan.FlagsAtStop
            && missingParentESI.StopAtRawFileOffsetExclusive == 0x00BBDDC5
            && missingParentESI.FreshCookieRead is null
            && missingParentESI.CookieXor is null
            && missingParentESI.CookiePush is null
            && missingParentESI.FreshCookieGlobalDWORDCaptureState
                == NetworkDispatcherEntryCookiePreparationCaptureState.SuppliedUnused);

        NetworkDispatcherEntryCookiePreparationPlan aliasedAddress = Project(
            cookie: 0xAABBCCDD,
            entryESP: 0x01DC5028,
            entryEDI: 0xDEADBEEF);
        check("The operation-time global read remains independent when its address aliases a prior EDI stack store",
            aliasedAddress.ParentRegisterSavePlan.Pushes[^1].StackAddress
                == NetworkDispatcherEntryCookiePreparationProjection.FreshCookieGlobalVirtualAddress
            && aliasedAddress.ParentRegisterSavePlan.Pushes[^1].Value == 0xDEADBEEF
            && aliasedAddress.FreshCookieRead?.Address == 0x01DC5000
            && aliasedAddress.FreshCookieRead?.Value == 0xAABBCCDD
            && aliasedAddress.CookieXor?.LeftEAX == 0xAABBCCDD
            && aliasedAddress.CookiePush?.Value == (0xAABBCCDD ^ 0x01DC5024));

        NetworkDispatcherEntryCookiePreparationPlan ordinary = Project(
            cookie: 0x12345678);
        NetworkDispatcherEntryCookiePreparationPlan reprojectedLegacyForgery = Project(
            cookie: 0x12345678,
            forgeLegacyParentPlan: true);
        check("A legacy supplied parent plan cannot forge or reject the immutable capture reprojection",
            ordinary.ParentRegisterSavePlan.ParentPlanMatchesCapture
            && reprojectedLegacyForgery.ParentRegisterSavePlan.ParentPlanMatchesCapture
            && ordinary.ParentRegisterSavePlan.Outcome
                == reprojectedLegacyForgery.ParentRegisterSavePlan.Outcome
            && ordinary.ParentRegisterSavePlan.CompletedInstructionOffsets.SequenceEqual(
                reprojectedLegacyForgery.ParentRegisterSavePlan.CompletedInstructionOffsets)
            && ordinary.ParentRegisterSavePlan.Pushes.SequenceEqual(
                reprojectedLegacyForgery.ParentRegisterSavePlan.Pushes)
            && ordinary.Outcome == reprojectedLegacyForgery.Outcome
            && ordinary.FreshCookieRead == reprojectedLegacyForgery.FreshCookieRead
            && ordinary.CookieXor == reprojectedLegacyForgery.CookieXor
            && ordinary.CookiePush == reprojectedLegacyForgery.CookiePush
            && ordinary.FlagsAtStop == reprojectedLegacyForgery.FlagsAtStop
            && ordinary.StopAtRawFileOffsetExclusive
                == reprojectedLegacyForgery.StopAtRawFileOffsetExclusive);
    }

    private static NetworkDispatcherEntryFramePrefixCapture FrameCapture(
        uint? entryESP,
        uint? entryEBP) =>
        new(
            new NetworkIoCompletionClockGateCapture(
                EntryValidated: true,
                EntryEBP: 0x2000,
                EntryFlags: null,
                FreshGlobalPointer: 0x1000,
                FreshClockField: 500,
                CachedClockTargetVirtualAddress: 0x12345678,
                ClockReturnedNormally: true,
                ClockReturnEAX: 1000,
                FreshSavedPointerRead: 0x2222,
                DispatcherReturnedNormally: null),
            FreshEntryESP: entryESP,
            FreshEntryEBP: entryEBP,
            FreshEntryFlags: new NetworkIoCompletionClockGateFlags(
                Carry: true,
                Parity: false,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false),
            FreshFSOffset0DWORD: 0xAABBCCDD);
}
