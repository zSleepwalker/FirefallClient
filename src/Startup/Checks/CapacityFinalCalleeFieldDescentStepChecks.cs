using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeFieldDescentStepChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The one-step instruction window, branch targets, and addresses match",
            CapacityFinalCalleeFieldDescentStepProjection.EntryRawFileOffset
                == 0x012C8FC2
            && CapacityFinalCalleeFieldDescentStepProjection.EntryVirtualAddress
                == 0x016C9BC2
            && CapacityFinalCalleeFieldDescentStepProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeFieldDescentStepProjection.EntryRawFileOffset)
                == CapacityFinalCalleeFieldDescentStepProjection.EntryVirtualAddress
            && CapacityFinalCalleeFieldDescentStepProjection.LoopbackRawFileOffset
                == 0x012C8FC5
            && CapacityFinalCalleeFieldDescentStepProjection.TerminalRawFileOffset
                == 0x012C8FDD
            && CapacityFinalCalleeFieldDescentStepProjection.BoundaryRawFileOffsetExclusive
                == 0x012C8FDD
            && Normalize(CapacityFinalCalleeFieldDescentStepProjection.CapturedWindowBytes)
                == "8B55FC837814008D48147509837810008D481074068B018BD1EBE8"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeFieldDescentStepPlan nonzero14 =
            CapacityFinalCalleeFieldDescentStepProjection.Project(Capture(
                freshEaxPlus14: 1,
                freshEaxPlus10: 0x2222,
                freshEcxValue: 0x3333));
        check("A nonzero EAX+14 compare takes the first branch and reloads ECX independently",
            nonzero14.Outcome
                == CapacityFinalCalleeFieldDescentStepOutcome.LoopbackHasUnreachedCaptures
            && nonzero14.FreshFrameMinus4Captured
            && nonzero14.EDXLoadedFromFrameMinus4 == 0x3000
            && nonzero14.EaxPlus14CompareReached
            && nonzero14.EaxPlus14EqualsZero == false
            && nonzero14.FirstLeaReached
            && nonzero14.ECXAfterFirstLea == 0x1014
            && nonzero14.FirstLeaPreservedZeroFlag == false
            && nonzero14.FirstJneTaken == true
            && !nonzero14.EaxPlus10ReadReached
            && nonzero14.UnreachedFreshEaxPlus10CaptureProvided
            && nonzero14.FreshEcxAddress == 0x1014
            && nonzero14.FreshEcxValue == 0x3333
            && nonzero14.EAXLoadedFromEcx == 0x3333
            && nonzero14.EDXLoadedFromEcx == 0x1014
            && nonzero14.LoopbackReached
            && nonzero14.StopAtRawFileOffsetExclusive == 0x012C8FC5);

        CapacityFinalCalleeFieldDescentStepPlan zero14Nonzero10 =
            CapacityFinalCalleeFieldDescentStepProjection.Project(Capture(
                freshEaxPlus14: 0,
                freshEaxPlus10: 2,
                freshEcxValue: 0x4444));
        check("A zero EAX+14 compare preserves flags across LEA before the nonzero EAX+10 route",
            zero14Nonzero10.Outcome
                == CapacityFinalCalleeFieldDescentStepOutcome.ReachedLoopback
            && zero14Nonzero10.EaxPlus14EqualsZero == true
            && zero14Nonzero10.FirstLeaPreservedZeroFlag == true
            && zero14Nonzero10.FirstJneTaken == false
            && zero14Nonzero10.EaxPlus10CompareReached
            && zero14Nonzero10.EaxPlus10EqualsZero == false
            && zero14Nonzero10.SecondLeaReached
            && zero14Nonzero10.ECXAfterSecondLea == 0x1010
            && zero14Nonzero10.SecondLeaPreservedZeroFlag == false
            && zero14Nonzero10.SecondJeTaken == false
            && zero14Nonzero10.FreshEcxAddress == 0x1010
            && zero14Nonzero10.FreshEcxValue == 0x4444
            && zero14Nonzero10.EDXLoadedFromFrameMinus4 == 0x3000
            && zero14Nonzero10.EDXLoadedFromEcx == 0x1010
            && !zero14Nonzero10.UnreachedFreshEaxPlus10CaptureProvided
            && zero14Nonzero10.StopAtRawFileOffsetExclusive == 0x012C8FC5);

        CapacityFinalCalleeFieldDescentStepPlan allZero =
            CapacityFinalCalleeFieldDescentStepProjection.Project(Capture(
                freshEaxPlus14: 0,
                freshEaxPlus10: 0,
                freshEcxValue: 0x5555));
        check("Two captured zero fields take the exact FDD terminal without an ECX memory load",
            allZero.Outcome
                == CapacityFinalCalleeFieldDescentStepOutcome.TerminalHasUnreachedCaptures
            && allZero.EaxPlus14Captured
            && allZero.EaxPlus10Captured
            && allZero.FirstLeaPreservedZeroFlag == true
            && allZero.SecondLeaPreservedZeroFlag == true
            && allZero.SecondJeTaken == true
            && allZero.TerminalReached
            && !allZero.FreshEcxReadReached
            && !allZero.FreshEcxValueCaptured
            && allZero.UnreachedFreshEcxCaptureProvided
            && allZero.EDXLoadedFromFrameMinus4 == 0x3000
            && allZero.EDXLoadedFromEcx is null
            && allZero.StopAtRawFileOffsetExclusive == 0x012C8FDD);

        CapacityFinalCalleeFieldDescentStepPlan cleanTerminal =
            CapacityFinalCalleeFieldDescentStepProjection.Project(Capture(
                freshEaxPlus14: 0,
                freshEaxPlus10: 0,
                freshEcxValue: null));
        CapacityFinalCalleeFieldDescentStepPlan cleanLoopback =
            CapacityFinalCalleeFieldDescentStepProjection.Project(Capture(
                freshEaxPlus14: 0x10,
                freshEaxPlus10: null,
                freshEcxValue: 0));
        check("Clean terminal and loopback endpoints are exact and zero is a captured value",
            cleanTerminal.Outcome
                == CapacityFinalCalleeFieldDescentStepOutcome.ReachedTerminal
            && !cleanTerminal.AnyUnreachedCaptureProvided
            && cleanTerminal.StopAtRawFileOffsetExclusive == 0x012C8FDD
            && cleanLoopback.Outcome
                == CapacityFinalCalleeFieldDescentStepOutcome.ReachedLoopback
            && cleanLoopback.FreshEcxValueCaptured
            && cleanLoopback.FreshEcxValue == 0
            && cleanLoopback.EAXLoadedFromEcx == 0
            && cleanLoopback.EDXLoadedFromEcx == 0x1014
            && !cleanLoopback.UnreachedFreshEaxPlus10CaptureProvided
            && !cleanLoopback.AnyUnreachedCaptureProvided
            && cleanLoopback.StopAtRawFileOffsetExclusive == 0x012C8FC5);

        CapacityFinalCalleeFieldDescentStepPlan missingEbp =
            CapacityFinalCalleeFieldDescentStepProjection.Project(Capture(
                validatedEntryEBP: null,
                freshFrameMinus4: 0,
                freshEaxPlus14: 0,
                freshEaxPlus10: 0,
                freshEcxValue: 0));
        CapacityFinalCalleeFieldDescentStepPlan missingFrameRead =
            CapacityFinalCalleeFieldDescentStepProjection.Project(Capture(
                freshFrameMinus4: null,
                freshEaxPlus14: 0,
                freshEaxPlus10: 0,
                freshEcxValue: 0));
        CapacityFinalCalleeFieldDescentStepPlan missingEax =
            CapacityFinalCalleeFieldDescentStepProjection.Project(Capture(
                validatedEntryEAX: null,
                freshFrameMinus4: 0,
                freshEaxPlus14: 0,
                freshEaxPlus10: 0,
                freshEcxValue: 0));
        check("Missing first-use entry gates stop in order and retain any completed frame load",
            missingEbp.Outcome
                == CapacityFinalCalleeFieldDescentStepOutcome.MissingValidatedEntryEBP
            && !missingEbp.FreshFrameMinus4ReadReached
            && missingEbp.UnreachedFreshFrameMinus4CaptureProvided
            && missingEbp.UnreachedValidatedEntryEAXCaptureProvided
            && missingEbp.StopAtRawFileOffsetExclusive == 0x012C8FC2
            && missingFrameRead.Outcome
                == CapacityFinalCalleeFieldDescentStepOutcome.MissingFreshFrameMinus4
            && missingFrameRead.FreshFrameMinus4ReadReached
            && !missingFrameRead.FreshFrameMinus4Captured
            && missingFrameRead.FreshFrameMinus4Address == 0x1FFC
            && missingFrameRead.UnreachedValidatedEntryEAXCaptureProvided
            && missingFrameRead.StopAtRawFileOffsetExclusive == 0x012C8FC2
            && missingEax.Outcome
                == CapacityFinalCalleeFieldDescentStepOutcome.MissingValidatedEntryEAX
            && missingEax.EDXLoadedFromFrameMinus4 == 0
            && missingEax.FreshFrameMinus4Captured
            && missingEax.UnreachedFreshEaxPlus14CaptureProvided
            && missingEax.StopAtRawFileOffsetExclusive == 0x012C8FC5);

        CapacityFinalCalleeFieldDescentStepPlan missing14 =
            CapacityFinalCalleeFieldDescentStepProjection.Project(Capture(
                freshEaxPlus14: null,
                freshEaxPlus10: 0,
                freshEcxValue: 0));
        CapacityFinalCalleeFieldDescentStepPlan missing10 =
            CapacityFinalCalleeFieldDescentStepProjection.Project(Capture(
                freshEaxPlus14: 0,
                freshEaxPlus10: null,
                freshEcxValue: 0));
        CapacityFinalCalleeFieldDescentStepPlan missingEcx =
            CapacityFinalCalleeFieldDescentStepProjection.Project(Capture(
                freshEaxPlus14: 0x1,
                freshEaxPlus10: null,
                freshEcxValue: null));
        check("Missing fresh reads remain distinct from zero and preserve earlier EDX state",
            missing14.Outcome
                == CapacityFinalCalleeFieldDescentStepOutcome.MissingFreshEaxPlus14
            && missing14.EaxPlus14ReadReached
            && !missing14.EaxPlus14Captured
            && missing14.EaxPlus14EqualsZero is null
            && missing14.EDXLoadedFromFrameMinus4 == 0x3000
            && missing14.UnreachedFreshEaxPlus10CaptureProvided
            && missing14.StopAtRawFileOffsetExclusive == 0x012C8FC5
            && missing10.Outcome
                == CapacityFinalCalleeFieldDescentStepOutcome.MissingFreshEaxPlus10
            && missing10.FirstLeaPreservedZeroFlag == true
            && missing10.EaxPlus10ReadReached
            && !missing10.EaxPlus10Captured
            && missing10.EaxPlus10EqualsZero is null
            && missing10.ECXAfterFirstLea == 0x1014
            && missing10.EDXLoadedFromFrameMinus4 == 0x3000
            && missing10.UnreachedFreshEcxCaptureProvided
            && missing10.StopAtRawFileOffsetExclusive == 0x012C8FCE
            && missingEcx.Outcome
                == CapacityFinalCalleeFieldDescentStepOutcome.MissingFreshEcxValue
            && missingEcx.FreshEcxReadReached
            && !missingEcx.FreshEcxValueCaptured
            && missingEcx.FreshEcxAddress == 0x1014
            && missingEcx.EAXLoadedFromEcx is null
            && missingEcx.EDXLoadedFromFrameMinus4 == 0x3000
            && missingEcx.StopAtRawFileOffsetExclusive == 0x012C8FD7);

        CapacityFinalCalleeFieldDescentStepPlan independentReload =
            CapacityFinalCalleeFieldDescentStepProjection.Project(Capture(
                freshEaxPlus14: 0x7777,
                freshEaxPlus10: null,
                freshEcxValue: 0x8888));
        CapacityFinalCalleeFieldDescentStepPlan wrapped =
            CapacityFinalCalleeFieldDescentStepProjection.Project(Capture(
                validatedEntryEBP: 2,
                validatedEntryEAX: 0xFFFFFFF8,
                freshFrameMinus4: 0xAAAAAAAA,
                freshEaxPlus14: 0xBBBBBBBB,
                freshEaxPlus10: null,
                freshEcxValue: 0xCCCCCCCC));
        check("The FD7 dereference is an independent read and all DWORD address arithmetic wraps",
            independentReload.Outcome
                == CapacityFinalCalleeFieldDescentStepOutcome.ReachedLoopback
            && independentReload.FreshEaxPlus14Address == 0x1014
            && independentReload.FreshEaxPlus14Value == 0x7777
            && independentReload.FreshEcxAddress == 0x1014
            && independentReload.FreshEcxValue == 0x8888
            && independentReload.EAXLoadedFromEcx == 0x8888
            && independentReload.FreshEaxPlus14Value != independentReload.EAXLoadedFromEcx
            && wrapped.FreshFrameMinus4Address == 0xFFFFFFFE
            && wrapped.FreshEaxPlus14Address == 0xC
            && wrapped.FirstLeaPreservedZeroFlag == false
            && wrapped.FreshEcxAddress == 0xC
            && wrapped.EDXLoadedFromEcx == 0xC
            && wrapped.EAXLoadedFromEcx == 0xCCCCCCCC
            && CapacityFinalCalleeFieldDescentStepProjection.DeriveFrameMinus4Address(0)
                == 0xFFFFFFFC
            && CapacityFinalCalleeFieldDescentStepProjection.DeriveEaxPlus14Address(
                0xFFFFFFF8) == 0xC
            && CapacityFinalCalleeFieldDescentStepProjection.DeriveEaxPlus10Address(
                0xFFFFFFF8) == 8);
    }

    private static CapacityFinalCalleeFieldDescentStepCapture Capture(
        uint? validatedEntryEBP = 0x2000,
        uint? validatedEntryEAX = 0x1000,
        uint? freshFrameMinus4 = 0x3000,
        uint? freshEaxPlus14 = 0,
        uint? freshEaxPlus10 = 0,
        uint? freshEcxValue = 0x4000) =>
        new(
            validatedEntryEBP,
            validatedEntryEAX,
            freshFrameMinus4,
            freshEaxPlus14,
            freshEaxPlus10,
            freshEcxValue);

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, string Bytes)[]
        {
            (0x012C8FC2, "8B 55 FC"),
            (0x012C8FC5, "83 78 14 00"),
            (0x012C8FC9, "8D 48 14"),
            (0x012C8FCC, "75 09"),
            (0x012C8FCE, "83 78 10 00"),
            (0x012C8FD2, "8D 48 10"),
            (0x012C8FD5, "74 06"),
            (0x012C8FD7, "8B 01"),
            (0x012C8FD9, "8B D1"),
            (0x012C8FDB, "EB E8")
        };
        uint nextRawOffset =
            CapacityFinalCalleeFieldDescentStepProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeFieldDescentStepInstructionEvidence instruction
            in CapacityFinalCalleeFieldDescentStepProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeFieldDescentStepProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeFieldDescentStepProjection.BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeFieldDescentStepProjection.CapturedWindowBytes)
            && CapacityFinalCalleeFieldDescentStepProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
