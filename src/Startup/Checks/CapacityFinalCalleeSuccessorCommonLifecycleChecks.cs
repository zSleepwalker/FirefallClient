using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeSuccessorCommonLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeSuccessorCommonLifecyclePlan missingBothUpstream =
            CapacityFinalCalleeSuccessorCommonLifecycleProjection.Project(new(
                EntryECX: null,
                EntryEAX: null,
                CallerEDI: 0x10,
                CallerESI: 0x20));
        check("Missing both upstream registers blocks and flags both later captures",
            missingBothUpstream.Outcome
                == CapacityFinalCalleeSuccessorCommonLifecycleOutcome
                    .MissingEntryECXAndEAX
            && !missingBothUpstream.Upstream.FirstStoreReached
            && !missingBothUpstream.Upstream.SecondStoreReached
            && !missingBothUpstream.CallerEDICaptureReached
            && !missingBothUpstream.CallerESICaptureReached
            && missingBothUpstream.CallerEDICaptureProvidedButUnreached
            && missingBothUpstream.CallerESICaptureProvidedButUnreached
            && missingBothUpstream.CommonTail is null
            && missingBothUpstream.Writes.IsEmpty
            && !missingBothUpstream.ExactCleanExitAdmitted
            && missingBothUpstream.StopAtRawFileOffsetExclusive == 0x012C9190);

        CapacityFinalCalleeSuccessorCommonLifecyclePlan missingEcx =
            CapacityFinalCalleeSuccessorCommonLifecycleProjection.Project(new(
                EntryECX: null,
                EntryEAX: 0,
                CallerEDI: 0,
                CallerESI: 0));
        check("Missing upstream ECX blocks both later captures despite zero EAX",
            missingEcx.Outcome
                == CapacityFinalCalleeSuccessorCommonLifecycleOutcome.MissingEntryECX
            && missingEcx.Upstream.EntryEAXCaptured
            && !missingEcx.Upstream.FirstStoreReached
            && !missingEcx.CallerEDICaptureReached
            && !missingEcx.CallerESICaptureReached
            && missingEcx.CallerEDICaptureProvidedButUnreached
            && missingEcx.CallerESICaptureProvidedButUnreached
            && missingEcx.Writes.IsEmpty
            && missingEcx.StopAtRawFileOffsetExclusive == 0x012C9190);

        CapacityFinalCalleeSuccessorCommonLifecyclePlan missingEax =
            CapacityFinalCalleeSuccessorCommonLifecycleProjection.Project(new(
                EntryECX: 0,
                EntryEAX: null,
                CallerEDI: 0,
                CallerESI: 0));
        check("Missing upstream EAX blocks both later captures despite zero ECX",
            missingEax.Outcome
                == CapacityFinalCalleeSuccessorCommonLifecycleOutcome.MissingEntryEAX
            && missingEax.Upstream.EntryECXCaptured
            && !missingEax.Upstream.FirstStoreReached
            && !missingEax.CallerEDICaptureReached
            && !missingEax.CallerESICaptureReached
            && missingEax.CallerEDICaptureProvidedButUnreached
            && missingEax.CallerESICaptureProvidedButUnreached
            && missingEax.Writes.IsEmpty
            && missingEax.StopAtRawFileOffsetExclusive == 0x012C9190);

        CapacityFinalCalleeSuccessorCommonLifecyclePlan missingBothCallers =
            CapacityFinalCalleeSuccessorCommonLifecycleProjection.Project(new(
                EntryECX: 0x10,
                EntryEAX: 0x20,
                CallerEDI: null,
                CallerESI: null));
        check("Missing both caller captures stops at the child entry after upstream writes",
            missingBothCallers.Outcome
                == CapacityFinalCalleeSuccessorCommonLifecycleOutcome.MissingCallerEDI
            && missingBothCallers.CallerEDICaptureReached
            && !missingBothCallers.CallerESICaptureReached
            && missingBothCallers.CommonTail?.Outcome
                == CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryEDI
            && missingBothCallers.Writes.Select(write =>
                (write.RawFileOffset, write.Address, write.Value)).SequenceEqual(new[]
                {
                    (0x012C9190u, 0x1Cu, 0x20u),
                    (0x012C9193u, 0x28u, 0x10u)
                })
            && !missingBothCallers.ExactCleanExitAdmitted
            && missingBothCallers.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeSuccessorCommonLifecyclePlan missingEdi =
            CapacityFinalCalleeSuccessorCommonLifecycleProjection.Project(new(
                EntryECX: 0x10,
                EntryEAX: 0x20,
                CallerEDI: null,
                CallerESI: 0x30));
        check("Missing caller EDI leaves supplied ESI unreached and preserves both upstream writes",
            missingEdi.Outcome
                == CapacityFinalCalleeSuccessorCommonLifecycleOutcome.MissingCallerEDI
            && missingEdi.CallerEDICaptureReached
            && !missingEdi.CallerESICaptureReached
            && missingEdi.CallerESICaptureProvidedButUnreached
            && missingEdi.Writes.Length == 2
            && !missingEdi.ExactCleanExitAdmitted
            && missingEdi.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeSuccessorCommonLifecyclePlan missingEsi =
            CapacityFinalCalleeSuccessorCommonLifecycleProjection.Project(new(
                EntryECX: 0x10,
                EntryEAX: 0x20,
                CallerEDI: 0x44,
                CallerESI: null));
        check("Missing caller ESI retains both upstream writes and the child's OR flags",
            missingEsi.Outcome
                == CapacityFinalCalleeSuccessorCommonLifecycleOutcome.MissingCallerESI
            && missingEsi.Writes.Length == 2
            && missingEsi.CommonTail?.EAXAfterOr == 0x45
            && missingEsi.CommonTail?.FirstStoreReached == false
            && missingEsi.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && missingEsi.FlagsAtStop == missingEsi.CommonTail?.OrFlags
            && missingEsi.Writes.Select(write =>
                (write.RawFileOffset, write.Address, write.Value)).SequenceEqual(new[]
                {
                    (0x012C9190u, 0x1Cu, 0x20u),
                    (0x012C9193u, 0x28u, 0x10u)
                })
            && !missingEsi.ExactCleanExitAdmitted
            && missingEsi.StopAtRawFileOffsetExclusive == 0x012C92C2);

        CapacityFinalCalleeSuccessorCommonLifecyclePlan zeros =
            CapacityFinalCalleeSuccessorCommonLifecycleProjection.Project(new(
                EntryECX: 0,
                EntryEAX: 0,
                CallerEDI: 0,
                CallerESI: 0));
        check("Zero inputs complete all four ordered writes and stop cleanly at 92C8",
            zeros.Outcome
                == CapacityFinalCalleeSuccessorCommonLifecycleOutcome.Completed
            && zeros.Writes.Select(write =>
                (write.RawFileOffset, write.Address, write.Value)).SequenceEqual(new[]
                {
                    (0x012C9190u, 0x0Cu, 0u),
                    (0x012C9193u, 0x08u, 0u),
                    (0x012C92C2u, 0x04u, 1u),
                    (0x012C92C5u, 0u, 0u)
                })
            && zeros.FlagsPreservedThroughUpstream
            && zeros.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && zeros.FlagsAtStop == zeros.CommonTail?.OrFlags
            && zeros.Upstream.JumpReached
            && zeros.Upstream.JumpTargetRawFileOffset == 0x012C92BD
            && zeros.Upstream.JumpTargetVirtualAddress == 0x016C9EBD
            && zeros.CommonTail?.Capture.EntryESI == 0
            && zeros.ExactCleanExitAdmitted
            && zeros.StopAtRawFileOffsetExclusive == 0x012C92C8);

        CapacityFinalCalleeSuccessorCommonLifecyclePlan wrapped =
            CapacityFinalCalleeSuccessorCommonLifecycleProjection.Project(new(
                EntryECX: 0xFFFFFFF4,
                EntryEAX: 0xFFFFFFF8,
                CallerEDI: 0xFFFFFFFC,
                CallerESI: 4));
        check("DWORD wrapping is applied independently to all four ordered writes",
            wrapped.Writes.Select(write =>
                (write.RawFileOffset, write.Address, write.Value)).SequenceEqual(new[]
                {
                    (0x012C9190u, 0u, 0xFFFFFFF8u),
                    (0x012C9193u, 0u, 0xFFFFFFF4u),
                    (0x012C92C2u, 8u, 0xFFFFFFFDu),
                    (0x012C92C5u, 0u, 0xFFFFFFFCu)
                })
            && wrapped.ExactCleanExitAdmitted
            && wrapped.StopAtRawFileOffsetExclusive == 0x012C92C8);

        CapacityFinalCalleeSuccessorCommonLifecyclePlan fourWayAlias =
            CapacityFinalCalleeSuccessorCommonLifecycleProjection.Project(new(
                EntryECX: 4,
                EntryEAX: 8,
                CallerEDI: 4,
                CallerESI: 12));
        check("ECX equals caller EDI and all four aliasing stores remain ordered",
            fourWayAlias.Capture.EntryECX == fourWayAlias.Capture.CallerEDI
            && fourWayAlias.Writes.Select(write =>
                (write.RawFileOffset, write.Address, write.Value)).SequenceEqual(new[]
                {
                    (0x012C9190u, 0x10u, 8u),
                    (0x012C9193u, 0x10u, 4u),
                    (0x012C92C2u, 0x10u, 5u),
                    (0x012C92C5u, 0x10u, 4u)
                })
            && fourWayAlias.Writes.Select(write => write.Address)
                .Distinct().SequenceEqual(new[] { 0x10u })
            && fourWayAlias.CommonTail?.OrFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && fourWayAlias.FlagsAtStop == fourWayAlias.CommonTail?.OrFlags
            && fourWayAlias.ExactCleanExitAdmitted
            && fourWayAlias.StopAtRawFileOffsetExclusive == 0x012C92C8);

        CapacityFinalCalleeSuccessorCommonLifecyclePlan independentCaller =
            CapacityFinalCalleeSuccessorCommonLifecycleProjection.Project(new(
                EntryECX: 0x10,
                EntryEAX: 0x20,
                CallerEDI: 0x99,
                CallerESI: 0x30));
        check("Caller EDI is consumed independently rather than inferred from upstream stores",
            independentCaller.Writes[^1]
                == new CapacityFinalCalleeSuccessorCommonLifecycleWrite(
                    0x012C92C5, 0xC9, 0x99)
            && independentCaller.CommonTail?.Capture.EntryEDI == 0x99);
    }
}
