using Firefall.Reconstruction.Startup;

internal static class UiChildShaderBlobCleanupChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("UI child shader helper offsets and output-local layout match the captured stack evidence",
            UiChildShaderBlobCleanupNativeLayout.VertexShaderHelperFileOffset == 0xCA7090
            && UiChildShaderBlobCleanupNativeLayout.PixelShaderHelperFileOffset == 0xCA71F0
            && UiChildShaderBlobCleanupNativeLayout.CodeBlobOutputEbpDisplacement == -0x04
            && UiChildShaderBlobCleanupNativeLayout.ErrorBlobOutputEbpDisplacement == 0x10
            && UiChildShaderBlobCleanupNativeLayout.OutputLocalsInitiallyZero
            && UiChildShaderBlobCleanupNativeLayout.CallerOutputPointerSavedInEbx);

        check("Negative HRESULT path conditionally logs the error blob and uses common failure cleanup",
            UiChildShaderBlobCleanupNativeLayout.CompileFailureTestsNegativeHresult
            && UiChildShaderBlobCleanupNativeLayout.BlobVirtualCallVtableSlotOffset == 0x0C
            && UiChildShaderBlobCleanupNativeLayout.CompileFailureErrorBlobVirtualCallRequiresNonzeroPointer
            && !UiChildShaderBlobCleanupNativeLayout.CompileFailureCodeBlobReadOrCleanupObserved
            && UiChildShaderBlobCleanupNativeLayout.CompileFailureCommonCleanupUsesErrorBlob
            && UiChildShaderBlobCleanupNativeLayout.CompileFailureCommonCleanupRequiresNonzeroPointer
            && UiChildShaderBlobCleanupNativeLayout.VertexCompileFailureCommonCleanupFileOffset == 0xCA71DD
            && UiChildShaderBlobCleanupNativeLayout.PixelCompileFailureCommonCleanupFileOffset == 0xCA7313
            && UiChildShaderBlobCleanupNativeLayout.CompileFailureCleanupVtableSlotOffset == 0x08
            && UiChildShaderBlobCleanupNativeLayout.CompileFailureReturnAl == 0);

        check("Nonnegative HRESULT path conditionally cleans and clears a nonzero error blob",
            UiChildShaderBlobCleanupNativeLayout.NonnegativeHresultErrorBlobCleanupRequiresNonzeroPointer
            && UiChildShaderBlobCleanupNativeLayout.NonnegativeErrorBlobCleanupVtableSlotOffset == 0x08
            && UiChildShaderBlobCleanupNativeLayout.VertexNonnegativeErrorBlobCleanupFileOffset == 0xCA714F
            && UiChildShaderBlobCleanupNativeLayout.PixelNonnegativeErrorBlobCleanupFileOffset == 0xCA72AC
            && UiChildShaderBlobCleanupNativeLayout.VertexErrorBlobZeroingFileOffset == 0xCA7152
            && UiChildShaderBlobCleanupNativeLayout.PixelErrorBlobZeroingFileOffset == 0xCA72AF);

        check("Code blob virtual call precedes dependency dispatch and accepted-path cleanup is guarded",
            UiChildShaderBlobCleanupNativeLayout.VertexCodeBlobVirtualCallFileOffset == 0xCA718F
            && UiChildShaderBlobCleanupNativeLayout.PixelCodeBlobVirtualCallFileOffset == 0xCA72C5
            && UiChildShaderBlobCleanupNativeLayout.CodeBlobVirtualCallPrecedesDependencyDispatch
            && UiChildShaderBlobCleanupNativeLayout.CodeBlobVirtualCallUsesBlobVtableSlot
            && UiChildShaderBlobCleanupNativeLayout.BlobVirtualCallVtableSlotOffset == 0x0C
            && UiChildShaderBlobCleanupNativeLayout.AcceptedPredicateCodeBlobCleanupRequiresNonzeroPointer
            && UiChildShaderBlobCleanupNativeLayout.AcceptedPredicateCleanupVtableSlotOffset == 0x08
            && UiChildShaderBlobCleanupNativeLayout.VertexAcceptedPredicateCodeBlobCleanupFileOffset == 0xCA71B2
            && UiChildShaderBlobCleanupNativeLayout.PixelAcceptedPredicateCodeBlobCleanupFileOffset == 0xCA72E8
            && UiChildShaderBlobCleanupNativeLayout.AcceptedPredicateReturnAl == 1);

        check("Terminal predicate failure uses common cleanup for the code blob and returns AL zero",
            UiChildShaderBlobCleanupNativeLayout.VertexTerminalFailureCommonCleanupFileOffset == 0xCA71DD
            && UiChildShaderBlobCleanupNativeLayout.PixelTerminalFailureCommonCleanupFileOffset == 0xCA7313
            && UiChildShaderBlobCleanupNativeLayout.TerminalFailureCommonCleanupUsesCodeBlob
            && UiChildShaderBlobCleanupNativeLayout.TerminalFailureCommonCleanupRequiresNonzeroPointer
            && UiChildShaderBlobCleanupNativeLayout.TerminalFailureReturnAl == 0
            && !UiChildShaderBlobCleanupNativeLayout.CommonCleanupAutomaticallyProcessesBothOutputLocals);

        check("Helper evidence does not claim source-object cleanup or runtime release ownership",
            !UiChildShaderBlobCleanupNativeLayout.SourceObjectCleanupObservedInHelpers);
    }
}
