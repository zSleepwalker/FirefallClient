using Firefall.Reconstruction.Startup;

internal static class NativeFolderPrefixAliasChecks
{
    public static void Run(Action<string, bool> check)
    {
        var roots = NativeFolderPrefixAlias.FolderOrder.ToArray()
            .ToDictionary(id => id, _ => new NativeFolderRootAttempt(false, null));
        roots[37] = new(true, @"C:\Windows\System32\");
        var aliased = NativeFolderPrefixAlias.Project(@"\\?\c:\windows\SYSTEM32\Foo\", roots,
            NativeFolderPrefixAlias.AsciiUppercaseCapture);
        check("Native captured folder alias removes fixed extended prefix, one trailing separator and separator after matched root",
            aliased is { NativeReturnValue: true, TextForLowercaseStage: "<37>Foo", FolderId: 37 });
        check("Native exact folder root aliases to token alone; forward slash after root is not an accepted boundary",
            NativeFolderPrefixAlias.Project(@"C:\Windows\System32", roots, NativeFolderPrefixAlias.AsciiUppercaseCapture)
                is { TextForLowercaseStage: "<37>", NativeReturnValue: true }
            && NativeFolderPrefixAlias.Project(@"C:\Windows\System32/Foo", roots, NativeFolderPrefixAlias.AsciiUppercaseCapture)
                is { TextForLowercaseStage: @"C:\Windows\System32/Foo", NativeReturnValue: false });
        check("Native unmatched folder normalization falls back to original text, not partially trimmed or prefix-removed candidate",
            NativeFolderPrefixAlias.Project(@"\\?\D:\Other\", roots, NativeFolderPrefixAlias.AsciiUppercaseCapture)
                is { TextForLowercaseStage: @"\\?\D:\Other\", NativeReturnValue: false }
            && NativeFolderPrefixAlias.RemoveOneTrailingSeparator(@"C:\Foo\\") == @"C:\Foo\"
            && NativeFolderPrefixAlias.RemoveOneTrailingSeparator("/") == "");
        roots[254] = new(true, @"C:\Windows");
        check("Native folder substitution uses first matching fixed-order provider, not the longest matching prefix",
            NativeFolderPrefixAlias.Project(@"C:\Windows\System32\Foo", roots, NativeFolderPrefixAlias.AsciiUppercaseCapture)
                is { FolderId: 254, TextForLowercaseStage: @"<254>System32\Foo" });
        roots[254] = new(null, null);
        check("Native unobserved earlier folder provider cannot be skipped in favor of a known later match",
            NativeFolderPrefixAlias.Project(@"C:\Windows\System32\Foo", roots, NativeFolderPrefixAlias.AsciiUppercaseCapture)
                is { Outcome: NativeFolderAliasOutcome.UnresolvedRootCapture, NativeReturnValue: null, TextForLowercaseStage: null });
        roots[254] = new(true, null);
        check("Native available folder with uncaptured path text remains unresolved rather than empty or skipped",
            NativeFolderPrefixAlias.Project(@"C:\Windows\System32\Foo", roots, NativeFolderPrefixAlias.AsciiUppercaseCapture)
                is { Outcome: NativeFolderAliasOutcome.UnresolvedRootCapture, NativeReturnValue: null });
        roots[254] = new(true, "");
        roots[255] = new(true, @"C:\Temp\");
        check("Native empty available root is ignored, temporary-root token is255 and no provider is needed for empty original text",
            NativeFolderPrefixAlias.Project(@"c:\temp\File", roots, NativeFolderPrefixAlias.AsciiUppercaseCapture)
                is { FolderId: 255, TextForLowercaseStage: "<255>File" }
            && NativeFolderPrefixAlias.Project("", new Dictionary<uint, NativeFolderRootAttempt>(),
                NativeFolderPrefixAlias.AsciiUppercaseCapture)
                is { TextForLowercaseStage: "", NativeReturnValue: false });
        roots[255] = new(false, null);
        roots[37] = new(true, "C:\\\u00C9");
        var missingCase = NativeFolderPrefixAlias.Project("C:\\\u00E9\\File", roots,
            NativeFolderPrefixAlias.AsciiUppercaseCapture);
        var knownCase = NativeFolderPrefixAlias.Project("C:\\\u00E9\\File", roots,
            value => value == '\u00E9' ? '\u00C9' : value);
        check("Native non-ASCII case comparison remains unresolved without captured towupper mapping and accepts explicit captured mapping",
            missingCase is { Outcome: NativeFolderAliasOutcome.UnresolvedUppercaseCapture, NativeReturnValue: null }
            && knownCase is { TextForLowercaseStage: "<37>File", NativeReturnValue: true });
        roots[37] = new(true, @"C:\Windows\System32");
        var caseBeforeLower = NativeFolderPrefixAlias.Project(@"C:\Windows\System32\Foo", roots,
            NativeFolderPrefixAlias.AsciiUppercaseCapture);
        var tail = NativeMetadataTextTail.EncodeAfterCapturedNormalization(caseBeforeLower.TextForLowercaseStage!.ToLowerInvariant());
        check("Captured folder token flows into separately captured ASCII lowercase and strict UTF8 stages",
            tail.Utf8Bytes is { } bytes && bytes.Span.SequenceEqual("<37>foo"u8));
    }
}
