using System.Collections.Immutable;
using Firefall.Reconstruction.Startup;

internal static class StartupHelper12CC2B0Checks
{
    public static void Run(Action<string, bool> check)
    {
        check("Raw 0x12CC2B0 helper anchors match the pinned sample and full function window",
            StartupHelper12CC2B0Projection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && StartupHelper12CC2B0Projection.EntryRawFileOffset == 0x12CC2B0
            && StartupHelper12CC2B0Projection.EntryEndRawFileOffsetExclusive == 0x12CC2E1
            && StartupHelper12CC2B0Projection.FunctionBytes
                == "56 57 8B F9 8D 4F 08 E8 24 18 00 00 85 C0 74 13 "
                    + "8B C8 E8 99 CA FF FF 8D 4F 08 E8 11 18 00 00 "
                    + "85 C0 75 ED 8B CF E8 86 CA FF FF 8B C7 5F 5E C2 04 00");

        StartupHelper12CC2B0Plan immediateEmpty =
            StartupHelper12CC2B0Projection.Project(new StartupHelper12CC2B0Capture(
                0x10203040,
                ImmutableArray.Create(0u),
                ImmutableArray.Create<bool?>(true)));
        check("An immediate zero pop calls the backend with original this and returns it after normal return",
            immediateEmpty.Actions.SequenceEqual(new[]
            {
                new StartupHelper12CC2B0Action(
                    StartupHelper12CC2B0ActionKind.CallPop,
                    0x12CC2B7,
                    0x10203048,
                    0x12CDAE0),
                new StartupHelper12CC2B0Action(
                    StartupHelper12CC2B0ActionKind.CallBackendForOriginalThis,
                    0x12CC2D5,
                    0x10203040,
                    0x12C8D60),
                new StartupHelper12CC2B0Action(
                    StartupHelper12CC2B0ActionKind.ReturnNormallyWithOriginalThis,
                    0x12CC2DA,
                    0x10203040)
            })
            && immediateEmpty.Boundary == StartupHelper12CC2B0Boundary.ReturnedNormally
            && immediateEmpty.ReturnedEax == 0x10203040
            && immediateEmpty.ReturnStackPopBytes == 4);

        StartupHelper12CC2B0Plan popThenEmpty =
            StartupHelper12CC2B0Projection.Project(new StartupHelper12CC2B0Capture(
                0x10203040,
                ImmutableArray.Create(0x55667788u, 0u),
                ImmutableArray.Create<bool?>(true, true)));
        check("A nonzero pop is passed to the backend before a later zero pop selects original this",
            popThenEmpty.Actions.SequenceEqual(new[]
            {
                new StartupHelper12CC2B0Action(
                    StartupHelper12CC2B0ActionKind.CallPop,
                    0x12CC2B7,
                    0x10203048,
                    0x12CDAE0),
                new StartupHelper12CC2B0Action(
                    StartupHelper12CC2B0ActionKind.CallBackendForCapturedPop,
                    0x12CC2C2,
                    0x55667788,
                    0x12C8D60),
                new StartupHelper12CC2B0Action(
                    StartupHelper12CC2B0ActionKind.CallPop,
                    0x12CC2CA,
                    0x10203048,
                    0x12CDAE0),
                new StartupHelper12CC2B0Action(
                    StartupHelper12CC2B0ActionKind.CallBackendForOriginalThis,
                    0x12CC2D5,
                    0x10203040,
                    0x12C8D60),
                new StartupHelper12CC2B0Action(
                    StartupHelper12CC2B0ActionKind.ReturnNormallyWithOriginalThis,
                    0x12CC2DA,
                    0x10203040)
            })
            && popThenEmpty.Boundary == StartupHelper12CC2B0Boundary.ReturnedNormally
            && popThenEmpty.ReturnedEax == 0x10203040);

        StartupHelper12CC2B0Plan missingLaterPop =
            StartupHelper12CC2B0Projection.Project(new StartupHelper12CC2B0Capture(
                0x10203040,
                ImmutableArray.Create(0x55667788u),
                ImmutableArray.Create<bool?>(true)));
        check("A missing later pop result remains incomplete after the first backend returns",
            missingLaterPop.Boundary == StartupHelper12CC2B0Boundary.IncompleteAtPopResult
            && missingLaterPop.Actions[^1].Kind == StartupHelper12CC2B0ActionKind.CallPop
            && missingLaterPop.Actions[^1].InstructionRawFileOffset == 0x12CC2CA
            && missingLaterPop.ReturnedEax is null
            && missingLaterPop.ReturnStackPopBytes is null);

        StartupHelper12CC2B0Plan stoppedBackend =
            StartupHelper12CC2B0Projection.Project(new StartupHelper12CC2B0Capture(
                0x10203040,
                ImmutableArray.Create(0u),
                ImmutableArray.Create<bool?>(false)));
        StartupHelper12CC2B0Plan missingBackendReturn =
            StartupHelper12CC2B0Projection.Project(new StartupHelper12CC2B0Capture(
                0x10203040,
                ImmutableArray.Create(0u),
                ImmutableArray<bool?>.Empty));
        check("Missing and non-normal backend returns stop before the projected return",
            stoppedBackend.Boundary == StartupHelper12CC2B0Boundary.StoppedAtNonReturningBackend
            && stoppedBackend.ReturnedEax is null
            && missingBackendReturn.Boundary == StartupHelper12CC2B0Boundary.IncompleteAtBackendReturn
            && missingBackendReturn.ReturnedEax is null);

        StartupHelper12CC2B0Plan wrappingThis =
            StartupHelper12CC2B0Projection.Project(new StartupHelper12CC2B0Capture(
                0xFFFFFFFC,
                ImmutableArray.Create(0u),
                ImmutableArray.Create<bool?>(true)));
        check("The pop receiver uses wrapping 32-bit this-plus-eight arithmetic",
            wrappingThis.Actions[0].Argument == 4);
    }
}
