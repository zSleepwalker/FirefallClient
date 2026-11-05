using Firefall.Reconstruction.Startup;

internal static class DeferredControlCallbackChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Deferred-control anchors pin the verified sample, raw range, and raw-to-VA mapping",
            DeferredControlCallbackProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && DeferredControlCallbackProjection.EntryRawFileOffset == 0x00BD37D8
            && DeferredControlCallbackProjection.EndRawFileOffsetExclusive == 0x00BD37EB
            && DeferredControlCallbackProjection.CodeVirtualAddressDeltaFromRawFileOffset == 0x00400C00
            && DeferredControlCallbackProjection.EntryVirtualAddress == 0x00FD43D8
            && DeferredControlCallbackProjection.CallReturnRawFileOffset == 0x00BD37E4
            && DeferredControlCallbackProjection.CallReturnVirtualAddress == 0x00FD43E4
            && DeferredControlCallbackProjection.ZeroBitBranchTargetRawFileOffset == 0x00BD3805
            && DeferredControlCallbackProjection.ZeroBitBranchTargetVirtualAddress == 0x00FD4405
            && DeferredControlCallbackProjection.PayloadStackArgumentOffset == 4
            && DeferredControlCallbackProjection.PayloadVtablePointerOffset == 8
            && DeferredControlCallbackProjection.IndirectSlotOffset == 4
            && DeferredControlCallbackProjection.ReturnedPointerTestMask == 8);

        check("Instruction evidence matches the exact byte slice and preserves argument and branch semantics",
            DeferredControlCallbackProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes, instruction.BranchTargetRawFileOffset))
                .SequenceEqual(new (uint RawFileOffset, string Bytes, uint? BranchTargetRawFileOffset)[]
                {
                    (0x00BD37D8, "8B 47 08", null),
                    (0x00BD37DB, "8D 4F 08", null),
                    (0x00BD37DE, "FF 77 04", null),
                    (0x00BD37E1, "FF 50 04", null),
                    (0x00BD37E4, "8B D8", null),
                    (0x00BD37E6, "F6 03 08", null),
                    (0x00BD37E9, "74 1A", 0x00BD3805)
                })
            && string.Join(" ",
                DeferredControlCallbackProjection.Instructions.Select(i => i.Bytes))
                == "8B 47 08 8D 4F 08 FF 77 04 FF 50 04 8B D8 F6 03 08 74 1A"
            && unchecked((int)0x00BD37EB + 0x1A) == 0x00BD3805);

        DeferredControlCallbackPlan missingVtable =
            DeferredControlCallbackProjection.Project(
                Capture(vtablePointer: null, stackArgumentValue: null,
                    callResult: null, returnedPointerByte: null));
        DeferredControlCallbackPlan laterStackArgument =
            DeferredControlCallbackProjection.Project(
                Capture(vtablePointer: null, stackArgumentValue: 0xAABBCCDD,
                    callResult: null, returnedPointerByte: null));
        check("The vtable pointer is a required first capture and later values cannot fill an unreached read",
            missingVtable.Outcome == DeferredControlCallbackOutcome.MissingVtablePointer
            && missingVtable.StopAtRawFileOffsetExclusive == 0x00BD37D8
            && laterStackArgument.Outcome
                == DeferredControlCallbackOutcome.UnreachedCaptureProvided
            && laterStackArgument.IndirectSlotAddress is null);

        DeferredControlCallbackPlan missingStackArgument =
            DeferredControlCallbackProjection.Project(
                Capture(vtablePointer: 0x22334455, stackArgumentValue: null,
                    callResult: null, returnedPointerByte: null));
        DeferredControlCallbackPlan laterCallResult =
            DeferredControlCallbackProjection.Project(
                Capture(vtablePointer: 0x22334455, stackArgumentValue: null,
                    callResult: 0x66778899, returnedPointerByte: null));
        check("The stack argument is captured independently before the indirect call",
            missingStackArgument.Outcome
                == DeferredControlCallbackOutcome.MissingStackArgument
            && missingStackArgument.IndirectCallReached == false
            && missingStackArgument.IndirectSlotAddress is null
            && missingStackArgument.StackArgumentAddress == 0x10203044
            && missingStackArgument.StopAtRawFileOffsetExclusive == 0x00BD37DE
            && laterCallResult.Outcome
                == DeferredControlCallbackOutcome.UnreachedCaptureProvided
            && laterCallResult.IndirectCallReached == false);

        DeferredControlCallbackPlan missingCallResult =
            DeferredControlCallbackProjection.Project(
                Capture(vtablePointer: 0x22334455, stackArgumentValue: 0x11223344,
                    callResult: null, returnedPointerByte: null));
        DeferredControlCallbackPlan laterByte =
            DeferredControlCallbackProjection.Project(
                Capture(vtablePointer: 0x22334455, stackArgumentValue: 0x11223344,
                    callResult: null, returnedPointerByte: 8));
        check("The indirect target identity is not required, but a normal-return EAX capture precedes its byte read",
            missingCallResult.Outcome == DeferredControlCallbackOutcome.MissingCallResult
            && missingCallResult.IndirectCallReached
            && missingCallResult.IndirectSlotAddress == 0x22334459
            && missingCallResult.ReturnedPointerByteAddress is null
            && missingCallResult.StopAtRawFileOffsetExclusive == 0x00BD37E4
            && laterByte.Outcome == DeferredControlCallbackOutcome.UnreachedCaptureProvided);

        DeferredControlCallbackPlan missingByte =
            DeferredControlCallbackProjection.Project(
                Capture(vtablePointer: 0x22334455, stackArgumentValue: 0x11223344,
                    callResult: 0x55667788));
        DeferredControlCallbackPlan nullReturnedPointer =
            DeferredControlCallbackProjection.Project(
                Capture(vtablePointer: 0x22334455, stackArgumentValue: 0x11223344,
                    callResult: 0, returnedPointerByte: 0));
        check("A missing returned byte stops at its read while EAX zero still attempts a captured byte read at address zero",
            missingByte.Outcome
                == DeferredControlCallbackOutcome.MissingReturnedPointerByte
            && missingByte.ReturnedPointerByteAddress == 0x55667788
            && missingByte.StopAtRawFileOffsetExclusive == 0x00BD37E6
            && nullReturnedPointer.Outcome
                == DeferredControlCallbackOutcome.MaskBitClearBranchesToZeroTarget
            && nullReturnedPointer.ReturnedPointerByteAddress == 0
            && nullReturnedPointer.TestedBits == 0);

        DeferredControlCallbackPlan separateArguments =
            DeferredControlCallbackProjection.Project(
                Capture(payload: 0x10203040, vtablePointer: 0x50607080,
                    stackArgumentValue: 0x90A0B0C0, callResult: 0xD0E0F000,
                    returnedPointerByte: 0x08));
        check("ECX receives wrapped payload+8, independently from the vtable pointer, and the pushed value is preserved",
            separateArguments.Outcome
                == DeferredControlCallbackOutcome.MaskBitSetFallsThrough
            && separateArguments.EcxArgumentAddress == 0x10203048
            && separateArguments.EcxArgumentAddress != separateArguments.Capture.VtablePointerAtPayload8
            && separateArguments.StackArgumentAddress == 0x10203044
            && separateArguments.Capture.StackArgumentValueAtPayload4 == 0x90A0B0C0
            && separateArguments.IndirectSlotAddress == 0x50607084
            && separateArguments.ReturnedPointerByteAddress == 0xD0E0F000
            && separateArguments.TestedBits == 8
            && separateArguments.RouteRawFileOffset == 0x00BD37EB
            && separateArguments.StopAtRawFileOffsetExclusive == 0x00BD37EB);

        bool allByteValuesMatchBitTest = true;
        for (int value = byte.MinValue; value <= byte.MaxValue; value++)
        {
            byte returnedByte = (byte)value;
            DeferredControlCallbackPlan plan =
                DeferredControlCallbackProjection.Project(
                    Capture(vtablePointer: 0x22334455,
                        stackArgumentValue: 0x11223344,
                        callResult: 0x55667788,
                        returnedPointerByte: returnedByte));
            bool bitSet = (returnedByte & 8) != 0;
            allByteValuesMatchBitTest &= plan.Outcome == (bitSet
                ? DeferredControlCallbackOutcome.MaskBitSetFallsThrough
                : DeferredControlCallbackOutcome.MaskBitClearBranchesToZeroTarget);
            allByteValuesMatchBitTest &= plan.TestedBits
                == (byte)(returnedByte & 8);
            allByteValuesMatchBitTest &= plan.RouteRawFileOffset
                == (bitSet ? 0x00BD37EBu : 0x00BD3805u);
        }

        check("Every returned byte value follows the bit-3 branch predicate",
            allByteValuesMatchBitTest);

        DeferredControlCallbackPlan wrappedAddresses =
            DeferredControlCallbackProjection.Project(
                Capture(
                    payload: uint.MaxValue - 7,
                    vtablePointer: uint.MaxValue - 1,
                    stackArgumentValue: 0x12345678,
                    callResult: 0,
                    returnedPointerByte: 8));
        check("Payload and slot addresses wrap as 32-bit values without inventing memory access",
            wrappedAddresses.VtablePointerAddress == 0
            && wrappedAddresses.EcxArgumentAddress == 0
            && wrappedAddresses.StackArgumentAddress == uint.MaxValue - 3
            && wrappedAddresses.IndirectSlotAddress == 2
            && wrappedAddresses.ReturnedPointerByteAddress == 0
            && wrappedAddresses.Outcome
                == DeferredControlCallbackOutcome.MaskBitSetFallsThrough);
    }

    private static DeferredControlCallbackCapture Capture(
        uint payload = 0x10203040,
        uint? vtablePointer = 0x22334455,
        uint? stackArgumentValue = 0x11223344,
        uint? callResult = 0x55667788,
        byte? returnedPointerByte = null) =>
        new(
            PayloadEdi: payload,
            VtablePointerAtPayload8: vtablePointer,
            StackArgumentValueAtPayload4: stackArgumentValue,
            CallResultEax: callResult,
            ReturnedPointerByte: returnedPointerByte);
}
