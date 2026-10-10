# FirefallClient.exe address translation

The active IDA database loads the executable as one raw file segment at address `0`, rather than loading PE sections at their virtual addresses. **Function addresses in the reconstruction are raw file offsets.** Direct relative calls generally still point at useful raw code offsets, but absolute pointers into data and switch jump tables can point at unrelated bytes unless translated through the PE section table.

PE32 (`MZ`, `PE\0\0` at file offset `0x170`), machine `0x014C` (x86), image base `0x400000`, entry RVA `0x136B9B0`, six sections:

| Section | RVA | Virtual size | Raw file offset | Raw size |
| --- | ---: | ---: | ---: | ---: |
| `.text` | `0x1000` | `0x16C27F5` | `0x400` | `0x16C2800` |
| `.rdata` | `0x16C4000` | `0x30062E` | `0x16C2C00` | `0x300800` |
| `.data` | `0x19C5000` | `0x15647C` | `0x19C3400` | `0x71800` |
| `text` | `0x1B1C000` | `0x606` | `0x1A34C00` | `0x800` |
| `.rsrc` | `0x1B1D000` | `0xB16668` | `0x1A35400` | `0xB16800` |
| `.reloc` | `0x2634000` | `0x108A64` | `0x254BC00` | `0x108C00` |

For an absolute virtual address `VA`, compute `RVA = VA - 0x400000`, find its section, then `fileOffset = section.rawOffset + (RVA - section.RVA)`. For a raw code offset in `.text`, reverse this formula before comparing it to a PE VA. For example, function `0x1310060` maps to VA `0x1710C60`, while the absolute table address `0x1C5A3B8` maps to raw offset `0x1858FB8` in `.rdata`; reading raw offset `0x1C5A3B8` instead lands in `.rsrc`. The type-switch byte table at VA `0x17124C4` is actually at raw `0x13118C4`; incorrect raw-map jump target interpretation explains why the IDA decompilation of `0x1311100` runs into nonsensical addresses.

The section layout was read directly from the PE header through the IDA raw-image bytes. Other subsystem notes may use IDA's preexisting names, but absolute references must be checked with this mapping before assigning semantics.
