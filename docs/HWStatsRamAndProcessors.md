# HWStats RAM arguments and processor count

Static inspection of `HWStats.exe`, 594,648 bytes, SHA256
`0AE559DA740DE2C51A77A652906F021833A17E2A634757C15A6ADE5BEB9D679D`.
Addresses below are preferred PE VAs (image base `0x400000`).
`src\HWStats\RamAndProcessorNativeLayout.cs` saves pure argument and count
captures; it does not call memory or affinity APIs or reproduce CRT output.

## Physical memory

Function `0x4035A0` initializes only `MEMORYSTATUSEX.dwLength` to 64 at
`0x4035B6`. It calls `GlobalMemoryStatusEx` through IAT `0x440174` at
`0x4035C0`, then immediately reads the two DWORDs of `ullTotalPhys`
(structure offset 8). There is no BOOL test or failure branch. Unwritten
fields are not initialized to zero. A failed API call therefore does not
justify a deterministic zero-memory capture; the pure method requires an
observed field value.

`0x4035D0` shifts the 64-bit total right by 20, but the subsequent unsigned
comparison uses only the low DWORD of that result. The branch threshold is
1024, inclusive for the GB path. It is not a full-width physical-byte test.

| Branch | Instruction evidence | Saved arguments |
|---|---|---|
| Low DWORD below 1024 | `0x4035DF` pushes original high and low physical-byte DWORDs, then format VA `0x443120` | Exact literal `ram = %ull MB\n`, raw original bytes |
| Low DWORD at least 1024 | `0x4035E8` converts signed DWORD to double, `0x4035F6` adds 0 or 4294967296 using its sign bit | Unsigned whole-MB low DWORD, rounded to float |
| GB scaling | `0x4035FF` rounds double to float; `0x403603` multiplies by float 1/1024; `0x40360B` promotes back to double | Format VA `0x443130`, `ram = %.2f GB\n`, promoted float GB value |

Both branches call the variadic writer `0x406A20` at `0x403619`.
The small literal is **`%ull`**, not `%llu`; the suffix must not be repaired,
and the original integer argument must not be replaced with converted MB.
The source captures the two pushed DWORDs without claiming that the format
consumes both or implementing a CRT formatter.

Synthetic boundary examples: exactly 1 GiB selects GB with argument 1;
1 GiB plus less than 1 MiB still passes 1 because the fraction was discarded.
At 2^51 bytes, unsigned conversion yields GB argument 2097152, unlike the
DriveSpace helper's signed-conversion quirk. At 2^52 bytes the whole-MB low
DWORD wraps to zero and the small-format branch receives original DWORDs
`0x00000000` and `0x00100000`.

## Processor count

`0x403621` and `0x40362B` initialize the process and system affinity outputs
to zero. After `GetCurrentProcess`, `GetProcessAffinityMask` is called
through IAT `0x440170` at `0x40363E`. Its BOOL is not checked either.
`0x403644` loads the **system** output at `[ebp-0x4C]`, not the process
output at `[ebp-0x50]`.

The loop `0x403647`-`0x403666` uses a zero high DWORD and counts set low bits
while shifting. This x86 capture can count at most 32 bits; it does not
enumerate processor groups or count only processors allowed to this
process. At `0x403668`-`0x403670` the resulting count is passed to the key
writer `0x406AE0` with `nprocessors` (VA `0x443140`). The source explicitly
ignores its process-mask argument. A failed API that leaves both initialized
outputs unchanged produces zero, but arbitrary failure-path output values
are not inferred.

## Evidence and bounds

Companion checks compare the native initialization, imports, argument
pushes, threshold, unsigned-conversion correction table, float instructions,
count loop and literals against the hash-guarded sample. Pure cases cover
thresholds, fraction loss, truncation, unsigned high-bit conversion and
disagreeing process/system masks.

No sample was executed, no real hardware was queried, and no actual output
fixture was collected. CRT-specific `%ull` rendering remains outside the
saved model.
