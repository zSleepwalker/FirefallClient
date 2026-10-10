namespace Firefall.Reconstruction.Serialization;

/// <summary>
/// Converts raw rows from a parsed SDB table into a caller-specified runtime
/// layout. This models field lookup/null propagation/conversion, not the
/// native allocator or its row-buffer callback.
/// </summary>
public static class SdbRowConverter
{
    public static IReadOnlyList<byte[]> ConvertRows(
        SdbBlock sourceBlock,
        int sourceTableIndex,
        SdbRuntimeTableLayout destinationLayout)
    {
        ArgumentNullException.ThrowIfNull(sourceBlock);
        ArgumentNullException.ThrowIfNull(destinationLayout);
        ArgumentNullException.ThrowIfNull(destinationLayout.Fields);
        if ((uint)sourceTableIndex >= (uint)sourceBlock.Tables.Count)
            throw new ArgumentOutOfRangeException(nameof(sourceTableIndex));
        if (destinationLayout.RowStride > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(destinationLayout), "Destination row stride is too large.");

        var sourceTable = sourceBlock.Tables[sourceTableIndex];
        if (sourceTable.NameHash != destinationLayout.NameHash)
            throw new ArgumentException("The SDB source and destination table hashes differ.", nameof(destinationLayout));
        var sourceLayout = SdbRuntimeTableLayout.FromSdbTable(sourceTable);
        int[] sourceIndexes = SdbRuntimeTableLayout.MapSourceFieldsByHash(sourceLayout, destinationLayout);
        var rows = new byte[sourceTable.RowCount][];

        for (uint rowIndex = 0; rowIndex < sourceTable.RowCount; rowIndex++)
        {
            byte[] sourceRow = sourceBlock.GetRawRow(sourceTableIndex, rowIndex);
            byte[] destinationRow = new byte[(int)destinationLayout.RowStride];
            for (int destinationIndex = 0; destinationIndex < sourceIndexes.Length; destinationIndex++)
            {
                int sourceIndex = sourceIndexes[destinationIndex];
                if (sourceIndex < 0)
                    continue;

                var sourceField = sourceTable.Fields[sourceIndex];
                var destinationField = destinationLayout.Fields[destinationIndex];
                byte[]? value = sourceBlock.GetRawField(sourceTableIndex, rowIndex, sourceIndex);
                if (value is null)
                {
                    SdbRuntimeTableLayout.MarkDestinationFieldNull(
                        destinationRow, destinationLayout.NullBitmapOffset, destinationField.NullBitIndex);
                    continue;
                }

                var convertedType = destinationField.TypeId <= byte.MaxValue
                    ? SdbFieldTypes.Lookup((byte)destinationField.TypeId)
                    : null;
                if (convertedType is null)
                    throw new NotSupportedException(
                        $"Unknown SDB destination field type {destinationField.TypeId}.");
                if ((ulong)destinationField.ValueOffset + (uint)convertedType.StorageBytes
                    > (ulong)destinationRow.Length)
                    throw new InvalidDataException("SDB destination field exceeds its row.");

                SdbFieldConversion.Convert(
                    value, sourceField.Type, destinationRow.AsSpan((int)destinationField.ValueOffset),
                    (byte)destinationField.TypeId);
            }

            rows[(int)rowIndex] = destinationRow;
        }

        return rows;
    }
}
