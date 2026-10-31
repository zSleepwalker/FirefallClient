# Startup config pipeline (Round 4)

Addresses: file offsets (= IDA addresses); VA code = raw+0x400C00, .rdata = raw+0x401400. Strings are found in code by their VA immediate (IDA xrefs on raw string addresses return nothing).

## Observed
- GetCommandLineW: single reference 0x2EAA69 in 0x2EA990 (crash-dump/dumptruck command-line builder; appends `--bigdump` (0x173DE3C) and `--dumptruck-autosubmit` (0x173DE70) when 0x2EA6F0 finds them). GetCommandLineA: single reference 0x304D64 in 0x304BB0 (firefall:// protocol registration, `--open="%1"`).
- 0x2EA6F0 `bool(const char*)`: exact strcmp over argv[1..argc-1] (argc/argv via dword_1AC4874/dword_1AC4878).
- 0xE51590: reads argv[0], finds last `\`, copies the directory (<1024) and passes it to 0x12B70D0.
- Initialize (0x3063D0) strings: "Unable to load config (ini)" @0x30665B, "Failed to init write directory..." @0x306BDC, `system/vt/lowmiptextures.pak` @0x307089.
- 0x304170 loads `firefall.ini` (0x173E9BC, @0x3041BF) into the container at App+0xA6C, and `localassets/firefall.ini` (0x173E9A0, @0x304260) merged via 0x12DB5D0.
- 0x3047E0: "Missing [FilePaths]WritePath in firefall.ini" (0x173EBD0).
- 0x12DA710 (container loader, this=App+0xA6C): looks up text-registry entries by name (FindTextRegistryEntryByExactName 0x12BE6E0), base path via 0x12B6E90, splits lists via 0x12BE760, loads each file via 0x12DAC30 / 0x12DB5D0, logs per file.
- Other strings: `hotfix.ini` (0x1744318, ref 0x387225 in 0x386D90), `*.cfg` (ref 0x136191D), `AssetDB search paths` (ref 0x13185A7 in 0x1318400, which only logs lists from this+8/this+16), `localassets` (ref 0xE523BA in 0xE52370), RTTI fsAssetDB/tsAssetDB/tfAssetDB.
- File API callers: CreateFileW 18 sites (e.g. 0x12544C0, 0x1254C00, 0x126ADC0), CreateFileMappingW 0x12544C0/0x126E660, FindFirstFileW 0x126D270.

## Interpreted
- Pipeline: argv/dumptruck diagnostics -> firefall.ini + localassets/firefall.ini into App+0xA6C -> WritePath resolver -> write dir init -> mount lowmiptextures.pak -> hotfix.ini later.
- 0x1254xxxx/0x126Dxxxx/0x126Exxxx are probably the filesystem/archive layer.

## Unresolved
- Several name strings (asc_1B3FDCC, asc_1B3FE88, switch strings near 0x1B3F23C) are junk bytes at rest (obfuscated); decryptor not found.
- Not decompiled: 0x136191D (*.cfg), 0x2D2F74, 0x386D90 hotfix, pak/archive format, fsAssetDB mount calls.
- Negative scope: no general `--option` parser was seen among the functions examined (0x2EA6F0, 0x2EA990, 0xE51590); not claimed for the whole binary.
