using Firefall.Reconstruction.Startup;

internal static class MatrixServerMessageNativeTypeChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Incoming RTTI catalog preserves numeric native log names separately from all35 original types",
            MatrixServerMessageNativeTypes.Entries.Count == 35
            && MatrixServerMessageCatalog.Entries.All(entry =>
                entry.OriginalType.MessageId == entry.DecodedMessageId
                && entry.NativeLogName == $"msg-{entry.DecodedMessageId}")
            && MatrixServerMessageCatalog.Get(57).OriginalType.QualifiedName == "matrix_fury::SynchronizationRequest"
            && MatrixServerMessageCatalog.Get(59).OriginalType.ClassName == "SuperPong"
            && MatrixServerMessageCatalog.Get(60).OriginalType.ClassName == "ServerProfiler_SendNames"
            && MatrixServerMessageCatalog.Get(61).OriginalType.ClassName == "ServerProfiler_SendFrame");
    }
}
