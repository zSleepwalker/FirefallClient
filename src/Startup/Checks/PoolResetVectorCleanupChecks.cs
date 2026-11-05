using Firefall.Reconstruction.Startup;

internal static class PoolResetVectorCleanupChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Callback sample identity, raw offsets, and raw-to-code-VA mapping are pinned",
            PoolResetVectorCleanupProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && PoolResetVectorCleanupProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && PoolResetVectorCleanupProjectionRules.CallbackRawFileOffset == 0x12C0920
            && PoolResetVectorCleanupProjectionRules.CallbackVirtualAddress == 0x16C1520
            && PoolResetVectorCleanupProjectionRules.CallbackEndRawFileOffsetExclusive == 0x12C0949
            && PoolResetVectorCleanupProjectionRules.CallbackSize == 0x29
            && PoolResetVectorCleanupProjectionRules.CallbackRawFileOffset
                + PoolResetVectorCleanupProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == PoolResetVectorCleanupProjectionRules.CallbackVirtualAddress
            && PoolResetVectorCleanupProjectionRules.RegistrationCallerRawFileOffset == 0x12C02F0
            && PoolResetVectorCleanupProjectionRules.RegistrationCallBytesRawFileOffset == 0x12C0333
            && PoolResetVectorCleanupProjectionRules.CallbackPointerPushRawFileOffset == 0x12C0336
            && PoolResetVectorCleanupProjectionRules.ResetReceiverThisOffset == 0x20
            && PoolResetVectorCleanupProjectionRules.ResetReceiverRegisterMoveRawFileOffset == 0x12C033B
            && PoolResetVectorCleanupProjectionRules.ResetCallRawFileOffset == 0x12C0340
            && PoolResetVectorCleanupProjectionRules.ResetTargetRawFileOffset == 0x12CDC50);

        check("Registration call bytes push callback VA 0x16C1520 and target reset 0x12CDC50",
            PoolResetVectorCleanupProjectionRules.CapturedRegistrationCallBytes
                == "8D 43 20 68 20 15 6C 01 8B C8 89 45 F0 E8 0B D9 00 00"
            && PoolResetVectorCleanupProjectionRules.CapturedRegistrationCallBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x12
            && Slice(PoolResetVectorCleanupProjectionRules.CapturedRegistrationCallBytes, 0, 3)
                == "8D 43 20"
            && Slice(PoolResetVectorCleanupProjectionRules.CapturedRegistrationCallBytes, 3, 5)
                == "68 20 15 6C 01"
            && Slice(PoolResetVectorCleanupProjectionRules.CapturedRegistrationCallBytes, 8, 2)
                == "8B C8"
            && Slice(PoolResetVectorCleanupProjectionRules.CapturedRegistrationCallBytes, 13, 5)
                == "E8 0B D9 00 00"
            && PoolResetVectorCleanupProjectionRules.CallbackPointerPushRawFileOffset
                == PoolResetVectorCleanupProjectionRules.RegistrationCallBytesRawFileOffset + 3
            && PoolResetVectorCleanupProjectionRules.ResetCallRawFileOffset
                == PoolResetVectorCleanupProjectionRules.RegistrationCallBytesRawFileOffset + 13
            && PoolResetVectorCleanupProjectionRules.ResetCallRawFileOffset + 5 + 0xD90B
                == PoolResetVectorCleanupProjectionRules.ResetTargetRawFileOffset);

        check("Exact callback bytes establish one [EBP+8] argument, null branch, helper target, and conditional stores",
            PoolResetVectorCleanupProjectionRules.CapturedCallbackBytes
                == "55 8B EC 56 8B 75 08 8B 0E 85 C9 74 19 E8 2E 84 00 00 "
                    + "C7 06 00 00 00 00 C7 46 04 00 00 00 00 C7 46 08 00 00 00 00 "
                    + "5E 5D C3"
            && PoolResetVectorCleanupProjectionRules.CapturedCallbackBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
                == PoolResetVectorCleanupProjectionRules.CallbackSize
            && Slice(PoolResetVectorCleanupProjectionRules.CapturedCallbackBytes, 4, 3) == "8B 75 08"
            && Slice(PoolResetVectorCleanupProjectionRules.CapturedCallbackBytes, 7, 2) == "8B 0E"
            && Slice(PoolResetVectorCleanupProjectionRules.CapturedCallbackBytes, 9, 2) == "85 C9"
            && Slice(PoolResetVectorCleanupProjectionRules.CapturedCallbackBytes, 11, 2) == "74 19"
            && Slice(PoolResetVectorCleanupProjectionRules.CapturedCallbackBytes, 13, 5)
                == "E8 2E 84 00 00"
            && Slice(PoolResetVectorCleanupProjectionRules.CapturedCallbackBytes, 18, 6)
                == "C7 06 00 00 00 00"
            && Slice(PoolResetVectorCleanupProjectionRules.CapturedCallbackBytes, 24, 7)
                == "C7 46 04 00 00 00 00"
            && Slice(PoolResetVectorCleanupProjectionRules.CapturedCallbackBytes, 31, 7)
                == "C7 46 08 00 00 00 00"
            && PoolResetVectorCleanupProjectionRules.CallbackArgumentLoadRawFileOffset == 0x12C0924
            && PoolResetVectorCleanupProjectionRules.CallbackStackArgumentOffset == 8
            && PoolResetVectorCleanupProjectionRules.FirstFieldTestRawFileOffset == 0x12C0929
            && PoolResetVectorCleanupProjectionRules.NullBranchRawFileOffset == 0x12C092B
            && PoolResetVectorCleanupProjectionRules.NullBranchTargetRawFileOffset == 0x12C0946
            && PoolResetVectorCleanupProjectionRules.NullBranchRawFileOffset + 2 + 0x19
                == PoolResetVectorCleanupProjectionRules.NullBranchTargetRawFileOffset
            && PoolResetVectorCleanupProjectionRules.OpaqueHelperCallRawFileOffset == 0x12C092D
            && PoolResetVectorCleanupProjectionRules.OpaqueHelperTargetRawFileOffset == 0x12C8D60
            && PoolResetVectorCleanupProjectionRules.OpaqueHelperCallRawFileOffset + 5 + 0x842E
                == PoolResetVectorCleanupProjectionRules.OpaqueHelperTargetRawFileOffset
            && PoolResetVectorCleanupProjectionRules.FirstZeroStoreRawFileOffset == 0x12C0932
            && PoolResetVectorCleanupProjectionRules.SecondZeroStoreRawFileOffset == 0x12C0938
            && PoolResetVectorCleanupProjectionRules.ThirdZeroStoreRawFileOffset == 0x12C093F
            && PoolResetVectorCleanupProjectionRules.NormalReturnRawFileOffset == 0x12C0948
            && PoolResetVectorCleanupProjectionRules.VectorFirstFieldOffset == 0
            && PoolResetVectorCleanupProjectionRules.VectorSecondFieldOffset == 4
            && PoolResetVectorCleanupProjectionRules.VectorThirdFieldOffset == 8);

        check("One stack argument uses cdecl cleanup in the caller, not RET immediate",
            PoolResetVectorCleanupProjectionRules.CallbackArgumentCount == 1
            && PoolResetVectorCleanupProjectionRules.CallbackCallingConvention == "cdecl"
            && PoolResetVectorCleanupProjectionRules.NormalReturnRawFileOffset
                == PoolResetVectorCleanupProjectionRules.CallbackEndRawFileOffsetExclusive - 1
            && Slice(PoolResetVectorCleanupProjectionRules.CapturedCallbackBytes, 0x28, 1) == "C3"
            && PoolResetVectorCleanupProjectionRules.CallbackInvocationArgumentPushRawFileOffset
                == 0x12CDDD3
            && PoolResetVectorCleanupProjectionRules.CallbackInvocationCallRawFileOffset
                == 0x12CDDD4
            && PoolResetVectorCleanupProjectionRules.CallbackCallerStackCleanupRawFileOffset
                == 0x12CDDDa
            && PoolResetVectorCleanupProjectionRules.CallbackCallerStackCleanupBytes == 4
            && PoolResetVectorCleanupProjectionRules.CapturedCallbackInvocationBytes
                == "51 FF 55 08 8B 55 CC 83 C4 04 FF 45 E8"
            && PoolResetVectorCleanupProjectionRules.CapturedCallbackInvocationBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x0D
            && Slice(PoolResetVectorCleanupProjectionRules.CapturedCallbackInvocationBytes, 0, 1) == "51"
            && Slice(PoolResetVectorCleanupProjectionRules.CapturedCallbackInvocationBytes, 1, 3)
                == "FF 55 08"
            && Slice(PoolResetVectorCleanupProjectionRules.CapturedCallbackInvocationBytes, 7, 3)
                == "83 C4 04");

        PoolResetVectorCleanupProjection nullFirstField =
            PoolResetVectorCleanupProjectionRules.Project(
                new PoolResetVectorCleanupCapture(
                    VectorAddress: 0x1000,
                    FirstFieldValue: 0,
                    OpaqueHelperReturnedNormally: null));
        check("A null first field skips the opaque helper and every zero store",
            nullFirstField.Capture.VectorAddress == 0x1000
            && nullFirstField.Capture.FirstFieldValue == 0
            && !nullFirstField.OpaqueHelperCallReached
            && nullFirstField.OpaqueHelperEcXValue is null
            && !nullFirstField.ZeroStoresReached
            && nullFirstField.Boundary == PoolResetVectorCleanupBoundary.NullFirstField);

        PoolResetVectorCleanupProjection helperCallBoundary =
            PoolResetVectorCleanupProjectionRules.Project(
                new PoolResetVectorCleanupCapture(
                    VectorAddress: 0x2000,
                    FirstFieldValue: 0x3000,
                    OpaqueHelperReturnedNormally: null));
        check("A non-null first field reaches only the opaque-helper call boundary without return evidence",
            helperCallBoundary.Capture.VectorAddress == 0x2000
            && helperCallBoundary.OpaqueHelperCallReached
            && helperCallBoundary.OpaqueHelperEcXValue == 0x3000
            && !helperCallBoundary.ZeroStoresReached
            && helperCallBoundary.Boundary
                == PoolResetVectorCleanupBoundary.OpaqueHelperCallBoundary);

        PoolResetVectorCleanupProjection helperDidNotReturn =
            PoolResetVectorCleanupProjectionRules.Project(
                new PoolResetVectorCleanupCapture(
                    VectorAddress: 0x4000,
                    FirstFieldValue: 0x5000,
                    OpaqueHelperReturnedNormally: false));
        check("A captured nonreturning opaque helper stops before all zero-store instructions",
            helperDidNotReturn.OpaqueHelperCallReached
            && helperDidNotReturn.OpaqueHelperEcXValue == 0x5000
            && !helperDidNotReturn.ZeroStoresReached
            && helperDidNotReturn.Boundary
                == PoolResetVectorCleanupBoundary.OpaqueHelperDidNotReturn);

        PoolResetVectorCleanupProjection helperReturned =
            PoolResetVectorCleanupProjectionRules.Project(
                new PoolResetVectorCleanupCapture(
                    VectorAddress: 0x6000,
                    FirstFieldValue: 0x7000,
                    OpaqueHelperReturnedNormally: true));
        check("Only a captured normal opaque-helper return reaches the three zero stores",
            helperReturned.OpaqueHelperCallReached
            && helperReturned.OpaqueHelperEcXValue == 0x7000
            && helperReturned.ZeroStoresReached
            && helperReturned.Boundary == PoolResetVectorCleanupBoundary.ZeroStoresReached);

        check("A helper return outcome is rejected when the null branch prevents that call",
            ThrowsArgumentException(() =>
                PoolResetVectorCleanupProjectionRules.Project(
                    new PoolResetVectorCleanupCapture(
                        VectorAddress: 0x8000,
                        FirstFieldValue: 0,
                        OpaqueHelperReturnedNormally: true))));
    }

    private static string Slice(string capturedBytes, int start, int count) =>
        string.Join(" ",
            capturedBytes.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(start).Take(count));

    private static bool ThrowsArgumentException(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentException)
        {
            return true;
        }
    }
}
