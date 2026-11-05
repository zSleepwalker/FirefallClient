using Firefall.Reconstruction.Startup;

internal static class ReliableReceiveEmptySlotCallbackChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned sample, raw range, VA mapping, offsets, and callback bytes are exact",
            ReliableReceiveEmptySlotCallbackProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && ReliableReceiveEmptySlotCallbackProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && ReliableReceiveEmptySlotCallbackProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && ReliableReceiveEmptySlotCallbackProjection.EntryRawFileOffset == 0x00BE538D
            && ReliableReceiveEmptySlotCallbackProjection.EntryVirtualAddress == 0x00FE5F8D
            && ReliableReceiveEmptySlotCallbackProjection.EndRawFileOffsetExclusive == 0x00BE53A2
            && ReliableReceiveEmptySlotCallbackProjection.NormalReturnVirtualAddress == 0x00FE5FA2
            && ReliableReceiveEmptySlotCallbackProjection.HeaderBranchTargetRawFileOffset == 0x00BE53E9
            && ReliableReceiveEmptySlotCallbackProjection.HeaderBranchTargetVirtualAddress == 0x00FE5FE9
            && ReliableReceiveEmptySlotCallbackProjection.PayloadDwordOffset == 8
            && ReliableReceiveEmptySlotCallbackProjection.IndirectTargetSlotOffset == 0x0C
            && ReliableReceiveEmptySlotCallbackProjection.ExplicitStackPushCount == 0
            && ReliableReceiveEmptySlotCallbackProjection.CapturedCallbackBytes
                == "66 8B 1F 66 C1 EB 0C 80 E3 03 74 50 8B 47 08 8D 4F 08 FF 50 0C"
            && ReliableReceiveEmptySlotCallbackProjection.CapturedCallbackBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 21
            && InstructionsMatch());

        check("The selector branch depends only on header bits 12 and 13 for all WORD values",
            Enumerable.Range(ushort.MinValue, ushort.MaxValue + 1).All(value =>
            {
                ushort header = (ushort)value;
                byte selectorBits = (byte)((header >> 12) & 3);
                ReliableReceiveEmptySlotCallbackPlan plan =
                    ReliableReceiveEmptySlotCallbackProjection.Project(
                        new(0x10203040, header, null, null, null));
                return plan.HeaderSelectorBits == selectorBits
                    && plan.BranchTaken == (selectorBits == 0)
                    && plan.PayloadDwordAt8Address
                        == (selectorBits == 0 ? null : 0x10203048u)
                    && plan.Outcome == (selectorBits == 0
                        ? ReliableReceiveEmptySlotCallbackOutcome.HeaderBranchTaken
                        : ReliableReceiveEmptySlotCallbackOutcome.MissingPayloadDwordAt8)
                    && plan.SelectedDestinationRawFileOffset
                        == (selectorBits == 0 ? 0x00BE53E9u : null);
            }));

        ReliableReceiveEmptySlotCallbackPlan missingHeader =
            ReliableReceiveEmptySlotCallbackProjection.Project(
                new(0x10203040, null, null, null, null));
        ReliableReceiveEmptySlotCallbackPlan laterHeaderCapture =
            ReliableReceiveEmptySlotCallbackProjection.Project(
                new(0x10203040, null, 0x11223344, null, null));
        check("The header WORD is required first and later captures cannot stand in for it",
            missingHeader.Outcome
                == ReliableReceiveEmptySlotCallbackOutcome.MissingPayloadHeader
            && missingHeader.HeaderSelectorBits is null
            && missingHeader.BranchTaken is null
            && missingHeader.StopAtRawFileOffsetExclusive == 0x00BE538D
            && laterHeaderCapture.Outcome
                == ReliableReceiveEmptySlotCallbackOutcome.UnreachedCaptureProvided);

        ReliableReceiveEmptySlotCallbackPlan branchWithLaterCapture =
            ReliableReceiveEmptySlotCallbackProjection.Project(
                new(0x10203040, 0x0FFF, 0x11223344, 0x55667788, true));
        ReliableReceiveEmptySlotCallbackPlan branchOnly =
            ReliableReceiveEmptySlotCallbackProjection.Project(
                new(0x10203040, 0x0FFF, null, null, null));
        check("A zero selector branches using only the captured header; supplied later reads are unreached",
            branchOnly.Outcome == ReliableReceiveEmptySlotCallbackOutcome.HeaderBranchTaken
            && branchOnly.BranchTaken == true
            && branchOnly.SelectedDestinationRawFileOffset == 0x00BE53E9
            && branchOnly.StopAtRawFileOffsetExclusive == 0x00BE53E9
            && branchWithLaterCapture.Outcome
                == ReliableReceiveEmptySlotCallbackOutcome.UnreachedCaptureProvided
            && branchWithLaterCapture.BranchTaken == true
            && branchWithLaterCapture.IndirectCallReached == false);

        ReliableReceiveEmptySlotCallbackPlan missingPayloadDword =
            ReliableReceiveEmptySlotCallbackProjection.Project(
                new(0x10203040, 0x1000, null, null, null));
        ReliableReceiveEmptySlotCallbackPlan laterTarget =
            ReliableReceiveEmptySlotCallbackProjection.Project(
                new(0x10203040, 0x1000, null, 0x55667788, null));
        check("A nonzero selector requires the DWORD read before its target or normal-return captures",
            missingPayloadDword.Outcome
                == ReliableReceiveEmptySlotCallbackOutcome.MissingPayloadDwordAt8
            && missingPayloadDword.BranchTaken == false
            && missingPayloadDword.PayloadDwordAt8Address == 0x10203048
            && missingPayloadDword.EcxArgumentAddress is null
            && missingPayloadDword.StopAtRawFileOffsetExclusive == 0x00BE5399
            && laterTarget.Outcome
                == ReliableReceiveEmptySlotCallbackOutcome.UnreachedCaptureProvided);

        ReliableReceiveEmptySlotCallbackPlan missingTarget =
            ReliableReceiveEmptySlotCallbackProjection.Project(
                new(0x10203040, 0x1000, 0, null, null));
        ReliableReceiveEmptySlotCallbackPlan laterNormalReturn =
            ReliableReceiveEmptySlotCallbackProjection.Project(
                new(0x10203040, 0x1000, 0, null, true));
        check("The indirect target read at captured DWORD8 plus C is required before normal-return evidence",
            missingTarget.Outcome
                == ReliableReceiveEmptySlotCallbackOutcome.MissingIndirectTarget
            && missingTarget.PayloadDwordAt8Address == 0x10203048
            && missingTarget.EcxArgumentAddress == 0x10203048
            && missingTarget.IndirectTargetSlotAddress == 0x0C
            && missingTarget.IndirectCallReached == false
            && missingTarget.StopAtRawFileOffsetExclusive == 0x00BE539F
            && laterNormalReturn.Outcome
                == ReliableReceiveEmptySlotCallbackOutcome.UnreachedCaptureProvided);

        ReliableReceiveEmptySlotCallbackPlan missingNormalReturn =
            ReliableReceiveEmptySlotCallbackProjection.Project(
                new(0x10203040, 0x2000, 0x50607080, 0x90A0B0C0, null));
        ReliableReceiveEmptySlotCallbackPlan nonReturningCall =
            ReliableReceiveEmptySlotCallbackProjection.Project(
                new(0x10203040, 0x2000, 0x50607080, 0, false));
        ReliableReceiveEmptySlotCallbackPlan normalReturn =
            ReliableReceiveEmptySlotCallbackProjection.Project(
                new(0x10203040, 0x2000, 0x50607080, 0, true));
        check("Only a captured normal return reaches the normal boundary, while target zero remains captured",
            missingNormalReturn.Outcome
                == ReliableReceiveEmptySlotCallbackOutcome.MissingNormalReturn
            && missingNormalReturn.IndirectCallReached
            && missingNormalReturn.EcxArgumentAddress == 0x10203048
            && missingNormalReturn.EcxArgumentAddress != 0x50607080
            && missingNormalReturn.SelectedDestinationRawFileOffset is null
            && missingNormalReturn.StopAtRawFileOffsetExclusive == 0x00BE539F
            && nonReturningCall.Outcome
                == ReliableReceiveEmptySlotCallbackOutcome.IndirectCallDidNotReturnNormally
            && nonReturningCall.CapturedIndirectTarget == 0
            && nonReturningCall.IndirectCallReached
            && nonReturningCall.SelectedDestinationRawFileOffset is null
            && nonReturningCall.StopAtRawFileOffsetExclusive == 0x00BE539F
            && normalReturn.Outcome
                == ReliableReceiveEmptySlotCallbackOutcome.IndirectCallReturnedNormally
            && normalReturn.CapturedIndirectTarget == 0
            && normalReturn.IndirectCallReached
            && normalReturn.SelectedDestinationRawFileOffset == 0x00BE53A2
            && normalReturn.StopAtRawFileOffsetExclusive == 0x00BE53A2);

        ReliableReceiveEmptySlotCallbackPlan wrappedAddresses =
            ReliableReceiveEmptySlotCallbackProjection.Project(
                new(uint.MaxValue - 7, 0x3000, uint.MaxValue - 0x0B, 0, true));
        check("Payload and indirect-target-slot addresses wrap as DWORDs, and ECX is payload+8",
            ReliableReceiveEmptySlotCallbackProjection.RawFileOffsetToVirtualAddress(uint.MaxValue)
                == 0x00400BFF
            && ReliableReceiveEmptySlotCallbackProjection.PayloadAddress(uint.MaxValue - 7, 8) == 0
            && ReliableReceiveEmptySlotCallbackProjection.IndirectTargetSlotAddress(uint.MaxValue - 0x0B) == 0
            && wrappedAddresses.PayloadDwordAt8Address == 0
            && wrappedAddresses.EcxArgumentAddress == 0
            && wrappedAddresses.IndirectTargetSlotAddress == 0
            && wrappedAddresses.CapturedIndirectTarget == 0
            && wrappedAddresses.ExplicitStackPushCount == 0);
    }

    private static bool InstructionsMatch()
    {
        var expected = new (uint Raw, uint Virtual, string Bytes, uint? TargetRaw, uint? TargetVirtual)[]
        {
            (0x00BE538D, 0x00FE5F8D, "66 8B 1F", null, null),
            (0x00BE5390, 0x00FE5F90, "66 C1 EB 0C", null, null),
            (0x00BE5394, 0x00FE5F94, "80 E3 03", null, null),
            (0x00BE5397, 0x00FE5F97, "74 50", 0x00BE53E9, 0x00FE5FE9),
            (0x00BE5399, 0x00FE5F99, "8B 47 08", null, null),
            (0x00BE539C, 0x00FE5F9C, "8D 4F 08", null, null),
            (0x00BE539F, 0x00FE5F9F, "FF 50 0C", null, null)
        };
        return ReliableReceiveEmptySlotCallbackProjection.Instructions
            .Select(instruction =>
                (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes,
                    instruction.BranchTargetRawFileOffset, instruction.BranchTargetVirtualAddress))
            .SequenceEqual(expected)
            && unchecked(0x00BE5399 + 0x50) == 0x00BE53E9;
    }
}
