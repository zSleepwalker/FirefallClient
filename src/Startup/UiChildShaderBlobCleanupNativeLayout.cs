namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Static control-flow and stack-layout evidence for the two UI child shader helpers.
/// These contracts describe observed conditional virtual calls, not runtime COM ownership or release behavior.
/// </summary>
public static class UiChildShaderBlobCleanupNativeLayout
{
    public const uint VertexShaderHelperFileOffset = 0xCA7090;
    public const uint PixelShaderHelperFileOffset = 0xCA71F0;

    public const int CodeBlobOutputEbpDisplacement = -0x04;
    public const int ErrorBlobOutputEbpDisplacement = 0x10;
    public const bool OutputLocalsInitiallyZero = true;
    public const bool CallerOutputPointerSavedInEbx = true;

    public const bool CompileFailureTestsNegativeHresult = true;
    public const int BlobVirtualCallVtableSlotOffset = 0x0C;
    public const bool CompileFailureErrorBlobVirtualCallRequiresNonzeroPointer = true;
    public const bool CompileFailureCodeBlobReadOrCleanupObserved = false;
    public const bool CompileFailureCommonCleanupUsesErrorBlob = true;
    public const bool CompileFailureCommonCleanupRequiresNonzeroPointer = true;
    public const uint VertexCompileFailureCommonCleanupFileOffset = 0xCA71DD;
    public const uint PixelCompileFailureCommonCleanupFileOffset = 0xCA7313;
    public const int CompileFailureCleanupVtableSlotOffset = 0x08;
    public const byte CompileFailureReturnAl = 0;

    public const bool NonnegativeHresultErrorBlobCleanupRequiresNonzeroPointer = true;
    public const int NonnegativeErrorBlobCleanupVtableSlotOffset = 0x08;
    public const uint VertexNonnegativeErrorBlobCleanupFileOffset = 0xCA714F;
    public const uint PixelNonnegativeErrorBlobCleanupFileOffset = 0xCA72AC;
    public const uint VertexErrorBlobZeroingFileOffset = 0xCA7152;
    public const uint PixelErrorBlobZeroingFileOffset = 0xCA72AF;

    public const uint VertexCodeBlobVirtualCallFileOffset = 0xCA718F;
    public const uint PixelCodeBlobVirtualCallFileOffset = 0xCA72C5;
    public const bool CodeBlobVirtualCallPrecedesDependencyDispatch = true;
    public const bool CodeBlobVirtualCallUsesBlobVtableSlot = true;
    public const bool AcceptedPredicateCodeBlobCleanupRequiresNonzeroPointer = true;
    public const int AcceptedPredicateCleanupVtableSlotOffset = 0x08;
    public const uint VertexAcceptedPredicateCodeBlobCleanupFileOffset = 0xCA71B2;
    public const uint PixelAcceptedPredicateCodeBlobCleanupFileOffset = 0xCA72E8;
    public const byte AcceptedPredicateReturnAl = 1;

    public const uint VertexTerminalFailureCommonCleanupFileOffset = 0xCA71DD;
    public const uint PixelTerminalFailureCommonCleanupFileOffset = 0xCA7313;
    public const bool TerminalFailureCommonCleanupUsesCodeBlob = true;
    public const bool TerminalFailureCommonCleanupRequiresNonzeroPointer = true;
    public const byte TerminalFailureReturnAl = 0;

    public const bool CommonCleanupAutomaticallyProcessesBothOutputLocals = false;
    public const bool SourceObjectCleanupObservedInHelpers = false;
}
