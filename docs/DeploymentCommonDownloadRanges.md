# DeploymentCommon download range construction

## Sample and method evidence

Sample: `D:\SteamLibrary\steamapps\common\Firefall\system\bin\DeploymentCommon.dll`,
76,800 bytes, SHA-256
`33848E8CA80DFAD7B6883D5A57FC55834411E574CEE6467C753B77AD4523DAE0`.
Read through PE/CLI metadata without loading or executing the assembly.
Addresses below are method-body RVAs; IL offsets are relative to the code
after the method header, not relative to the body RVA.
The PE image base is `0x00400000`; `.text` RVA `0x2000` maps to file
offset `0x200`. All three methods have12-byte fat headers. In particular,
the planner code begins at RVA `0x38BC` and the request code at `0x455C`.
The total getter is MethodDef `0x06000072` (signature `20000A`, instance
Int64); the clamp call is MemberRef `0x0A00007E`
(`System.Math.Min(Int64,Int64)`, signature `00020A0A0A`).

| MethodDef | Body RVA | Method | Relevant IL |
|---|---|---|---|
| `0x0600007C` | `0x38B0` | `ThreadedFileDownloader.DoFileDownload` | `0x00C0-0x00D4`: start zero, first end `Math.Min(total,start+10485760)`; `0x00E4-0x011F`: constructor arguments and zero-based part increment; `0x015E-0x0181`: advance and signed loop test |
| `0x0600009D` | `0x44F4` | `DownloadObject..ctor` | Stores the two supplied endpoints and part number; cache path is prefix + `.part` + number |
| `0x0600009E` | `0x4550` | `DownloadObject.DoDownload` | `0x0015-0x0025`: expected length `end-start+1`; `0x01AD-0x01F6`: endpoints formatted directly as `bytes={0}-{1}` and added under `Range` |

## Saved behavior

`src\DeploymentCommon\DownloadPlanning.cs` now preserves the observed
zero-based part numbers, inclusive endpoints, and asymmetric first clamp.
Previously it guessed one-based parts and uniform, correctly clamped chunks;
that interpretation was not faithful to this sample.

Let C = 10,485,760. Initialize start = 0, end = min(total,C), part = 0.
While signed start < total, construct a chunk using those endpoints and the
matching part-MD5 key. Then increment part, set start = end + 1 and
end = min(total - 1,end + C). Arithmetic uses ordinary unchecked IL add/sub,
not overflow-checked operations.

| Total bytes | Requested inclusive ranges |
|---|---|
| Zero or negative | None |
| 1 | `0..1` |
| C | `0..C` |
| C + 1 | `0..C` |
| C + 2 | `0..C`, `C+1..C+1` |
| 2C + 1 | `0..C`, `C+1..2C` |
| 2C + 2 | `0..C`, `C+1..2C`, `2C+1..2C+1` |

For positive totals at or below C, the requested first end is one byte
beyond the advertised last byte. For larger totals, the first chunk can
contain C+1 bytes. Subsequent ranges are adjacent and inclusive.
The first cache path is `.part0` and its digest lookup uses key0.

This is an inert reconstruction of planning, not a downloader or a proposed
repair of the original application. No network request was issued. Server
handling of beyond-EOF ranges, actual transfer lengths and downstream retry
outcomes are not established by this trace. Extreme-size plans are not
enumerated by tests; the original unchecked arithmetic is not a guarantee
of termination for every synthetic Int64 input.
