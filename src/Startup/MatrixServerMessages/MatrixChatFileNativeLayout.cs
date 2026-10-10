namespace Firefall.Reconstruction.Startup;

public readonly record struct MatrixChatFileInitializationSelection(
    string OpenMode, uint FilePointerToken, bool LogOpenFailure,
    bool WriteStartMarker, bool AttemptFlagAfter);

public readonly record struct MatrixChatFileShutdownSelection(
    bool WriteShutdownMarker, bool CloseFile, uint FilePointerTokenAfter);

public readonly record struct MatrixChatFileViewSelection(
    bool LogChatNotInitialized, bool FlushBeforeOpen, bool CallShellOpen);

/// <summary>Offline selections only; native FILE pointers remain opaque tokens and no files/processes are opened.</summary>
public static class MatrixChatFileNativeLayout
{
    public const string NativeClassName = "tfChat";
    public const int NativeAllocationSize = 0x208;
    public const uint VtableVirtualAddress = 0x1C67CDC;
    public const uint RttiTypeDescriptorVirtualAddress = 0x1E12EF8;
    public const uint ConstructorFileOffset = 0x6E26D0;
    public const uint DestructorFileOffset = 0x6E2C80;
    public const uint GetCurrentChatFileOffset = 0x6F4540;
    public const uint ComposeLogPathFileOffset = 0x6F4610;
    public const uint FlushFileOffset = 0x6F46E0;
    public const uint OpenInShellFileOffset = 0x6ACA60;
    public const uint Utf8PathOpenWrapperFileOffset = 0x12BA030;
    public const uint PreviousAttemptFlagVirtualAddress = 0x1EDB0A4;
    public const uint SessionMarkerAuthorIdVirtualAddress = 0x1E68530;
    public const int FilePointerOffset = 0x18;
    public const string RelativeLogPath = "chat.log";
    public const string FirstAttemptOpenMode = "wt";
    public const string LaterAttemptOpenMode = "at";
    public const string OpenFailureText = "Failed to open chat log file for writing";
    public const string SessionStartedText = "Chat session started";
    public const string SessionShutdownText = "Chat session cleanly shut down";
    public const string ChatNotInitializedText = "OpenChatLog: Chat not initialized";
    public const string ShellOperation = "open";
    public const int ShellShowCommand = 1;
    public const bool StartMarkerDetailedTimestamp = true;
    public const bool ShutdownMarkerDetailedTimestamp = false;

    public static MatrixChatFileInitializationSelection SelectInitialization(
        bool previousAttemptFlag, uint nativeOpenResult) =>
        new(previousAttemptFlag ? LaterAttemptOpenMode : FirstAttemptOpenMode,
            nativeOpenResult, nativeOpenResult == 0, nativeOpenResult != 0, true);

    public static MatrixChatFileShutdownSelection SelectShutdown(uint filePointerToken) =>
        new(filePointerToken != 0, filePointerToken != 0, 0);

    public static bool SelectFlush(uint filePointerToken) => filePointerToken != 0;

    public static MatrixChatFileViewSelection SelectOpenInShell(bool chatPresent, uint filePointerToken) =>
        new(!chatPresent, chatPresent && filePointerToken != 0, chatPresent);
}
