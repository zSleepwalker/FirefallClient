namespace Firefall.Reconstruction.Serialization;

public sealed record SdbTypeInfo(byte Id, string Label, int StorageBytes);

public static class SdbFieldTypes
{
    // Storage sizes from the table referenced at VA 0x1C5A3B8 (raw 0x1858FB8).
    // Labels are adjacent binary strings; duplicate labels do not imply identical semantics.
    private static readonly SdbTypeInfo?[] Known =
    [
        null,
        new(1, "U08", 1),
        new(2, "U16", 2),
        new(3, "U32", 4),
        new(4, "U64", 8),
        new(5, "S08", 1),
        new(6, "S16", 2),
        new(7, "S32", 4),
        new(8, "S64", 8),
        new(9, "F32", 4),
        new(10, "F64", 8),
        new(11, "char", 4),
        new(12, "sl::math::vec2", 8),
        new(13, "sl::math::vec3", 12),
        new(14, "sl::math::vec4", 16),
        new(15, "sl::math::mat4", 64),
        new(16, "void", 4),
        new(17, "sl::math::box3", 24),
        new(18, "sl::math::vec2", 4),
        new(19, "sl::math::vec3", 4),
        new(20, "sl::math::vec4", 4),
        new(21, "char", 1),
        new(22, "U08", 4),
        new(23, "U16", 4),
        new(24, "U32", 4),
        new(25, "sl::math::halfmat4x3", 24),
        new(26, "F16", 2)
    ];

    public static SdbTypeInfo? Lookup(byte id) => id < Known.Length ? Known[id] : null;
}
