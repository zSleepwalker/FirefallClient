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
Check("Metadata owner names and thread stream preserve their independently traced offsets",
    DumperHandoffNativeLayout.OwnerApplicationNameOffset == 0x20
    && DumperHandoffNativeLayout.OwnerSourceRevisionOffset == 0x24
    && DumperHandoffNativeLayout.OwnerBuildNumberOffset == 0x28
    && DumperHandoffNativeLayout.OwnerEnvironmentMnemonicOffset == 0x2C
    && DumperHandoffNativeLayout.OwnerEnvironmentNameOffset == 0x30
    && DumperHandoffNativeLayout.ThreadIdStreamType == 0x1000A);
foreach (var numbers in new (int Source, int Build, uint ExpectedSource, uint ExpectedBuild)[]
{
    (0, 0, 117123, 1962),
    (1, 0, 1, 1962),
    (0, 1, 117123, 1),
    (117123, 1962, 117123, 1962),
    (-1, -1, uint.MaxValue, uint.MaxValue),
    (int.MinValue, int.MaxValue, 0x80000000, 0x7FFFFFFF)
})
{
    Check($"Post-atoi build defaults source={numbers.Source}, build={numbers.Build}",
        DumperHandoffNativeLayout.CaptureBuildNumbers(numbers.Source, numbers.Build)
            == new DumperBuildNumbers(numbers.ExpectedSource, numbers.ExpectedBuild));
}
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

foreach (bool hasRecord in new[] { false, true })
foreach (uint code in new uint[] { 0, 0xC0000005,
    CrashMetadataNativeLayout.EmbeddedSourceExceptionCode,
    CrashMetadataNativeLayout.FullMemorySourceExceptionCode })
foreach (uint count in new uint[] { 0, 1, 2, uint.MaxValue })
foreach (uint length in new uint[] { 0, 1, 1024, 1025, uint.MaxValue })
foreach (bool readSucceeded in new[] { false, true })
{
    bool custom = code == 0x012ED500 || code == 0x012ED501;
    bool read = hasRecord && custom && count >= 2 && length <= 1024;
    uint flags = hasRecord && code == 0x012ED501 ? 2u : 0x40u;
    DebugSourceKind kind = !hasRecord || (custom && count < 2)
        ? DebugSourceKind.Unspecified
        : !custom ? DebugSourceKind.Exception
        : length > 1024 ? DebugSourceKind.TooLong
        : readSucceeded ? DebugSourceKind.RemoteBytes : DebugSourceKind.BadMemory;
    Check($"Source gate record={hasRecord}, code={code:X}, count={count}, length={length}, read={readSucceeded}",
        CrashMetadataNativeLayout.CaptureDebugSource(hasRecord, code, count, length, readSucceeded)
            == new DebugSourceDecision(kind, flags, read, read ? length : 0));
}
foreach (var measurement in new (ulong Preferred, uint PageFile, ulong Expected, bool Fallback)[]
{
    (0, 0, 0, true),
    (0, 123, 123, true),
    (0, uint.MaxValue, uint.MaxValue, true),
    (1, uint.MaxValue, 1, false),
    (0x100000000, 123, 0x100000000, false),
    (ulong.MaxValue, 0, ulong.MaxValue, false)
})
{
    Check($"Process-memory source preferred={measurement.Preferred}, pagefile={measurement.PageFile}",
        CrashMetadataNativeLayout.CaptureProcessMemory(measurement.Preferred, measurement.PageFile)
            == new ProcessMemoryMetadataDecision(measurement.Expected, measurement.Fallback));
}
foreach (bool success in new[] { false, true })
foreach (var limits in new (ulong PageFile, ulong Virtual, ulong Expected)[]
{
    (0, 0, 0), (1, 2, 1), (2, 1, 1), (0x100000000, 0xFFFFFFFF, 0xFFFFFFFF),
    (0xFFFFFFFF, 0x100000000, 0xFFFFFFFF), (ulong.MaxValue, ulong.MaxValue, ulong.MaxValue)
})
{
    Check($"Memory ceiling status={success}, pagefile={limits.PageFile}, virtual={limits.Virtual}",
        CrashMetadataNativeLayout.CaptureMemoryCeiling(success, limits.PageFile, limits.Virtual)
            == (success ? limits.Expected : 0));
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
    Check("RedHanded parses signed option numbers and substitutes defaults only for zero",
        red.ImportSlotNames(0x5447C4, "atoi")
        && red.Matches(0x4048C2, "8B35C447540051FFD6")
        && red.Matches(0x4048F1, "51FFD6")
        && red.Matches(0x4049BF, "8B442428B983C9010085C00F44C1B9AA070000")
        && red.Matches(0x4049D6, "8B44242C85C00F44C1"));
    Check("RedHanded initializes stream-owner fields from app/build/environment option locals",
        red.Matches(0x404A72, "89842480000000")
        && red.Matches(0x404A82, "8B442428898424840000008B44242C89842488000000")
        && red.Matches(0x404AAB, "8984248C000000")
        && red.Matches(0x404ACE, "89842490000000")
        && red.Matches(0x404B36, "8D4C246850")
        && red.Calls(0x404B3B, 0x406820));
    Check("Stream-owner initializer names the source revision, build number, and environment options",
        red.Matches(0x40489E, "B9E4515400")
        && red.Ascii(0x5451E4) == "sourcerev"
        && red.Matches(0x4048D2, "B9F4515400")
        && red.Ascii(0x5451F4) == "buildnumber"
        && red.Matches(0x4048FB, "B900525400")
        && red.Ascii(0x545200) == "envmnemonic"
        && red.Matches(0x40495D, "B90C525400")
        && red.Ascii(0x54520C) == "envname");
    Check("Both exception and RIP-event handlers pass the DEBUG_EVENT thread ID to stream1000A",
        red.Matches(0x4098B6, "8D4D90")
        && red.Calls(0x4098B9, 0x458B20)
        && red.Matches(0x4099FB, "8D459C5051FF75988BCF")
        && red.Calls(DumperHandoffNativeLayout.ExceptionHandlerCallVa,
            DumperHandoffNativeLayout.BuildMetadataVa)
        && red.Matches(0x409A7B, "6A000F45C851FF75988BCF")
        && red.Calls(DumperHandoffNativeLayout.RipHandlerCallVa,
            DumperHandoffNativeLayout.BuildMetadataVa)
        && red.Matches(0x407665, "8B4D08")
        && red.Matches(0x407670, "898DC4FCFFFF")
        && red.Matches(0x408031, "FFB5C4FCFFFF")
        && red.Matches(0x40803D, "680A000100"));
    Check("Source reads require a private exception, two parameters, and length at most1024",
        red.Matches(0x40789A, "8B062D00D52E01741748740A")
        && red.Matches(0x4078B0, "C785CCFCFFFF02000000")
        && red.Matches(0x4078BA, "837E1002727C8B761481FE000400007761")
        && red.Matches(0x407908, "FF7018")
        && red.Matches(CrashMetadataNativeLayout.ReadSourceCallVa, "FF15BC405400")
        && red.ImportSlotNames(0x5440BC, "ReadProcessMemory")
        && red.Matches(0x407916, "85C07528"));
    Check("Source failures use explicit markers rather than fabricated source text",
        red.Ascii(0x5456B0) == CrashMetadataNativeLayout.UnspecifiedSourceMarker
        && red.Ascii(0x5456C0) == CrashMetadataNativeLayout.BadMemorySourceMarker
        && red.Ascii(0x5456CC) == CrashMetadataNativeLayout.TooLongSourceMarker
        && red.Ascii(0x5456D8) == CrashMetadataNativeLayout.ExceptionSourceMarker
        && red.Matches(0x407871, "68B0565400")
        && red.Matches(0x4078A6, "68D8565400")
        && red.Matches(0x40791A, "68C0565400")
        && red.Matches(0x40792C, "68CC565400"));
    Check("PDH identifies the process before reading a LARGE Virtual Bytes counter",
        red.Ascii(0x545468) == CrashMetadataNativeLayout.VirtualBytesCounterPattern
        && red.Matches(0x406CFD, "6868545400")
        && red.Ascii(0x545484) == CrashMetadataNativeLayout.ProcessIdCounterName
        && red.Matches(0x406E84, "6884545400")
        && red.ImportSlotNames(0x54497C, "PdhGetFormattedCounterValue")
        && red.Matches(0x406F1A, "8B1D7C495400")
        && red.Matches(0x406F36, "6800010000")
        && red.Matches(0x406F48, "398560F7FFFF7411")
        && red.Matches(0x406F6A, "6800040000FF760CFFD3")
        && red.Matches(0x406F7C, "8B9D50F7FFFF8BB554F7FFFF")
        && red.Matches(0x406FD5, "8BC38BD6"));
    Check("Zero in both preferred-result halves selects unchecked DWORD PagefileUsage",
        red.Calls(CrashMetadataNativeLayout.ProcessVirtualBytesCallVa,
            CrashMetadataNativeLayout.ProcessVirtualBytesVa)
        && red.Matches(0x407A3B, "85C9755185C0754D")
        && red.Matches(0x407A60, "C78534FDFFFF28000000")
        && red.Matches(CrashMetadataNativeLayout.ProcessMemoryInfoCallVa, "FF15F8475400")
        && red.ImportSlotNames(0x5447F8, "GetProcessMemoryInfo")
        && red.Matches(0x407A7A, "8B8554FDFFFF8985BCFCFFFFC785E8FCFFFF00000000")
        && red.Matches(0x407EF6, "FFB5E8FCFFFF")
        && red.Matches(0x407F02, "FFB5BCFCFFFF"));
    Check("Global memory ceiling compares full unsigned QWORDs and uses zero on API failure",
        red.ImportSlotNames(0x5440B8, "GlobalMemoryStatusEx")
        && red.Matches(0x4079AE, "FF15B840540085C07450")
        && red.Matches(0x4079B8, "8B8510FDFFFF3B8520FDFFFF7729720E")
        && red.Matches(0x4079C8, "8B850CFDFFFF3B851CFDFFFF7319")
        && red.Matches(0x407988, "660F1385DCFCFFFF")
        && red.Matches(0x407A08, "8B85E0FCFFFF8985B8FCFFFF8B85DCFCFFFF8985B4FCFFFF"));
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
