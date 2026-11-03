# DumpTruck crash-report body

This is a static producer reconstruction of the report body created by
`DumpTruck.exe!0x40A3F0`. It is not a report parser, an upload implementation,
or a byte-for-byte validated reproduction of a captured report. The authorized
sample is `D:\SteamLibrary\steamapps\common\Firefall\system\bin\DumpTruck.exe`,
16,675,032 bytes, SHA-256
`53796D440D586A306C36ADAE86744C580B9F9ECD3987D2C0D3CAB0EA77ADD6E4`.
All addresses here are preferred x86 PE VAs (image base `0x400000`), translated
through PE sections by the evidence checks.

Saved metadata and decisions are in
`src\DumpTruck\CrashReportNativeLayout.cs`. Shared layer framing and the
hardware-memory compressor remain in `HardwareReportNativeLayout.cs`.
The [connection ledger](CompanionBinaryConnections.md) records the related
RedHanded, r5dumper, dbghelp, and HWStats boundaries.

## Outer body and layer ledger

The HTTP request constructor `0x40D8C0` initializes its body `slBlob` at
request `+0x2C` with null backing fields (`0x40D91B..0x40D936`).
Submit binds a fresh blob packer to it at `0x40A854..0x40A862`.
The packer constructor `0x425DA0` clears its position/buffer fields and
does not write a header itself. The first explicit write is
`0x40A879 -> 0x40C980`, copying the DWORD at VA `0x476314`.
Those four sample bytes are **`ED C8 FE DE`**, value **`0xDEFEC8ED`**.
No separate version or total-body-length field is written between that
magic and the first layer in this producer path.

After the magic, each nonzero layer is a little-endian DWORD type,
little-endian DWORD payload byte count, then exactly that many body bytes.
Layer close patches the payload-only count, excluding the8-byte header.
There is no observed alignment padding between these writes.
The final tag is DWORD0 at `0x40AD23`; it has **no following length field**.
Report finalization at `0x40AD2E` commits the accumulated serializer bytes.

| Producer order | Type | Payload | Type evidence | Inclusion / failure behavior |
|---|---:|---|---|---|
| 1 | 1 | Compressed core-dump file | `0x40A8FA`, layer constructor call `0x40A909` | Written if owner `+0` path is nonempty; an attempted open or compression failure aborts packaging. Initialization separately rejects an unspecified dump. |
| 2 | 4 | Application-name bytes including NUL, **not compressed** | `0x40A9A1`, type writer `0x40A9B5` | Written unconditionally after the core-dump branch; null text yields a one-byte NUL payload. |
| 3 | 2 | Compressed main log, possibly clipped | `0x40AA99`, helper call `0x40AAA3` | Nonempty owner `+4` path required. Open failure logs and continues; compression failure aborts. |
| 4 | 5 | Compressed sibling `awesomium.log`, possibly clipped | `0x40AB7F`, helper call `0x40AB89` | Inside the nonempty main-log-path branch; additionally requires a nonempty directory result. Open failure logs and continues; compression failure aborts. |
| 5 | 3 | Compressed HWStats stdout plus final NUL | `0x40AC4E`, layer constructor call `0x40AC5A` | Generator exit0 required. Nonzero exit logs and skips; compression failure aborts. |
| End | 0 | No payload or length | `0x40AD15..0x40AD27` | Normal completion only. |

This order is not ascending type order. Optional sections may be absent.
The body layout is established by producer instructions, not by an actual
submitted report sample. Layer-presence decisions do not promise successful
allocation, file access, or compression.

### Type4 is the application name, not the message

Submit reads owner `+0x0C` at `0x40AA08` and again at `0x40AA33`.
The string's stored length is incremented by1 at `0x40AA23`; this count
is reserved at `0x40AA2E` and copied with memcpy at `0x40AA42`.
The layer then closes at `0x40AA51`. No compressor is called for this body.

Independent option initialization identifies that field:
`0x40541C` loads the `app` literal at `0x475D68`, retrieves its value,
and copies it into owner `+0x0C` at `0x40542D..0x405434`.
The `msg` literal at `0x475D6C` instead populates owner `+0x10`
(`0x40543A..0x405452`).
Do not label type4 as message text from a plausible field-order guess.
The exact text encoding is not established by these byte-copy routines.

## File-backed compression is a different loop

Core-dump compression invokes `0x409660` at `0x40A91B`.
The optional-file writer `0x40A070` invokes the same helper for logs whose
observed signed `ftell` result is at most `0x40000` (256KiB).
These calls use the embedded deflate-family initializer `0x45FA80`,
processor `0x460210`, and cleanup `0x460A90`, also used by the memory path.
They do not prepend an observed uncompressed-size field inside the layer.

The input and output buffers each allocate `0x4000` bytes. Despite that,
the native loop requests **four** input bytes per `fread` and advertises
**four** output bytes per deflate call:

| Stage | Native instruction facts |
|---|---|
| Need input | If `avail_in == 0`, `0x409757..0x40975F` calls `fread(input, 1, 4, file)`; `ferror` at `0x409769` sends failure to cleanup. Otherwise existing input is retained. |
| Output | `0x409781` sets `avail_out=4`; `0x409788` resets next_out. |
| Flush | `feof` is queried at `0x40978B`; `0x409793..0x40979A` converts false/true to0/4, i.e. `Z_NO_FLUSH`/`Z_FINISH`. |
| Status gate | `0x4097A1..0x4097A5` rejects **only `-2`**, unlike the memory helper's0/1 gate. |
| Append | `0x4097A9..0x4097B7` appends `4 - avail_out` bytes when nonzero. |
| Drain again | `avail_out == 0` repeats at `0x409780`, without first reading more input. |
| More input or success | With output space left, a **second** `feof` query at `0x4097C4` selects another input cycle or cleanup/success. |

The first EOF query selects flush, while the later query controls return;
the decision captures model them separately. The loop does not explicitly
require `Z_STREAM_END`, nor does it explicitly reject every other zlib error.
This describes its actual branch structure, not an endorsement of arbitrary
status combinations. In particular, no return-success claim is made for
an entire file based only on one synthetic loop decision.
`CaptureFileDrain` preserves the sole `-2` rejection, EOF/space branches,
and unchecked DWORD subtraction. The existing hardware/memory capture
remains distinct.

## Large-log clipping before compression

`0x40A070` seeks to end and stores `ftell` as a signed32-bit value.
The comparison at `0x40A0B9..0x40A0C3` takes the clipped path only when
that result is **greater than** `0x40000`, not when equal. A negative result,
including the usual `ftell` failure value `-1`, follows the nonclipped path;
these instructions do not turn it into a large unsigned size.
The seek/tell return values are not separately validated here.
`SelectsClippedLogPath` captures the comparison without performing file I/O.

The clipped branch:

- Resizes a local `slBlob` to `0x40000` at `0x40A19D`; its backing `+0x0C`
  is logical length, distinct from allocation capacity.
- Seeks to start and requests a2048-byte prefix (`0x40A1B8..0x40A1C5`).
- Appends the `_snprintf` marker
  `"\r\n\r\n... clipping approximately %u bytes ...\r\n\r\n"` at
  `0x40A238..0x40A246`. The argument is original observed length minus
  the initial blob length, so the wording is **approximately** rather than
  an exact count of the omitted interval.
- Computes remaining space after the actual prefix-read count and marker's
  returned count; seeks to original length minus that remaining space at
  `0x40A268`, then reads a tail into the same blob at `0x40A2C0`.
- Resizes to prefix-plus-marker-plus-actual-tail-read count at `0x40A2D1`,
  wraps it in the chosen layer type, and compresses the memory range via
  `0x409540` at `0x40A340`, not the file-streaming helper.

There is no independently appended NUL in this clipped-log construction.
Under normal successful reads the tail overwrites the marker formatter's
terminating NUL; only the bytes counted by the final resize are compressed.
Do not copy the HWStats-specific trailing-NUL rule onto log layers.
Short-read, seek-failure, and formatter-failure handling is not promoted
into a safe managed clipping implementation: only static facts and the
signed path choice are captured here.

## Checks and unresolved boundaries

```powershell
dotnet build Firefall.Reconstruction.slnx -v:q
dotnet run --no-build --project tests\CompanionTools\CompanionTools.Checks.csproj -- --samples "D:\SteamLibrary\steamapps\common\Firefall\system\bin"
```

Checks guard all nine binary identities, then compare the real DWORD magic,
option-to-owner offsets, type arguments, copy calls, clipping threshold,
and compression-loop instruction bytes. Pure checks exercise signed threshold
boundaries and distinguish file versus memory deflate statuses. They do not
execute DumpTruck, read arbitrary process memory, or upload anything.

Confidence is high for the instruction-backed producer layout and branch
decisions. An authorized offline `save_poo` report fixture would be the
smallest independent check of actual emitted layer lengths, compressed
streams, and text bytes. Without such a fixture, byte-for-byte compression
output and a complete report reader remain unverified. Request-header,
authorization, confirmation, and server-side parsing are outside this body
trace. No packets or requests were sent to investigate them.
