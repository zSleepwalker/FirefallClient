using Firefall.Reconstruction.Startup;

internal static class NativeModulePathProviderChecks
{
    public static void Run(Action<string, bool> check)
    {
        NativeModuleFilenameAttempt[] attempts = [new(260, 30, 0)];
        var split = new NativeModulePathSplitCapture(true, "C:", @"\Game\", "Client", ".exe");
        check("Native module path selector enum maps full path, filename, filename+extension and drive+directory",
            Enumerable.Range(0, 4).Select(selector => NativeModulePathProvider.Project(
                (uint)selector, attempts, @"C:\Game\Client.exe", split).SelectedText)
                .SequenceEqual([@"C:\Game\Client.exe", "Client", "Client.exe", @"C:\Game\"]));
        NativeModuleFilenameAttempt[] retry = [new(260, 0, 122), new(1300, null, 122), new(5460, 30, 5)];
        var directory = NativeModulePathProvider.Project(3, retry, null, split);
        check("Native module filename retries LastError122 regardless of return, grows260+4*old and allows other errors with nonzero return",
            directory is { NativeReturnValue: true, SelectedText: @"C:\Game\", AttemptsConsumed: 3, NextBufferCodeUnits: 5460 }
            && NativeModulePathProvider.NextBufferLength(5460) == 22100
            && NativeModulePathProvider.NextBufferLength(uint.MaxValue) == 256);
        check("Native module provider checks query and split failures before unsupported selector",
            NativeModulePathProvider.Project(99, [new(260, 0, 5)], null, null)
                .Outcome == NativeModulePathOutcome.ModuleQueryFailed
            && NativeModulePathProvider.Project(99, attempts, null, split with { NativeSucceeded = false })
                .Outcome == NativeModulePathOutcome.SplitFailed
            && NativeModulePathProvider.Project(99, attempts, null, split)
                .Outcome == NativeModulePathOutcome.UnsupportedSelector);
        check("Native module provider unresolved error, pending retry, split or selected text never become fabricated failure/success",
            NativeModulePathProvider.Project(3, [new(260, 30, null)], null, split).NativeReturnValue is null
            && NativeModulePathProvider.Project(3, [new(260, 30, 122)], null, split)
                is { NativeReturnValue: null, NextBufferCodeUnits: 1300 }
            && NativeModulePathProvider.Project(3, attempts, null, null).NativeReturnValue is null
            && NativeModulePathProvider.Project(0, attempts, null, split).NativeReturnValue is null);
        var roots = NativeFolderPrefixAlias.FolderOrder.ToArray()
            .ToDictionary(id => id, _ => new NativeFolderRootAttempt(false, null));
        roots[254] = directory.AsFolderRootCapture();
        check("Native captured module-directory selector3 supplies folder alias254 before other providers",
            NativeFolderPrefixAlias.Project(@"c:\game\Data\file", roots, NativeFolderPrefixAlias.AsciiUppercaseCapture)
                is { NativeReturnValue: true, FolderId: 254, TextForLowercaseStage: @"<254>Data\file" });
        bool rejectsCapacity = false;
        try { NativeModulePathProvider.Project(3, [new(261, 30, 0)], null, split); }
        catch (ArgumentException) { rejectsCapacity = true; }
        check("Native module provider rejects inconsistent captured retry capacity", rejectsCapacity);
    }
}
