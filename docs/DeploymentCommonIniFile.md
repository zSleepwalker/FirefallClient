# DeploymentCommon INI wrapper boundaries

Static, read-only IL reconstruction of IniFile in the identified
DeploymentCommon.dll (76800 bytes, SHA256
`33848E8CA80DFAD7B6883D5A57FC55834411E574CEE6467C753B77AD4523DAE0`).
No configuration file was opened, parsed, or modified during analysis.

The constructor (`0x060000D8`, body RVA `0x5310`) stores its supplied
path directly in m_iniFilePath. No constructor path normalization or
default file selection occurs in the traced IL.

## Imports and result handling

| MethodDef token | kernel32 import | Raw signature | ImplMap flags |
|---|---|---|---|
| `0x060000E2` | GetPrivateProfileString | `00 06 09 0E 0E 0E 1D 03 09 0E` | `0x0106` |
| `0x060000E3` | WritePrivateProfileString | `00 04 09 0E 0E 0E 0E` | `0x0100` |
| `0x060000E4` | WritePrivateProfileSection | `00 03 09 0E 0E 0E` | `0x0100` |

These are uint-returning declarations. Read uses a char[] buffer and
CharSetMask/Auto flag value `0x0006`; writes do not record an explicit
charset flag. None of these rows sets SetLastError. This records the
metadata asymmetry, not a runtime A/W export-resolution trace.

SetString (`0xDB`, RVA `0x53B8`) forwards section/key/value/path and
returns native-result!=0. DeleteSectionKeys (`0xE1`, RVA `0x54BF`)
calls WritePrivateProfileSection(section, **empty string**, path) and
discards its result. It does not pass a null section-content pointer
in the traced call, and no success check or method-local error log is
performed there.

## Growing read buffer and null results

GetString (`0xDA`, body RVA `0x5328`) starts with512 chars and calls
GetPrivateProfileString with default="" and the stored path.

| Returned character count | Wrapper action |
|---|---|
|0 | Return null |
|Exactly current buffer length-1 | Double capacity and retry if new capacity<=32768 |
|Same saturation at32768 | Throw OutOfMemoryException with `Ini value too large: [{0}], [{1}]` |
|Other nonzero count | Copy exactly that count into a new char[] and construct string |

The capacity sequence is512,1024,2048,4096,8192,16384,32768.
The advertised MaxKeyValueSize getter (`0xD9`, RVA `0x531F`) returns
32766. There is no separate nSize-2 branch: a returned count of510
with a512-char buffer is accepted, not retried. The same reader is used
for section/key multi-string queries, so the wrapper does not independently
guard a multi-string nSize-2 truncation indication.

Zero count conflates absent/empty/native-failure observations at this
boundary; the wrapper returns null rather than preserving a cause.
This is original behavior, not an added success-shaped fallback.

## Boolean and key checks preserve surprising null/default behavior

GetBool with default (`0xDD`, RVA `0x53E8`) calls GetString, then calls
**instance** string.Equals("true", OrdinalIgnoreCase) or Equals("1",...).
Only those two nonnull values produce an unconditional true.
Any other nonnull text returns the supplied default, including "false"
and "0". Thus those strings still produce true when default=true.
No whitespace trim is performed. The two-argument overload (`0xDC`,
RVA `0x53DA`) supplies default=false.

If GetString returned null, the instance Equals call throws
NullReferenceException before the default can be used. The pure saved
capture deliberately preserves that exception kind; it does not
"repair" it into a null-safe Boolean parser.

GetSections (`0xDE`, RVA `0x5420`) calls GetString(null,null);
GetSectionKeys (`0xDF`, RVA `0x5450`) calls GetString(section,null).
Null results become empty string arrays. Otherwise the returned string
is split on a zero-initialized one-char array (NUL), with
RemoveEmptyEntries=1. No standalone INI or multi-string parser was added
to the reconstructed code.

KeyExists (`0xE0`, RVA `0x5480`) compares requested key against each
enumerated key using instance requestedKey.Equals(..., OrdinalIgnoreCase).
Null requested key throws when a candidate is visited, but an empty
enumeration returns false without dereferencing it.

`IniFileNativeLayout.cs` saves inert buffer/read, Boolean/default,
key-comparison, and write-result decisions only. CompanionTools checks
the import metadata and complete wrapper IL, exercises every native
buffer capacity and its adjacent count boundaries, and uses synthetic
text to distinguish defaults, case, whitespace, and null failures.
