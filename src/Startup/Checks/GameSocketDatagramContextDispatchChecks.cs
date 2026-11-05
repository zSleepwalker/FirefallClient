using Firefall.Reconstruction.Startup;

internal static class GameSocketDatagramContextDispatchChecks
{
    public static void Run(Action<string, bool> check)
    {
        static GameSocketSenderBindingProjectionInputs Parent(
            byte? bindingFlag = 0,
            GameSocketPacketAdmissionOutcome admission =
                GameSocketPacketAdmissionOutcome.AcceptedNonzeroContext,
            uint? socket = 0x1000,
            uint? sender = null,
            bool? receiverReturn = null,
            bool? predicateReturn = null,
            byte? predicateAl = null,
            bool? fallbackReturn = null,
            bool? queueReturn = null) =>
            new(
                admission,
                bindingFlag,
                socket,
                sender,
                receiverReturn,
                predicateReturn,
                predicateAl,
                fallbackReturn,
                queueReturn);

        static GameSocketDatagramContextDispatchProjectionResult Project(
            GameSocketSenderBindingProjectionInputs? parent = null,
            uint? count = 10,
            uint? context = 0,
            bool? zeroReturn = true,
            bool? nonzeroReturn = null,
            uint? highWater = null) =>
            GameSocketDatagramContextDispatchProjection.Project(
                parent ?? Parent(),
                count,
                context,
                zeroReturn,
                nonzeroReturn,
                highWater);

        var zero = Project();
        check("Context dispatch begins at the exact clean sender-binding tail and compares fresh ESI context",
            zero.ParentProjection.IsCleanCompletion
            && zero.ParentProjection.StopAtRaw
                == GameSocketSenderBindingProjection.CommonTailFileOffset
            && zero.FreshContextReadAttempted
            && zero.FreshContextReadAddress == 0x1114
            && zero.FreshContextReadValue == 0
            && zero.ParserTargetFileOffset
                == GameSocketDatagramContextDispatchProjection.ZeroContextParserFileOffset
            && zero.ParserCallAttempted
            && zero.ParserReturnedNormally == true
            && zero.Stop == GameSocketDatagramContextDispatchStop.ZeroParserReturnedAtBoundedD7
            && zero.StopAtRaw
                == GameSocketDatagramContextDispatchProjection.ZeroParserReturnFileOffset
            && zero.ReachedBoundedEndpoint
            && zero.IsCleanCompletion
            && zero.ParserLength == 6
            && zero.ParserBufferAddress == 0x127C
            && zero.FlagsAfterFreshContextCompare is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zero.FlagsAtStop is null
            && zero.OrderedMemoryEffects.Select(e => e.Kind).SequenceEqual(
            [
                GameSocketDatagramContextDispatchMemoryEffectKind.ParentBindingFlagRead,
                GameSocketDatagramContextDispatchMemoryEffectKind.FreshContextWordRead
            ])
            && zero.OrderedMemoryEffects[0].Address == 0x125A
            && zero.OrderedMemoryEffects[0].Value == 0
            && zero.OrderedMemoryEffects[0].ReadCommitted == false
            && zero.OrderedMemoryEffects[1].Address == 0x1114
            && zero.OrderedMemoryEffects[1].Value == 0
            && zero.OrderedMemoryEffects[1].ReadCommitted == false
            && zero.FreshContextReadCommitted == false
            && !zero.BoundedTargetEntered
            && !zero.ReturnInstructionModeled);

        var expectedZeroOrder = new[]
        {
            (GameSocketDatagramContextDispatchInstructionKind.CompareFreshContextWord, 0xBD30BCu),
            (GameSocketDatagramContextDispatchInstructionKind.ComputeParserLength, 0xBD30C3u),
            (GameSocketDatagramContextDispatchInstructionKind.PushParserLength, 0xBD30C6u),
            (GameSocketDatagramContextDispatchInstructionKind.LoadParserBufferAddress, 0xBD30C7u),
            (GameSocketDatagramContextDispatchInstructionKind.MoveEcxFromSocketEsi, 0xBD30CDu),
            (GameSocketDatagramContextDispatchInstructionKind.PushParserBufferAddress, 0xBD30CFu),
            (GameSocketDatagramContextDispatchInstructionKind.BranchIfFreshContextNonzero, 0xBD30D0u),
            (GameSocketDatagramContextDispatchInstructionKind.CallZeroContextParser, 0xBD30D2u)
        };
        check("Zero-context trace preserves native instruction and parser-call order without modeling stack addresses",
            zero.InstructionOrder.Select(i => (i.Kind, i.FileOffset)).SequenceEqual(expectedZeroOrder)
            && zero.InstructionOrder[1].OperandValue == 10
            && zero.InstructionOrder[2].OperandValue == 6
            && zero.InstructionOrder[5].OperandValue == 0x127C
            && zero.InstructionOrder[7].TargetFileOffset == 0xBD39D0
            && zero.InstructionOrder.All(i => i.EffectiveAddress != 0)
            && !zero.OpaqueParsersExecuted);

        var enabledParent = Project(
            Parent(
                bindingFlag: 1,
                sender: 0x2000,
                receiverReturn: true,
                predicateReturn: true,
                predicateAl: 0xFF,
                queueReturn: true),
            context: 0x8000_0000,
            count: 0x7FFF_FFFF,
            zeroReturn: null,
            nonzeroReturn: true,
            highWater: 0x8000_0000);
        check("Enabled clean parent admits the exact tail while a fresh nonzero context selects its own parser",
            enabledParent.ParentProjection.IsCleanCompletion
            && enabledParent.FreshContextReadValue == 0x8000_0000
            && enabledParent.ParserTargetFileOffset == 0xBD3140
            && enabledParent.ParserLength == 0x7FFF_FFFB
            && enabledParent.ParserBufferAddress == 0x127C
            && enabledParent.Stop
                == GameSocketDatagramContextDispatchStop.HighWaterAtLeastPacketCountAtBounded3130
            && enabledParent.StopAtRaw == 0xBD3130
            && enabledParent.FreshHighWaterReadAddress == 0x1D88
            && enabledParent.FreshHighWaterReadValue == 0x8000_0000
            && enabledParent.FlagsAfterFreshHighWaterCompare is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && enabledParent.FreshContextReadAddress != enabledParent.FreshHighWaterReadAddress
            && enabledParent.OrderedMemoryEffects.Select(e => e.Kind).SequenceEqual(
            [
                GameSocketDatagramContextDispatchMemoryEffectKind.ParentBindingFlagRead,
                GameSocketDatagramContextDispatchMemoryEffectKind.ParentBindingFlagClear,
                GameSocketDatagramContextDispatchMemoryEffectKind.FreshContextWordRead,
                GameSocketDatagramContextDispatchMemoryEffectKind.FreshHighWaterRead
            ])
            && enabledParent.OrderedMemoryEffects[0].Address == 0x125A
            && enabledParent.OrderedMemoryEffects[1].Address == 0x125A
            && enabledParent.OrderedMemoryEffects[1].WriteCommitted == true
            && enabledParent.OrderedMemoryEffects[2].Address == 0x1114
            && enabledParent.OrderedMemoryEffects[3].Address == 0x1D88
            && enabledParent.OrderedMemoryEffects[2].Address
                != enabledParent.OrderedMemoryEffects[0].Address
            && enabledParent.OrderedMemoryEffects[3].Address
                != enabledParent.OrderedMemoryEffects[1].Address
            && enabledParent.ExactBoundedCompletion
            && Array.FindIndex(enabledParent.InstructionOrder.ToArray(), i =>
                i.Kind == GameSocketDatagramContextDispatchInstructionKind.CallNonzeroContextParser)
                < Array.FindIndex(enabledParent.InstructionOrder.ToArray(), i =>
                    i.Kind == GameSocketDatagramContextDispatchInstructionKind.CompareFreshHighWater)
            && !enabledParent.FreshHighWaterStoreAttempted);

        var oldAdmissionZeroFreshNonzero = Project(
            Parent(admission: GameSocketPacketAdmissionOutcome.AcceptedZeroContext),
            context: 1,
            zeroReturn: null,
            nonzeroReturn: null);
        check("Internal fresh context dispatch is independent of the outer admission context classification",
            oldAdmissionZeroFreshNonzero.ParentProjection.Inputs.Admission
                == GameSocketPacketAdmissionOutcome.AcceptedZeroContext
            && oldAdmissionZeroFreshNonzero.FreshContextReadValue == 1
            && oldAdmissionZeroFreshNonzero.ParserTargetFileOffset == 0xBD3140
            && oldAdmissionZeroFreshNonzero.InstructionOrder.Single(i =>
                i.Kind == GameSocketDatagramContextDispatchInstructionKind.BranchIfFreshContextNonzero)
                .BranchTaken == true);

        var missingCount = Project(count: null, context: 0x55);
        check("Missing original EDI stops at C3 after fresh-context CMP and before length derivation",
            missingCount.Stop
                == GameSocketDatagramContextDispatchStop.AwaitingOriginalPacketCountBeforeLength
            && missingCount.StopAtRaw == 0xBD30C3
            && missingCount.FreshContextReadValue == 0x55
            && missingCount.ParserLength is null
            && !missingCount.ParserCallAttempted
            && missingCount.InstructionOrder.Last().FileOffset == 0xBD30C3
            && missingCount.SuppliedButUnusedInputs.Contains(
                GameSocketDatagramContextDispatchInput.ZeroParserReturnedNormally));

        var wrappedLength = Project(count: 2, context: 0, zeroReturn: null);
        check("LEA length uses unsigned DWORD wraparound and records an opaque zero-parser call attempt",
            wrappedLength.ParserLength == 0xFFFF_FFFE
            && wrappedLength.ParserCallAttempted
            && wrappedLength.ParserTargetFileOffset == 0xBD39D0
            && wrappedLength.Stop == GameSocketDatagramContextDispatchStop.AwaitingZeroParserReturn
            && wrappedLength.StopAtRaw == 0xBD30D2
            && wrappedLength.FlagsAtStop == wrappedLength.FlagsAfterFreshContextCompare
            && !wrappedLength.OpaqueParsersExecuted);

        var zeroDidNotReturn = Project(context: 0, zeroReturn: false);
        var nonzeroDidNotReturn = Project(
            context: 1,
            zeroReturn: true,
            nonzeroReturn: false,
            highWater: 10);
        check("Selected parser return capture distinguishes pending and nonreturning calls without advancing",
            zeroDidNotReturn.Stop
                == GameSocketDatagramContextDispatchStop.ZeroParserDidNotReturnNormally
            && zeroDidNotReturn.StopAtRaw == 0xBD30D2
            && zeroDidNotReturn.ParserReturnedNormally == false
            && zeroDidNotReturn.FlagsAtStop == zeroDidNotReturn.FlagsAfterFreshContextCompare
            && nonzeroDidNotReturn.Stop
                == GameSocketDatagramContextDispatchStop.NonzeroParserDidNotReturnNormally
            && nonzeroDidNotReturn.StopAtRaw == 0xBD30DF
            && nonzeroDidNotReturn.ParserTargetFileOffset == 0xBD3140
            && !nonzeroDidNotReturn.FreshHighWaterReadAttempted
            && nonzeroDidNotReturn.FlagsAtStop
                == nonzeroDidNotReturn.FlagsAfterFreshContextCompare
            && nonzeroDidNotReturn.SuppliedButUnusedInputs.Contains(
                GameSocketDatagramContextDispatchInput.ZeroParserReturnedNormally)
            && nonzeroDidNotReturn.SuppliedButUnusedInputs.Contains(
                GameSocketDatagramContextDispatchInput.FreshHighWaterDword));

        var missingHighWater = Project(
            context: 0xFFFFFFFF,
            zeroReturn: null,
            nonzeroReturn: true,
            highWater: null);
        check("Normal nonzero-parser return invalidates prior flags even when the subsequent high-water capture is missing",
            missingHighWater.Stop
                == GameSocketDatagramContextDispatchStop.AwaitingFreshHighWaterAtCompare
            && missingHighWater.StopAtRaw == 0xBD30E4
            && missingHighWater.ParserReturnedNormally == true
            && missingHighWater.FreshHighWaterReadAttempted
            && missingHighWater.FreshHighWaterReadAddress == 0x1D88
            && missingHighWater.FreshHighWaterReadValue is null
            && missingHighWater.FlagsAtStop is null
            && missingHighWater.FlagsAfterFreshContextCompare is not null);

        var lowerUnsigned = Project(
            count: 0x8000_0000,
            context: 0x8000_0000,
            zeroReturn: null,
            nonzeroReturn: true,
            highWater: 0x7FFF_FFFF);
        check("Unsigned high-water comparison stores original EDI on carry while preserving signed overflow flags",
            lowerUnsigned.Stop
                == GameSocketDatagramContextDispatchStop.HighWaterBelowPacketCountAtBoundedF2
            && lowerUnsigned.StopAtRaw == 0xBD30F2
            && lowerUnsigned.FlagsAfterFreshHighWaterCompare is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && lowerUnsigned.FlagsAtStop == lowerUnsigned.FlagsAfterFreshHighWaterCompare
            && lowerUnsigned.FreshHighWaterStoreAttempted
            && lowerUnsigned.FreshHighWaterStoreCommitted
            && lowerUnsigned.FreshHighWaterStoreValue == 0x8000_0000
            && lowerUnsigned.OrderedMemoryEffects[^1].Kind
                == GameSocketDatagramContextDispatchMemoryEffectKind.FreshHighWaterWrite
            && lowerUnsigned.OrderedMemoryEffects[^1].Value == 0x8000_0000
            && lowerUnsigned.OrderedMemoryEffects[^1].WriteCommitted == true
            && lowerUnsigned.InstructionOrder[^2].FileOffset == 0xBD30EA
            && lowerUnsigned.InstructionOrder[^1].FileOffset == 0xBD30EC);

        var unsignedAboveSigned = Project(
            count: 0x7FFF_FFFF,
            context: 0x8000_0000,
            zeroReturn: null,
            nonzeroReturn: true,
            highWater: 0x8000_0000);
        check("JNB compares DWORDs unsigned even when the high-water value is signed-negative",
            unsignedAboveSigned.Stop
                == GameSocketDatagramContextDispatchStop.HighWaterAtLeastPacketCountAtBounded3130
            && unsignedAboveSigned.InstructionOrder[^1].BranchTaken == true
            && unsignedAboveSigned.FlagsAfterFreshHighWaterCompare is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && !unsignedAboveSigned.FreshHighWaterStoreAttempted);

        var equalHighWater = Project(
            count: 17,
            context: 0xFF,
            zeroReturn: null,
            nonzeroReturn: true,
            highWater: 17);
        check("Equal high-water and packet count take JNB with complete DWORD compare flags",
            equalHighWater.Stop
                == GameSocketDatagramContextDispatchStop.HighWaterAtLeastPacketCountAtBounded3130
            && equalHighWater.FlagsAfterFreshHighWaterCompare is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equalHighWater.InstructionOrder[^1].TargetFileOffset == 0xBD3130);

        var unusedZeroCaptures = Project(
            context: 0,
            zeroReturn: true,
            nonzeroReturn: false,
            highWater: 0);
        check("Unused nonzero-path captures spoil clean completion without changing the zero-path D7 bound",
            unusedZeroCaptures.Stop
                == GameSocketDatagramContextDispatchStop.ZeroParserReturnedAtBoundedD7
            && unusedZeroCaptures.StopAtRaw == 0xBD30D7
            && unusedZeroCaptures.ReachedBoundedEndpoint
            && unusedZeroCaptures.HasSuppliedButUnusedInputs
            && !unusedZeroCaptures.IsCleanCompletion
            && unusedZeroCaptures.SuppliedButUnusedInputs.Contains(
                GameSocketDatagramContextDispatchInput.NonzeroParserReturnedNormally)
            && unusedZeroCaptures.SuppliedButUnusedInputs.Contains(
                GameSocketDatagramContextDispatchInput.FreshHighWaterDword)
            && !unusedZeroCaptures.FreshHighWaterReadAttempted);

        var missingFreshContext = Project(context: null);
        check("Fresh context read is independent and required before count use or parser selection",
            missingFreshContext.Stop
                == GameSocketDatagramContextDispatchStop.AwaitingFreshContextWordAtCompare
            && missingFreshContext.StopAtRaw == 0xBD30BC
            && missingFreshContext.FreshContextReadAttempted
            && missingFreshContext.FreshContextReadAddress == 0x1114
            && missingFreshContext.FreshContextReadValue is null
            && missingFreshContext.FlagsAfterFreshContextCompare is null
            && missingFreshContext.ParentProjection.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && missingFreshContext.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && missingFreshContext.FreshContextReadCommitted == false
            && missingFreshContext.OrderedMemoryEffects[^1].Value is null
            && missingFreshContext.OrderedMemoryEffects[^1].ReadCommitted == false
            && missingFreshContext.InstructionOrder.Length == 1
            && missingFreshContext.SuppliedButUnusedInputs.Contains(
                GameSocketDatagramContextDispatchInput.OriginalPacketCount));

        var wrappingBuffer = Project(
            Parent(socket: 0xFFFF_FF00),
            context: 0,
            zeroReturn: true);
        check("Parser buffer and fresh context addresses wrap from the inherited socket ESI",
            wrappingBuffer.FreshContextReadAddress == 0x14
            && wrappingBuffer.ParserBufferAddress == 0x17C
            && wrappingBuffer.InstructionOrder.Single(i =>
                i.Kind == GameSocketDatagramContextDispatchInstructionKind.LoadParserBufferAddress)
                .EffectiveAddress == 0x17C);

        var dirtyParentInputs = Parent(sender: 0x2000);
        var dirtyParent = Project(dirtyParentInputs, context: 0, highWater: 0);
        var missingParent = Project(Parent(bindingFlag: null), context: 0);
        var rejectedParent = Project(
            Parent(admission: GameSocketPacketAdmissionOutcome.InvalidLength),
            context: 0);
        check("Dirty, missing-state, and rejected parent outcomes preserve their exact stop and block internal dispatch",
            dirtyParent.ParentProjection.ReachedCommonTail
            && !dirtyParent.ParentProjection.IsCleanCompletion
            && dirtyParent.ParentProjection.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && dirtyParent.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && dirtyParent.ExactBoundedCompletion
            && !dirtyParent.ParentExactCleanCommonTailEntered
            && !dirtyParent.ReachedBoundedEndpoint
            && !dirtyParent.BoundedTargetEntered
            && dirtyParent.Stop
                == GameSocketDatagramContextDispatchStop.ParentDidNotReachExactCleanCommonTail
            && dirtyParent.StopAtRaw == dirtyParent.ParentProjection.StopAtRaw
            && dirtyParent.InstructionOrder.IsEmpty
            && !dirtyParent.FreshContextReadAttempted
            && dirtyParent.SuppliedButUnusedInputs.Contains(
                GameSocketDatagramContextDispatchInput.FreshContextWord)
            && dirtyParent.SuppliedButUnusedInputs.Contains(
                GameSocketDatagramContextDispatchInput.FreshHighWaterDword)
            && missingParent.ParentProjection.Stop
                == GameSocketSenderBindingStop.AwaitingBindingFlagAtCompare
            && missingParent.FlagsAtStop is null
            && !missingParent.ExactBoundedCompletion
            && missingParent.StopAtRaw == missingParent.ParentProjection.StopAtRaw
            && !missingParent.FreshContextReadAttempted
            && rejectedParent.ParentProjection.Stop
                == GameSocketSenderBindingStop.AdmissionRejected
            && rejectedParent.StopAtRaw is null
            && rejectedParent.InstructionOrder.IsEmpty);

        check("Context dispatch offsets, field offsets, parser targets, and bounded endpoints match the observed tail",
            GameSocketDatagramContextDispatchProjection.MethodFileOffset == 0xBD30BC
            && GameSocketDatagramContextDispatchProjection.FreshContextCompareFileOffset == 0xBD30BC
            && GameSocketDatagramContextDispatchProjection.ParserLengthLoadFileOffset == 0xBD30C3
            && GameSocketDatagramContextDispatchProjection.ParserLengthPushFileOffset == 0xBD30C6
            && GameSocketDatagramContextDispatchProjection.ParserBufferAddressLoadFileOffset == 0xBD30C7
            && GameSocketDatagramContextDispatchProjection.SocketEcxLoadFileOffset == 0xBD30CD
            && GameSocketDatagramContextDispatchProjection.ParserBufferPushFileOffset == 0xBD30CF
            && GameSocketDatagramContextDispatchProjection.NonzeroContextBranchFileOffset == 0xBD30D0
            && GameSocketDatagramContextDispatchProjection.ZeroContextParserCallFileOffset == 0xBD30D2
            && GameSocketDatagramContextDispatchProjection.ZeroParserReturnFileOffset == 0xBD30D7
            && GameSocketDatagramContextDispatchProjection.NonzeroContextParserCallFileOffset == 0xBD30DF
            && GameSocketDatagramContextDispatchProjection.FreshHighWaterCompareFileOffset == 0xBD30E4
            && GameSocketDatagramContextDispatchProjection.HighWaterBranchFileOffset == 0xBD30EA
            && GameSocketDatagramContextDispatchProjection.FreshHighWaterStoreFileOffset == 0xBD30EC
            && GameSocketDatagramContextDispatchProjection.BelowHighWaterBoundedFileOffset == 0xBD30F2
            && GameSocketDatagramContextDispatchProjection.AtOrAboveHighWaterBoundedFileOffset == 0xBD3130
            && GameSocketDatagramContextDispatchProjection.ZeroContextParserFileOffset == 0xBD39D0
            && GameSocketDatagramContextDispatchProjection.NonzeroContextParserFileOffset == 0xBD3140
            && GameSocketDatagramContextDispatchProjection.FreshContextWordOffset == 0x114
            && GameSocketDatagramContextDispatchProjection.ParserBufferOffset == 0x27C
            && GameSocketDatagramContextDispatchProjection.FreshHighWaterOffset == 0xD88);
    }
}
