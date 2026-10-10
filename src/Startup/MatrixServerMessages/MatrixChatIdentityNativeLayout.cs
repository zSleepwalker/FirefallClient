namespace Firefall.Reconstruction.Startup;

public readonly record struct MatrixChatIdentitySnapshot(ulong RawId, ReadOnlyMemory<byte> TextBytes);
public readonly record struct MatrixChatGeneratedIdSelection(ulong RawId, uint CounterAfter);

public static class MatrixChatIdentityNativeLayout
{
    public const uint WorldInitializationFileOffset = 0x64B460;
    public const uint LoginInitializationFileOffset = 0x63B250;
    public const uint GenerateIdFileOffset = 0xBC9A20;
    public const uint CounterVirtualAddress = 0x1DFC588;
    public const uint BasePairVirtualAddress = 0x1E487F8;
    public const uint WorldIdentityCopyFileOffset = 0x64B5CF;
    public const int WorldOwnerPointerOffset = 0x10;
    public const int WorldChatPointerOffset = 0x28E8;
    public const int OwnerIdLowWordOffset = 0x848;
    public const int OwnerIdHighWordOffset = 0x84C;
    public const int OwnerTextPointerOffset = 0x850;
    public const int ChatIdOffset = 8;
    public const int ChatTextOffset = 0x10;
    public const int LoginChatPointerOffset = 0x14;
    public const int LoginGateOwnerPointerOffset = 0xA28;
    public const int LoginGateByteOffset = 0x34;
    public const byte ChatIdentityTypeByte = 1;
    public const byte WorldChatIdTypeByte = ChatIdentityTypeByte;

    /// <summary>Projects the copy at the successful World-initialization tail; does not simulate earlier initialization or native ownership.</summary>
    public static MatrixChatIdentitySnapshot ProjectSuccessfulWorldInitialization(
        ulong ownerIdAt848, ReadOnlySpan<byte> ownerTextAt850) =>
        ProjectType1Identity(ownerIdAt848, ownerTextAt850);

    public static MatrixChatIdentitySnapshot ProjectSuccessfulLoginInitialization(
        ulong generatedPair, ReadOnlySpan<byte> callerText) =>
        ProjectType1Identity(generatedPair, callerText);

    public static MatrixChatIdentitySnapshot CreateFreshChatIdentity() => new(0, ReadOnlyMemory<byte>.Empty);

    public static bool SelectLoginInitialization(byte sampledGateByte) => sampledGateByte != 0;

    /// <summary>The counter value is the sampled old value from native lock xadd; no shared/atomic state is changed here.</summary>
    public static MatrixChatGeneratedIdSelection ProjectGeneratedId(ulong nativeBasePair, uint sampledOldCounter) =>
        new((nativeBasePair | ((ulong)sampledOldCounter << 8)) & 0xFFFFFFFFFFFFFF00,
            unchecked(sampledOldCounter + 1));

    private static MatrixChatIdentitySnapshot ProjectType1Identity(ulong sourceId, ReadOnlySpan<byte> textBytes)
    {
        int nul = textBytes.IndexOf((byte)0);
        if (nul >= 0)
            textBytes = textBytes[..nul];
        return new((sourceId & 0xFFFFFFFFFFFFFF00) | ChatIdentityTypeByte, textBytes.ToArray());
    }
}
