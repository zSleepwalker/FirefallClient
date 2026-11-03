using Firefall.Reconstruction.Awesomium;
using Firefall.Reconstruction.DumpTruck;
using Firefall.Reconstruction.FirefallProcess;
using Firefall.Reconstruction.HWStats;
using Firefall.Reconstruction.R5Dumper;
using Firefall.Reconstruction.RedHanded;

if (args.Length != 0 && (args.Length != 2 || args[0] != "--samples"))
{
    Console.Error.WriteLine("Usage: CompanionTools.Checks [--samples <system-bin-directory>]");
    return 2;
}

int passed = 0, failed = 0;
void Check(string name, bool condition)
{
    Console.WriteLine($"{(condition ? "PASS" : "FAIL")}  {name}");
    if (condition) passed++; else failed++;
}

Check("Producer records all eleven actual stream IDs, not decompiler-truncated low bytes",
    DumperHandoffNativeLayout.Fields.Select(field => field.StreamType).SequenceEqual(
        new uint[] { 0x10000, 0x10002, 0x10003, 0x10004, 0x10005, 0x10006,
            0x10007, 0x10008, 0x10009, 0x1000A, 0x1000B }));
Check("Metadata record and empty-line terminator use CRLF, not EOF",
    DumperHandoffNativeLayout.LineTerminator == UserStreamNativeLayout.LineTerminator
    && DumperHandoffNativeLayout.EndOfMetadata == "\r\n");
Check("Minidump descriptors are twelve native bytes with fixed-width integer buffers",
    UserStreamNativeLayout.DescriptorNativeByteCount == 12
    && UserStreamNativeLayout.DescriptorTypeOffset == 0
    && UserStreamNativeLayout.DescriptorBufferSizeOffset == 4
    && UserStreamNativeLayout.DescriptorBufferPointerOffset == 8
    && UserStreamNativeLayout.Int32BufferByteCount == 4
    && UserStreamNativeLayout.Int64BufferByteCount == 8);
foreach (bool success in new[] { false, true })
foreach (var counts in new (uint Requested, uint Written)[]
{
    (0, 0), (1, 0), (1, 1), (100, 99), (100, 100), (100, 101),
    (uint.MaxValue, uint.MaxValue - 1), (uint.MaxValue, uint.MaxValue)
})
{
    var decision = DumperHandoffNativeLayout.CapturePipeWrite(success, counts.Requested, counts.Written);
    bool complete = success && counts.Requested == counts.Written;
    Check($"Parent write success={success}, requested={counts.Requested}, written={counts.Written}",
        decision.WaitForDumper == complete
        && decision.TerminateExitCode == (complete ? null : (uint?)0xBADC0DE));
}

if (args.Length == 2)
{
    string root = args[1];
    SampleImage Load(string name, long size, string hash) => new(Path.Combine(root, name), size, hash);
    var red = Load("RedHanded.exe", RedHandedLayout.FileSize, RedHandedLayout.Sha256);
    var dumper = Load("r5dumper.exe", R5DumperLayout.FileSize, R5DumperLayout.Sha256);
    var truck = Load("DumpTruck.exe", DumpTruckLayout.FileSize, DumpTruckLayout.Sha256);
    var hardware = Load("HWStats.exe", HWStatsLayout.FileSize, HWStatsLayout.Sha256);
    var child = Load("FirefallProcess.exe", FirefallProcessLayout.FileSize, FirefallProcessLayout.Sha256);
    var browser = Load("awesomium.dll", 23470048, AwesomiumLayout.Sha256);
    var dbghelp = Load("dbghelp_red5.dll", 1213200,
        "C06430B8CB025BE506BE50A756488E1BCC3827C4F45158D93E4E3EEB98CE1E4F");
    var deployment = Load("DeploymentCommon.dll", 76800,
        "33848E8CA80DFAD7B6883D5A57FC55834411E574CEE6467C753B77AD4523DAE0");
    var client = Load("FirefallClient.exe.original", 40197848,
        "E0E8448393AC1CE05BE73E06F62DD70E3EFF3404CC2E7AAF3F04C79DEC0DE235");
    var samples = new[] { red, dumper, truck, hardware, child, browser, dbghelp, deployment, client };
    Check("All nine identified samples have I386 PE headers; only DeploymentCommon has a CLR header",
        samples.All(sample => sample.IsI386)
        && samples.Count(sample => sample.HasClrHeader) == 1 && deployment.HasClrHeader);
    Check("Companion VAs use their actual image bases, not the client's raw-image address convention",
        red.ImageBase == 0x400000 && dumper.ImageBase == 0x400000
        && truck.ImageBase == 0x400000 && browser.ImageBase == 0x10000000
        && dbghelp.ImageBase == 0x3000000);

    foreach (var field in DumperHandoffNativeLayout.Fields)
    {
        string valueFormat = field.Kind switch
        {
            "str" => "%s", "i32" => "%u", "i64" => "%llu",
            _ => throw new InvalidDataException($"Unobserved stream kind: {field.Kind}")
        };
        Check($"RedHanded stream {field.StreamType:X} uses {field.Kind}, decimal ID, and CRLF",
            red.Matches(field.StreamTypePushVa, "68")
            && red.Word(field.StreamTypePushVa + 1) == field.StreamType
            && red.Ascii(field.FormatStringVa) == $"%d|{field.Kind}|{valueFormat}\r\n"
            && red.Calls(field.FormatCallVa, 0x40FD60));
    }
    Check("RedHanded pipe capacity is 16KiB and handle inheritance is enabled for child creation",
        red.Matches(0x4070FD, "6800400000")
        && red.ImportSlotNames(0x5440E8, "CreatePipe")
        && red.Matches(0x407117, "FF15E8405400")
        && red.Matches(0x407443, "68000000086A01")
        && red.Matches(0x407457, "FF15E8425400")
        && red.ImportSlotNames(0x5442E8, "CreateProcessA"));
    Check("RedHanded writes once, requires exact byte count, then waits or terminates with BADC0DE",
        red.Matches(0x40749B, "FF15D8405400")
        && red.ImportSlotNames(0x5440D8, "WriteFile")
        && red.Matches(0x4074C3, "3B854CDFFFFF7560")
        && red.Matches(0x4074EB, "FF15D0405400")
        && red.ImportSlotNames(0x5440D0, "WaitForSingleObject")
        && red.Matches(0x407574, "68DEC0AD0B")
        && red.ImportSlotNames(0x5440D4, "TerminateProcess"));
    Check("RedHanded calls the same dumper launcher for ordinary and full-memory dumps",
        red.Calls(DumperHandoffNativeLayout.PrimarySpawnCallVa, DumperHandoffNativeLayout.SpawnVa)
        && red.Matches(0x40787C, "C785CCFCFFFF40000000")
        && red.Matches(0x408320, "6A02")
        && red.Calls(DumperHandoffNativeLayout.FullMemorySpawnCallVa, DumperHandoffNativeLayout.SpawnVa));
    Check("RedHanded appends a blank CRLF line after all eleven metadata records",
        red.Matches(0x408167, "6A02689C575400")
        && red.Ascii(0x54579C) == DumperHandoffNativeLayout.EndOfMetadata
        && red.Calls(0x408174, 0x40FF30));
    Check("r5dumper reads 512-byte chunks, appends as a C string, and extracts CRLF lines",
        dumper.Matches(0x401320, "6800020000")
        && dumper.ImportSlotNames(0x404008, "ReadFile")
        && dumper.Matches(0x40133D, "C644041000")
        && dumper.Ascii(UserStreamNativeLayout.CrLfLiteralVa) == "\r\n"
        && dumper.Matches(0x4013A9, "68DC41400053")
        && dumper.Matches(0x4014C1, "837C247C00")
        && dumper.Matches(0x4014C6, "0F84C2030000"));
    Check("Native tokenizer skips repeated separators and preserves the entire third-field remainder",
        dumper.Matches(0x40115F, "6A7C")
        && dumper.Matches(0x4011C4, "6A7C")
        && dumper.Matches(0x4011F6, "83F8027585")
        && dumper.Matches(0x401208, "8B085156")
        && dumper.Matches(0x401213, "FF1568404000"));
    Check("r5dumper copies the original stream type into twelve-byte descriptors",
        dumper.Matches(0x401AF5, "8B46048947FC")
        && dumper.Matches(0x401B03, "894704")
        && dumper.Matches(0x401B0F, "890783C70C")
        && dumper.Matches(0x402180, "B804000000C3")
        && dumper.Matches(0x4021A0, "B808000000C3")
        && dumper.Matches(0x401270, "8B411C40C3"));
    Check("r5dumper passes user streams but null exception and callback information",
        dumper.Matches(0x401B6A, "6A008D4C240C51")
        && dumper.Matches(0x401B75, "6A00")
        && dumper.Matches(0x401B8E, "FF15D0644000"));
    Check("Bundled dbghelp actually exports the named writer, ordinal31, without forwarding",
        dbghelp.Export(UserStreamNativeLayout.DbgHelpWriteExport)
            == (UserStreamNativeLayout.DbgHelpWriteExportRva,
                (uint)UserStreamNativeLayout.DbgHelpWriteExportOrdinal, false));
    Check("DumpTruck redirects HWStats stdout through a separate 2048-byte pipe",
        truck.Matches(0x42ABE2, "C785A8F7FFFF00010000")
        && truck.Matches(0x42AC0A, "6800080000")
        && truck.Matches(0x42AC50, "FF1518514700")
        && truck.ImportSlotNames(0x475118, "CreatePipe")
        && truck.Matches(0x42AD33, "6800080000")
        && truck.Matches(0x42AD60, "FF5018")
        && truck.Matches(0x42ADC7, "6A0168F16B4700")
        && truck.Matches(0x42ADCE, "FF5018"));
    Check("DumpTruck appends a NUL to its output sink rather than treating hardware text as dump metadata",
        truck.Ascii(0x476BF1) == ""
        && DumpTruckLayout.HwReportReadChunkByteCount == 0x800
        && DumpTruckLayout.HwReportSinkVtableSlotOffset == 0x18);

    var childImports = child.ImportsFrom(FirefallProcessLayout.ImportModule);
    Check("FirefallProcess imports exactly Awesomium ChildProcessMain, which the DLL exports",
        childImports.SequenceEqual(new[] { FirefallProcessLayout.ImportSymbol })
        && child.Matches(0x40103E, "FF15DC604000")
        && child.ImportSlotNames(FirefallProcessLayout.ChildProcessMainIatVa, FirefallProcessLayout.ImportSymbol)
        && browser.Export(FirefallProcessLayout.ImportSymbol) is { } childExport
        && browser.ImageBase + childExport.Rva == AwesomiumLayout.ChildProcessMainVa
        && !childExport.Forwarded);
    var clientImports = client.ImportsFrom("awesomium.dll");
    Check("All86 original client Awesomium imports resolve to exports in the bundled DLL",
        clientImports.Count == AwesomiumLayout.ClientImportCount
        && clientImports.All(symbol => browser.Export(symbol) is not null));
    Check("DeploymentCommon is a managed deployment assembly, not an observed native import of these helpers",
        deployment.ImportsFrom("mscoree.dll").Contains("_CorDllMain")
        && new[] { red, dumper, truck, hardware, child, browser, client }
            .All(sample => sample.ImportsFrom("DeploymentCommon.dll").Count == 0));
}
else
{
    Console.WriteLine("Binary evidence checks not run: supply --samples with the authorized local sample directory.");
}

Console.WriteLine($"{passed} passed; {failed} failed.");
return failed == 0 ? 0 : 1;
