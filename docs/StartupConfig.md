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
- (Corrected in Round 5: those strings are not obfuscated; the junk was an addressing error. See Round 5.)
- Not decompiled: 0x136191D (*.cfg), 0x2D2F74, 0x386D90 hotfix, pak/archive format, fsAssetDB mount calls.
- Negative scope: no general `--option` parser was seen among the functions examined (0x2EA6F0, 0x2EA990, 0xE51590); not claimed for the whole binary.

## Round 5: string addressing, decoded names, *.cfg loader, LMTD pak, hotfix

### Observed
- Addressing: IDA labels such as `asc_1B3FDCC` are VAs shown as raw offsets. The bytes live at raw = VA - 0x401400. `asc_1B3FDCC` -> raw 0x173E9CC `additionalini`; `asc_1B3FE88` -> 0x173EA88 `console`; `asc_1B3F23C` -> 0x173DE3C `--bigdump`. For the strings examined they are plain ASCII at rest; no de-obfuscation routine was found or needed. Not proven for every string in the binary.
- 0x304170 strings: `firefall.ini`, `localassets/firefall.ini`, `additionalini`, `.wedge`, `console` plus "Adding additional ini file %s" / "Failed to load additional ini file %s" / "Failed to load .wedge even though it was found".
- 0x3047E0: `WritePath`, `FilePaths`, `%LOCALAPPDATA%/Red 5 Studios/Firefall/`, `write_test.txt`, "Missing [FilePaths]WritePath in firefall.ini".
- Initialize 0x3063D0 key strings: `FirefallInstanceMutex`, `nowatchdog`, `exittimer`, `[Config]UsesLauncher`, `OperatorHost` (default `operator.firefall.com`), `RegistryBase`, `[Engine]maxFpsLimit`, `[Development]InstallDir`, `[Config]ForceLanguage`, `system/vt/lowmiptextures.pak`.
- Switch registry names read at raw 0x18559D4.. : `dir`, `platform`, `preview-ini`, `preview.ini`, `config`, `oconfig`, `dump-config`.
- 0x12BE6E0 `FindTextRegistryEntryByExactName`: linear strcmp scan over a vector (8-byte entries) at `[dword_1E004FC]`. 65 direct call sites.
- 0x12BDCA0 and 0x12BE230 reference `dword_1E004FC`, convert argv entries via WideCharToMultiByte(65001) and push entries with 0x12BEC70.
- `*.cfg` (raw 0x185BA4C) is referenced at 0x136191D inside the function starting 0x1361890 (not an IDB function). Single direct caller 0xF07FFB. It iterates a path vector, calls 0x12B6C60 with `*.cfg` per entry, then `VIBE_ComposeSharedTextPath` into an output vector.
- 0x12B6C60: formats `dir/pattern`, `/`->`\`, FindFirstFile/FindNextFile/FindClose via import thunks, skips `.`/`..`/directories, appends UTF-8 names.
- Pak: Initialize 0x307040-0x3070E6 calls 0x10D8A40 (engine init, one caller 0x30705A), then composes `[Development]InstallDir` + `system/vt/lowmiptextures.pak` and calls 0xEB38B0 (this=`dword_1E00178`, call site 0x3070E6). 0xEB38B0 opens the file, checks magic `LMTD` (raw 0x17E73D8), reads two uint32 (compressed/uncompressed sizes), decompresses via 0xEB3840, reads entry count then entries (stride 0x240), builds a name->entry map and a blob at this+0x2C, with a lock at this+0x38. Errors: "Wrong magic number.", "Could not decompress texture pak.".
- Hotfix 0x386D90 (wrapper 0x386D80): strings `hotfix.ini`, `[Hotfix]Download-Path`, "Hotfix download starting: level %u", "...finished: level %u" and failure messages (master ini open/write, path conflict, level not found, missing files).

### Interpreted
- 0x1361890 = `ExpandCfgFilesInSearchPaths`; 0x12B6C60 = `ListDirectoryFiles`; 0xEB38B0 = `LowMipTextureDb::LoadPak`; 0x10D8A40 = engine init; 0x386D90 = level-keyed hotfix downloader writing a master `hotfix.ini`.
- `dword_1E004FC` is the command-line switch/option registry; the listed switch names are looked up in it. (Section, key) pairings in Initialize are likely, not each proven.

### Unresolved
- Hotfix 0x386D90 not fully decompiled; its network transport is unidentified.
- The generic (non-texture) asset mount / `fsAssetDB` path is not found; `lowmiptextures.pak` is a texture pak only.
- Which `*.cfg` contents are parsed after 0xF07FFB, and the registry-key names of 0x304BB0, are not decoded.
- Unreferenced wide-string twins of some switches exist; their users are unknown.
