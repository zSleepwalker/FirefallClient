using Firefall.Scripting.LuaCache;

int failures = 0;
void Check(bool ok, string name) { Console.WriteLine($"{(ok ? "PASS" : "FAIL")} {name}"); if (!ok) failures++; }

// Standard FNV-1a 64 vectors.
Check(LuaCacheKey.HashSourcePath("") == 0xCBF29CE484222325UL, "fnv1a64 empty");
Check(LuaCacheKey.HashSourcePath("a") == 0xAF63DC4C8601EC8CUL, "fnv1a64 'a'");
Check(LuaCacheKey.GetRelativeCachePath(0xABCUL) == @"cache\gui\lua_0000000000000abc.bytecode", "%016llx lowercase");
Check(LuaCacheKey.HashSourcePath("Gui/A.lua") != LuaCacheKey.HashSourcePath("gui/a.lua"), "case-sensitive key");

Check(LuaCacheKey.IsCacheFresh(1000, 1000), "fresh equal");
Check(LuaCacheKey.IsCacheFresh(1000, 4600) && LuaCacheKey.IsCacheFresh(4600, 1000), "fresh +/-3600");
Check(!LuaCacheKey.IsCacheFresh(1000, 4601), "stale 3601");
Check(!LuaCacheKey.IsCacheFresh(0, 0), "missing cache");

var hdr = LuaBytecodeFormat.BuildExpectedHeader();
Check(hdr.Length == 29 && hdr[4] == 0x53 && hdr[17] == 0x78 && hdr[18] == 0x56 && BitConverter.ToDouble(hdr, 21) == 370.5, "5.3 header layout");

Check(UiLuaChunkLoader.DirectoryPrefix(@"C:\x\cache\gui\lua_1.bytecode") == @"C:\x\cache\gui\", "dirname");

// Flow: miss -> text load -> dump -> set mtime; then hit -> binary load; corrupt -> delete + retry.
var host = new FakeHost();
var L = new FakeLua();
var loader = new UiLuaChunkLoader(host);
host.Files["gui/a.lua"] = ("src"u8.ToArray(), 500);
Check(loader.Load("gui/a.lua", L, "@a", out var st) && st == 0 && L.LastMode == "t", "miss loads text");
string cp = "BASE\\" + LuaCacheKey.GetRelativeCachePath("gui/a.lua");
Check(host.Files.TryGetValue(cp, out var c) && c.mtime == 500 && LuaBytecodeFormat.HasExpectedHeader(c.data), "cache written with source mtime");
Check(loader.Load("gui/a.lua", L, "@a", out _) && L.LastMode == "b", "hit loads binary");
L.FailBinary = true;
Check(loader.Load("gui/a.lua", L, "@a", out st) && st == 0 && L.Modes.TakeLast(2).SequenceEqual(["b", "t"]), "bad cache retried from source");
host.NoCache = true; L.Modes.Clear(); host.Files.Remove(cp);
Check(loader.Load("gui/a.lua", L, null, out _) && !host.Files.ContainsKey(cp), "disabled: no cache written");

Console.WriteLine(failures == 0 ? "ALL CHECKS PASSED" : $"{failures} FAILED");
return failures == 0 ? 0 : 1;

sealed class FakeHost : ILuaCacheHost
{
    public Dictionary<string, (byte[] data, long mtime)> Files = new();
    public bool NoCache;
    public bool NoLuaByteCodeCache => NoCache;
    public string ComposeCachePath(string r) => "BASE\\" + r;
    public long GetModifiedTime(string p) => Files.TryGetValue(p, out var f) ? f.mtime : 0;
    public bool TryReadFile(string p, out byte[] d) { var ok = Files.TryGetValue(p, out var f); d = f.data ?? []; return ok; }
    public bool DeleteFile(string p) => Files.Remove(p);
    public void CreateDirectories(string d) { }
    public Stream? OpenForWrite(string p) => new Capture(this, p);
    public bool SetModifiedTime(string p, long m) { Files[p] = (Files[p].data, m); return true; }
    public void Log(string f, string a) => Console.WriteLine("  log: " + f.Replace("%s", a));
    sealed class Capture(FakeHost h, string p) : MemoryStream
    {
        protected override void Dispose(bool disposing) { if (disposing && !h.Files.ContainsKey(p)) h.Files[p] = (ToArray(), 0); base.Dispose(disposing); }
    }
}

sealed class FakeLua : ILuaChunkState
{
    public bool FailBinary;
    public List<string> Modes = new();
    public string? LastMode => Modes.LastOrDefault();
    public int Load(byte[] chunk, string? name, string mode)
    {
        Modes.Add(mode);
        return mode == "b" && FailBinary ? 3 : 0;
    }
    public int Dump(Func<byte[], int> writer, bool strip) =>
        writer(LuaBytecodeFormat.BuildExpectedHeader()) | writer([0x01]);
}
