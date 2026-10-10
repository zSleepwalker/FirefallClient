# UI Lua bytecode cache (FirefallClient.exe)

Evidence-based reconstruction of the cluster rooted at raw **0xC3BCD0** (VA 0x103C8D0).
All offsets are **raw file offsets** (the IDA DB is raw-mapped). Translation used:
`.text raw = VA - 0x400C00`, `.rdata raw = VA - 0x401400`. Absolute operands shown by IDA
(e.g. `unk_1BBD83C`) are VAs and were translated before reading.

## Files
- `LuaCacheKey.cs` – key hash, path format, freshness rule.
- `LuaBytecodeFormat.cs` – on-disk header (Lua 5.3 `luaU_dump` layout).
- `UiLuaChunkLoader.cs` – control flow of 0xC3BCD0 with host/VM abstractions.
- `Checks/` – standalone self-check (`dotnet run` in `Checks`): all pass.

## Functions
| raw | role (evidence) |
|---|---|
| 0xC3BCD0 | `bool(srcPath@ecx, L@edx, chunkName, int* status)`; callers 0xC29A7B, 0xC34920, 0xC34AC2, self 0xC3BE94 |
| 0x12F17E0 | FNV-1a 64 of path bytes (basis 0xCBF29CE484222325, prime 0x100000001B3 via `_allmul` 0x159DC30) |
| 0x12B7730 | `_wstat64` → `st_mtime` (0 on failure); UTF-8→UTF-16 via `MultiByteToWideChar(0xFDE9)` |
| 0xC53570 | mtimes equal, or differ by exactly ±0xE10 (3600 s) |
| 0x12BB2A0 | read file: `/`→`\`, `_wfopen "rb"` (mode raw 0x1814178), `_fstat64`, `fread` 16 MiB chunks |
| 0xC28AE0 | `lua_load(L, reader, ud, chunkname or "?", mode)`; sets upvalue 1 = registry[2] |
| 0xC3BC40 | `lua_Reader`: returns whole buffer once (buffer vtbl +0x0C size, +0x04 data), then NULL; assert line 0x110B |
| 0xC28B80 | `lua_dump(L, writer, ud, strip)`; returns 1 if top is not tag 0x46 (Lua closure) |
| 0xCD3A70 | dump header writer |
| 0xC3BCA0 | `lua_Writer`: `fwrite(p,sz,1,FILE*)==1 ? 0 : 1`, sz 0 → 0 |
| 0x12BB9D0 | dirname incl. last `\` `/` `:` |
| 0x12BA130 | per-`\` prefix `_wmkdir` (0x12B71A0; EEXIST=0x11 OK) |
| 0x12BA030 | `_wfopen` (mode "wb" raw 0x17BC3B8) |
| 0x12B76A0 | `_wunlink` |
| 0x12B77B0 | `_wsopen_s`+`_fstat64i32`+`_futime64`: set modtime = source mtime, keep atime |

## Strings (raw)
`ui.noLuaByteCodeCache` 0x17BC424 · `cache\gui\lua_%016llx.bytecode` 0x17BC43C · `b` 0x17BC45C · `t` 0x17BC460 ·
`Failed to open lua cache file '%s'` 0x17BC3BC · `Failed to dump lua cache for file '%s'` 0x17BC3E0 · `?` 0x17B9968.

## Key derivation / invalidation
- Key = FNV-1a-64 over the caller-supplied source path bytes, verbatim (no case/slash normalisation).
- Path = `cache\gui\lua_<key as 16 lowercase hex>.bytecode`, composed onto `VIBE_GetCachedComposedBasePath()` (0x12BBD80).
- Cache used only if enabled (`ui.noLuaByteCodeCache` byte at cvar+0x1C == 0), cache mtime ≠ 0, and it matches the source mtime (±3600 s tolerated, likely DST).
- Hit: load cache with mode `"b"`; load error → `_wunlink` cache and recurse (reloads source as `"t"` and rewrites). Cache read failure → return false (no fallback).
- Miss: load source with mode `"t"`; on success create dirs, dump (strip=1), `fclose`, then set cache mtime to source mtime. Open failure / dump failure are logged; dump failure deletes the file. Return value stays true.

## File format
Raw `lua_dump` output, no wrapper: `1B 4C 75 61` `53` `00` `19 93 0D 0A 1A 0A` `04 04 04 04 08` `78 56 00 00` (LUAC_INT) `double 370.5` (raw 0x1880AF0), then 1 byte `sizeupvalues` (Proto+0x0C), then the stripped main function (`DumpFunction`, 0xCD3900, not analysed).
Note `sizeof(lua_Integer) == 4`.

## Unknowns
- Base directory returned by 0x12BBD80; exact config object (0xC06210/0xCCF3E0) beyond name and +0x1C byte.
- Internals of `luaD_protectedparser`-like 0xCD7A80 and `DumpFunction` 0xCD3900 (out of scope).
- Log sink VA 0x1E68668 identity; buffer object (vtable VA 0x1C80CC4) full layout.
- Whether callers normalise paths before hashing.
