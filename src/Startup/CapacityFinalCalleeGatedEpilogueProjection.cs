using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeGatedEpilogueCapture(
    bool EntryValidated,
    uint EntryEsp,
    uint EntryEbp,
    byte? TestedByte,
    uint? StackPopEdiValue,
    uint? StackPopEsiValue,
    uint? StackPopEbxValue,
    uint? CallCellTargetVirtualAddress,
    bool? CallReturnedNormally,
    uint? PostCallEbp,
    uint? StackPopEbpValue,
    uint? ReturnTargetVirtualAddress);

public enum CapacityFinalCalleeGatedEpilogueOutcome
{
    EntryNotValidated,
    MissingTestedByte,
    MissingStackPopEdiValue,
    MissingStackPopEsiValue,
    MissingStackPopEbxValue,
    CallSkipped,
    MissingCallCellTarget,
    MissingCallReturnStatus,
    CallDidNotReturnNormally,
    MissingPostCallEbp,
    MissingStackPopEbpValue,
    MissingReturnTarget,
    ReturnedNormally
}

public enum CapacityFinalCalleeGatedEpilogueStackAccessKind
{
    PopEdi,
    PopEsi,
    PopEbx,
    PushImmediate,
    PopEbp,
    ReturnTarget
}

public readonly record struct CapacityFinalCalleeGatedEpilogueStackAccess(
    uint RawFileOffset,
    CapacityFinalCalleeGatedEpilogueStackAccessKind Kind,
    uint Address,
    uint? Value);

public readonly record struct CapacityFinalCalleeGatedEpilogueInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? PeVirtualAddressOperand = null,
    byte? ImmediateValue = null,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeGatedEpilogueTestFlags(
    bool Carry,
    bool Parity,
    bool? AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeGatedEpiloguePlan(
    CapacityFinalCalleeGatedEpilogueCapture Capture,
    CapacityFinalCalleeGatedEpilogueOutcome Outcome,
    bool TestReadReached,
    bool TestedByteCaptured,
    byte? TestedByte,
    byte? TestResult,
    CapacityFinalCalleeGatedEpilogueTestFlags? TestFlags,
    bool? TestedBitIsSet,
    bool StackPopEdiReached,
    uint? StackPopEdiAddress,
    uint? PoppedEdi,
    bool StackPopEsiReached,
    uint? StackPopEsiAddress,
    uint? PoppedEsi,
    bool StackPopEbxReached,
    uint? StackPopEbxAddress,
    uint? PoppedEbx,
    bool BranchReached,
    bool? BranchTaken,
    bool PushImmediateReached,
    uint? PushImmediateAddress,
    uint? PushedImmediateVirtualAddress,
    bool CallCellReadReached,
    bool CallCellTargetCaptured,
    uint? CallCellTargetVirtualAddress,
    bool CallReached,
    bool? CallReturnedNormally,
    bool PostCallEbpCaptureReached,
    bool PostCallEbpCaptured,
    uint? PostCallEbp,
    bool MovEspFromEbpReached,
    uint? MovEspSourceEbp,
    uint? EpilogueEsp,
    bool StackPopEbpReached,
    uint? StackPopEbpAddress,
    uint? PoppedEbp,
    bool ReturnTargetReadReached,
    uint? ReturnTargetStackAddress,
    bool ReturnTargetCaptured,
    uint? ReturnTargetVirtualAddress,
    uint? EspAfterReturn,
    bool ReturnReached,
    bool ExactCleanExitAdmitted,
    ImmutableArray<CapacityFinalCalleeGatedEpilogueStackAccess> StackAccesses,
    bool UnreachedTestedByteCaptureProvided,
    bool UnreachedStackPopEdiCaptureProvided,
    bool UnreachedStackPopEsiCaptureProvided,
    bool UnreachedStackPopEbxCaptureProvided,
    bool UnreachedCallCellTargetCaptureProvided,
    bool UnreachedCallReturnStatusProvided,
    bool UnreachedPostCallEbpCaptureProvided,
    bool UnreachedStackPopEbpCaptureProvided,
    bool UnreachedReturnTargetCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the observed TEST/POP/branch window and its bounded epilogue. The indirect call
/// remains opaque; register and stack captures after it are independent inputs.
/// </summary>
public static class CapacityFinalCalleeGatedEpilogueProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C9441;
    public const uint EntryVirtualAddress = 0x016CA041;
    public const uint BranchDestinationRawFileOffset = 0x012C9458;
    public const uint BranchDestinationVirtualAddress = 0x016CA058;
    public const uint CallRawFileOffset = 0x012C9452;
    public const uint ReturnRawFileOffset = 0x012C945B;
    public const uint ReturnBoundaryRawFileOffsetExclusive = 0x012C945C;
    public const uint ReturnBoundaryVirtualAddressExclusive = 0x016CA05C;

    public const uint TestByteVirtualAddress = 0x01EDAF84;
    public const byte TestMask = 0x02;
    public const uint PushedImmediateVirtualAddress = 0x01EDAF88;
    public const uint CallCellVirtualAddress = 0x01AC427C;
    public const uint StackSlotSize = 4;

    public const uint TestRawFileOffset = 0x012C9441;
    public const uint PopEdiRawFileOffset = 0x012C9448;
    public const uint PopEsiRawFileOffset = 0x012C9449;
    public const uint PopEbxRawFileOffset = 0x012C944A;
    public const uint BranchRawFileOffset = 0x012C944B;
    public const uint PushImmediateRawFileOffset = 0x012C944D;
    public const uint MovEspFromEbpRawFileOffset = 0x012C9458;
    public const uint PopEbpRawFileOffset = 0x012C945A;

    public const string CapturedWindowBytes =
        "F6 05 84 AF ED 01 02 5F 5E 5B 74 0B 68 88 AF ED 01 FF 15 7C 42 AC 01 8B E5 5D C3";

    public static ImmutableArray<CapacityFinalCalleeGatedEpilogueInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Test bit 1 of the byte at PE VA 1EDAF84",
                TestRawFileOffset, "F6 05 84 AF ED 01 02",
                peVirtualAddressOperand: TestByteVirtualAddress,
                immediateValue: TestMask),
            Instruction("Pop EDI", PopEdiRawFileOffset, "5F"),
            Instruction("Pop ESI", PopEsiRawFileOffset, "5E"),
            Instruction("Pop EBX", PopEbxRawFileOffset, "5B"),
            Instruction("JZ to the shared MOV ESP,EBP epilogue",
                BranchRawFileOffset, "74 0B",
                branchTargetRawFileOffset: BranchDestinationRawFileOffset,
                branchTargetVirtualAddress: BranchDestinationVirtualAddress),
            Instruction("Push PE VA 1EDAF88 on the nonzero fallthrough",
                PushImmediateRawFileOffset, "68 88 AF ED 01",
                peVirtualAddressOperand: PushedImmediateVirtualAddress),
            Instruction("Call through the cell at PE VA 1AC427C",
                CallRawFileOffset, "FF 15 7C 42 AC 01",
                peVirtualAddressOperand: CallCellVirtualAddress),
            Instruction("Set ESP from EBP",
                MovEspFromEbpRawFileOffset, "8B E5"),
            Instruction("Pop EBP", PopEbpRawFileOffset, "5D"),
            Instruction("Return", ReturnRawFileOffset, "C3"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint AddStackSlots(uint address, uint slots) =>
        unchecked(address + slots * StackSlotSize);

    public static uint PushAddress(uint stackPointer) =>
        unchecked(stackPointer - StackSlotSize);

    public static CapacityFinalCalleeGatedEpiloguePlan Project(
        CapacityFinalCalleeGatedEpilogueCapture capture)
    {
        bool testReadReached = false;
        byte? testResult = null;
        CapacityFinalCalleeGatedEpilogueTestFlags? testFlags = null;
        bool? testedBitIsSet = null;
        bool popEdiReached = false;
        uint? popEdiAddress = null;
        uint? poppedEdi = null;
        bool popEsiReached = false;
        uint? popEsiAddress = null;
        uint? poppedEsi = null;
        bool popEbxReached = false;
        uint? popEbxAddress = null;
        uint? poppedEbx = null;
        bool branchReached = false;
        bool? branchTaken = null;
        bool pushImmediateReached = false;
        uint? pushImmediateAddress = null;
        bool callCellReadReached = false;
        bool callCellTargetCaptured = false;
        uint? callCellTarget = null;
        bool callReached = false;
        bool? callReturnedNormally = null;
        bool postCallEbpCaptureReached = false;
        bool postCallEbpCaptured = false;
        uint? postCallEbp = null;
        bool movEspReached = false;
        uint? movEspSourceEbp = null;
        uint? epilogueEsp = null;
        bool popEbpReached = false;
        uint? popEbpAddress = null;
        uint? poppedEbp = null;
        bool returnTargetReadReached = false;
        uint? returnTargetStackAddress = null;
        bool returnTargetCaptured = false;
        uint? returnTarget = null;
        uint? espAfterReturn = null;
        var stackAccesses =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeGatedEpilogueStackAccess>();

        CapacityFinalCalleeGatedEpiloguePlan Finish(
            CapacityFinalCalleeGatedEpilogueOutcome outcome,
            uint stopAt,
            bool returnReached = false)
        {
            bool exactCleanExitAdmitted =
                returnReached
                && (outcome is CapacityFinalCalleeGatedEpilogueOutcome.CallSkipped
                    or CapacityFinalCalleeGatedEpilogueOutcome.ReturnedNormally)
                && stopAt == ReturnBoundaryRawFileOffsetExclusive
                && !(!testReadReached && capture.TestedByte.HasValue)
                && !(!popEdiReached && capture.StackPopEdiValue.HasValue)
                && !(!popEsiReached && capture.StackPopEsiValue.HasValue)
                && !(!popEbxReached && capture.StackPopEbxValue.HasValue)
                && !(!callCellReadReached
                    && capture.CallCellTargetVirtualAddress.HasValue)
                && !(!callReached && capture.CallReturnedNormally.HasValue)
                && !(!postCallEbpCaptured && capture.PostCallEbp.HasValue)
                && !(!popEbpReached && capture.StackPopEbpValue.HasValue)
                && !(!returnTargetReadReached
                    && capture.ReturnTargetVirtualAddress.HasValue);

            return new(
                capture,
                outcome,
                testReadReached,
                testReadReached && capture.TestedByte.HasValue,
                testReadReached ? capture.TestedByte : null,
                testResult,
                testFlags,
                testedBitIsSet,
                popEdiReached,
                popEdiAddress,
                poppedEdi,
                popEsiReached,
                popEsiAddress,
                poppedEsi,
                popEbxReached,
                popEbxAddress,
                poppedEbx,
                branchReached,
                branchTaken,
                pushImmediateReached,
                pushImmediateAddress,
                pushImmediateReached ? PushedImmediateVirtualAddress : null,
                callCellReadReached,
                callCellTargetCaptured,
                callCellTarget,
                callReached,
                callReached ? callReturnedNormally : null,
                postCallEbpCaptureReached,
                postCallEbpCaptured,
                postCallEbp,
                movEspReached,
                movEspSourceEbp,
                epilogueEsp,
                popEbpReached,
                popEbpAddress,
                poppedEbp,
                returnTargetReadReached,
                returnTargetStackAddress,
                returnTargetCaptured,
                returnTarget,
                espAfterReturn,
                returnReached,
                exactCleanExitAdmitted,
                stackAccesses.ToImmutable(),
                !testReadReached && capture.TestedByte.HasValue,
                !popEdiReached && capture.StackPopEdiValue.HasValue,
                !popEsiReached && capture.StackPopEsiValue.HasValue,
                !popEbxReached && capture.StackPopEbxValue.HasValue,
                !callCellReadReached && capture.CallCellTargetVirtualAddress.HasValue,
                !callReached && capture.CallReturnedNormally.HasValue,
                !postCallEbpCaptured && capture.PostCallEbp.HasValue,
                !popEbpReached && capture.StackPopEbpValue.HasValue,
                !returnTargetReadReached && capture.ReturnTargetVirtualAddress.HasValue,
                stopAt);
        }

        if (!capture.EntryValidated)
        {
            return Finish(
                CapacityFinalCalleeGatedEpilogueOutcome.EntryNotValidated,
                EntryRawFileOffset);
        }

        testReadReached = true;
        if (capture.TestedByte is not byte testedByte)
        {
            return Finish(
                CapacityFinalCalleeGatedEpilogueOutcome.MissingTestedByte,
                TestRawFileOffset);
        }

        byte result = (byte)(testedByte & TestMask);
        testResult = result;
        testFlags = new CapacityFinalCalleeGatedEpilogueTestFlags(
            Carry: false,
            Parity: HasEvenParity(result),
            AuxiliaryCarry: null,
            Zero: result == 0,
            Sign: (result & 0x80) != 0,
            Overflow: false);
        testedBitIsSet = !testFlags.Value.Zero;
        uint esp = capture.EntryEsp;

        popEdiReached = true;
        popEdiAddress = esp;
        stackAccesses.Add(new(
            PopEdiRawFileOffset,
            CapacityFinalCalleeGatedEpilogueStackAccessKind.PopEdi,
            esp,
            capture.StackPopEdiValue));
        if (capture.StackPopEdiValue is not uint edi)
        {
            return Finish(
                CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEdiValue,
                PopEdiRawFileOffset);
        }

        poppedEdi = edi;
        esp = AddStackSlots(esp, 1);

        popEsiReached = true;
        popEsiAddress = esp;
        stackAccesses.Add(new(
            PopEsiRawFileOffset,
            CapacityFinalCalleeGatedEpilogueStackAccessKind.PopEsi,
            esp,
            capture.StackPopEsiValue));
        if (capture.StackPopEsiValue is not uint esi)
        {
            return Finish(
                CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEsiValue,
                PopEsiRawFileOffset);
        }

        poppedEsi = esi;
        esp = AddStackSlots(esp, 1);

        popEbxReached = true;
        popEbxAddress = esp;
        stackAccesses.Add(new(
            PopEbxRawFileOffset,
            CapacityFinalCalleeGatedEpilogueStackAccessKind.PopEbx,
            esp,
            capture.StackPopEbxValue));
        if (capture.StackPopEbxValue is not uint ebx)
        {
            return Finish(
                CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEbxValue,
                PopEbxRawFileOffset);
        }

        poppedEbx = ebx;
        esp = AddStackSlots(esp, 1);
        branchReached = true;
        branchTaken = testFlags.Value.Zero;

        if (branchTaken.Value)
        {
            movEspReached = true;
            movEspSourceEbp = capture.EntryEbp;
            epilogueEsp = capture.EntryEbp;
            esp = capture.EntryEbp;
        }
        else
        {
            pushImmediateReached = true;
            pushImmediateAddress = PushAddress(esp);
            stackAccesses.Add(new(
                PushImmediateRawFileOffset,
                CapacityFinalCalleeGatedEpilogueStackAccessKind.PushImmediate,
                pushImmediateAddress.Value,
                PushedImmediateVirtualAddress));
            esp = pushImmediateAddress.Value;

            callCellReadReached = true;
            if (capture.CallCellTargetVirtualAddress is not uint target)
            {
                return Finish(
                    CapacityFinalCalleeGatedEpilogueOutcome.MissingCallCellTarget,
                    CallRawFileOffset);
            }

            callCellTargetCaptured = true;
            callCellTarget = target;
            callReached = true;
            if (capture.CallReturnedNormally is not bool returnedNormally)
            {
                return Finish(
                    CapacityFinalCalleeGatedEpilogueOutcome.MissingCallReturnStatus,
                    CallRawFileOffset);
            }

            callReturnedNormally = returnedNormally;
            if (!returnedNormally)
            {
                return Finish(
                    CapacityFinalCalleeGatedEpilogueOutcome.CallDidNotReturnNormally,
                    CallRawFileOffset);
            }

            postCallEbpCaptureReached = true;
            if (capture.PostCallEbp is not uint postCallFrame)
            {
                movEspReached = true;
                return Finish(
                    CapacityFinalCalleeGatedEpilogueOutcome.MissingPostCallEbp,
                    MovEspFromEbpRawFileOffset);
            }

            postCallEbpCaptured = true;
            postCallEbp = postCallFrame;
            movEspReached = true;
            movEspSourceEbp = postCallFrame;
            epilogueEsp = postCallFrame;
            esp = postCallFrame;
        }

        popEbpReached = true;
        popEbpAddress = esp;
        stackAccesses.Add(new(
            PopEbpRawFileOffset,
            CapacityFinalCalleeGatedEpilogueStackAccessKind.PopEbp,
            esp,
            capture.StackPopEbpValue));
        if (capture.StackPopEbpValue is not uint finalEbp)
        {
            return Finish(
                CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEbpValue,
                PopEbpRawFileOffset);
        }

        poppedEbp = finalEbp;
        esp = AddStackSlots(esp, 1);

        returnTargetReadReached = true;
        returnTargetStackAddress = esp;
        stackAccesses.Add(new(
            ReturnRawFileOffset,
            CapacityFinalCalleeGatedEpilogueStackAccessKind.ReturnTarget,
            esp,
            capture.ReturnTargetVirtualAddress));
        if (capture.ReturnTargetVirtualAddress is not uint returnAddress)
        {
            return Finish(
                CapacityFinalCalleeGatedEpilogueOutcome.MissingReturnTarget,
                ReturnRawFileOffset);
        }

        returnTargetCaptured = true;
        returnTarget = returnAddress;
        espAfterReturn = AddStackSlots(esp, 1);
        return Finish(
            branchTaken.Value
                ? CapacityFinalCalleeGatedEpilogueOutcome.CallSkipped
                : CapacityFinalCalleeGatedEpilogueOutcome.ReturnedNormally,
            ReturnBoundaryRawFileOffsetExclusive,
            returnReached: true);
    }

    private static CapacityFinalCalleeGatedEpilogueInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes,
        uint? peVirtualAddressOperand = null,
        byte? immediateValue = null,
        uint? branchTargetRawFileOffset = null,
        uint? branchTargetVirtualAddress = null) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes,
            peVirtualAddressOperand,
            immediateValue,
            branchTargetRawFileOffset,
            branchTargetVirtualAddress);

    private static bool HasEvenParity(byte value)
    {
        value ^= (byte)(value >> 4);
        value ^= (byte)(value >> 2);
        value ^= (byte)(value >> 1);
        return (value & 1) == 0;
    }
}
