# Matrix secondary selectors

Detailed static analysis of the secondary command dispatcher and its verified selector cases, extracted verbatim from [MatrixConnection.md](MatrixConnection.md) to keep the transport and connection notes navigable.

## Secondary command dispatcher and verified selector cases

The secondary-dispatch target `0x81CC50` is a large raw function
(`0x4B9E` bytes) with two separate selector tables. It reads a leading byte
from its input structure. Byte0 and values `0xF0..0xFF` use the secondary
selector path. For other values, it decrements the leading byte, compares the
result with `0x35`, then uses that decremented value as the index into the
54-byte map at VA `0xC2240C` (raw `0x82180C`). Thus nonzero leading bytes
`0x01..0x36` map to indices `0x00..0x35`; the map result selects a seven-entry
table at VA `0xC223F0` (raw `0x8217F0`). The observed leading-byte ranges are:

| Leading byte | Route index | Raw target | Observation |
| ---: | ---: | ---: | --- |
| `0x01..0x0E` | 0 | `0x81CCAD` | Nested selector table |
| `0x0F..0x12`, `0x20..0x21`, `0x29..0x33`, `0x36` | 1 | `0x8217D9` | Returns zero |
| `0x13..0x19` | 2 | `0x81EE63` | Three dispatcher values (`0x53..0x55`); value `0x53` documented below |
| `0x1A..0x1F` | 3 | `0x81EF6D` | Nine switch entries; entry 0 documented below |
| `0x22..0x25` | 4 | `0x81F206` | Three dispatcher values (`0x53..0x55`); value `0x53` documented below |
| `0x26..0x28` | 5 | `0x81F2EF` | Value `0x53` documented below |
| `0x34..0x35` | 6 | `0x81F34A` | Value `0x53` documented below |

Leading byte0 or `0xF0..0xFF` uses a second table at VA `0xC22630`
(raw `0x821A30`) when its selector is `0x20..0x7A`; values outside that
selector range return zero. Leading bytes `0x37..0xEF` also reach the
zero-return path. These comparisons establish dispatch boundaries only;
RTTI-backed secondary-selector identities are documented below.

The five case-bearing primary routes (indices 2..6) have 17 directly selected
subcases: three at route2, nine at route3, three at route4, and one each at
routes5 and6. All 17 subcases now have native layouts.
Route0's nested table has a separate, complete set of 40 documented selectors
(`0x53..0x7A`); route1 is the zero-return route. The secondary selector table
records 91 dispatch targets. Selectors `0x20..0x5F` now have 64 RTTI-backed
class and body-reader layouts; selectors `0x60..0x67`, `0x68..0x6F`, and
`0x70..0x77` each add eight verified layouts; selectors `0x78..0x7A` add the
final three. All 91 targets now have distinct, RTTI-backed message identities
and reconstructed reader layouts.


## Topic files

- [Core selector entries `0x20..0x7A`](MatrixSecondaryCore.md#secondary-selector-entries-0x200x7a)
- [Routes 2-6 and nested selectors](MatrixSecondaryRoutes.md#route-index-2-dispatcher-value-0x53-areavisualdatalootobjectcollected)
- [Character combat selectors `0x55..0x64`](MatrixSecondaryCombat.md#selector-0x55-tookdebugweaponhitpublic)
- [Character action selectors `0x65..0x73`](MatrixSecondaryActions.md#selector-0x65-jumpactioned)
- [Game-protocol selectors `0x74..0x7A`](MatrixSecondaryGameProtocol.md#selector-0x74-abilityactivated)
- [Message provenance and receiver factories](MatrixSecondaryFactories.md#message-discriminator-provenance-cross-check)
