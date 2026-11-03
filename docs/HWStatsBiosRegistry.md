# HWStats BIOS registry requests

Static inspection of the 594,648-byte `HWStats.exe`, SHA256
`0AE559DA740DE2C51A77A652906F021833A17E2A634757C15A6ADE5BEB9D679D`.
All addresses are preferred PE VAs (image base `0x400000`).
`src\HWStats\BiosRegistryNativeLayout.cs` saves inert request facts and
pure status/print-admission decisions. It does not open registry keys.

## Request chain

The System section `0x404020` calls value writer `0x4030F0` five times:

| Call VA | Base string VA | Value string VA | Value name |
|---|---|---|---|
| `0x40415B` | `0x443B50` | `0x443B2C` | Identifier |
| `0x40416F` | `0x443B80` | `0x443B6C` | SystemBiosDate |
| `0x404183` | `0x443BE0` | `0x443BA0` | SystemBiosVersion |
| `0x404197` | `0x443C10` | `0x443BFC` | VideoBiosDate |
| `0x4041AB` | `0x443C50` | `0x443C30` | VideoBiosVersion |

Every base literal is `HARDWARE/DESCRIPTION/SYSTEM`. The caller loads it
into EDX, the report object into ECX, and pushes the value-name pointer.
The section does not branch on individual writer results before trying
the next value.

At `0x403112` the writer supplies a 1024-byte output buffer, clears EDX
for an absent middle subkey, and calls query helper `0x406F30` at
`0x40312A`. Helper `0x406EC0`, called at `0x4070BE`, `0x4070C8` and
`0x4070D2`, copies value name, middle subkey and base path respectively,
replacing each byte `/` with `\`. It does not change character case.

`0x4070D7`-`0x407107` skips the optional middle component when empty.
`0x40710A`-`0x407114` nevertheless appends a final `\`, so the actual
opened subkey is `HARDWARE\DESCRIPTION\SYSTEM\`, including that trailing
separator. `RegOpenKeyExA` at `0x40712A` uses HKLM (`0x80000002`), zero
reserved options, and access mask **`0xF003F` (`KEY_ALL_ACCESS`)**, not
`KEY_READ`. No explicit WOW64 view-selection bit is present.

## Query and output boundaries

After a successful open, `0x407146` calls `RegQueryValueExA`, passing the
normalized value name, data pointer, type output and the supplied byte
capacity. At `0x407157` it copies the type output to the caller even if
the value query failed. `0x407159` closes the key; the close status does
not replace the query status. `0x4071AD`-`0x4071B7` collapses a nonzero
open/query status to `0x80004005` and returns zero for success.

The writer checks type **before** helper status:
`0x40313A`-`0x403142` accepts only `REG_SZ` (1) or `REG_MULTI_SZ` (7).
Other types, including `REG_EXPAND_SZ`, return `0x80004005`. The accepted
type path tests helper status at `0x403159`; only zero reaches output.
The source's print predicate requires both conditions; it does not invent
a type output for failed opens, where the caller's type slot remains
uninitialized.

The narrow output format is `%s = %s\n` at `0x443C7C`, used at
`0x40319F`, with optional report indentation. The buffer is passed
directly as a C string: there is no REG_MULTI_SZ joining, explicit
terminator insertion or size-retry loop on this path. For a terminated
multi-string this exposes only the first component. Malformed or
unterminated returned data is not given invented safe rendering by the
saved model.

## Evidence and bounds

Hash-guarded companion checks verify all five call-site argument literals,
slash replacement, buffer size, optional-component setup, unconditional
trailing separator, exact hive/access pushes, ANSI registry imports,
query/close calls, HRESULT collapse and output gates. Pure checks cover
success and several error statuses plus accepted and rejected types.

No sample was executed, no registry was read, and no BIOS contents were
collected. OS-description construction at `0x403A20` and the separate
DirectX version read remain outside this capture.
