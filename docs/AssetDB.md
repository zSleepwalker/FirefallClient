# AssetDB (partial, static)

## Observed
- 0x1318400 (`__thiscall`) logs the AssetDB search paths. Output is a banner of asterisks, "Search paths in order of access", "Ordinary search paths" (list at this+8), then "AssetDB search paths" (list at this+16), each printed as `%2i) %s`. Lists are circular doubly-linked, with the head sentinel at the list pointer. It also takes a lock at this+24.
- The singleton is `dword_1DFFF5C`. 0x13178B0 and 0x1319520 are called on it from startup (see round 3 and 4).
- Generic file open candidate: 0x12BA030 (also used by the LMTD pak loader).

## Interpreted
- The object keeps two ordered search-path lists, "ordinary" (loose files) and "AssetDB" (packed database), consulted in order.

## Unresolved
- The asset DB file format, the code that populates the two lists, and the name-to-file open path. Not decompiled this round.
## Round 7: search roots and file opening (static)

### Observed
- 0x13178B0 (on the AssetDB singleton) gets a base path from `VIBE_LoadBasePathFromSystemWideApi` (0x12B6E90) and passes it to 0x1317810.
- 0x1317810 logs `Setting file search root directory to "%s"`, stores the string at this+0, converts `/` to `\`, and appends a trailing `\` if missing. An empty argument logs an error instead.
- 0x1319520 resolves a relative name to directories. It converts `/` to `\`. If the name starts with `!`, it strips that character and returns a single-entry result without searching. Otherwise it walks the ordinary list (this+8), appends the name to each root, converts the path from UTF-8 to wide, calls GetFileAttributes-like `dword_1AC4318`, and keeps entries whose attributes include the directory bit (0x10). The result is a 3-dword vector, and the list is guarded by a lock at this+24.
- 0x12BA030 `(path, mode)` converts both UTF-8 arguments to wide (code page 65001, 1024 and 256 wide-char buffers) and calls a wide file-open function (`dword_1AC4704`). Callers use modes such as `"wb"`.

### Interpreted
- Loose files are found by joining a root from the ordinary list with the relative name. `!` marks a name that bypasses the search.
- 0x12BA030 is a UTF-8 `fopen`, not an archive reader.

### Unresolved
- The AssetDB database file format and the "AssetDB" list (this+16) population, since 0x1319520 only reads the ordinary list (this+8). Other readers and writers of the lists were not located.
- The name of import `dword_1AC4704` (inferred from signature only).
- Scope: only the functions above were read; a different open path may exist elsewhere.
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