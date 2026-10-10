using Firefall.Reconstruction.Startup;

internal static class MatrixChatIdentityChecks
{
    public static void Run(Action<string, bool> check)
    {
        foreach (byte type in new byte[] { 0, 1, 4, 0x80, 0xFF })
        {
            var identity = MatrixChatIdentityNativeLayout.ProjectSuccessfulWorldInitialization(
                0x0102030405060700UL | type, "Owner"u8);
            check($"World chat identity preserves upper56bits and forces type1 from {type:X2}",
                identity.RawId == 0x0102030405060701 && identity.TextBytes.Span.SequenceEqual("Owner"u8));
        }
        check("World chat identity does not introduce an unobserved invalid-ID filter",
            MatrixChatIdentityNativeLayout.ProjectSuccessfulWorldInitialization(0, default).RawId == 1
            && MatrixChatIdentityNativeLayout.ProjectSuccessfulWorldInitialization(ulong.MaxValue, default).RawId
                == 0xFFFFFFFFFFFFFF01);
        check("Login gate preserves exact nonzero-byte selection and a fresh tfChat starts with zero/empty identity",
            !MatrixChatIdentityNativeLayout.SelectLoginInitialization(0)
            && MatrixChatIdentityNativeLayout.SelectLoginInitialization(1)
            && MatrixChatIdentityNativeLayout.SelectLoginInitialization(0x80)
            && MatrixChatIdentityNativeLayout.CreateFreshChatIdentity().RawId == 0
            && MatrixChatIdentityNativeLayout.CreateFreshChatIdentity().TextBytes.IsEmpty);
        check("Login ID generator ORs the sampled counter shifted8 with its base, clears low8 and wraps the counter",
            MatrixChatIdentityNativeLayout.ProjectGeneratedId(0, 0x01020304)
                == new MatrixChatGeneratedIdSelection(0x0102030400, 0x01020305)
            && MatrixChatIdentityNativeLayout.ProjectGeneratedId(0, uint.MaxValue)
                == new MatrixChatGeneratedIdSelection(0x000000FFFFFFFF00, 0)
            && MatrixChatIdentityNativeLayout.ProjectGeneratedId(0x0100000000000080, 1)
                == new MatrixChatGeneratedIdSelection(0x0100000000000100, 2));
        var generated = MatrixChatIdentityNativeLayout.ProjectGeneratedId(0, 0x01020304);
        var loginIdentity = MatrixChatIdentityNativeLayout.ProjectSuccessfulLoginInitialization(generated.RawId, "Input"u8);
        check("Login uses generated identity/caller text, not World's owner fields, but shares the type1 CString copy",
            loginIdentity.RawId == 0x0102030401
            && loginIdentity.TextBytes.Span.SequenceEqual("Input"u8)
            && TfWorldNativeLayout.ChatPointerOffset == 0x28E8
            && TfWorldNativeLayout.ChatAllocationSize == 0x208);
        byte[] source = [0x80, 0, 0xFF];
        var copied = MatrixChatIdentityNativeLayout.ProjectSuccessfulWorldInitialization(4, source);
        source[0] = 0x81;
        check("World chat text is copied as CString bytes, not decoded text or aliased source storage",
            copied.TextBytes.Span.SequenceEqual(new byte[] { 0x80 })
            && MatrixChatIdentityNativeLayout.ProjectSuccessfulWorldInitialization(4, default).TextBytes.IsEmpty);
        check("Recovered World identity is accepted by the existing raw-byte chat-log model",
            MatrixChatLogNativeLayout.BuildFormattedPrefix("T"u8, "Hi"u8, null, copied.TextBytes)
                .AsSpan().SequenceEqual(new byte[] { (byte)'T', 32, 0x80, (byte)':', 32, (byte)'H', (byte)'i', 32 })
            && MatrixChatLogNativeLayout.ComputeChecksum("T Hi "u8, 0x0102030405060704, "Owner"u8)
                == MatrixChatLogNativeLayout.ComputeChecksum("T Hi "u8, 0x0102030405060701, "Owner"u8));
    }
}
