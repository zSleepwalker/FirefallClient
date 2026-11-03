# HWStats DriveSpace output

Static, read-only analysis of HWStats.exe (594648 bytes, SHA256
`0AE559DA740DE2C51A77A652906F021833A17E2A634757C15A6ADE5BEB9D679D`).
Addresses are preferred PE VAs, image base `0x400000`.
No drives or disk-space APIs were queried during this reconstruction.

## This section is disk space, not memory

The report section at `0x4037F0` prints the `DriveSpace` header
(literal `0x443160`) and enumerates **C through Z**, inclusive
(`0x40381C`, `0x40394B..0x40394E`). It does not enumerate A/B.
GetDriveTypeW at `0x403843` admits only result3 (DRIVE_FIXED).
RAM is handled separately by `0x4035A0`; the earlier human-facing
`memory (sub_4037F0)` label has been corrected.

For each fixed drive, helper `0x403690` constructs a root path and calls
GetDiskFreeSpaceExW at `0x4036E8`. Its numeric source is
**FreeBytesAvailableToCaller**, not TotalNumberOfBytes or
TotalNumberOfFreeBytes. Stack push/store/read evidence at
`0x4036D8..0x4036E8` and `0x403717..0x40371C` identifies the used
output slot `ebp-0x1C`.

The helper accepts C-Z/c-z drive characters and otherwise substitutes C
(`0x4036B8..0x4036D4`). The section's own loop supplies uppercase C-Z.
API failure writes both numeric output floats to zero. The caller does
not check the helper's EAX return; on successful paths that register is
clobbered by arithmetic and is not a reliable preserved API BOOL.

## Preserve the literal float arithmetic

The helper computes two float outputs. With `lo` and `hi` the low/high
32-bit parts of available-to-caller bytes, the instruction-equivalent
steps are:

```csharp
uint shiftedLow = (lo >> 20) | (hi << 12);
float fraction = (float)(lo & 0xFFFFF) * (1f / 1048576);
float mb = (float)unchecked((int)shiftedLow) + fraction;
float scaledMb = mb * (1f / 1024);
float gb = (float)(hi >> 8) + scaledMb;
```

Evidence includes signed cvtdq2ps at `0x40374E`, float reciprocal
constants at `0x48634C` (`0x35800000`, 2^-20) and `0x486358`
(`0x3A800000`, 2^-10), and the ordered multiply/add instructions at
`0x403758..0x40376F`.

The extra `hi>>8` term is really present; it is not replaced with a
generic bytes-to-GiB converter. For synthetic available=2^40 bytes,
this sequence yields mb=1048576 and gb=1025, not1024. The signed
conversion also yields unusual large-input results when the shifted
low DWORD has its sign bit set. Those are instruction-derived test
cases, not captured measurements of an actual drive.

The MB/GB labels are the literal report text despite binary scale
constants and the arithmetic quirk. The saved code names these
reported amounts, rather than asserting exact SI-unit conversion.

## Output admission and totals

Per-drive and total selection use **strictly greater than1.0f**:

| Amounts after native computation | Output |
|---|---|
|gb>1 | GB format |
|Otherwise mb>1 | MB format |
|Otherwise | No value line |

Thus exactly1 GiB selects `1024.000 MB`, while exactly1 MiB produces
no per-drive value line. The four literal formats are `%c = %.3f GB\n`,
`%c = %.3f MB\n`, `total = %.3f GB\n`, `total = %.3f MB\n`.
Threshold branches occur at `0x403896`, `0x4038E4`, `0x403957`,
and `0x4039A8`, with1.0f literal bits `0x3F800000`.

Both output floats are accumulated before selecting whether to print
the current drive (`0x403878..0x403891`). Even individually suppressed
drives can therefore contribute to an admitted total line. Query
failures contribute zero; all-zero amounts suppress the total value
line while the section header is still emitted.

`sub_406A20` formats through _vsnprintf_s and writes via fputs.
`DriveSpaceNativeLayout.cs` preserves only source, arithmetic, admission,
and accumulation decisions; it does not call APIs or reproduce the
entire CRT/locale-dependent text stream. Checks verify literal formats,
API import/call/stack slots, arithmetic bytes/constants, drive range,
thresholds, and synthetic float edge cases.

The output ultimately follows the existing [DumpTruck hardware
handoff](CompanionBinaryConnections.md) into report layer3. No report
generation, upload, or real disk survey was run here.
