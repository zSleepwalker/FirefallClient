using System.Collections.ObjectModel;

namespace Firefall.Reconstruction.Startup;

public sealed record MatrixProfilerNamesState(
    IReadOnlyDictionary<ushort, ReadOnlyMemory<byte>> FirstNames,
    IReadOnlyDictionary<ushort, ReadOnlyMemory<byte>> SecondNames, uint TickMarkerAt18C)
{
    public IReadOnlyDictionary<ushort, ReadOnlyMemory<byte>> RootNames => FirstNames;
    public IReadOnlyDictionary<ushort, ReadOnlyMemory<byte>> NodeNames => SecondNames;
}

public enum MatrixProfilerNamesOutcome
{
    ConsumerAbsent,
    Applied
}

public sealed record MatrixProfilerNamesResult(MatrixProfilerNamesOutcome Outcome, MatrixProfilerNamesState State);

/// <summary>Logical two-map updates from ServerProfiler_SendNames; native shared-text ownership and map iteration order are not modeled.</summary>
public static class MatrixServerProfilerNames60NativeLayout
{
    public const uint HandlerFileOffset = 0x4967B0;
    public const uint ConsumerFileOffset = 0xE64A00;
    public const uint LookupFileOffset = 0xE671C0;
    public const uint NodeInitializerFileOffset = 0xE6AF80;
    public const int OwnerFirstNameMapOffset = 0x190;
    public const int OwnerSecondNameMapOffset = 0x1B0;
    public const int OwnerTickMarkerOffset = 0x18C;
    public const int NativeNodeSize = 0x10;
    public const int NativeNodeKeyOffset = 8;
    public const int NativeNodeSharedTextOffset = 0xC;

    public static MatrixProfilerNamesState Empty(uint sampledTickMarker) =>
        new(new ReadOnlyDictionary<ushort, ReadOnlyMemory<byte>>(new Dictionary<ushort, ReadOnlyMemory<byte>>()),
            new ReadOnlyDictionary<ushort, ReadOnlyMemory<byte>>(new Dictionary<ushort, ReadOnlyMemory<byte>>()),
            sampledTickMarker);

    public static MatrixProfilerNamesResult Apply(bool sampledConsumerPresent, MatrixProfilerNamesState state,
        MatrixWordTextListsPayload payload, uint sampledTick)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (!sampledConsumerPresent)
            return new(MatrixProfilerNamesOutcome.ConsumerAbsent, state);
        ArgumentNullException.ThrowIfNull(payload);
        return new(MatrixProfilerNamesOutcome.Applied,
            new(Update(state.FirstNames, payload.ListAt28), Update(state.SecondNames, payload.ListAt34),
                unchecked(sampledTick - 5000)));
    }

    private static IReadOnlyDictionary<ushort, ReadOnlyMemory<byte>> Update(
        IReadOnlyDictionary<ushort, ReadOnlyMemory<byte>> current, IReadOnlyList<MatrixWordTextEntry> entries)
    {
        var names = current.ToDictionary(pair => pair.Key,
            pair => (ReadOnlyMemory<byte>)pair.Value.ToArray());
        foreach (var entry in entries)
            names[entry.WordAt0] = entry.TextBytesAt4.ToArray();
        return new ReadOnlyDictionary<ushort, ReadOnlyMemory<byte>>(names);
    }
}
