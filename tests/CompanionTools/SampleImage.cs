using System.Buffers.Binary;
using System.Reflection.PortableExecutable;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;

internal sealed class SampleImage
{
    private readonly byte[] bytes;
    private readonly PEHeaders headers;

    public SampleImage(string path, long expectedSize, string expectedSha256)
    {
        bytes = File.ReadAllBytes(path);
        if (bytes.LongLength != expectedSize
            || !Convert.ToHexString(SHA256.HashData(bytes)).Equals(expectedSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Sample identity mismatch: {path}");
        using var reader = new PEReader(new MemoryStream(bytes, writable: false));
        headers = reader.PEHeaders;
    }

    public bool IsI386 => headers.CoffHeader.Machine == Machine.I386;
    public bool HasClrHeader => headers.CorHeader is not null;
    public uint ImageBase => checked((uint)headers.PEHeader!.ImageBase);

    private int Offset(uint va)
    {
        uint rva = checked(va - ImageBase);
        foreach (var section in headers.SectionHeaders)
        {
            long delta = (long)rva - section.VirtualAddress;
            if (delta >= 0 && delta < section.SizeOfRawData)
                return checked(section.PointerToRawData + (int)delta);
        }
        throw new InvalidDataException($"VA {va:X8} has no file-backed section.");
    }

    public uint Word(uint va) =>
        BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(Offset(va), 4));

    private ushort HalfWord(uint va) =>
        BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(Offset(va), 2));

    public string Ascii(uint va)
    {
        int start = Offset(va);
        int end = Array.IndexOf(bytes, (byte)0, start);
        if (end < 0)
            throw new InvalidDataException($"Unterminated text at {va:X8}.");
        return Encoding.ASCII.GetString(bytes, start, end - start);
    }

    public bool Matches(uint va, string hex) =>
        bytes.AsSpan(Offset(va), hex.Length / 2).SequenceEqual(Convert.FromHexString(hex));

    public bool ManagedImportMatches(int methodRow, string module, string name, string signature, ushort flags)
    {
        using var pe = new PEReader(new MemoryStream(bytes, writable: false));
        var metadata = pe.GetMetadataReader();
        var method = metadata.GetMethodDefinition(MetadataTokens.MethodDefinitionHandle(methodRow));
        var import = method.GetImport();
        return !import.Module.IsNil
            && metadata.GetString(metadata.GetModuleReference(import.Module).Name) == module
            && metadata.GetString(import.Name) == name
            && (ushort)import.Attributes == flags
            && metadata.GetBlobBytes(method.Signature).AsSpan().SequenceEqual(Convert.FromHexString(signature));
    }

    public bool ManagedUserStringsMatch(params (int Offset, string Value)[] values)
    {
        using var pe = new PEReader(new MemoryStream(bytes, writable: false));
        var metadata = pe.GetMetadataReader();
        return values.All(value =>
            metadata.GetUserString(MetadataTokens.UserStringHandle(value.Offset)) == value.Value);
    }

    public bool ManagedIlMatches(int methodRow, uint bodyRva, string expectedIl)
    {
        using var pe = new PEReader(new MemoryStream(bytes, writable: false));
        var metadata = pe.GetMetadataReader();
        var method = metadata.GetMethodDefinition(MetadataTokens.MethodDefinitionHandle(methodRow));
        return method.RelativeVirtualAddress == bodyRva
            && pe.GetMethodBody(method.RelativeVirtualAddress).GetILContent().AsSpan()
                .SequenceEqual(Convert.FromHexString(expectedIl));
    }

    public bool ManagedCatchMatches(int methodRow, int tryOffset, int tryLength,
        int handlerOffset, int handlerLength, string exceptionType)
    {
        using var pe = new PEReader(new MemoryStream(bytes, writable: false));
        var metadata = pe.GetMetadataReader();
        var method = metadata.GetMethodDefinition(MetadataTokens.MethodDefinitionHandle(methodRow));
        foreach (var region in pe.GetMethodBody(method.RelativeVirtualAddress).ExceptionRegions)
        {
            if (region.Kind != ExceptionRegionKind.Catch || region.CatchType.Kind != HandleKind.TypeReference)
                continue;
            var type = metadata.GetTypeReference((TypeReferenceHandle)region.CatchType);
            if (region.TryOffset == tryOffset && region.TryLength == tryLength
                && region.HandlerOffset == handlerOffset && region.HandlerLength == handlerLength
                && $"{metadata.GetString(type.Namespace)}.{metadata.GetString(type.Name)}" == exceptionType)
                return true;
        }
        return false;
    }

    public bool ManagedFinallyMatches(int methodRow, int tryOffset, int tryLength,
        int handlerOffset, int handlerLength)
    {
        using var pe = new PEReader(new MemoryStream(bytes, writable: false));
        var metadata = pe.GetMetadataReader();
        var method = metadata.GetMethodDefinition(MetadataTokens.MethodDefinitionHandle(methodRow));
        return pe.GetMethodBody(method.RelativeVirtualAddress).ExceptionRegions.Any(region =>
            region.Kind == ExceptionRegionKind.Finally
            && region.TryOffset == tryOffset && region.TryLength == tryLength
            && region.HandlerOffset == handlerOffset && region.HandlerLength == handlerLength);
    }

    public bool Calls(uint va, uint target) =>
        bytes[Offset(va)] == 0xE8
        && (long)va + 5 + BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(Offset(va) + 1, 4)) == target;

    public bool ImportSlotNames(uint slotVa, string symbol)
    {
        uint nameRva = Word(slotVa);
        return (nameRva & 0x80000000) == 0 && Ascii(checked(ImageBase + nameRva + 2)) == symbol;
    }

    public IReadOnlyList<string> ImportsFrom(string module)
    {
        var directory = headers.PEHeader!.ImportTableDirectory;
        var result = new List<string>();
        for (int i = 0; i + 20 <= directory.Size; i += 20)
        {
            uint descriptor = checked(ImageBase + (uint)directory.RelativeVirtualAddress + (uint)i);
            uint nameRva = Word(descriptor + 12);
            if (nameRva == 0)
                break;
            if (!Ascii(checked(ImageBase + nameRva)).Equals(module, StringComparison.OrdinalIgnoreCase))
                continue;
            uint thunkRva = Word(descriptor);
            if (thunkRva == 0)
                thunkRva = Word(descriptor + 16);
            for (uint j = 0; ; j += 4)
            {
                uint name = Word(checked(ImageBase + thunkRva + j));
                if (name == 0)
                    return result;
                result.Add((name & 0x80000000) != 0
                    ? $"#{name & 0xFFFF}"
                    : Ascii(checked(ImageBase + name + 2)));
            }
        }
        return result;
    }

    public (uint Rva, uint Ordinal, bool Forwarded)? Export(string symbol)
    {
        var directory = headers.PEHeader!.ExportTableDirectory;
        uint start = checked(ImageBase + (uint)directory.RelativeVirtualAddress);
        uint ordinalBase = Word(start + 16);
        uint count = Word(start + 24);
        uint functions = checked(ImageBase + Word(start + 28));
        uint names = checked(ImageBase + Word(start + 32));
        uint ordinals = checked(ImageBase + Word(start + 36));
        for (uint i = 0; i < count; i++)
        {
            if (Ascii(checked(ImageBase + Word(names + 4 * i))) != symbol)
                continue;
            ushort index = HalfWord(ordinals + 2 * i);
            uint rva = Word(functions + 4u * index);
            bool forwarded = rva >= directory.RelativeVirtualAddress
                && (long)rva < (long)directory.RelativeVirtualAddress + directory.Size;
            return (rva, ordinalBase + index, forwarded);
        }
        return null;
    }
}
