using Firefall.Reconstruction.Startup;

internal static class MatrixChatFileChecks
{
    public static void Run(Action<string, bool> check)
    {
        foreach (bool previousAttempt in new[] { false, true })
        {
            foreach (uint result in new[] { 0u, 0x12345678u })
            {
                var selection = MatrixChatFileNativeLayout.SelectInitialization(previousAttempt, result);
                check($"Chat-file attempt previous={previousAttempt}, pointer={result:X} preserves failure and text-mode choice",
                    selection.OpenMode == (previousAttempt ? "at" : "wt")
                    && selection.FilePointerToken == result
                    && selection.LogOpenFailure == (result == 0)
                    && selection.WriteStartMarker == (result != 0)
                    && selection.AttemptFlagAfter);
            }
        }
        var failed = MatrixChatFileNativeLayout.SelectInitialization(false, 0);
        check("Failed first chat-file open still selects append on the next constructor",
            MatrixChatFileNativeLayout.SelectInitialization(failed.AttemptFlagAfter, 1).OpenMode == "at");
        check("Chat-file shutdown selects marker then fclose only for a present FILE and clears its token",
            MatrixChatFileNativeLayout.SelectShutdown(0) == new MatrixChatFileShutdownSelection(false, false, 0)
            && MatrixChatFileNativeLayout.SelectShutdown(1) == new MatrixChatFileShutdownSelection(true, true, 0)
            && !MatrixChatFileNativeLayout.SelectFlush(0) && MatrixChatFileNativeLayout.SelectFlush(1));
        check("OpenChatLog needs chat but not an open FILE; a present FILE is flushed before shell open",
            MatrixChatFileNativeLayout.SelectOpenInShell(false, 0)
                == new MatrixChatFileViewSelection(true, false, false)
            && MatrixChatFileNativeLayout.SelectOpenInShell(false, 1)
                == new MatrixChatFileViewSelection(true, false, false)
            && MatrixChatFileNativeLayout.SelectOpenInShell(true, 0)
                == new MatrixChatFileViewSelection(false, false, true)
            && MatrixChatFileNativeLayout.SelectOpenInShell(true, 1)
                == new MatrixChatFileViewSelection(false, true, true));
        check("Chat session markers have distinct native timestamp flags and use an opaque global author-ID pair",
            MatrixChatFileNativeLayout.StartMarkerDetailedTimestamp
            && !MatrixChatFileNativeLayout.ShutdownMarkerDetailedTimestamp
            && MatrixChatFileNativeLayout.SessionMarkerAuthorIdVirtualAddress == 0x1E68530
            && MatrixChatFileNativeLayout.ShellOperation == "open"
            && MatrixChatFileNativeLayout.ShellShowCommand == 1
            && MatrixChatFileNativeLayout.RelativeLogPath == "chat.log");
    }
}
