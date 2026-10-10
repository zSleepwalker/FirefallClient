namespace Firefall.Scripting.LuaCache;

/// <summary>Lua VM operations used by raw 0xC3BCD0 (the VM itself is out of scope).</summary>
public interface ILuaChunkState
{
    /// <summary>
    /// raw 0xC28AE0: lua_load-style (ZIO over the single-shot reader at raw 0xC3BC40, chunkname
    /// defaulting to "?" at raw 0x17B9968, mode "b"/"t"; on status 0 sets upvalue 1 from registry[2]).
    /// Returns the Lua status (0 = OK).
    /// </summary>
    int Load(byte[] chunk, string? chunkName, string mode);

    /// <summary>
    /// raw 0xC28B80: lua_dump(L, writer, data, strip). Returns 1 without writing when the stack top
    /// is not a Lua closure (tag 0x46 check at raw 0xC28B89); otherwise the DumpState status.
    /// The writer returns non-zero on failure.
    /// </summary>
    int Dump(Func<byte[], int> writer, bool strip);
}

/// <summary>Engine/CRT services reached from raw 0xC3BCD0.</summary>
public interface ILuaCacheHost
{
    /// <summary>Config lookup raw 0xC06210 + refresh raw 0xCCF3E0; byte at object+0x1C is the value.</summary>
    bool NoLuaByteCodeCache { get; }

    /// <summary>VIBE_ComposeSharedTextPath(VIBE_GetCachedComposedBasePath(), relative) — base unknown.</summary>
    string ComposeCachePath(string relativePath);

    /// <summary>raw 0x12B7730: _wstat64(UTF-16(path)).st_mtime, or 0 on failure.</summary>
    long GetModifiedTime(string path);

    /// <summary>raw 0x12BB2A0: '/'→'\', _wfopen "rb", _fstat64 size, fread in 16 MiB chunks.</summary>
    bool TryReadFile(string path, out byte[] data);

    /// <summary>raw 0x12B76A0: _wunlink; true on success.</summary>
    bool DeleteFile(string path);

    /// <summary>raw 0x12BA130: for each '\' in the prefix, _wmkdir(prefix up to it); EEXIST tolerated (raw 0x12B71A0).</summary>
    void CreateDirectories(string directoryPrefix);

    /// <summary>raw 0x12BA030 with mode "wb" (raw 0x17BC3B8): _wfopen; null on failure.</summary>
    Stream? OpenForWrite(string path);

    /// <summary>raw 0x12B77B0: _wsopen_s + _fstat64i32 (keeps atime) + _futime64 with modtime = mtime.</summary>
    bool SetModifiedTime(string path, long mtime);

    /// <summary>Log sink at VA 0x1E68668 via raw 0x2FCC60-wrapper (printf-style).</summary>
    void Log(string format, string argument);
}

/// <summary>
/// Reconstruction of raw 0xC3BCD0 (VA 0x103C8D0):
/// <c>bool __usercall LoadUiLuaChunk(const char* sourcePath@ecx, lua_State* L@edx, const char* chunkName, int* status)</c>.
/// Callers: raw 0xC29A7B, 0xC34920 (chunkname "@testharness"), 0xC34AC2, and itself at raw 0xC3BE94.
/// </summary>
public sealed class UiLuaChunkLoader(ILuaCacheHost host)
{
    public const string BinaryMode = "b";  // raw 0x17BC45C
    public const string TextMode = "t";    // raw 0x17BC460
    public const string OpenFailedFormat = "Failed to open lua cache file '%s'";   // raw 0x17BC3BC
    public const string DumpFailedFormat = "Failed to dump lua cache for file '%s'"; // raw 0x17BC3E0

    public bool Load(string sourcePath, ILuaChunkState L, string? chunkName, out int status)
    {
        status = 0;                                          // raw 0xC3BD06
        bool cacheEnabled = !host.NoLuaByteCodeCache;        // raw 0xC3BD20..0xC3BD2A (var_D)
        bool useCache = false;                               // bh
        string? cachePath = null;                            // var_18
        long sourceMtime = 0;

        if (cacheEnabled)
        {
            cachePath = host.ComposeCachePath(LuaCacheKey.GetRelativeCachePath(sourcePath)); // raw 0xC3BD4A..0xC3BDBA
            sourceMtime = host.GetModifiedTime(sourcePath);                                 // raw 0xC3BDC2
            long cacheMtime = host.GetModifiedTime(cachePath);                              // raw 0xC3BDD6
            useCache = LuaCacheKey.IsCacheFresh(sourceMtime, cacheMtime);                   // raw 0xC3BDDB..0xC3BDF9
        }

        // raw 0xC3BE2B / 0xC3BEA3: read cache file or source file into a buffer object (vtable VA 0x1C80CC4).
        byte[] chunk;
        if (!host.TryReadFile(useCache ? cachePath! : sourcePath, out chunk))
            return false;                                                                    // raw 0xC3BEDC

        status = L.Load(chunk, chunkName, useCache ? BinaryMode : TextMode);                 // raw 0xC3BE48..0xC3BE72
        if (status != 0)
        {
            if (!useCache)
                return false;
            host.DeleteFile(cachePath!);                                                     // raw 0xC3BE83: stale/corrupt cache
            return Load(sourcePath, L, chunkName, out status);                               // raw 0xC3BE94: retry (cache now missing)
        }

        if (cacheEnabled && !useCache)                                                       // raw 0xC3BEE3..0xC3BEEF
            WriteCache(sourcePath, cachePath!, sourceMtime, L);
        return true;                                                                         // raw 0xC3BFBC
    }

    private void WriteCache(string sourcePath, string cachePath, long sourceMtime, ILuaChunkState L)
    {
        host.CreateDirectories(DirectoryPrefix(cachePath));                                  // raw 0xC3BEFF / 0xC3BF11
        using Stream? file = host.OpenForWrite(cachePath);                                   // raw 0xC3BF2E
        if (file is null)
        {
            host.Log(OpenFailedFormat, cachePath);                                           // raw 0xC3BF4C
            return;
        }

        int dumpStatus = L.Dump(bytes => Writer(file, bytes), strip: true);                  // raw 0xC3BF64 (push 1)
        file.Dispose();                                                                      // fclose raw 0xC3BF86 / 0xC3BF9E
        if (dumpStatus != 0)
        {
            host.Log(DumpFailedFormat, sourcePath);                                          // raw 0xC3BF7D
            host.DeleteFile(cachePath);                                                      // raw 0xC3BF96
            return;
        }
        host.SetModifiedTime(cachePath, sourceMtime);                                        // raw 0xC3BFB4
    }

    /// <summary>raw 0xC3BCA0 lua_Writer: size 0 → 0; else fwrite(p, size, 1, *ud) == 1 ? 0 : 1.</summary>
    private static int Writer(Stream file, byte[] bytes)
    {
        if (bytes.Length == 0) return 0;
        try { file.Write(bytes); return 0; }
        catch (IOException) { return 1; }
    }

    /// <summary>raw 0x12BB9D0: prefix up to and including the last '\', '/' or ':'; empty if none.</summary>
    public static string DirectoryPrefix(string path)
    {
        int i = path.Length;
        while (i > 0 && path[i - 1] is not ('\\' or '/' or ':')) i--;
        return path[..i];
    }
}
