using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UnreliableReceiveHighCountForwardCapture(
    bool ValidatedHighCountBoundary,
    uint ReceiverEdi,
    uint? ReceiverDwordAtOffset04,
    uint? ReceiverDwordAtOffset18,
    bool? ForwardHelperReturnedNormally,
    uint? ReceiverDwordAtOffset08,
    uint? ObjectDwordAtOffset1C,
    bool? SecondHelperReturnedNormally);

public enum UnreliableReceiveHighCountForwardOutcome
{
    EntryBoundaryNotValidated,
    MissingReceiverDwordAtOffset04,
    MissingReceiverDwordAtOffset18,
    MissingForwardHelperReturn,
    ForwardHelperDidNotReturnNormally,
    MissingReceiverDwordAtOffset08,
    MissingObjectDwordAtOffset1C,
    MissingSecondHelperReturn,
    SecondHelperDidNotReturnNormally,
    SecondHelperReturnedNormally,
    UnreachedCaptureProvided
}

public enum UnreliableReceiveHighCountForwardInstruction
{
    Push2D8,
    Push1BB8EC0,
    PushReceiverDwordAtOffset04,
    Push1BB8F0C,
    PushZero,
    PushReceiverDwordAtOffset18,
    CallForwardHelper,
    ReloadReceiverDwordAtOffset08IntoEax,
    LeaReceiverPlus3CIntoEcx,
    AddEsp18,
    IncrementObjectDwordAtOffset1C,
    CallSecondHelper
}

public readonly record struct UnreliableReceiveHighCountForwardPlan(
    UnreliableReceiveHighCountForwardCapture Capture,
    UnreliableReceiveHighCountForwardOutcome Outcome,
    uint? ReceiverDwordAtOffset04Address,
    bool ReceiverDwordAtOffset04Read,
    uint? CapturedReceiverDwordAtOffset04,
    uint? ReceiverDwordAtOffset18Address,
    bool ReceiverDwordAtOffset18Read,
    uint? CapturedReceiverDwordAtOffset18,
    ImmutableArray<uint> StackPushesInInstructionOrder,
    ImmutableArray<UnreliableReceiveHighCountForwardInstruction> InstructionsExecuted,
    bool ForwardCallReached,
    bool? ForwardHelperReturnedNormally,
    uint? ReceiverDwordAtOffset08Address,
    bool ReceiverDwordAtOffset08Read,
    uint? LoadedObjectPointerEax,
    uint? ReceiverPlus3CAddressPassedInEcx,
    bool StackCleanupProjected,
    byte? StackCleanupBytes,
    uint? ObjectDwordAtOffset1CAddress,
    bool ObjectDwordAtOffset1CRead,
    uint? CounterValueBeforeIncrement,
    bool CounterIncrementProjected,
    uint? CounterValueAfterIncrement,
    bool SecondCallReached,
    bool? SecondHelperReturnedNormally,
    bool NormalCallBoundaryReached,
    bool UnreachedReceiverDwordAtOffset04CaptureProvided,
    bool UnreachedReceiverDwordAtOffset18CaptureProvided,
    bool UnreachedForwardHelperReturnCaptureProvided,
    bool UnreachedReceiverDwordAtOffset08CaptureProvided,
    bool UnreachedObjectDwordAtOffset1CCaptureProvided,
    bool UnreachedSecondHelperReturnCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct UnreliableReceiveHighCountForwardInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? CallTargetRawFileOffset = null,
    uint? CallTargetVirtualAddress = null);

/// <summary>
/// Projects the validated high-count forwarding path through the second helper's return boundary.
/// </summary>
public static class UnreliableReceiveHighCountForwardProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE5BC4;
    public const uint EntryVirtualAddress = 0x00FE67C4;
    public const uint ForwardCallRawFileOffset = 0x00BE5BDB;
    public const uint ForwardCallTargetRawFileOffset = 0x00BE2F50;
    public const uint ForwardCallTargetVirtualAddress = 0x00FE3B50;
    public const uint ForwardCallReturnRawFileOffset = 0x00BE5BE0;
    public const uint ReceiverDwordAtOffset08ReadRawFileOffset = ForwardCallReturnRawFileOffset;
    public const uint StackCleanupRawFileOffset = 0x00BE5BE6;
    public const uint CounterIncrementRawFileOffset = 0x00BE5BE9;
    public const uint SecondCallRawFileOffset = 0x00BE5BEC;
    public const uint SecondCallTargetRawFileOffset = 0x00BE6440;
    public const uint SecondCallTargetVirtualAddress = 0x00FE7040;
    public const uint EndRawFileOffsetExclusive = 0x00BE5BF1;
    public const uint EndVirtualAddressExclusive = 0x00FE67F1;

    public const uint ReceiverDwordAtOffset04 = 0x04;
    public const uint ReceiverDwordAtOffset08 = 0x08;
    public const uint ReceiverDwordAtOffset18 = 0x18;
    public const uint ObjectDwordAtOffset1C = 0x1C;
    public const uint ReceiverPlus3COffset = 0x3C;
    public const uint PushImmediate2D8 = 0x2D8;
    public const uint PushImmediate1BB8EC0 = 0x01BB8EC0;
    public const uint PushImmediate1BB8F0C = 0x01BB8F0C;
    public const byte StackCleanupByteCount = 0x18;

    public const string CapturedWindowBytes =
        "68 D8 02 00 00 68 C0 8E BB 01 FF 77 04 68 0C 8F BB 01 "
        + "6A 00 FF 77 18 E8 70 D3 FF FF 8B 47 08 8D 4F 3C "
        + "83 C4 18 FF 40 1C E8 4F 08 00 00";

    public static ImmutableArray<UnreliableReceiveHighCountForwardInstruction> InstructionsInOrder { get; } =
        ImmutableArray.Create(
            UnreliableReceiveHighCountForwardInstruction.Push2D8,
            UnreliableReceiveHighCountForwardInstruction.Push1BB8EC0,
            UnreliableReceiveHighCountForwardInstruction.PushReceiverDwordAtOffset04,
            UnreliableReceiveHighCountForwardInstruction.Push1BB8F0C,
            UnreliableReceiveHighCountForwardInstruction.PushZero,
            UnreliableReceiveHighCountForwardInstruction.PushReceiverDwordAtOffset18,
            UnreliableReceiveHighCountForwardInstruction.CallForwardHelper,
            UnreliableReceiveHighCountForwardInstruction.ReloadReceiverDwordAtOffset08IntoEax,
            UnreliableReceiveHighCountForwardInstruction.LeaReceiverPlus3CIntoEcx,
            UnreliableReceiveHighCountForwardInstruction.AddEsp18,
            UnreliableReceiveHighCountForwardInstruction.IncrementObjectDwordAtOffset1C,
            UnreliableReceiveHighCountForwardInstruction.CallSecondHelper);

    public static ImmutableArray<UnreliableReceiveHighCountForwardInstructionEvidence> Evidence { get; } =
        ImmutableArray.Create(
            new UnreliableReceiveHighCountForwardInstructionEvidence(
                "Push immediate 0x2D8",
                EntryRawFileOffset,
                RawFileOffsetToVirtualAddress(EntryRawFileOffset),
                "68 D8 02 00 00"),
            new UnreliableReceiveHighCountForwardInstructionEvidence(
                "Push immediate 0x01BB8EC0",
                0x00BE5BC9,
                RawFileOffsetToVirtualAddress(0x00BE5BC9),
                "68 C0 8E BB 01"),
            new UnreliableReceiveHighCountForwardInstructionEvidence(
                "Push the DWORD read from [EDI+4]",
                0x00BE5BCE,
                RawFileOffsetToVirtualAddress(0x00BE5BCE),
                "FF 77 04"),
            new UnreliableReceiveHighCountForwardInstructionEvidence(
                "Push immediate 0x01BB8F0C",
                0x00BE5BD1,
                RawFileOffsetToVirtualAddress(0x00BE5BD1),
                "68 0C 8F BB 01"),
            new UnreliableReceiveHighCountForwardInstructionEvidence(
                "Push zero",
                0x00BE5BD6,
                RawFileOffsetToVirtualAddress(0x00BE5BD6),
                "6A 00"),
            new UnreliableReceiveHighCountForwardInstructionEvidence(
                "Push the DWORD read from [EDI+0x18]",
                0x00BE5BD8,
                RawFileOffsetToVirtualAddress(0x00BE5BD8),
                "FF 77 18"),
            new UnreliableReceiveHighCountForwardInstructionEvidence(
                "Call raw 0xBE2F50; the normal return continues at raw 0xBE5BE0",
                ForwardCallRawFileOffset,
                RawFileOffsetToVirtualAddress(ForwardCallRawFileOffset),
                "E8 70 D3 FF FF",
                ForwardCallTargetRawFileOffset,
                ForwardCallTargetVirtualAddress),
            new UnreliableReceiveHighCountForwardInstructionEvidence(
                "Independently reload the DWORD at [EDI+8] into EAX",
                ReceiverDwordAtOffset08ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverDwordAtOffset08ReadRawFileOffset),
                "8B 47 08"),
            new UnreliableReceiveHighCountForwardInstructionEvidence(
                "Form EDI+0x3C in ECX",
                0x00BE5BE3,
                RawFileOffsetToVirtualAddress(0x00BE5BE3),
                "8D 4F 3C"),
            new UnreliableReceiveHighCountForwardInstructionEvidence(
                "Add 0x18 to ESP",
                StackCleanupRawFileOffset,
                RawFileOffsetToVirtualAddress(StackCleanupRawFileOffset),
                "83 C4 18"),
            new UnreliableReceiveHighCountForwardInstructionEvidence(
                "Increment the DWORD at [EAX+0x1C] with DWORD wraparound",
                CounterIncrementRawFileOffset,
                RawFileOffsetToVirtualAddress(CounterIncrementRawFileOffset),
                "FF 40 1C"),
            new UnreliableReceiveHighCountForwardInstructionEvidence(
                "Call raw 0xBE6440; normal return reaches exclusive raw 0xBE5BF1",
                SecondCallRawFileOffset,
                RawFileOffsetToVirtualAddress(SecondCallRawFileOffset),
                "E8 4F 08 00 00",
                SecondCallTargetRawFileOffset,
                SecondCallTargetVirtualAddress));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ReceiverDwordAtOffset04Address(uint receiverEdi) =>
        unchecked(receiverEdi + ReceiverDwordAtOffset04);

    public static uint ReceiverDwordAtOffset08Address(uint receiverEdi) =>
        unchecked(receiverEdi + ReceiverDwordAtOffset08);

    public static uint ReceiverDwordAtOffset18Address(uint receiverEdi) =>
        unchecked(receiverEdi + ReceiverDwordAtOffset18);

    public static uint ReceiverPlus3CAddress(uint receiverEdi) =>
        unchecked(receiverEdi + ReceiverPlus3COffset);

    public static uint ObjectDwordAtOffset1CAddress(uint objectPointer) =>
        unchecked(objectPointer + ObjectDwordAtOffset1C);

    public static uint RelativeCallTarget(uint instructionRawFileOffset, int displacement) =>
        unchecked(instructionRawFileOffset + 5u + (uint)displacement);

    public static UnreliableReceiveHighCountForwardPlan Project(
        UnreliableReceiveHighCountForwardCapture capture)
    {
        if (!capture.ValidatedHighCountBoundary)
        {
            return Stop(
                capture,
                UnreliableReceiveHighCountForwardOutcome.EntryBoundaryNotValidated,
                unreachedReceiverDwordAtOffset18CaptureProvided: capture.ReceiverDwordAtOffset18.HasValue,
                unreachedForwardHelperReturnCaptureProvided: capture.ForwardHelperReturnedNormally.HasValue,
                unreachedReceiverDwordAtOffset08CaptureProvided: capture.ReceiverDwordAtOffset08.HasValue,
                unreachedObjectDwordAtOffset1CCaptureProvided: capture.ObjectDwordAtOffset1C.HasValue,
                unreachedSecondHelperReturnCaptureProvided: capture.SecondHelperReturnedNormally.HasValue,
                stopAt: EntryRawFileOffset);
        }

        ImmutableArray<uint>.Builder pushes = ImmutableArray.CreateBuilder<uint>(6);
        ImmutableArray<UnreliableReceiveHighCountForwardInstruction>.Builder instructions =
            ImmutableArray.CreateBuilder<UnreliableReceiveHighCountForwardInstruction>(InstructionsInOrder.Length);
        pushes.Add(PushImmediate2D8);
        instructions.Add(UnreliableReceiveHighCountForwardInstruction.Push2D8);
        pushes.Add(PushImmediate1BB8EC0);
        instructions.Add(UnreliableReceiveHighCountForwardInstruction.Push1BB8EC0);

        uint receiverDwordAtOffset04Address = ReceiverDwordAtOffset04Address(capture.ReceiverEdi);
        if (capture.ReceiverDwordAtOffset04 is not uint receiverDwordAtOffset04)
        {
            return Stop(
                capture,
                HasUnreachedCaptureAfterReceiver04(capture)
                    ? UnreliableReceiveHighCountForwardOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveHighCountForwardOutcome.MissingReceiverDwordAtOffset04,
                receiverDwordAtOffset04Address: receiverDwordAtOffset04Address,
                stackPushes: pushes.ToImmutable(),
                instructions: instructions.ToImmutable(),
                unreachedReceiverDwordAtOffset18CaptureProvided: capture.ReceiverDwordAtOffset18.HasValue,
                unreachedForwardHelperReturnCaptureProvided: capture.ForwardHelperReturnedNormally.HasValue,
                unreachedReceiverDwordAtOffset08CaptureProvided: capture.ReceiverDwordAtOffset08.HasValue,
                unreachedObjectDwordAtOffset1CCaptureProvided: capture.ObjectDwordAtOffset1C.HasValue,
                unreachedSecondHelperReturnCaptureProvided: capture.SecondHelperReturnedNormally.HasValue,
                stopAt: 0x00BE5BCE);
        }

        pushes.Add(receiverDwordAtOffset04);
        instructions.Add(UnreliableReceiveHighCountForwardInstruction.PushReceiverDwordAtOffset04);
        pushes.Add(PushImmediate1BB8F0C);
        instructions.Add(UnreliableReceiveHighCountForwardInstruction.Push1BB8F0C);
        pushes.Add(0);
        instructions.Add(UnreliableReceiveHighCountForwardInstruction.PushZero);

        uint receiverDwordAtOffset18Address = ReceiverDwordAtOffset18Address(capture.ReceiverEdi);
        if (capture.ReceiverDwordAtOffset18 is not uint receiverDwordAtOffset18)
        {
            return Stop(
                capture,
                HasUnreachedCaptureAfterReceiver18(capture)
                    ? UnreliableReceiveHighCountForwardOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveHighCountForwardOutcome.MissingReceiverDwordAtOffset18,
                receiverDwordAtOffset04Address: receiverDwordAtOffset04Address,
                receiverDwordAtOffset04Read: true,
                capturedReceiverDwordAtOffset04: receiverDwordAtOffset04,
                receiverDwordAtOffset18Address: receiverDwordAtOffset18Address,
                stackPushes: pushes.ToImmutable(),
                instructions: instructions.ToImmutable(),
                unreachedForwardHelperReturnCaptureProvided: capture.ForwardHelperReturnedNormally.HasValue,
                unreachedReceiverDwordAtOffset08CaptureProvided: capture.ReceiverDwordAtOffset08.HasValue,
                unreachedObjectDwordAtOffset1CCaptureProvided: capture.ObjectDwordAtOffset1C.HasValue,
                unreachedSecondHelperReturnCaptureProvided: capture.SecondHelperReturnedNormally.HasValue,
                stopAt: 0x00BE5BD8);
        }

        pushes.Add(receiverDwordAtOffset18);
        instructions.Add(UnreliableReceiveHighCountForwardInstruction.PushReceiverDwordAtOffset18);
        instructions.Add(UnreliableReceiveHighCountForwardInstruction.CallForwardHelper);

        if (capture.ForwardHelperReturnedNormally is not bool forwardHelperReturnedNormally)
        {
            return Stop(
                capture,
                HasUnreachedCaptureAfterForwardCall(capture)
                    ? UnreliableReceiveHighCountForwardOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveHighCountForwardOutcome.MissingForwardHelperReturn,
                receiverDwordAtOffset04Address: receiverDwordAtOffset04Address,
                receiverDwordAtOffset04Read: true,
                capturedReceiverDwordAtOffset04: receiverDwordAtOffset04,
                receiverDwordAtOffset18Address: receiverDwordAtOffset18Address,
                receiverDwordAtOffset18Read: true,
                capturedReceiverDwordAtOffset18: receiverDwordAtOffset18,
                stackPushes: pushes.ToImmutable(),
                instructions: instructions.ToImmutable(),
                forwardCallReached: true,
                unreachedReceiverDwordAtOffset08CaptureProvided: capture.ReceiverDwordAtOffset08.HasValue,
                unreachedObjectDwordAtOffset1CCaptureProvided: capture.ObjectDwordAtOffset1C.HasValue,
                unreachedSecondHelperReturnCaptureProvided: capture.SecondHelperReturnedNormally.HasValue,
                stopAt: ForwardCallRawFileOffset);
        }

        if (!forwardHelperReturnedNormally)
        {
            return Stop(
                capture,
                HasUnreachedCaptureAfterForwardCall(capture)
                    ? UnreliableReceiveHighCountForwardOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveHighCountForwardOutcome.ForwardHelperDidNotReturnNormally,
                receiverDwordAtOffset04Address: receiverDwordAtOffset04Address,
                receiverDwordAtOffset04Read: true,
                capturedReceiverDwordAtOffset04: receiverDwordAtOffset04,
                receiverDwordAtOffset18Address: receiverDwordAtOffset18Address,
                receiverDwordAtOffset18Read: true,
                capturedReceiverDwordAtOffset18: receiverDwordAtOffset18,
                stackPushes: pushes.ToImmutable(),
                instructions: instructions.ToImmutable(),
                forwardCallReached: true,
                forwardHelperReturnedNormally: false,
                unreachedReceiverDwordAtOffset08CaptureProvided: capture.ReceiverDwordAtOffset08.HasValue,
                unreachedObjectDwordAtOffset1CCaptureProvided: capture.ObjectDwordAtOffset1C.HasValue,
                unreachedSecondHelperReturnCaptureProvided: capture.SecondHelperReturnedNormally.HasValue,
                stopAt: ForwardCallRawFileOffset);
        }

        instructions.Add(UnreliableReceiveHighCountForwardInstruction.ReloadReceiverDwordAtOffset08IntoEax);
        uint receiverDwordAtOffset08Address = ReceiverDwordAtOffset08Address(capture.ReceiverEdi);
        if (capture.ReceiverDwordAtOffset08 is not uint loadedObjectPointerEax)
        {
            return Stop(
                capture,
                HasUnreachedCaptureAfterReceiver08(capture)
                    ? UnreliableReceiveHighCountForwardOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveHighCountForwardOutcome.MissingReceiverDwordAtOffset08,
                receiverDwordAtOffset04Address: receiverDwordAtOffset04Address,
                receiverDwordAtOffset04Read: true,
                capturedReceiverDwordAtOffset04: receiverDwordAtOffset04,
                receiverDwordAtOffset18Address: receiverDwordAtOffset18Address,
                receiverDwordAtOffset18Read: true,
                capturedReceiverDwordAtOffset18: receiverDwordAtOffset18,
                stackPushes: pushes.ToImmutable(),
                instructions: instructions.ToImmutable(),
                forwardCallReached: true,
                forwardHelperReturnedNormally: true,
                receiverDwordAtOffset08Address: receiverDwordAtOffset08Address,
                unreachedObjectDwordAtOffset1CCaptureProvided: capture.ObjectDwordAtOffset1C.HasValue,
                unreachedSecondHelperReturnCaptureProvided: capture.SecondHelperReturnedNormally.HasValue,
                stopAt: ReceiverDwordAtOffset08ReadRawFileOffset);
        }

        uint receiverPlus3CAddress = ReceiverPlus3CAddress(capture.ReceiverEdi);
        instructions.Add(UnreliableReceiveHighCountForwardInstruction.LeaReceiverPlus3CIntoEcx);
        instructions.Add(UnreliableReceiveHighCountForwardInstruction.AddEsp18);

        uint objectDwordAtOffset1CAddress = ObjectDwordAtOffset1CAddress(loadedObjectPointerEax);
        if (capture.ObjectDwordAtOffset1C is not uint counterValue)
        {
            return Stop(
                capture,
                capture.SecondHelperReturnedNormally.HasValue
                    ? UnreliableReceiveHighCountForwardOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveHighCountForwardOutcome.MissingObjectDwordAtOffset1C,
                receiverDwordAtOffset04Address: receiverDwordAtOffset04Address,
                receiverDwordAtOffset04Read: true,
                capturedReceiverDwordAtOffset04: receiverDwordAtOffset04,
                receiverDwordAtOffset18Address: receiverDwordAtOffset18Address,
                receiverDwordAtOffset18Read: true,
                capturedReceiverDwordAtOffset18: receiverDwordAtOffset18,
                stackPushes: pushes.ToImmutable(),
                instructions: instructions.ToImmutable(),
                forwardCallReached: true,
                forwardHelperReturnedNormally: true,
                receiverDwordAtOffset08Address: receiverDwordAtOffset08Address,
                receiverDwordAtOffset08Read: true,
                loadedObjectPointerEax: loadedObjectPointerEax,
                receiverPlus3CAddressPassedInEcx: receiverPlus3CAddress,
                stackCleanupProjected: true,
                stackCleanupBytes: StackCleanupByteCount,
                objectDwordAtOffset1CAddress: objectDwordAtOffset1CAddress,
                unreachedSecondHelperReturnCaptureProvided: capture.SecondHelperReturnedNormally.HasValue,
                stopAt: CounterIncrementRawFileOffset);
        }

        uint incrementedCounter = unchecked(counterValue + 1);
        instructions.Add(UnreliableReceiveHighCountForwardInstruction.IncrementObjectDwordAtOffset1C);
        instructions.Add(UnreliableReceiveHighCountForwardInstruction.CallSecondHelper);

        if (capture.SecondHelperReturnedNormally is not bool secondHelperReturnedNormally)
        {
            return Stop(
                capture,
                UnreliableReceiveHighCountForwardOutcome.MissingSecondHelperReturn,
                receiverDwordAtOffset04Address: receiverDwordAtOffset04Address,
                receiverDwordAtOffset04Read: true,
                capturedReceiverDwordAtOffset04: receiverDwordAtOffset04,
                receiverDwordAtOffset18Address: receiverDwordAtOffset18Address,
                receiverDwordAtOffset18Read: true,
                capturedReceiverDwordAtOffset18: receiverDwordAtOffset18,
                stackPushes: pushes.ToImmutable(),
                instructions: instructions.ToImmutable(),
                forwardCallReached: true,
                forwardHelperReturnedNormally: true,
                receiverDwordAtOffset08Address: receiverDwordAtOffset08Address,
                receiverDwordAtOffset08Read: true,
                loadedObjectPointerEax: loadedObjectPointerEax,
                receiverPlus3CAddressPassedInEcx: receiverPlus3CAddress,
                stackCleanupProjected: true,
                stackCleanupBytes: StackCleanupByteCount,
                objectDwordAtOffset1CAddress: objectDwordAtOffset1CAddress,
                objectDwordAtOffset1CRead: true,
                counterValueBeforeIncrement: counterValue,
                counterIncrementProjected: true,
                counterValueAfterIncrement: incrementedCounter,
                secondCallReached: true,
                stopAt: SecondCallRawFileOffset);
        }

        return Stop(
            capture,
            secondHelperReturnedNormally
                ? UnreliableReceiveHighCountForwardOutcome.SecondHelperReturnedNormally
                : UnreliableReceiveHighCountForwardOutcome.SecondHelperDidNotReturnNormally,
            receiverDwordAtOffset04Address: receiverDwordAtOffset04Address,
            receiverDwordAtOffset04Read: true,
            capturedReceiverDwordAtOffset04: receiverDwordAtOffset04,
            receiverDwordAtOffset18Address: receiverDwordAtOffset18Address,
            receiverDwordAtOffset18Read: true,
            capturedReceiverDwordAtOffset18: receiverDwordAtOffset18,
            stackPushes: pushes.ToImmutable(),
            instructions: instructions.ToImmutable(),
            forwardCallReached: true,
            forwardHelperReturnedNormally: true,
            receiverDwordAtOffset08Address: receiverDwordAtOffset08Address,
            receiverDwordAtOffset08Read: true,
            loadedObjectPointerEax: loadedObjectPointerEax,
            receiverPlus3CAddressPassedInEcx: receiverPlus3CAddress,
            stackCleanupProjected: true,
            stackCleanupBytes: StackCleanupByteCount,
            objectDwordAtOffset1CAddress: objectDwordAtOffset1CAddress,
            objectDwordAtOffset1CRead: true,
            counterValueBeforeIncrement: counterValue,
            counterIncrementProjected: true,
            counterValueAfterIncrement: incrementedCounter,
            secondCallReached: true,
            secondHelperReturnedNormally: secondHelperReturnedNormally,
            normalCallBoundaryReached: secondHelperReturnedNormally,
            stopAt: secondHelperReturnedNormally
                ? EndRawFileOffsetExclusive
                : SecondCallRawFileOffset);
    }

    private static bool HasUnreachedCaptureAfterReceiver04(
        UnreliableReceiveHighCountForwardCapture capture) =>
        capture.ReceiverDwordAtOffset18.HasValue
        || capture.ForwardHelperReturnedNormally.HasValue
        || capture.ReceiverDwordAtOffset08.HasValue
        || capture.ObjectDwordAtOffset1C.HasValue
        || capture.SecondHelperReturnedNormally.HasValue;

    private static bool HasUnreachedCaptureAfterReceiver18(
        UnreliableReceiveHighCountForwardCapture capture) =>
        capture.ForwardHelperReturnedNormally.HasValue
        || capture.ReceiverDwordAtOffset08.HasValue
        || capture.ObjectDwordAtOffset1C.HasValue
        || capture.SecondHelperReturnedNormally.HasValue;

    private static bool HasUnreachedCaptureAfterForwardCall(
        UnreliableReceiveHighCountForwardCapture capture) =>
        capture.ReceiverDwordAtOffset08.HasValue
        || capture.ObjectDwordAtOffset1C.HasValue
        || capture.SecondHelperReturnedNormally.HasValue;

    private static bool HasUnreachedCaptureAfterReceiver08(
        UnreliableReceiveHighCountForwardCapture capture) =>
        capture.ObjectDwordAtOffset1C.HasValue
        || capture.SecondHelperReturnedNormally.HasValue;

    private static UnreliableReceiveHighCountForwardPlan Stop(
        UnreliableReceiveHighCountForwardCapture capture,
        UnreliableReceiveHighCountForwardOutcome outcome,
        uint? receiverDwordAtOffset04Address = null,
        bool receiverDwordAtOffset04Read = false,
        uint? capturedReceiverDwordAtOffset04 = null,
        uint? receiverDwordAtOffset18Address = null,
        bool receiverDwordAtOffset18Read = false,
        uint? capturedReceiverDwordAtOffset18 = null,
        ImmutableArray<uint> stackPushes = default,
        ImmutableArray<UnreliableReceiveHighCountForwardInstruction> instructions = default,
        bool forwardCallReached = false,
        bool? forwardHelperReturnedNormally = null,
        uint? receiverDwordAtOffset08Address = null,
        bool receiverDwordAtOffset08Read = false,
        uint? loadedObjectPointerEax = null,
        uint? receiverPlus3CAddressPassedInEcx = null,
        bool stackCleanupProjected = false,
        byte? stackCleanupBytes = null,
        uint? objectDwordAtOffset1CAddress = null,
        bool objectDwordAtOffset1CRead = false,
        uint? counterValueBeforeIncrement = null,
        bool counterIncrementProjected = false,
        uint? counterValueAfterIncrement = null,
        bool secondCallReached = false,
        bool? secondHelperReturnedNormally = null,
        bool normalCallBoundaryReached = false,
        bool unreachedReceiverDwordAtOffset18CaptureProvided = false,
        bool unreachedForwardHelperReturnCaptureProvided = false,
        bool unreachedReceiverDwordAtOffset08CaptureProvided = false,
        bool unreachedObjectDwordAtOffset1CCaptureProvided = false,
        bool unreachedSecondHelperReturnCaptureProvided = false,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            receiverDwordAtOffset04Address,
            receiverDwordAtOffset04Read,
            capturedReceiverDwordAtOffset04,
            receiverDwordAtOffset18Address,
            receiverDwordAtOffset18Read,
            capturedReceiverDwordAtOffset18,
            stackPushes.IsDefault ? ImmutableArray<uint>.Empty : stackPushes,
            instructions.IsDefault
                ? ImmutableArray<UnreliableReceiveHighCountForwardInstruction>.Empty
                : instructions,
            forwardCallReached,
            forwardHelperReturnedNormally,
            receiverDwordAtOffset08Address,
            receiverDwordAtOffset08Read,
            loadedObjectPointerEax,
            receiverPlus3CAddressPassedInEcx,
            stackCleanupProjected,
            stackCleanupBytes,
            objectDwordAtOffset1CAddress,
            objectDwordAtOffset1CRead,
            counterValueBeforeIncrement,
            counterIncrementProjected,
            counterValueAfterIncrement,
            secondCallReached,
            secondHelperReturnedNormally,
            normalCallBoundaryReached,
            outcome == UnreliableReceiveHighCountForwardOutcome.EntryBoundaryNotValidated
                ? capture.ReceiverDwordAtOffset04.HasValue
                : false,
            unreachedReceiverDwordAtOffset18CaptureProvided,
            unreachedForwardHelperReturnCaptureProvided,
            unreachedReceiverDwordAtOffset08CaptureProvided,
            unreachedObjectDwordAtOffset1CCaptureProvided,
            unreachedSecondHelperReturnCaptureProvided,
            stopAt);
}
