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

## Round 6: URI registration and hotfix (static, partial)

### Observed
- 0x304BB0 registers a `firefall://` protocol handler under HKCU: key `Software\Classes\firefall`, value `URL Protocol`, `registered_build` compared with the build string `prod-1962`, and `shell\open\command` set to `%COMSPEC% /q /c start "%s" /D"%s" %s --open="%1"`. It also reads the `--open` switch and logs "Unable to open uri: %s". The earlier obfuscation claim was an addressing error (string raw = VA - 0x401400).
- 0x386D90 (hotfix download, `__thiscall`, level byte at this+28) logs "Hotfix download starting: level %u", reads `[Hotfix] Download-Path` from the ini container at App+0xA6C, opens `hotfix.ini`, and uses the `.include "%u\%s"` format. Its error strings are listed in the round-5 notes.
- 0x388330 builds `<this+12 root>\<%u level>\<name>` and a URL-or-path. It tests for the `http://` prefix on the string at `this[9]+36`. It then calls 0x388BD0 (which logs "unable to init http", "Bad md5sum: expected %s for %s") or 0x388800.
- 0x3878F0 is the post-download step: it logs "Hotfix OnPostDownload()", copies binaries into `bin\`, renames the prior destination, and reports copy failures.

### Interpreted
- Transport is HTTP, with a local-path fallback, and downloaded files are md5-verified. 0x388BD0 is the HTTP route and 0x388800 is the non-HTTP route. Neither was fully decompiled.
- The level key is the `%u` directory under the download root.

### Unresolved
- The exact hotfix.ini section and key schema beyond `[Hotfix] Download-Path`, and the base URL value (config data, not a binary string).
- The bodies of 0x388BD0, 0x388800 and 0x389090.
## Round 7: hotfix fetchers (static)

### Observed
- 0x388330 selects the route: when the source string at `this[9]+36` starts with `http://` it calls 0x388BD0; otherwise 0x388800.
- 0x388BD0 (HTTP route): converts `\` to `/` in the URL, builds a `ClientApiRequest` (ctor 0x3843F0) with a completion object, pumps routes with a growing wait (100 ms steps up to 5000 ms), and accepts only when status (this+108) == 200 and the body is non-empty. If a 16-byte expected digest is given, it hashes the body (0x12F14A0) and compares; mismatch logs "Bad md5sum: expected %s for %s". On success it opens the destination with `"wb"` through 0x12BA030, writes in chunks of at most 0x2000 bytes, closes, and deletes the file (0x12B76A0) on a short write. If the client API is not ready it logs "Hotfix download: unable to init http (%s)".
- 0x388800 (local route): opens source and destination (modes at 0x1B45A44 / 0x1B45A48, not decoded), copies in 4096-byte blocks, updates a progress float, and sleeps 3 ms per block when progress is reported. It then re-reads the copy, hashes it and compares the digest, logging "Bad md5sum" on mismatch.
- 0x389090 is a trivial accessor (`this+8`, asserts `*this`).

### Interpreted
- Hotfix files are fetched over the game's HTTP client API with md5 verification, or copied from a local path. The digest is the 16 bytes from the per-file entry (stride 120 in 0x411650). Digest algorithm "md5" is taken from the log text, not from the hash routine body.

### Unresolved
- Mode strings at 0x1B45A44/0x1B45A48, the hotfix.ini key schema, the base URL value.
## Round 8: search-path writers and asset-path config loader

**Observed**
- `0x1318080` (add AssetDB path, list head this+16, count this+20) has one caller: wrapper `0x38D2B0`, called 4x from `0x38B310`. `0x1318300` (remove from this+16 list) is called from `0x38B310` (loop at 0x38B3DC). `0x1317F80` (remove from this+8 list) is also called from `0x38B310` (0x38B38C).
- `0x1317D00` (add ordinary, this+8) has one caller, wrapper `0xE56BD0`, called from `0xE52370` and `0x10D8A40`. Strings there: "localassets", "Engine", "system".
- `0x38B310` is a reconfigure routine on singleton `dword_1DFFF5C`: it clears both lists (stored in `this` vectors), reads config via `0x12DC080` (ini get) keys `Preview`, `searchlocalfirst`, `AssetPath` (default `system/assetdb`), `FilePaths`, `InstallDir` / `Development`, then re-adds paths via `0x1318080`.
- `0x1317AD0` normalises a path (length <= 0x200, appends separator, validates), logging on failure.
- `0x3289D0` is the TwitchTV worker (state `dword_1E6819C`, cases 1..4; strings "TTV_Start failed.", "TTV_Chat_Init failed.", etc.), not an app mode switch.
- Hotfix master ini lines are `.include "%u\%s"` with `hotfix.ini`; section `Hotfix`, key `Download-Path` read after fetch (0x386D90 state 1->2->3->4).

**Interpreted**: AssetDB list holds directories/DB roots from config key `AssetPath`; ordinary list from `FilePaths`.

**Unresolved**: on-disk AssetDB format; the `.include` parser inside ini object (0x12DAD70); caller(s) of `0x38B310`; the "AssetDB"/"fsAssetDB" strings were not located this round. Negative claims limited to xrefs IDA reported.
## Round 9: ini directives, hotfix reload, AssetDB RTTI

**Observed**
- `0x12DD300` (this = ini object) handles lines starting with `.`; after the dot it string-compares (`dword_1AC4894`, inferred strncmp) against `include ` (8), `include? ` (9) and `include-all ` (12). Other text logs "Did not understand INI processor instruction '%s'". `.include-all` logs ".include-all '%s': directory does not exist" if the directory is missing and otherwise loads each listed file via `0x12DCE00`; a failing file aborts it. `.include?` is treated as optional (observed: a UTF-8 locale check gates the result). The only caller of ini constructor `0x12DAD70` seen is hotfix `0x386D90`; other ini users use other paths (scope: IDA xrefs).
- `0x38B310` (the asset-path reconfigure, this = `dword_1DFFFD4`) callers: `ClientApplicationInitialize` (0x306D77), `0x3865C0` (post-hotfix) and `0x9B2F10` (called from Login_PickBackgroundZone, AppMode_LookingGlass_Initialize, ReplayReader_Initialize).
- `0x3865C0` is called from `0x386430`. It enumerates `bin\`, `\clientdb.sd2`, `components\` file sets, calls `0x38B310`, stores `LastHotfixLevel` (format `%s-%u/%u`), and logs "Hotfix: applied level %u." or "Hotfix: skipping reload systems (cvar setting) ...".
- RTTI strings exist for `tsAssetDB`, `fsAssetDB`, `tfAssetDB` (slSingleton); no xrefs to their type descriptors were found in IDA, so vtables were not located. "AssetDB search paths" is logged by `0x1318400`.
- `0x3289D0` (Twitch worker) is called from `ClientApplicationBootstrap` (0x2EB216) and `ClientApplicationRunLoop` (0x309B08), i.e. it is polled per frame by RunLoop. Config cvars `twitchtv_username/password/authkey/width/height`, console command `twitch_auth`. A hardcoded client-ID-like string exists at 0x1B41BD8 (not reproduced). API endpoints were not recovered (the TTV SDK is imported).

**Interpreted**: `clientdb.sd2` is a client DB reloaded after a hotfix.

**Unresolved**: the AssetDB on-disk format and file-open routines; vtables of fs/ts/tfAssetDB; `.include?` exact semantics.
## Round 10: directory resolver, `.include?`, engine start

**Observed**
- `0x1319520(this=dword_1DFFF5C, out vector, name)` reads the ordinary list (this+8): converts `/` to `\`, a leading `!` marks the name as already absolute (stripped, returned as-is); otherwise it joins each root with `name`, converts UTF-8 to UTF-16 (code page 65001), calls an inferred GetFileAttributesW and keeps entries that are directories (attr 0x10). Guarded by the lock at this+24. Callers: `0xEE5030`, `0xEE5790` and `0xF07FD6` (with "engine/autodetect" at 0xF07FD0; that site is a data-loading routine with 36-byte records, not a file-open routine).
- `0x12B78F0` (the `.include?` gate): converts the name to UTF-16 and returns true if an inferred stat/GetFileAttributes-like call (`dword_1AC4714`) fails to find it or its flag bit 0x4000 is clear. In `0x12DD300` the `.include?` result is `!gate(name) || load(name)`, so a name for which the gate is false is skipped as success; if the gate is true the file is loaded and a load failure fails the directive.
- `0xE54BF0` (oEngineSystem vtable slot 0, no direct xrefs because it is virtual) is the engine start routine: it logs "Operating System", "Total usable system memory", "Memory constrained mode", "Cinematic mode", "--- Initialization ---", "Initializing Main Window", queries monitors (inferred EnumDisplayDevices/EnumDisplaySettings), sets up window size at this+312..340, requires a windowing callback ("Required windowing callback not set"), then "--- Finished Initialization (%.2f seconds) ---".

**Interpreted**: the `.include?` gate looks like a "skip if file bit set/absent" test; exact meaning of the 0x4000 flag is unproven.

**Unresolved**: AssetDB on-disk format; vtables of fs/ts/tfAssetDB; the `.pak` handling at `0x1361890` (only one caller inside undefined code at 0xF07FFB; not decompiled). Negative claims are limited to IDA xrefs.
## Round 11: pak/list expander, ini loader, oEngineSystem vtable

**Observed**
- `0x1361890` (`__fastcall`-like: ecx = input string vector, edx = output; stack arg = scratch) iterates a vector of strings, expands each through `0x12B6C60` (the same directory/wildcard enumerator used by `.include-all`) and appends non-duplicate results via `0x12BBEC0`. It is a file-list/glob expander; IDA would not decompile it, so this comes from disassembly. Its only caller is the undefined region at `0xF07FFB` (outputs to object+0xC0/+0xCC; the same function that calls `0x1319520` with `engine/autodetect`). No `.pak` string or file open occurs in it.
- `0x12DCE00` (ini file loader used by `.include`/`.include?`): fails with a logged message if the name is empty, cannot be opened by `0x12BB660`, or the parsed content cannot be read (`0x12DB5D0`); otherwise it walks a section map (red-black tree) and merges each key/value into the target ini object (`this`), logging a message per key that cannot be inserted. Returns bool. A missing file therefore fails here; `.include?` tolerance comes only from the gate `0x12B78F0`.
- oEngineSystem constructor is `0xE4DFA0` (sole code xref `0xE4E6E3` in `0xE4E660`, which has no xrefs). The vtable is at file offset `0x187682C` / VA `0x1C77C2C` (slot 0 = `0xE54BF0`, slot +0x20 = `0xE55990`, slot -1 = `0x1CD439C`, likely RTTI locator); only code reference to the vtable VA is `0xE4DFDE`. Byte search for the slot-0 pointer (`F0 57 25 01`) found exactly one match.
- `dword_1E60EA4` has 424 data xrefs. Among them I scanned for `call [reg]` (slot 0) within 8 instructions: only `0xF54DAF/0xF54DFB`, which act on another object, so no slot-0 call on `[dword_1E60EA4]` was found. RunLoop uses slot +0x20 (0x3099DB); `0x3073F3` uses +0x54.

**Interpreted**: slot 0 is probably invoked through a different register path or a helper (e.g. via `0xE4E660` chain); unproven.

**Unresolved**: AssetDB on-disk format and fs/ts/tfAssetDB vtables (no constructor located; scoped to RTTI string/IDA xref search). The `.pak` open path is not identified.