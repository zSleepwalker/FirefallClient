# HWStats OS-description text fragments

Static inspection of the 594,648-byte `HWStats.exe`, SHA256
`0AE559DA740DE2C51A77A652906F021833A17E2A634757C15A6ADE5BEB9D679D`.
Addresses are preferred PE VAs (base `0x400000`).
`src\HWStats\OsDescriptionTextNativeLayout.cs` captures selected strings,
not the complete CRT stream or an OS detector. Apply the
[admission guards](HWStatsOsDescriptionGuards.md) before interpreting these
fragments as an emitted description.

## Input fields and assembly order

Builder `0x403A20` uses the observed extended version structure: major at
offset 4, minor 8, build 12, platform 16, service-pack wide text 20, suite
WORD 280 and product-type byte 282. Processor architecture is the first
WORD of its separate SYSTEM_INFO. Major 6 additionally uses the observed
GetProductInfo output; major 5/minor 2 uses the SM_SERVERR2 observation.
These are supplied observations, never queries performed by saved C#.

The stream receives:

1. `Microsoft `, wide literal VA `0x4431A8`, at `0x403B08`-`0x403B1A`.
2. Major-6 name plus optional mapped edition, or major-5 name/edition.
   Other admitted majors add neither of those version-specific pieces.
3. If the service-pack text is nonempty, ` ` (VA `0x443A48`), then that
   text, at `0x403E54`-`0x403E89`.
4. ` (build ` (VA `0x443A50`), then the build DWORD at `0x403E8E`-`0x403EAA`.
5. Closing delimiter via helper `0x406310`, then any architecture suffix.

Wide helper `0x4060C0` measures the supplied WORD string and sends it to
the wide stream buffer; it does not invent separators between literals.
Build insertion IAT `0x4402E4` names the unsigned-long overload (mangled
argument `K`), not a signed build-number conversion. Closing helper
`0x406310` uses a one-byte `)` literal at `0x443A4C` and the stream's
ctype<wchar_t> widening facet. The source preserves that input byte rather
than emulating locale/facet behavior.

## Major 6 names and product-code table

At `0x403B2C`-`0x403B86`, product type exactly 1 selects workstation;
all other values take the alternate branch.

| Minor | Workstation literal | Alternate literal |
|---|---|---|
| 0 | `Windows Vista ` at `0x4431C0` | `Windows Server 2008 ` at `0x4431E0` |
| 1 | `Windows 7 ` at `0x44320C` | `Windows Server 2008 R2 ` at `0x443224` |
| 2 | `Windows 8 ` at `0x443254` | `Windows Server 8 ` at `0x44326C` |
| Other | `Windows ? ` at `0x443290` | Same |

`0x403BD1` subtracts 1 from the product code and admits unsigned indices
0-47. The 48-byte index table at `0x403FA8` selects one of 19 DWORD
destinations at `0x403F5C`; destination 18 is the no-edition continuation
`0x403BF5`. The remaining entries select these literal loads:

| Product code | Text | Literal VA |
|---|---|---|
| 1 | Ultimate Edition | `0x4432D4` |
| 2 | Home Basic Edition | `0x443340` |
| 3 | Home Premium Edition | `0x443314` |
| 4 | Enterprise Edition | `0x443368` |
| 6 | Business Edition | `0x443390` |
| 7 | Standard Edition | `0x4435CC` |
| 8 | Datacenter Edition | `0x443404` |
| 9 | Small Business Server | `0x44342C` |
| 10 | Enterprise Edition | `0x4434A8` |
| 11 | Starter Edition | `0x4433B4` |
| 12 | Datacenter Edition (core installation) | `0x443458` |
| 13 | Standard Edition (core installation) | `0x4435F0` |
| 14 | Enterprise Edition (core installation) | `0x4434D0` |
| 15 | Enterprise Edition for Itanium - based Systems | `0x443520` |
| 17 | Web Server Edition | `0x44363C` |
| 18 | Cluster Server Edition | `0x4433D4` |
| 25 | Small Business Server Premium Edition | `0x443580` |
| 48 | Professional | `0x4432F8` |

Other product codes append no edition, including in-range codes 5 and 16.
Literal `Windows Server 8 ` and the spaced `Itanium - based` text are
preserved, not modernized.

## Major 5 branches

`0x403BF5`-`0x403E4F` handles minors 0, 1 and 2; other minors add no
major-5 name. The source retains exact case, punctuation and spaces.

| Minor | Native decisions |
|---|---|
| 0 | `Windows 2000 `; product type 1 adds Professional, else suite bit 0x80 adds Datacenter Server, else bit 2 adds Advanced Server, else Server |
| 1 | `Windows XP `; suite bit 0x200 adds Home Edition, otherwise Professional |
| 2 | Prefix and optional edition selected as below |

For minor 2, the prefix priority is nonzero SM_SERVERR2
(`Windows Server 2003 R2, `), then suite 0x2000
(`Windows Storage Server 2003`), then suite 0x8000 (`Windows Home Server`),
then workstation plus architecture 9 (`Windows XP Professional x64 Edition`),
otherwise `Windows Server 2003, `. See metric call `0x403C13`, suite tests
`0x403CDA`/`0x403CE8`, and workstation/architecture tests `0x403CF6`.

Product type 1 skips any subsequent edition at `0x403D1B`. Otherwise:

| Architecture | Edition priority |
|---|---|
| 6 (Itanium) | Suite 0x80: Datacenter Edition for Itanium-based Systems; else bit 2: Enterprise Edition for Itanium-based Systems; else no edition |
| 9 (AMD64) | Suite 0x80: Datacenter x64 Edition; else bit 2: Enterprise x64 Edition; else Standard x64 Edition |
| Other | Suite 0x4000: Compute Cluster Edition; else 0x80: Datacenter Edition; else 2: Enterprise Edition; else 0x400: Web Edition; else Standard Edition |

These are the actual branch priorities at `0x403D25`-`0x403DB2`, not a
generic interpretation of suite bits. Storage/Home Server prefixes have
no trailing separator; the model deliberately preserves combinations such
as `Windows Storage Server 2003Standard Edition`. It does not repair
spacing, normalize contradictory observations, or add an Itanium Standard
edition absent from the native path.

## Architecture and bounds

At `0x403EB7`-`0x403EE4`, only unsigned major >= 6 can append a suffix:
architecture 9 gives `, 64-bit` (VA `0x443A64`), architecture 0 gives
`, 32-bit` (VA `0x443A78`), others give none. Major 5 can already have an
architecture-specific name but receives no additional suffix.

Companion checks bind literals to instruction loads, preserve the complete
index table and each edition destination, verify suite/architecture/metric
branch priorities and suffix/build/helper calls, and exercise synthetic
version/product/suite combinations. No target, host API, registry or
hardware survey was executed. Full CRT locale/padding/error behavior,
numeric build formatting and the outer OS/DirectX line remain unimplemented.
