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