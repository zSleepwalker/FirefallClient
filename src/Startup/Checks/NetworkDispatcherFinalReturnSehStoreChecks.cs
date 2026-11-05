using Firefall.Reconstruction.Startup;

internal static class NetworkDispatcherFinalReturnSehStoreChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        static NetworkDispatcherFinalReturnSehStoreCapture Capture(
            uint? target = 0x11223344,
            bool? returnedNormally = true,
            uint? postCallEbp = 0x3010,
            uint? localMinusC = 0xAABBCCDD,
            bool? parentReturnedNormally = true,
            uint? parentEax = 0x12345678,
            uint? parentPostCellEbp = 0x3010,
            uint? parentPostCellEsi = 0x2F99,
            uint? stackScalar = 0x11223344,
            uint? fieldScalar = 0x55667788,
            uint? postCellEbx = 0x4000,
            uint? counter = 9,
            uint? entryEsp = 0x8000,
            uint? argument = 0xAABBCCDD)
        {
            var lockCapture = new NetworkDispatcherLockGateCapture(
                ResultEAX: 1,
                EntryESIReceiver: 0x1000,
                FreshCounterValue: 1,
                EntryEBP: 0x2000,
                FreshArgumentByte: 1,
                OpaqueImportReturnedNormally: null);
            var collection = new NetworkDispatcherCollectionGateCapture(
                lockCapture,
                EntryESP: 0x7000,
                EntryECX: 0x4444,
                FreshThresholdDWORD: 1,
                OpaqueCallReturnedNormally: true,
                FreshPostLoggingESI: 0x2000,
                FreshEndDWORD: 0x2004,
                FreshStartDWORD: 0x2000,
                FreshReloadDWORD: 0x4000,
                FreshCollectionEntryDWORD: 1);
            var preparation = new NetworkDispatcherEntryCallPreparationInputs(
                collection,
                FreshEntryESP: 0x8000,
                FreshVptrDWORD: 0x5000);
            var count = new NetworkDispatcherPostCallCountGateCapture(
                preparation,
                VtableCallReturnedNormally: true,
                FreshPostCallESI: 0x3000,
                FreshEndDWORD: 0x3008,
                FreshPostCallEBX: 1,
                FreshStartDWORD: 0x3000);
            var cell = new NetworkDispatcherCellReturnAddressGateCapture(
                count,
                parentReturnedNormally,
                parentEax,
                parentPostCellEbp,
                parentPostCellEsi);
            var publication = new NetworkDispatcherFencedPublicationCapture(
                cell,
                stackScalar,
                fieldScalar,
                postCellEbx,
                counter);
            var parent = new NetworkDispatcherFinalCallPreparationCapture(
                publication,
                entryEsp,
                argument);
            return new(parent, target, returnedNormally, postCallEbp, localMinusC);
        }

        NetworkDispatcherFinalReturnSehStorePlan normal = Project(Capture());
        check("The B0 call, B6 local load, B9 FS store, and C0 exclusive bound are pinned",
            NetworkDispatcherFinalReturnSehStoreProjection.CapturedWindowBytes
                == "FF157C42AC018B4DF464890D00000000"
            && Convert.ToHexString(Convert.FromHexString(
                NetworkDispatcherFinalReturnSehStoreProjection.CapturedWindowBytes))
                == "FF157C42AC018B4DF464890D00000000"
            && NetworkDispatcherFinalReturnSehStoreProjection.EntryRawFileOffset
                == 0x00BBDEB0
            && NetworkDispatcherFinalReturnSehStoreProjection.IndirectCallRawFileOffset
                == 0x00BBDEB0
            && NetworkDispatcherFinalReturnSehStoreProjection.PostCallLocalReadRawFileOffset
                == 0x00BBDEB6
            && NetworkDispatcherFinalReturnSehStoreProjection.FsStoreRawFileOffset
                == 0x00BBDEB9
            && NetworkDispatcherFinalReturnSehStoreProjection.PopRawFileOffset
                == 0x00BBDEC0
            && NetworkDispatcherFinalReturnSehStoreProjection.IndirectMemoryOperandAddress
                == 0x01AC427C
            && NetworkDispatcherFinalReturnSehStoreProjection.LocalDisplacement == 0x0C
            && NetworkDispatcherFinalReturnSehStoreProjection.FsSegmentOffset == 0
            && NetworkDispatcherFinalReturnSehStoreProjection.DwordWidth == 4
            && !NetworkDispatcherFinalReturnSehStoreProjection.ExecutesNativeInstructions);

        check("A normal opaque return reads the independent post-call local and stores it through FS offset zero",
            normal.ChildGateAdmitted
            && normal.Outcome
                == NetworkDispatcherFinalReturnSehStoreOutcome.BoundedBeforePop
            && normal.EntryRawFileOffset == 0x00BBDEB0
            && normal.IndirectTargetValue == 0x11223344
            && normal.Calls is [{
                RawFileOffset: 0x00BBDEB0,
                IndirectMemoryOperandAddress: 0x01AC427C,
                TargetValue: 0x11223344,
                CallSiteReached: true,
                TargetReadAttempted: true,
                TargetCaptured: true,
                CallAttempted: true,
                ReturnedNormally: true
            }]
            && normal.PostCallEBP == 0x3010
            && normal.LocalAddress == 0x3004
            && normal.LocalValue == 0xAABBCCDD
            && normal.Reads is [
                {
                    RawFileOffset: 0x00BBDEB0,
                    Address: 0x01AC427C,
                    Width: 4,
                    Attempted: true,
                    Captured: true,
                    Value: 0x11223344,
                    Committed: false
                },
                {
                    RawFileOffset: 0x00BBDEB6,
                    Address: 0x3004,
                    Width: 4,
                    Attempted: true,
                    Captured: true,
                    Value: 0xAABBCCDD,
                    Committed: false
                }
            ]
            && normal.Stores is [{
                RawFileOffset: 0x00BBDEB9,
                Segment: NetworkDispatcherFinalReturnSehStoreSegment.FS,
                SegmentOffset: 0,
                Width: 4,
                Value: 0xAABBCCDD,
                Attempted: true,
                Committed: true
            }]
            && normal.FsLinearAddress is null
            && normal.FlagsAtStop is {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && normal.StopAtRawFileOffsetExclusive == 0x00BBDEC0
            && normal.BoundedCompletionBoundaryReached
            && normal.ExactBoundedCompletion
            && normal.ExactCleanCompletion
            && normal.CaptureStates.FreshIndirectTargetDWORD
                == NetworkDispatcherFinalReturnSehStoreCaptureState.Captured
            && normal.CaptureStates.OpaqueCallReturnedNormally
                == NetworkDispatcherFinalReturnSehStoreCaptureState.Captured
            && normal.CaptureStates.FreshPostCallEBP
                == NetworkDispatcherFinalReturnSehStoreCaptureState.Captured
            && normal.CaptureStates.FreshLocalMinusCDWORD
                == NetworkDispatcherFinalReturnSehStoreCaptureState.Captured
            && !normal.PopInstructionEntered
            && normal.StackPointerAtStop is null
            && !normal.EAXInferred
            && !normal.AbiInferred
            && !normal.CalleeIdentityInferred);

        check("Zero and maximum target DWORDs are valid captured scalars when normal return is explicit",
            Project(Capture(target: 0)).ExactCleanCompletion
            && Project(Capture(target: 0)).IndirectTargetValue == 0
            && Project(Capture(target: uint.MaxValue)).ExactCleanCompletion
            && Project(Capture(target: uint.MaxValue)).IndirectTargetValue == uint.MaxValue);

        NetworkDispatcherFinalReturnSehStorePlan missingTarget = Project(
            Capture(target: null, returnedNormally: true));
        NetworkDispatcherFinalReturnSehStorePlan pendingReturn = Project(
            Capture(returnedNormally: null));
        NetworkDispatcherFinalReturnSehStorePlan abnormalReturn = Project(
            Capture(returnedNormally: false));
        check("Missing target, pending return, and abnormal return stop before post-call effects",
            missingTarget.Outcome
                == NetworkDispatcherFinalReturnSehStoreOutcome.MissingFreshIndirectTargetDWORD
            && missingTarget.Calls.IsEmpty
            && missingTarget.Reads is [{ Attempted: true, Captured: false }]
            && missingTarget.CaptureStates.OpaqueCallReturnedNormally
                == NetworkDispatcherFinalReturnSehStoreCaptureState.SuppliedUnused
            && missingTarget.CaptureStates.FreshPostCallEBP
                == NetworkDispatcherFinalReturnSehStoreCaptureState.SuppliedUnused
            && missingTarget.CaptureStates.FreshLocalMinusCDWORD
                == NetworkDispatcherFinalReturnSehStoreCaptureState.SuppliedUnused
            && missingTarget.FlagsAtStop == missingTarget.ParentPlan.FlagsAtStop
            && missingTarget.StopAtRawFileOffsetExclusive == 0x00BBDEB0
            && !missingTarget.ExactCleanCompletion
            && pendingReturn.Outcome
                == NetworkDispatcherFinalReturnSehStoreOutcome.OpaqueCallReturnPending
            && pendingReturn.Calls.Single().CallAttempted
            && pendingReturn.Calls.Single().ReturnedNormally is null
            && pendingReturn.CaptureStates.OpaqueCallReturnedNormally
                == NetworkDispatcherFinalReturnSehStoreCaptureState.Pending
            && pendingReturn.FlagsAtStop == pendingReturn.ParentPlan.FlagsAtStop
            && pendingReturn.CaptureStates.FreshPostCallEBP
                == NetworkDispatcherFinalReturnSehStoreCaptureState.SuppliedUnused
            && abnormalReturn.Outcome
                == NetworkDispatcherFinalReturnSehStoreOutcome.OpaqueCallDidNotReturnNormally
            && abnormalReturn.Calls.Single().CallAttempted
            && abnormalReturn.FlagsAtStop == abnormalReturn.ParentPlan.FlagsAtStop
            && abnormalReturn.CaptureStates.FreshPostCallEBP
                == NetworkDispatcherFinalReturnSehStoreCaptureState.SuppliedUnused
            && pendingReturn.StopAtRawFileOffsetExclusive == 0x00BBDEB0
            && abnormalReturn.StopAtRawFileOffsetExclusive == 0x00BBDEB0
            && !pendingReturn.ExactCleanCompletion
            && !abnormalReturn.ExactCleanCompletion
            && !pendingReturn.ExactBoundedCompletion
            && !abnormalReturn.ExactBoundedCompletion);

        NetworkDispatcherFinalReturnSehStorePlan missingEbp = Project(
            Capture(postCallEbp: null, localMinusC: 0));
        NetworkDispatcherFinalReturnSehStorePlan wrappedLocal = Project(
            Capture(postCallEbp: 0, localMinusC: 0));
        NetworkDispatcherFinalReturnSehStorePlan missingLocal = Project(
            Capture(postCallEbp: 0x10, localMinusC: null));
        check("Fresh post-call EBP is required and local address arithmetic wraps independently",
            missingEbp.Outcome
                == NetworkDispatcherFinalReturnSehStoreOutcome.MissingFreshPostCallEBP
            && missingEbp.Reads.Length == 1
            && missingEbp.CaptureStates.FreshPostCallEBP
                == NetworkDispatcherFinalReturnSehStoreCaptureState.Missing
            && missingEbp.CaptureStates.FreshLocalMinusCDWORD
                == NetworkDispatcherFinalReturnSehStoreCaptureState.SuppliedUnused
            && missingEbp.FlagsAtStop is { Carry: null, Parity: null, Zero: null }
            && missingEbp.StopAtRawFileOffsetExclusive == 0x00BBDEB6
            && wrappedLocal.LocalAddress == 0xFFFFFFF4
            && wrappedLocal.LocalValue == 0
            && wrappedLocal.ExactCleanCompletion
            && missingLocal.Outcome
                == NetworkDispatcherFinalReturnSehStoreOutcome.MissingFreshLocalMinusCDWORD
            && missingLocal.Reads is [
                { RawFileOffset: 0x00BBDEB0, Captured: true },
                {
                    RawFileOffset: 0x00BBDEB6,
                    Address: 4,
                    Width: 4,
                    Attempted: true,
                    Captured: false,
                    Value: null,
                    Committed: false
                }
            ]
            && missingLocal.Stores.IsEmpty
            && missingLocal.StopAtRawFileOffsetExclusive == 0x00BBDEB6
            && !missingEbp.ExactCleanCompletion
            && !missingLocal.ExactCleanCompletion);

        NetworkDispatcherFinalReturnSehStorePlan alias = Project(
            Capture(
                parentPostCellEbp: 0x8014,
                argument: 0xDEADBEEF,
                entryEsp: 0x8000,
                postCallEbp: 0x8008,
                localMinusC: 0xCAFEBABE));
        check("The post-call local is freshly sampled even when its address aliases the prior argument read and push slot",
            alias.ParentPlan.ArgumentAddress == 0x7FFC
            && alias.ParentPlan.ArgumentValue == 0xDEADBEEF
            && alias.ParentPlan.Pushes.Single().StackPointerAfter == 0x7FFC
            && alias.LocalAddress == 0x7FFC
            && alias.LocalAddress == alias.ParentPlan.ArgumentAddress
            && alias.LocalValue == 0xCAFEBABE
            && alias.ParentPlan.ArgumentValue != alias.LocalValue
            && alias.Stores.Single().Value == alias.LocalValue);

        NetworkDispatcherFinalReturnSehStorePlan dirtyParent = Project(Capture(
            parentEax: null,
            parentPostCellEbp: null,
            parentPostCellEsi: null,
            stackScalar: null,
            fieldScalar: null,
            postCellEbx: null,
            counter: null,
            target: 0,
            returnedNormally: true,
            postCallEbp: 0x1234,
            localMinusC: 0x5678));
        check("An unclean parent preserves its exact bound and leaves every child capture unused",
            !dirtyParent.ChildGateAdmitted
            && dirtyParent.Outcome
                == NetworkDispatcherFinalReturnSehStoreOutcome.ParentHandoffNotAdmitted
            && dirtyParent.EntryRawFileOffset
                == dirtyParent.ParentPlan.StopAtRawFileOffsetExclusive
            && dirtyParent.StopAtRawFileOffsetExclusive
                == dirtyParent.ParentPlan.StopAtRawFileOffsetExclusive
            && dirtyParent.Reads.IsEmpty
            && dirtyParent.Calls.IsEmpty
            && dirtyParent.Stores.IsEmpty
            && dirtyParent.CaptureStates.FreshIndirectTargetDWORD
                == NetworkDispatcherFinalReturnSehStoreCaptureState.SuppliedUnused
            && dirtyParent.CaptureStates.OpaqueCallReturnedNormally
                == NetworkDispatcherFinalReturnSehStoreCaptureState.SuppliedUnused
            && dirtyParent.CaptureStates.FreshPostCallEBP
                == NetworkDispatcherFinalReturnSehStoreCaptureState.SuppliedUnused
            && dirtyParent.CaptureStates.FreshLocalMinusCDWORD
                == NetworkDispatcherFinalReturnSehStoreCaptureState.SuppliedUnused
            && dirtyParent.ExactBoundedCompletion
                == dirtyParent.ParentPlan.ExactBoundedCompletion
            && !dirtyParent.BoundedCompletionBoundaryReached);

        NetworkDispatcherFinalReturnSehStorePlan rejectedWithoutCaptures = Project(Capture(
            parentEax: null,
            target: null,
            returnedNormally: null,
            postCallEbp: null,
            localMinusC: null));
        check("Unreached child captures remain distinct from supplied-but-unused captures",
            rejectedWithoutCaptures.CaptureStates.FreshIndirectTargetDWORD
                == NetworkDispatcherFinalReturnSehStoreCaptureState.Unreached
            && rejectedWithoutCaptures.CaptureStates.OpaqueCallReturnedNormally
                == NetworkDispatcherFinalReturnSehStoreCaptureState.Unreached
            && rejectedWithoutCaptures.CaptureStates.FreshPostCallEBP
                == NetworkDispatcherFinalReturnSehStoreCaptureState.Unreached
            && rejectedWithoutCaptures.CaptureStates.FreshLocalMinusCDWORD
                == NetworkDispatcherFinalReturnSehStoreCaptureState.Unreached
            && rejectedWithoutCaptures.Reads.IsEmpty
            && rejectedWithoutCaptures.Calls.IsEmpty
            && rejectedWithoutCaptures.Stores.IsEmpty);

        NetworkDispatcherFinalReturnSehStorePlan reprojection = Project(normal.Capture);
        check("Reprojection preserves the immutable parent prefix and ordered scalar effects semantically",
            ReferenceEquals(normal.Capture, reprojection.Capture)
            && normal.ParentPlan.EntryRawFileOffset
                == reprojection.ParentPlan.EntryRawFileOffset
            && normal.ParentPlan.StopAtRawFileOffsetExclusive
                == reprojection.ParentPlan.StopAtRawFileOffsetExclusive
            && normal.ParentPlan.OrderedEffects.Length
                == reprojection.ParentPlan.OrderedEffects.Length
            && normal.OrderedEffects.Length == reprojection.OrderedEffects.Length
            && normal.OrderedEffects.Select(effect => effect.Sequence)
                .SequenceEqual(Enumerable.Range(0, normal.OrderedEffects.Length))
            && normal.OrderedEffects.Select(effect => effect.Kind)
                .SequenceEqual(reprojection.OrderedEffects.Select(effect => effect.Kind))
            && normal.OrderedEffects.Select(effect => effect.RawFileOffset)
                .SequenceEqual(reprojection.OrderedEffects.Select(effect => effect.RawFileOffset))
            && normal.OrderedEffects.Take(normal.ParentPlan.OrderedEffects.Length)
                .All(effect => effect.ParentEffect.HasValue)
            && normal.OrderedEffects.Skip(normal.ParentPlan.OrderedEffects.Length)
                .Select(effect => effect.Kind)
                .SequenceEqual(new[]
                {
                    NetworkDispatcherFinalReturnSehStoreOrderedEffectKind
                        .FreshIndirectTargetDWORDRead,
                    NetworkDispatcherFinalReturnSehStoreOrderedEffectKind.OpaqueCall,
                    NetworkDispatcherFinalReturnSehStoreOrderedEffectKind
                        .OpaqueCallReturnObservation,
                    NetworkDispatcherFinalReturnSehStoreOrderedEffectKind
                        .FreshLocalMinusCDWORDRead,
                    NetworkDispatcherFinalReturnSehStoreOrderedEffectKind.FsSegmentDWORDStore
                })
            && normal.OrderedEffects.Skip(normal.ParentPlan.OrderedEffects.Length)
                .Select(effect => effect.Kind)
                .SequenceEqual(reprojection.OrderedEffects
                    .Skip(reprojection.ParentPlan.OrderedEffects.Length)
                    .Select(effect => effect.Kind))
            && normal.Reads.Select(read => (read.RawFileOffset, read.Address, read.Value))
                .SequenceEqual(reprojection.Reads.Select(read =>
                    (read.RawFileOffset, read.Address, read.Value))));
    }

    private static NetworkDispatcherFinalReturnSehStorePlan Project(
        NetworkDispatcherFinalReturnSehStoreCapture capture) =>
        NetworkDispatcherFinalReturnSehStoreProjection.Project(capture);
}
