using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityFinalReturnChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned PE identity, raw/VA mapping, and exact 15-byte final-return window agree",
            FrameHandoffCapacityFinalReturnProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && FrameHandoffCapacityFinalReturnProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && FrameHandoffCapacityFinalReturnProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && FrameHandoffCapacityFinalReturnProjection.EntryRawFileOffset == 0x00BE6CB1
            && FrameHandoffCapacityFinalReturnProjection.EntryVirtualAddress == 0x00FE78B1
            && FrameHandoffCapacityFinalReturnProjection.ReturnBoundaryRawFileOffsetExclusive
                == 0x00BE6CC0
            && FrameHandoffCapacityFinalReturnProjection.ReturnBoundaryVirtualAddressExclusive
                == 0x00FE78C0
            && FrameHandoffCapacityFinalReturnProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacityFinalReturnProjection.EntryRawFileOffset)
                == FrameHandoffCapacityFinalReturnProjection.EntryVirtualAddress
            && FrameHandoffCapacityFinalReturnProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacityFinalReturnProjection.ReturnBoundaryRawFileOffsetExclusive)
                == FrameHandoffCapacityFinalReturnProjection.ReturnBoundaryVirtualAddressExclusive
            && Normalize(FrameHandoffCapacityFinalReturnProjection.CapturedWindowBytes)
                == "017708895F045F5E5B8BE55DC20400"
            && FrameHandoffCapacityFinalReturnProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 15
            && InstructionEvidenceMatches());

        FrameHandoffCapacityFinalReturnPlan unvalidated =
            FrameHandoffCapacityFinalReturnProjection.Project(new(
                ValidatedNormalTail: false,
                EntryEdi: 0x1000,
                EntryEsi: 2,
                EntryEbx: 3,
                EntryEbp: 4,
                Receiver8Old: 5));
        check("An unvalidated tail reaches no receiver access or frame cleanup",
            unvalidated.Outcome == FrameHandoffCapacityFinalReturnOutcome.TailNotValidated
            && unvalidated.MemoryAccesses.IsEmpty
            && !unvalidated.Receiver8ReadReached
            && !unvalidated.Receiver8WriteReached
            && !unvalidated.Receiver4StoreReached
            && unvalidated.StackPopRegisters.IsEmpty
            && !unvalidated.EspSetFromEbpReached
            && !unvalidated.ReturnReached
            && unvalidated.UnreachedReceiver8OldCaptureProvided
            && unvalidated.StopAtRawFileOffsetExclusive == 0x00BE6CB1);

        FrameHandoffCapacityFinalReturnPlan missingOld =
            FrameHandoffCapacityFinalReturnProjection.Project(new(
                ValidatedNormalTail: true,
                EntryEdi: 0x2000,
                EntryEsi: 1,
                EntryEbx: 3,
                EntryEbp: 4,
                Receiver8Old: null));
        check("A missing receiver old-value capture stops at ADD without either write or return",
            missingOld.Outcome
                == FrameHandoffCapacityFinalReturnOutcome.MissingReceiver8OldCapture
            && missingOld.MemoryAccesses.SequenceEqual(new[]
            {
                new FrameHandoffCapacityFinalReturnMemoryAccess(
                    FrameHandoffCapacityFinalReturnMemoryAccessKind.Read,
                    0x00BE6CB1, 0x2008, null)
            })
            && missingOld.Receiver8ReadReached
            && !missingOld.Receiver8OldCaptured
            && !missingOld.Receiver8WriteReached
            && !missingOld.Receiver4StoreReached
            && !missingOld.ReturnReached
            && missingOld.StopAtRawFileOffsetExclusive == 0x00BE6CB1);

        FrameHandoffCapacityFinalReturnPlan zeroAddress =
            FrameHandoffCapacityFinalReturnProjection.Project(new(
                ValidatedNormalTail: true,
                EntryEdi: uint.MaxValue - 7,
                EntryEsi: 0,
                EntryEbx: 0xAABBCCDD,
                EntryEbp: 0,
                Receiver8Old: 0));
        check("Zero receiver addresses and values remain valid captures, including DWORD address wrap",
            FrameHandoffCapacityFinalReturnProjection.Receiver8Address(uint.MaxValue - 7) == 0
            && FrameHandoffCapacityFinalReturnProjection.Receiver4Address(uint.MaxValue - 7)
                == uint.MaxValue - 3
            && zeroAddress.Outcome == FrameHandoffCapacityFinalReturnOutcome.ReturnedNormally
            && zeroAddress.Receiver8ReadAddress == 0
            && zeroAddress.Receiver8ValueBeforeAdd == 0
            && zeroAddress.Receiver8WriteAddress == 0
            && zeroAddress.Receiver8ValueAfterAdd == 0
            && zeroAddress.Receiver4StoreAddress == uint.MaxValue - 3
            && zeroAddress.Receiver4StoreValue == 0xAABBCCDD
            && zeroAddress.ReturnReached);

        FrameHandoffCapacityFinalReturnPlan wrapping =
            FrameHandoffCapacityFinalReturnProjection.Project(new(
                ValidatedNormalTail: true,
                EntryEdi: 0xFFFFFFFC,
                EntryEsi: 2,
                EntryEbx: uint.MaxValue,
                EntryEbp: 0x1234,
                Receiver8Old: uint.MaxValue));
        check("The receiver ADD wraps at DWORD width and precedes the independent EBX store",
            wrapping.Receiver8ReadAddress == 4
            && wrapping.Receiver8ValueBeforeAdd == uint.MaxValue
            && wrapping.Receiver8ValueAfterAdd == 1
            && wrapping.MemoryAccesses.SequenceEqual(new[]
            {
                new FrameHandoffCapacityFinalReturnMemoryAccess(
                    FrameHandoffCapacityFinalReturnMemoryAccessKind.Read,
                    0x00BE6CB1, 4, uint.MaxValue),
                new FrameHandoffCapacityFinalReturnMemoryAccess(
                    FrameHandoffCapacityFinalReturnMemoryAccessKind.Write,
                    0x00BE6CB1, 4, 1),
                new FrameHandoffCapacityFinalReturnMemoryAccess(
                    FrameHandoffCapacityFinalReturnMemoryAccessKind.Write,
                    0x00BE6CB4, 0, uint.MaxValue)
            })
            && wrapping.Receiver4StoreReached
            && wrapping.Receiver4StoreAddress == 0
            && wrapping.Receiver4StoreValue == uint.MaxValue
            && wrapping.ReturnReached);

        FrameHandoffCapacityFinalReturnPlan completed =
            FrameHandoffCapacityFinalReturnProjection.Project(new(
                ValidatedNormalTail: true,
                EntryEdi: 0x3000,
                EntryEsi: 7,
                EntryEbx: 0x11223344,
                EntryEbp: 0x55667788,
                Receiver8Old: 0xFFFFFFFC));
        check("Completion records both ordered writes, exact frame cleanup, and RET 4 without an EAX promise",
            completed.Outcome == FrameHandoffCapacityFinalReturnOutcome.ReturnedNormally
            && completed.MemoryAccesses.Where(access =>
                access.Kind == FrameHandoffCapacityFinalReturnMemoryAccessKind.Write)
                .SequenceEqual(new[]
                {
                    new FrameHandoffCapacityFinalReturnMemoryAccess(
                        FrameHandoffCapacityFinalReturnMemoryAccessKind.Write,
                        0x00BE6CB1, 0x3008, 3),
                    new FrameHandoffCapacityFinalReturnMemoryAccess(
                        FrameHandoffCapacityFinalReturnMemoryAccessKind.Write,
                        0x00BE6CB4, 0x3004, 0x11223344)
                })
            && completed.StackPopRegisters.SequenceEqual(new[] { "EDI", "ESI", "EBX", "EBP" })
            && completed.EspSetFromEbpReached
            && completed.ReturnReached
            && completed.ReturnImmediateBytes == 4
            && completed.StopAtRawFileOffsetExclusive == 0x00BE6CC0);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, string Bytes)[]
        {
            (0x00BE6CB1, "01 77 08"),
            (0x00BE6CB4, "89 5F 04"),
            (0x00BE6CB7, "5F"),
            (0x00BE6CB8, "5E"),
            (0x00BE6CB9, "5B"),
            (0x00BE6CBA, "8B E5"),
            (0x00BE6CBC, "5D"),
            (0x00BE6CBD, "C2 04 00")
        };
        uint nextRawOffset = FrameHandoffCapacityFinalReturnProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (FrameHandoffCapacityFinalReturnInstructionEvidence instruction
            in FrameHandoffCapacityFinalReturnProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != FrameHandoffCapacityFinalReturnProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == FrameHandoffCapacityFinalReturnProjection.ReturnBoundaryRawFileOffsetExclusive
            && bytes == Normalize(FrameHandoffCapacityFinalReturnProjection.CapturedWindowBytes)
            && FrameHandoffCapacityFinalReturnProjection.Instructions
                .Select(instruction => (instruction.RawFileOffset, instruction.Bytes))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
