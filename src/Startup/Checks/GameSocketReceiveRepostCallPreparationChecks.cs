using System.Collections.Immutable;
using Firefall.Reconstruction.Startup;

internal static class GameSocketReceiveRepostCallPreparationChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static GameSocketReceiveRepostCallPreparationProjectionResult Project(
            bool zeroBranch = false,
            uint entryEsp = 0x5000,
            uint esi = 0x1000,
            bool? diagnosticReturnedNormally = true,
            uint? freshPostDiagnosticEsp = 0x6000,
            uint? freshPostDiagnosticEsi = 0x7000,
            uint? freshPostDiagnosticEbx = 0x8000,
            uint? operationDword = 0x100,
            uint? stackDword = 0x200,
            uint? socketDword = 0x300,
            bool dirtyParent = false,
            bool comparisonCaptured = true,
            bool supplyZeroCapturesOnDirect = false)
        {
            var postPacketInputs =
                new GameSocketReceivePostPacketStackPreparationProjectionInputs(
                    new(
                        new(
                            new(
                                1,
                                null,
                                null,
                                null,
                                null,
                                null,
                                null),
                            0x1234,
                            1,
                            0x20,
                            null,
                            null,
                            0x10000),
                        true,
                        esi,
                        entryEsp,
                        1),
                    true,
                    esi,
                    entryEsp,
                    1);
            var reloadInputs = new GameSocketReceiveOperationReloadGateProjectionInputs(
                postPacketInputs,
                null,
                null,
                null,
                1);
            var parentInputs = new GameSocketReceiveBufferStatusGateProjectionInputs(
                reloadInputs,
                null,
                dirtyParent ? 0x9000u : null,
                dirtyParent ? 0xA000u : null,
                0x3000,
                comparisonCaptured ? (zeroBranch ? 0u : 1u) : null);
            return GameSocketReceiveRepostCallPreparationProjection.Project(new(
                parentInputs,
                zeroBranch || supplyZeroCapturesOnDirect
                    ? diagnosticReturnedNormally
                    : null,
                zeroBranch || supplyZeroCapturesOnDirect
                    ? freshPostDiagnosticEsp
                    : null,
                zeroBranch || supplyZeroCapturesOnDirect
                    ? freshPostDiagnosticEsi
                    : null,
                zeroBranch || supplyZeroCapturesOnDirect
                    ? freshPostDiagnosticEbx
                    : null,
                operationDword,
                stackDword,
                socketDword));
        }

        byte[] bytes = ParseBytes(
            GameSocketReceiveRepostCallPreparationProjection.WindowBytes);
        check("The complete FBA-to-FE2-exclusive instruction bytes and boundaries are pinned",
            bytes.Length == 0x28
            && string.Join(" ", bytes.Select(value => value.ToString("X2")))
                == "8B 86 F4 0D 00 00 6A 00 FF 74 24 1C 83 C0 18 53 50 8D 44 24 2C 50 8D 44 24 34 50 6A 01 8D 44 24 40 50 FF B6 18 01 00 00"
            && GameSocketReceiveRepostCallPreparationProjection.MethodRawFileOffset
                == 0x00BD2FBA
            && GameSocketReceiveRepostCallPreparationProjection.ExclusiveStopRawFileOffset
                == 0x00BD2FE2
            && GameSocketReceiveRepostCallPreparationProjection.FollowingCellVirtualAddress
                == 0x01AC4D54
            && GameSocketReceiveRepostCallPreparationProjection.WindowBytes.Length
                == GameSocketReceiveRepostCallPreparationProjection.ExclusiveStopRawFileOffset
                    - GameSocketReceiveRepostCallPreparationProjection.MethodRawFileOffset);

        GameSocketReceiveRepostCallPreparationProjectionResult direct = Project();
        check("A clean nonzero parent branch derives ESI, EBX, and ESP without redundant captures",
            direct.Route == GameSocketReceiveRepostCallPreparationRoute.DirectNonzero
            && direct.ParentHandoffAdmitted
            && direct.ParentProjection.Stop
                == GameSocketReceiveBufferStatusGateStop.NonzeroBranchTargetAtFBA
            && direct.ParentProjection.ComparisonBranchTaken == true
            && direct.EffectiveEsi == direct.ParentProjection.EffectiveEsi
            && direct.EffectiveEbx == direct.ParentProjection.EbxAfterAdd
            && direct.EntryEsp == direct.ParentProjection.EffectiveEsp
            && direct.CaptureStates.FreshPostDiagnosticEsp
                == GameSocketReceiveRepostCallPreparationCaptureState.Unreached
            && direct.CaptureStates.FreshPostDiagnosticEsi
                == GameSocketReceiveRepostCallPreparationCaptureState.Unreached
            && direct.CaptureStates.FreshPostDiagnosticEbx
                == GameSocketReceiveRepostCallPreparationCaptureState.Unreached
            && direct.InstructionOrder.Length == 14
            && direct.MemoryEffects.Count(effect => effect.Kind
                == GameSocketReceiveRepostCallPreparationMemoryEffectKind.PushStackWrite) == 9
            && direct.Stop
                == GameSocketReceiveRepostCallPreparationStop.ExclusiveStopAtFE2
            && direct.StopAtRawFileOffsetExclusive
                == GameSocketReceiveRepostCallPreparationProjection.ExclusiveStopRawFileOffset
            && direct.ReachedBoundedEndpoint
            && direct.ExactBoundedCompletion
            && direct.IsCleanCompletion
            && !direct.FollowingImportCellReadAttempted
            && !direct.FollowingImportCallAttempted
            && !direct.ApiResultModeled
            && !direct.NativeInstructionsExecuted);

        GameSocketReceiveRepostCallPreparationProjectionResult unusedChildCaptures =
            Project(supplyZeroCapturesOnDirect: true);
        check("Captures supplied for the unchosen zero route remain distinctly unused",
            unusedChildCaptures.ReachedBoundedEndpoint
            && unusedChildCaptures.CaptureStates.DiagnosticReturnedNormally
                == GameSocketReceiveRepostCallPreparationCaptureState.SuppliedButUnused
            && unusedChildCaptures.CaptureStates.FreshPostDiagnosticEsp
                == GameSocketReceiveRepostCallPreparationCaptureState.SuppliedButUnused
            && unusedChildCaptures.CaptureStates.FreshPostDiagnosticEsi
                == GameSocketReceiveRepostCallPreparationCaptureState.SuppliedButUnused
            && unusedChildCaptures.CaptureStates.FreshPostDiagnosticEbx
                == GameSocketReceiveRepostCallPreparationCaptureState.SuppliedButUnused
            && !unusedChildCaptures.IsCleanCompletion
            && unusedChildCaptures.SuppliedButUnusedInputs.Length == 4);

        uint directEsp = direct.EntryEsp!.Value;
        uint directEsi = direct.EffectiveEsi!.Value;
        check("The first stack read uses pre-PUSH ESP, and all nine writes use wrapped DWORD stack arithmetic",
            direct.StackReadAddress == unchecked(directEsp + 0x18)
            && direct.OperationReadAddress == unchecked(directEsi + 0xDF4)
            && direct.SocketReadAddress == unchecked(directEsi + 0x118)
            && direct.MemoryEffects.Where(effect => effect.Kind
                    == GameSocketReceiveRepostCallPreparationMemoryEffectKind.PushStackWrite)
                .Select(effect => effect.Address)
                .SequenceEqual(new uint?[]
                {
                    unchecked(directEsp - 4),
                    unchecked(directEsp - 8),
                    unchecked(directEsp - 12),
                    unchecked(directEsp - 16),
                    unchecked(directEsp - 20),
                    unchecked(directEsp - 24),
                    unchecked(directEsp - 28),
                    unchecked(directEsp - 32),
                    unchecked(directEsp - 36)
                })
            && direct.MemoryEffects.Where(effect => effect.Kind
                    == GameSocketReceiveRepostCallPreparationMemoryEffectKind.PushStackWrite)
                .Select(effect => effect.Value)
                .SequenceEqual(new uint?[]
                {
                    0,
                    0x200,
                    direct.EffectiveEbx,
                    0x118,
                    unchecked(directEsp + 0x1C),
                    unchecked(directEsp + 0x20),
                    1,
                    unchecked(directEsp + 0x24),
                    0x300
                })
            && direct.FinalEsp == unchecked(directEsp - 36)
            && direct.EaxAtStop == unchecked(directEsp + 0x24)
            && direct.EaxAfterAdd18 == 0x118
            && direct.InstructionOrder.Single(instruction =>
                instruction.Kind
                    == GameSocketReceiveRepostCallPreparationInstructionKind.LeaEaxFromEsp40)
                .ResultValue == unchecked(directEsp + 0x24)
            && direct.MemoryEffects.Any(effect =>
                effect.Kind == GameSocketReceiveRepostCallPreparationMemoryEffectKind.FreshStackDwordRead
                && effect.Address == unchecked(directEsp + 0x18)
                && effect.Value == 0x200
                && !effect.Committed)
            && direct.FlagsAtStop == direct.InstructionOrder
                .Single(instruction => instruction.Kind
                    == GameSocketReceiveRepostCallPreparationInstructionKind.Add18ToEax)
                .FlagsAfter);

        GameSocketReceiveRepostCallPreparationProjectionResult carryAndAuxiliary =
            Project(operationDword: 0xFFFFFFFF);
        GameSocketReceiveRepostCallPreparationProjectionResult signedOverflow =
            Project(operationDword: 0x7FFFFFF0);
        GameSocketReceiveRepostCallPreparationProjectionResult carryZeroAndAuxiliary =
            Project(operationDword: 0xFFFFFFE8);
        check("ADD EAX,18 models carry, auxiliary carry, wrap-to-zero, parity, and signed overflow",
            carryAndAuxiliary.FlagsAtStop is {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && carryAndAuxiliary.EaxAfterAdd18 == 0x17
            && signedOverflow.FlagsAtStop is {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && signedOverflow.EaxAfterAdd18 == 0x80000008
            && carryZeroAndAuxiliary.FlagsAtStop is {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && carryZeroAndAuxiliary.EaxAfterAdd18 == 0);

        GameSocketReceiveRepostCallPreparationProjectionResult wrapped = Project(entryEsp: 0);
        check("An entry ESP of zero wraps every push address and leaves the ninth push at FFFFFFDC",
            wrapped.EntryEsp == 0
            && wrapped.FinalEsp == 0xFFFFFFDC
            && wrapped.EaxAtStop == 0x24
            && wrapped.MemoryEffects.Where(effect => effect.Kind
                    == GameSocketReceiveRepostCallPreparationMemoryEffectKind.PushStackWrite)
                .Select(effect => effect.Address)
                .SequenceEqual(new uint?[]
                {
                    0xFFFFFFFC,
                    0xFFFFFFF8,
                    0xFFFFFFF4,
                    0xFFFFFFF0,
                    0xFFFFFFEC,
                    0xFFFFFFE8,
                    0xFFFFFFE4,
                    0xFFFFFFE0,
                    0xFFFFFFDC
                }));

        GameSocketReceiveRepostCallPreparationProjectionResult socketAlias =
            Project(entryEsp: 0x1100, esi: 0x1000, stackDword: 0x1111, socketDword: 0x2222);
        check("Aliased stack and socket read addresses retain their independent captures",
            socketAlias.StackReadAddress == socketAlias.SocketReadAddress
            && socketAlias.MemoryEffects.Any(effect =>
                effect.Kind == GameSocketReceiveRepostCallPreparationMemoryEffectKind.FreshStackDwordRead
                && effect.Address == 0x1118
                && effect.Value == 0x1111)
            && socketAlias.MemoryEffects.Any(effect =>
                effect.Kind == GameSocketReceiveRepostCallPreparationMemoryEffectKind.FreshSocketDwordRead
                && effect.Address == 0x1118
                && effect.Value == 0x2222)
            && socketAlias.InstructionOrder.Single(instruction =>
                instruction.Kind
                    == GameSocketReceiveRepostCallPreparationInstructionKind.ReadAndPushStackDwordFromEsp1C)
                .OperandValue == 0x1111
            && socketAlias.InstructionOrder.Single(instruction =>
                instruction.Kind
                    == GameSocketReceiveRepostCallPreparationInstructionKind.ReadAndPushSocketDwordFromEsi118)
                .OperandValue == 0x2222);

        GameSocketReceiveRepostCallPreparationProjectionResult operationAlias =
            Project(entryEsp: 0x1DDC, esi: 0x1000, operationDword: 0x1111, stackDword: 0x2222);
        check("An operation/stack address alias does not reuse either captured value",
            operationAlias.OperationReadAddress == operationAlias.StackReadAddress
            && operationAlias.OperationDword == 0x1111
            && operationAlias.MemoryEffects.Any(effect =>
                effect.Kind == GameSocketReceiveRepostCallPreparationMemoryEffectKind.FreshStackDwordRead
                && effect.Address == operationAlias.OperationReadAddress
                && effect.Value == 0x2222));

        GameSocketReceiveRepostCallPreparationProjectionResult zero = Project(
            zeroBranch: true,
            diagnosticReturnedNormally: true,
            freshPostDiagnosticEsp: 0x7000,
            freshPostDiagnosticEsi: 0x8000,
            freshPostDiagnosticEbx: 0x9000);
        check("The parent FB1 route waits for normal return, adds ten to fresh ESP, and uses fresh ESI/EBX",
            zero.ParentProjection.Stop
                == GameSocketReceiveBufferStatusGateStop.DiagnosticImportBoundaryAtFB1
            && zero.ParentProjection.ComparisonBranchTaken == false
            && zero.Route == GameSocketReceiveRepostCallPreparationRoute.ZeroDiagnostic
            && zero.ParentHandoffAdmitted
            && zero.CaptureStates.DiagnosticReturnedNormally
                == GameSocketReceiveRepostCallPreparationCaptureState.Captured
            && zero.CaptureStates.FreshPostDiagnosticEsp
                == GameSocketReceiveRepostCallPreparationCaptureState.Captured
            && zero.EffectiveEsp == unchecked(0x7000 + 0x10)
            && zero.EntryEsp == zero.EffectiveEsp
            && zero.EffectiveEsi == 0x8000
            && zero.EffectiveEbx == 0x9000
            && zero.InstructionOrder[0] is {
                Kind: GameSocketReceiveRepostCallPreparationInstructionKind.Add10ToPostDiagnosticEsp,
                RawFileOffset: GameSocketReceiveRepostCallPreparationProjection.PostDiagnosticStackCleanupRawFileOffset,
                EffectiveAddress: 0x7000,
                ResultValue: 0x7010
            }
            && zero.FlagsAtStop == zero.InstructionOrder.Single(instruction =>
                instruction.Kind
                    == GameSocketReceiveRepostCallPreparationInstructionKind.Add18ToEax)
                .FlagsAfter
            && zero.IsCleanCompletion);

        GameSocketReceiveRepostCallPreparationProjectionResult pendingReturn =
            Project(zeroBranch: true, diagnosticReturnedNormally: null);
        GameSocketReceiveRepostCallPreparationProjectionResult falseReturn =
            Project(zeroBranch: true, diagnosticReturnedNormally: false);
        check("Pending and false diagnostic returns stop at FB1 without claiming a child bound",
            pendingReturn.Stop
                == GameSocketReceiveRepostCallPreparationStop.AwaitingDiagnosticReturnAtFB1
            && pendingReturn.CaptureStates.DiagnosticReturnedNormally
                == GameSocketReceiveRepostCallPreparationCaptureState.Missing
            && pendingReturn.FlagsAtStop == pendingReturn.ParentProjection.FlagsAtStop
            && pendingReturn.InstructionOrder.IsEmpty
            && !pendingReturn.ReachedBoundedEndpoint
            && !pendingReturn.ExactBoundedCompletion
            && falseReturn.Stop
                == GameSocketReceiveRepostCallPreparationStop.DiagnosticDidNotReturnNormally
            && falseReturn.CaptureStates.DiagnosticReturnedNormally
                == GameSocketReceiveRepostCallPreparationCaptureState.Captured
            && falseReturn.FlagsAtStop == falseReturn.ParentProjection.FlagsAtStop
            && falseReturn.InstructionOrder.IsEmpty
            && !falseReturn.ReachedBoundedEndpoint
            && !falseReturn.ExactBoundedCompletion);

        GameSocketReceiveRepostCallPreparationProjectionResult missingPostEsp =
            Project(zeroBranch: true, freshPostDiagnosticEsp: null);
        GameSocketReceiveRepostCallPreparationProjectionResult missingPostEsi =
            Project(zeroBranch: true, freshPostDiagnosticEsi: null);
        GameSocketReceiveRepostCallPreparationProjectionResult missingPostEbx =
            Project(zeroBranch: true, freshPostDiagnosticEbx: null);
        check("Every fresh zero-route register capture has its own missing stop and state",
            missingPostEsp.Stop
                == GameSocketReceiveRepostCallPreparationStop.AwaitingFreshPostDiagnosticEspAtFB7
            && missingPostEsp.CaptureStates.FreshPostDiagnosticEsp
                == GameSocketReceiveRepostCallPreparationCaptureState.Missing
            && missingPostEsp.FlagsAtStop is null
            && !missingPostEsp.ExactBoundedCompletion
            && missingPostEsi.Stop
                == GameSocketReceiveRepostCallPreparationStop.AwaitingFreshPostDiagnosticEsiAtFBA
            && missingPostEsi.CaptureStates.FreshPostDiagnosticEsi
                == GameSocketReceiveRepostCallPreparationCaptureState.Missing
            && missingPostEsi.FinalEsp == 0x6010
            && !missingPostEsi.ExactBoundedCompletion
            && missingPostEbx.Stop
                == GameSocketReceiveRepostCallPreparationStop.AwaitingFreshPostDiagnosticEbxAtFBA
            && missingPostEbx.CaptureStates.FreshPostDiagnosticEbx
                == GameSocketReceiveRepostCallPreparationCaptureState.Missing
            && missingPostEbx.FinalEsp == 0x6010
            && !missingPostEbx.ExactBoundedCompletion);

        GameSocketReceiveRepostCallPreparationProjectionResult missingOperation =
            Project(operationDword: null);
        GameSocketReceiveRepostCallPreparationProjectionResult missingStack =
            Project(stackDword: null);
        GameSocketReceiveRepostCallPreparationProjectionResult missingSocket =
            Project(socketDword: null);
        check("Missing operation, stack, and socket reads stop independently without inventing writes",
            missingOperation.Stop
                == GameSocketReceiveRepostCallPreparationStop.AwaitingFreshOperationDwordAtFBA
            && missingOperation.CaptureStates.FreshOperationDword
                == GameSocketReceiveRepostCallPreparationCaptureState.Missing
            && missingOperation.MemoryEffects is [{
                Kind: GameSocketReceiveRepostCallPreparationMemoryEffectKind.FreshOperationDwordRead,
                Attempted: true,
                Captured: false,
                Committed: false
            }]
            && missingOperation.EaxAtStop is null
            && missingOperation.FinalEsp == missingOperation.EntryEsp
            && !missingOperation.ExactBoundedCompletion
            && missingStack.Stop
                == GameSocketReceiveRepostCallPreparationStop.AwaitingFreshStackDwordAtFC2
            && missingStack.CaptureStates.FreshStackDword
                == GameSocketReceiveRepostCallPreparationCaptureState.Missing
            && missingStack.MemoryEffects.Count(effect => effect.Committed) == 1
            && missingStack.EaxAtStop == missingStack.OperationDword
            && missingStack.FinalEsp == unchecked(missingStack.EntryEsp!.Value - 4)
            && !missingStack.ExactBoundedCompletion
            && missingSocket.Stop
                == GameSocketReceiveRepostCallPreparationStop.AwaitingFreshSocketDwordAtFDC
            && missingSocket.CaptureStates.FreshSocketDword
                == GameSocketReceiveRepostCallPreparationCaptureState.Missing
            && missingSocket.MemoryEffects[^1] is {
                Kind: GameSocketReceiveRepostCallPreparationMemoryEffectKind.FreshSocketDwordRead,
                Attempted: true,
                Captured: false,
                Committed: false
            }
            && missingSocket.FinalEsp == unchecked(missingSocket.EntryEsp!.Value - 32)
            && missingSocket.FlagsAtStop == missingSocket.InstructionOrder.Single(instruction =>
                instruction.Kind
                    == GameSocketReceiveRepostCallPreparationInstructionKind.Add18ToEax)
                .FlagsAfter
            && !missingSocket.ExactBoundedCompletion
            && !missingOperation.ReachedBoundedEndpoint
            && !missingStack.ReachedBoundedEndpoint
            && !missingSocket.ReachedBoundedEndpoint);

        GameSocketReceiveRepostCallPreparationProjectionResult dirtyParent =
            Project(dirtyParent: true);
        GameSocketReceiveRepostCallPreparationProjectionResult unreachedParent =
            Project(comparisonCaptured: false);
        check("Dirty and unreached parent captures reject handoff at the parent's actual endpoint",
            dirtyParent.Stop
                == GameSocketReceiveRepostCallPreparationStop.ParentEndpointWasDirty
            && dirtyParent.StopAtRawFileOffsetExclusive
                == dirtyParent.ParentProjection.StopAtRawFileOffset
            && !dirtyParent.ParentHandoffAdmitted
            && dirtyParent.ExactBoundedCompletion
            && unreachedParent.Stop
                == GameSocketReceiveRepostCallPreparationStop.ParentDidNotReachRecognizedEndpoint
            && unreachedParent.StopAtRawFileOffsetExclusive
                == unreachedParent.ParentProjection.StopAtRawFileOffset
            && !unreachedParent.ParentHandoffAdmitted
            && !unreachedParent.ReachedBoundedEndpoint
            && !unreachedParent.InstructionOrder.Any());

        check("The parent trace is retained unchanged and child reads/writes are appended in order",
            direct.ParentTrace.SequenceEqual(direct.ParentProjection.OrderedMemoryEffects)
            && direct.OrderedMemoryEffects.Take(direct.ParentTrace.Length)
                .Select(step => step.ParentTraceStep)
                .SequenceEqual(direct.ParentTrace.Select(step => (GameSocketReceiveBufferStatusGateTraceStep?)step))
            && direct.OrderedMemoryEffects.Skip(direct.ParentTrace.Length)
                .All(step => step.PreparationMemoryEffect.HasValue)
            && direct.MemoryEffects.All(effect => effect.Kind
                    is GameSocketReceiveRepostCallPreparationMemoryEffectKind.FreshOperationDwordRead
                    or GameSocketReceiveRepostCallPreparationMemoryEffectKind.FreshStackDwordRead
                    or GameSocketReceiveRepostCallPreparationMemoryEffectKind.FreshSocketDwordRead
                    or GameSocketReceiveRepostCallPreparationMemoryEffectKind.PushStackWrite));
    }

    private static byte[] ParseBytes(ImmutableArray<byte> bytes) => bytes.ToArray();
}
