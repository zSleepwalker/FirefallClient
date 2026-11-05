using Firefall.Reconstruction.Startup;

internal static class ReliableReceiveAlternateHandoffChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned sample identity, exact raw window, VA mapping, bytes, and branch are recorded",
            ReliableReceiveAlternateHandoffProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && ReliableReceiveAlternateHandoffProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && ReliableReceiveAlternateHandoffProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && ReliableReceiveAlternateHandoffProjection.HandoffStartRawFileOffset == 0x00BE528F
            && ReliableReceiveAlternateHandoffProjection.HandoffStartVirtualAddress == 0x00FE5E8F
            && ReliableReceiveAlternateHandoffProjection.RawFileOffsetToVirtualAddress(0x00BE528F)
                == 0x00FE5E8F
            && ReliableReceiveAlternateHandoffProjection.HandoffEndRawFileOffsetExclusive == 0x00BE52A5
            && ReliableReceiveAlternateHandoffProjection.RawFileOffsetToVirtualAddress(0x00BE52A5)
                == 0x00FE5EA5
            && ReliableReceiveAlternateHandoffProjection.BranchRawFileOffset == 0x00BE52A3
            && ReliableReceiveAlternateHandoffProjection.BranchVirtualAddress == 0x00FE5EA3
            && ReliableReceiveAlternateHandoffProjection.BranchTakenDestinationRawFileOffset == 0x00BE52DF
            && ReliableReceiveAlternateHandoffProjection.BranchTakenDestinationVirtualAddress == 0x00FE5EDF
            && ReliableReceiveAlternateHandoffProjection.FallthroughDestinationRawFileOffset == 0x00BE52A5
            && ReliableReceiveAlternateHandoffProjection.FallthroughDestinationVirtualAddress == 0x00FE5EA5
            && ReliableReceiveAlternateHandoffProjection.CapturedHandoffBytes
                == "51 8D 8E 7C 04 00 00 E8 65 10 00 00 B8 00 80 00 00 "
                    + "66 3B D8 72 3A"
            && ReliableReceiveAlternateHandoffProjection.CapturedHandoffBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x16
            && InstructionEvidenceMatches());

        check("The push preserves the captured payload address, receiver ECX wraps, and helper target is pinned",
            ReliableReceiveAlternateHandoffProjection.ReceiverEcXOffset == 0x47C
            && ReliableReceiveAlternateHandoffProjection.CompareImmediate == 0x8000
            && ReliableReceiveAlternateHandoffProjection.ReceiverEcXAddress(uint.MaxValue - 0x47B) == 0
            && ReliableReceiveAlternateHandoffProjection.ReceiverEcXAddress(uint.MaxValue) == 0x47B
            && ReliableReceiveAlternateHandoffProjection.RelativeCallTarget(0x00BE5296, 0x1065)
                == 0x00BE6300
            && ReliableReceiveAlternateHandoffProjection.RawFileOffsetToVirtualAddress(0x00BE6300)
                == 0x00FE6F00
            && ShortBranchTarget(
                ReliableReceiveAlternateHandoffProjection.BranchRawFileOffset,
                0x3A) == ReliableReceiveAlternateHandoffProjection.BranchTakenDestinationRawFileOffset
            && ReliableReceiveAlternateHandoffProjection.RawFileOffsetToVirtualAddress(
                ReliableReceiveAlternateHandoffProjection.FallthroughDestinationRawFileOffset)
                == ReliableReceiveAlternateHandoffProjection.FallthroughDestinationVirtualAddress);

        ReliableReceiveAlternateHandoffPlan missingHelperReturn =
            ReliableReceiveAlternateHandoffProjection.Project(new(0x1000, 0xFFFF1234, 0x00FF, null));
        ReliableReceiveAlternateHandoffPlan nonReturningHelper =
            ReliableReceiveAlternateHandoffProjection.Project(new(0x1000, 0xFFFF1234, 0x8000, false));
        check("The opaque helper return gate prevents inferring a branch when missing or non-returning",
            missingHelperReturn.Outcome
                == ReliableReceiveAlternateHandoffOutcome.MissingHelperReturnCapture
            && missingHelperReturn.CapturedPayloadAddressPushed
            && missingHelperReturn.PushedPayloadWordAddress == 0xFFFF1234
            && missingHelperReturn.HelperEcX == 0x147C
            && missingHelperReturn.HelperCallReached
            && missingHelperReturn.HelperReturnedNormally is null
            && missingHelperReturn.BranchTaken is null
            && missingHelperReturn.SelectedDestinationRawFileOffset is null
            && missingHelperReturn.StopAtRawFileOffsetExclusive
                == ReliableReceiveAlternateHandoffProjection.OpaqueHelperCallReturnRawFileOffset
            && nonReturningHelper.Outcome
                == ReliableReceiveAlternateHandoffOutcome.HelperDidNotReturnNormally
            && nonReturningHelper.CapturedPayloadAddressPushed
            && nonReturningHelper.PushedPayloadWordAddress == 0xFFFF1234
            && nonReturningHelper.HelperEcX == 0x147C
            && nonReturningHelper.HelperCallReached
            && nonReturningHelper.HelperReturnedNormally == false
            && nonReturningHelper.BranchTaken is null
            && nonReturningHelper.SelectedDestinationRawFileOffset is null
            && nonReturningHelper.StopAtRawFileOffsetExclusive
                == ReliableReceiveAlternateHandoffProjection.OpaqueHelperCallReturnRawFileOffset);

        bool allEntryDeltasValidateAndAllBranchesMatch = Enumerable.Range(ushort.MinValue, ushort.MaxValue + 1)
            .All(value =>
            {
                ushort delta = (ushort)value;
                bool validEntry = delta < 0x0100 || delta > 0x7FFF;
                try
                {
                    ReliableReceiveAlternateHandoffPlan plan =
                        ReliableReceiveAlternateHandoffProjection.Project(
                            new(0xFFFFFB84, 0x12345678, delta, true));
                    bool branchTaken = delta < 0x8000;
                    return validEntry
                        && plan.Outcome == (branchTaken
                            ? ReliableReceiveAlternateHandoffOutcome.BranchTaken
                            : ReliableReceiveAlternateHandoffOutcome.FallthroughSelected)
                        && plan.CapturedPayloadAddressPushed
                        && plan.PushedPayloadWordAddress == 0x12345678
                        && plan.HelperEcX == 0
                        && plan.HelperReturnedNormally == true
                        && plan.BranchTaken == branchTaken
                        && plan.SelectedDestinationRawFileOffset
                            == (branchTaken
                                ? ReliableReceiveAlternateHandoffProjection.BranchTakenDestinationRawFileOffset
                                : ReliableReceiveAlternateHandoffProjection.FallthroughDestinationRawFileOffset)
                        && plan.StopAtRawFileOffsetExclusive
                            == (branchTaken
                                ? ReliableReceiveAlternateHandoffProjection.BranchTakenDestinationRawFileOffset
                                : ReliableReceiveAlternateHandoffProjection.FallthroughDestinationRawFileOffset);
                }
                catch (ArgumentOutOfRangeException)
                {
                    return !validEntry;
                }
            });
        check("All 65536 deltas enforce the prior JA entry interval and apply the following unsigned JB",
            allEntryDeltasValidateAndAllBranchesMatch);

        check("The explicit entry and JB boundaries select only the observed destinations",
            VerifyDelta(0x00FF, branchTaken: true, 0x00BE52DF)
            && VerifyDelta(0x0100, branchTaken: false, 0x00BE52A5, valid: false)
            && VerifyDelta(0x7FFF, branchTaken: true, 0x00BE52DF, valid: false)
            && VerifyDelta(0x8000, branchTaken: false, 0x00BE52A5)
            && VerifyDelta(0xFFFF, branchTaken: false, 0x00BE52A5));
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, uint Virtual, string Bytes, uint? TargetRaw, uint? TargetVirtual)[]
        {
            (0x00BE528F, 0x00FE5E8F, "51", null, null),
            (0x00BE5290, 0x00FE5E90, "8D 8E 7C 04 00 00", null, null),
            (0x00BE5296, 0x00FE5E96, "E8 65 10 00 00", null, null),
            (0x00BE529B, 0x00FE5E9B, "B8 00 80 00 00", null, null),
            (0x00BE52A0, 0x00FE5EA0, "66 3B D8", null, null),
            (0x00BE52A3, 0x00FE5EA3, "72 3A", 0x00BE52DF, 0x00FE5EDF)
        };
        return ReliableReceiveAlternateHandoffProjection.Instructions
            .Select(instruction =>
                (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes,
                    instruction.BranchTargetRawFileOffset, instruction.BranchTargetVirtualAddress))
            .SequenceEqual(expected);
    }

    private static bool VerifyDelta(
        ushort delta,
        bool branchTaken,
        uint destination,
        bool valid = true)
    {
        try
        {
            ReliableReceiveAlternateHandoffPlan plan =
                ReliableReceiveAlternateHandoffProjection.Project(
                    new(0x2000, 0xDEADBEEF, delta, true));
            return valid
                && plan.Capture.CapturedDelta == delta
                && plan.PushedPayloadWordAddress == 0xDEADBEEF
                && plan.BranchTaken == branchTaken
                && plan.SelectedDestinationRawFileOffset == destination;
        }
        catch (ArgumentOutOfRangeException)
        {
            return !valid;
        }
    }

    private static uint ShortBranchTarget(uint instructionRawFileOffset, byte displacement) =>
        unchecked(instructionRawFileOffset + 2 + (uint)(sbyte)displacement);
}
