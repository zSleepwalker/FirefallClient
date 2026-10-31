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