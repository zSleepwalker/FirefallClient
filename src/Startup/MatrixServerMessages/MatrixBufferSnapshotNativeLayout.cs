namespace Firefall.Reconstruction.Startup;

public readonly record struct MatrixBufferSnapshotSelection(
    bool ProcessThroughVirtual18, bool CreateSnapshot, bool ConsumeThroughVirtual14, bool PendingByteAfter)
{
    public bool ClearProcessedBlob => ConsumeThroughVirtual14;
}

public readonly record struct MatrixBufferSnapshotProjection(
    ReadOnlyMemory<byte> AccumulatedBytes, ReadOnlyMemory<byte>? SnapshotBytes, bool PendingByte);

public static class MatrixBufferSnapshotNativeLayout
{
    public const uint HandlerFileOffset = 0x490AA0;
    public const uint ConsumerFileOffset = 0x30EE20;
    public const uint SnapshotCloneFileOffset = 0x30EDA0;
    public const uint RequestSnapshotFileOffset = 0x30EED0;
    public const uint AccumulationBlobVtableVirtualAddress = 0x1C80CC4;
    public const uint BlobAppendFileOffset = 0x12CBA10;
    public const uint BlobClearFileOffset = 0x12CBA00;
    public const uint BlobLengthFileOffset = 0x12CB9C0;
    public const uint BlobDataAtOffsetFileOffset = 0x12CB960;
    public const int ManagerProcessedBufferOffset = 0xC44;
    public const int ManagerAccumulationBlobOffset = ManagerProcessedBufferOffset;
    public const int ManagerSnapshotCacheKeyOffset = 0xC40;
    public const int ManagerSnapshotPairPointerOffset = 0xC3C;
    public const int ManagerPendingByteOffset = 0xC38;
    public const int SnapshotDataPointerOffset = 0;
    public const int SnapshotByteLengthOffset = 4;

    public static MatrixBufferSnapshotSelection Select(byte rawFlag, bool previousPendingByte)
    {
        bool snapshot = rawFlag != 0;
        return new(true, snapshot, snapshot, snapshot || previousPendingByte);
    }

    /// <summary>Logical slBlob bytes only; native capacity, copy-on-write/refcounts and allocation failures are not simulated.</summary>
    public static MatrixBufferSnapshotProjection AppendAndSelectSnapshot(
        ReadOnlySpan<byte> accumulatedBytes, ReadOnlySpan<byte> incomingBytes, byte rawFlag,
        bool previousPendingByte, ReadOnlyMemory<byte>? previousSnapshot)
    {
        var bytes = new byte[checked(accumulatedBytes.Length + incomingBytes.Length)];
        accumulatedBytes.CopyTo(bytes);
        incomingBytes.CopyTo(bytes.AsSpan(accumulatedBytes.Length));
        if (rawFlag == 0)
            return new(bytes, previousSnapshot, previousPendingByte);
        return new(ReadOnlyMemory<byte>.Empty, new ReadOnlyMemory<byte>(bytes), true);
    }

    public static bool SelectCachedSnapshot(bool snapshotPresent, uint storedKey, uint requestedKey) =>
        snapshotPresent && storedKey == requestedKey;

    public static ReadOnlyMemory<byte>? CloneSnapshot(ReadOnlyMemory<byte>? snapshot)
    {
        if (!snapshot.HasValue)
            return default;
        return new ReadOnlyMemory<byte>(snapshot.Value.ToArray());
    }
}
