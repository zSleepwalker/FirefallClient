using System.Text.Json;
using Firefall.Reconstruction.Serialization;

if (args.Length is < 1 or > 2 || (args.Length == 2 && args[1] != "--mode-10"))
{
    Console.Error.WriteLine("Usage: SdbInspect <path-to-sdb> [--mode-10]");
    return 2;
}

try
{
    using var input = File.OpenRead(args[0]);
    var block = SdbDatabase.Read(input, modeFlag: args.Length == 2);
    var result = new
    {
        block.BuildIdentifier,
        block.Flags,
        DecodedLength = block.DecodedPayload.Length,
        block.TrailingDataOffset,
        block.Tables
    };
    Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions
    {
        WriteIndented = true
    }));
    return 0;
}
catch (Exception error) when (error is IOException or InvalidDataException or UnauthorizedAccessException
                              or ArgumentException or NotSupportedException)
{
    Console.Error.WriteLine($"SDB inspection failed: {error.Message}");
    return 1;
}
