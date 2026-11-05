using Firefall.Reconstruction.Startup;

internal static class ReliableReceiveForwardingCallerChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Both caller windows, PE raw-to-VA mapping, bytes, and helper return sites are exact",
            ReliableReceiveForwardingCallerProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && ReliableReceiveForwardingCallerProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && ReliableReceiveForwardingCallerProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && ReliableReceiveForwardingCallerProjection.StartRawFileOffset(
                ReliableReceiveForwardingCallerRoute.HighDelta) == 0x00BE52A5
            && ReliableReceiveForwardingCallerProjection.EndRawFileOffsetExclusive(
                ReliableReceiveForwardingCallerRoute.HighDelta) == 0x00BE52C7
            && ReliableReceiveForwardingCallerProjection.RawFileOffsetToVirtualAddress(0x00BE52A5)
                == 0x00FE5EA5
            && ReliableReceiveForwardingCallerProjection.StartRawFileOffset(
                ReliableReceiveForwardingCallerRoute.ExistingRecord) == 0x00BE5353
            && ReliableReceiveForwardingCallerProjection.EndRawFileOffsetExclusive(
                ReliableReceiveForwardingCallerRoute.ExistingRecord) == 0x00BE5375
            && ReliableReceiveForwardingCallerProjection.RawFileOffsetToVirtualAddress(0x00BE5353)
                == 0x00FE5F53
            && WindowBytesMatch(ReliableReceiveForwardingCallerRoute.HighDelta,
                "68 EF 01 00 00 68 78 8C BB 01 FF 76 04 68 AC 8B BB 01 "
                + "6A 00 FF 76 24 E8 8F DC FF FF 8B 46 08 83 C4 18")
            && WindowBytesMatch(ReliableReceiveForwardingCallerRoute.ExistingRecord,
                "68 FB 01 00 00 68 60 8B BB 01 FF 76 04 68 3C 8B BB 01 "
                + "6A 00 FF 76 24 E8 E1 DB FF FF 8B 46 08 83 C4 18")
            && ReliableReceiveForwardingCallerProjection.HelperTargetRawFileOffset == 0x00BE2F50
            && ReliableReceiveForwardingCallerProjection.CallRawFileOffset(
                ReliableReceiveForwardingCallerRoute.HighDelta) == 0x00BE52BC
            && ReliableReceiveForwardingCallerProjection.CallReturnRawFileOffset(
                ReliableReceiveForwardingCallerRoute.HighDelta) == 0x00BE52C1
            && ReliableReceiveForwardingCallerProjection.CallRawFileOffset(
                ReliableReceiveForwardingCallerRoute.ExistingRecord) == 0x00BE536A
            && ReliableReceiveForwardingCallerProjection.CallReturnRawFileOffset(
                ReliableReceiveForwardingCallerRoute.ExistingRecord) == 0x00BE536F
            && ReliableReceiveForwardingCallerProjection.ComputedHelperTargetRawFileOffset(
                ReliableReceiveForwardingCallerRoute.HighDelta) == 0x00BE2F50
            && ReliableReceiveForwardingCallerProjection.ComputedHelperTargetRawFileOffset(
                ReliableReceiveForwardingCallerRoute.ExistingRecord) == 0x00BE2F50
            && ReliableReceiveForwardingCallerProjection.RelativeCallTarget(
                0x00BE52BC, unchecked((int)0xFFFFDC8F))
                == 0x00BE2F50
            && ReliableReceiveForwardingCallerProjection.RelativeCallTarget(
                0x00BE536A, unchecked((int)0xFFFFDBE1)) == 0x00BE2F50
            && ReliableReceiveForwardingCallerProjection.HelperEntryReturnAddressStackOffset == 0
            && ReliableReceiveForwardingCallerProjection.HelperArgumentStackOffsets.SequenceEqual(
                new uint[] { 0x04, 0x08, 0x0C, 0x10, 0x14, 0x18 }));

        check("Each route preserves the six instruction-order pushes and helper stack arguments",
            VerifyRoute(ReliableReceiveForwardingCallerRoute.HighDelta,
                new(
                    ReliableReceiveForwardingCallerRoute.HighDelta,
                    0x1000,
                    0x11223344,
                    0x55667788,
                    true,
                    0),
                new uint[] { 0x01EF, 0x01BB8C78, 0x11223344, 0x01BB8BAC, 0, 0x55667788 },
                new uint[] { 0x55667788, 0, 0x01BB8BAC, 0x11223344, 0x01BB8C78, 0x01EF })
            && VerifyRoute(ReliableReceiveForwardingCallerRoute.ExistingRecord,
                new(
                    ReliableReceiveForwardingCallerRoute.ExistingRecord,
                    0x2000,
                    0xAABBCCDD,
                    0xEEFF0011,
                    true,
                    0),
                new uint[] { 0x01FB, 0x01BB8B60, 0xAABBCCDD, 0x01BB8B3C, 0, 0xEEFF0011 },
                new uint[] { 0xEEFF0011, 0, 0x01BB8B3C, 0xAABBCCDD, 0x01BB8B60, 0x01FB }));

        check("Receiver read addresses wrap as 32-bit pointers and zero is a valid captured DWORD",
            VerifyWrappedAddresses(ReliableReceiveForwardingCallerRoute.HighDelta)
            && VerifyWrappedAddresses(ReliableReceiveForwardingCallerRoute.ExistingRecord));

        ReliableReceiveForwardingCallerPlan missingOffset4 =
            ReliableReceiveForwardingCallerProjection.Project(new(
                ReliableReceiveForwardingCallerRoute.HighDelta, 0x1000, null, null, null, null));
        ReliableReceiveForwardingCallerPlan unreachedOffset24 =
            ReliableReceiveForwardingCallerProjection.Project(new(
                ReliableReceiveForwardingCallerRoute.HighDelta, 0x1000, null, 0, true, 0));
        ReliableReceiveForwardingCallerPlan missingOffset24 =
            ReliableReceiveForwardingCallerProjection.Project(new(
                ReliableReceiveForwardingCallerRoute.ExistingRecord, 0x2000, 0, null, null, null));
        ReliableReceiveForwardingCallerPlan unreachedHelper =
            ReliableReceiveForwardingCallerProjection.Project(new(
                ReliableReceiveForwardingCallerRoute.ExistingRecord, 0x2000, 0, null, true, 0));
        check("Receiver+4 is required before receiver+0x24; later supplied captures remain unreached",
            missingOffset4.Outcome == ReliableReceiveForwardingCallerOutcome.MissingReceiverOffset4Capture
            && missingOffset4.ReceiverOffset4Address == 0x1004
            && !missingOffset4.UnreachedReceiverOffset24CaptureProvided
            && unreachedOffset24.Outcome == ReliableReceiveForwardingCallerOutcome.UnreachedLaterCapture
            && !unreachedOffset24.ReceiverOffset4Read
            && unreachedOffset24.UnreachedReceiverOffset24CaptureProvided
            && unreachedOffset24.UnreachedHelperReturnCaptureProvided
            && unreachedOffset24.UnreachedReceiverOffset8CaptureProvided
            && unreachedOffset24.StopAtRawFileOffsetExclusive == 0x00BE52AF
            && missingOffset4.Outcome == ReliableReceiveForwardingCallerOutcome.MissingReceiverOffset4Capture
            && !missingOffset4.ReceiverOffset4Read
            && !missingOffset4.ReceiverOffset24Read
            && missingOffset4.ReceiverOffset24Address is null
            && missingOffset4.PushedValuesInInstructionOrder.SequenceEqual(new uint[] { 0x01EF, 0x01BB8C78 })
            && !missingOffset4.HelperCallReached
            && !missingOffset4.CleanupReached
            && missingOffset4.StopAtRawFileOffsetExclusive == 0x00BE52AF
            && missingOffset24.Outcome == ReliableReceiveForwardingCallerOutcome.MissingReceiverOffset24Capture
            && missingOffset24.ReceiverOffset4Read
            && missingOffset24.ReceiverOffset4Address == 0x2004
            && !missingOffset24.ReceiverOffset24Read
            && missingOffset24.ReceiverOffset24Address == 0x2024
            && !missingOffset24.UnreachedHelperReturnCaptureProvided
            && !missingOffset24.UnreachedReceiverOffset8CaptureProvided
            && missingOffset24.PushedValuesInInstructionOrder.SequenceEqual(
                new uint[] { 0x01FB, 0x01BB8B60, 0, 0x01BB8B3C, 0 })
            && !missingOffset24.HelperCallReached
            && !missingOffset24.CleanupReached
            && missingOffset24.StopAtRawFileOffsetExclusive == 0x00BE5367
            && unreachedHelper.Outcome == ReliableReceiveForwardingCallerOutcome.UnreachedLaterCapture
            && unreachedHelper.ReceiverOffset4Read
            && !unreachedHelper.ReceiverOffset24Read
            && unreachedHelper.UnreachedHelperReturnCaptureProvided
            && unreachedHelper.UnreachedReceiverOffset8CaptureProvided);

        ReliableReceiveForwardingCallerPlan missingReturn =
            ReliableReceiveForwardingCallerProjection.Project(new(
                ReliableReceiveForwardingCallerRoute.HighDelta, 0x3000, 1, 2, null, null));
        ReliableReceiveForwardingCallerPlan nonReturningHelper =
            ReliableReceiveForwardingCallerProjection.Project(new(
                ReliableReceiveForwardingCallerRoute.ExistingRecord, 0x4000, 1, 2, false, 0));
        ReliableReceiveForwardingCallerPlan nonReturningWithoutLaterCapture =
            ReliableReceiveForwardingCallerProjection.Project(new(
                ReliableReceiveForwardingCallerRoute.ExistingRecord, 0x4000, 1, 2, false, null));
        check("Missing or non-returning helper calls stop before the post-call read and cleanup",
            missingReturn.Outcome == ReliableReceiveForwardingCallerOutcome.MissingHelperReturnCapture
            && missingReturn.HelperCallReached
            && missingReturn.HelperTargetRawFileOffset == 0x00BE2F50
            && missingReturn.HelperCallContinuationRawFileOffset == 0x00BE52C1
            && missingReturn.HelperReturnedNormally is null
            && !missingReturn.ReceiverOffset8Read
            && !missingReturn.CleanupReached
            && missingReturn.CleanupBytes is null
            && missingReturn.StopAtRawFileOffsetExclusive == 0x00BE52C1
            && nonReturningHelper.Outcome == ReliableReceiveForwardingCallerOutcome.UnreachedReceiverOffset8Capture
            && nonReturningHelper.HelperCallReached
            && nonReturningHelper.HelperReturnedNormally == false
            && nonReturningHelper.UnreachedReceiverOffset8CaptureProvided
            && !nonReturningHelper.ReceiverOffset8Read
            && !nonReturningHelper.CleanupReached
            && nonReturningHelper.CleanupBytes is null
            && nonReturningHelper.StopAtRawFileOffsetExclusive == 0x00BE536F
            && nonReturningWithoutLaterCapture.Outcome
                == ReliableReceiveForwardingCallerOutcome.HelperDidNotReturnNormally
            && nonReturningWithoutLaterCapture.HelperReturnedNormally == false
            && !nonReturningWithoutLaterCapture.CleanupReached
            && nonReturningWithoutLaterCapture.StopAtRawFileOffsetExclusive == 0x00BE536F);

        ReliableReceiveForwardingCallerPlan missingPostCallRead =
            ReliableReceiveForwardingCallerProjection.Project(new(
                ReliableReceiveForwardingCallerRoute.HighDelta, 0x5000, 1, 2, true, null));
        ReliableReceiveForwardingCallerPlan completed =
            ReliableReceiveForwardingCallerProjection.Project(new(
                ReliableReceiveForwardingCallerRoute.HighDelta, 0x5000, 1, 2, true, 0));
        check("The post-call receiver+8 read gates cleanup; native zero still permits cleanup",
            missingPostCallRead.Outcome == ReliableReceiveForwardingCallerOutcome.MissingReceiverOffset8Capture
            && missingPostCallRead.ReceiverOffset8Address == 0x5008
            && !missingPostCallRead.ReceiverOffset8Read
            && missingPostCallRead.ReceiverOffset8DwordLoadedIntoEax is null
            && !missingPostCallRead.CleanupReached
            && missingPostCallRead.CleanupBytes is null
            && missingPostCallRead.StopAtRawFileOffsetExclusive == 0x00BE52C1
            && completed.Outcome == ReliableReceiveForwardingCallerOutcome.CleanupCompleted
            && completed.ReceiverOffset8Read
            && completed.ReceiverOffset8Address == 0x5008
            && completed.ReceiverOffset8DwordLoadedIntoEax == 0
            && completed.CleanupReached
            && completed.CleanupBytes == 0x18
            && completed.StopAtRawFileOffsetExclusive == 0x00BE52C7);
    }

    private static bool WindowBytesMatch(
        ReliableReceiveForwardingCallerRoute route,
        string expected)
    {
        string actual = ReliableReceiveForwardingCallerProjection.CapturedWindowBytes(route);
        return actual == expected
            && actual.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x22;
    }

    private static bool VerifyRoute(
        ReliableReceiveForwardingCallerRoute route,
        ReliableReceiveForwardingCallerCapture capture,
        uint[] expectedPushes,
        uint[] expectedArguments)
    {
        ReliableReceiveForwardingCallerPlan plan =
            ReliableReceiveForwardingCallerProjection.Project(capture);
        return plan.Capture.Route == route
            && plan.Outcome == ReliableReceiveForwardingCallerOutcome.CleanupCompleted
            && plan.PushedValuesInInstructionOrder.SequenceEqual(expectedPushes)
            && ReliableReceiveForwardingCallerProjection.HelperArgumentsInStackOrder(capture)
                .SequenceEqual(expectedArguments)
            && plan.HelperCallReached
            && plan.HelperTargetRawFileOffset == 0x00BE2F50
            && plan.HelperReturnedNormally == true
            && plan.ReceiverOffset8DwordLoadedIntoEax == 0
            && plan.CleanupReached
            && plan.CleanupBytes == 0x18;
    }

    private static bool VerifyWrappedAddresses(ReliableReceiveForwardingCallerRoute route)
    {
        var capture = new ReliableReceiveForwardingCallerCapture(
            route,
            uint.MaxValue - 3,
            0,
            0,
            true,
            0);
        ReliableReceiveForwardingCallerPlan plan =
            ReliableReceiveForwardingCallerProjection.Project(capture);
        return plan.ReceiverOffset4Address == 0
            && plan.ReceiverOffset24Address == 0x20
            && plan.ReceiverOffset8Address == 4
            && plan.ReceiverOffset8DwordLoadedIntoEax == 0
            && plan.CleanupReached
            && ReliableReceiveForwardingCallerProjection.HelperArgumentsInStackOrder(capture)[0] == 0
            && ReliableReceiveForwardingCallerProjection.HelperArgumentsInStackOrder(capture)[3] == 0;
    }
}
