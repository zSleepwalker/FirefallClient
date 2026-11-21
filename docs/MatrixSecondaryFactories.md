# Matrix secondary selectors: provenance and factories

### Message discriminator provenance cross-check

The four examples below trace constructor-written message `+0x10` values and
the conditional `0x81BB90` slot mapping only; they do not establish a global
selector namespace. In each parser case, the seemingly discriminator-like
DWORD is a stack local at `[esp+0x20]`, not a field in the allocated message:
selector `0x71` writes `0x79` at raw `0x81D560`, selector
`0x77` writes `0x7F` at `0x81D74A`, selector `0x79` writes `0x81` at
`0x81D7AC`, and route-3 `Vehicle::AbilityActivated` writes `0xD0` at
`0x81EF91`. Those local values remain semantically unassigned.

The constructors instead place their parser/message selector in the first
stack argument to the base helper, then that helper stores the argument at
message `+0x10`. For `Character::ForcedMovement` (`0x71`), `NPCInteraction`
(`0x77`), `PrivateDialog` (`0x79`), and `Vehicle::AbilityActivated` (`0x53`),
the constructor's stack immediate is respectively `0x71`, `0x77`, `0x79`,
and `0x53`; helper store instructions at `0x5A1191`, `0x5A1451`, `0x5A1531`,
and `0xB474D1` write that first helper argument to `[this+0x10]`. The helper
also writes a class-specific pointer to `+0x0C` (`0x1B5BDBC`, `0x1B5BD84`,
`0x1B5BD7C`, `0x1BB04E0`, respectively), copies the two constructor arguments
to `+0x18/+0x1C`, and zeros `+0x20/+0x24`; these other fields are not the
discriminator. The parser call sites pass source DWORDs `[ESI]` and
`[ESI+4]` as the constructor arguments. The helper temporarily installs its
common vptr `0x1C61B84` at `+0`, then its class-specific base vptr
(`0x1C65AF0`, `0x1C65AD0`, `0x1C65A70`, `0x1C72554`, respectively); each
derived constructor replaces that same primary vptr at `+0`. The four
concrete message vtables have RTTI Complete Object Locator offsets of zero.
For selector `0x71`, the separate state initializer
at `0x92B8A0` begins with a zero DWORD at message `+0x28`, not a secondary
vptr.

The receiver chain preserves the allocated message pointer: callback helper
`0x7FDC60` loads it from `[record]` into EBX at `0x7FDC6E`, loads the receiver
from callback-context `+8` at `0x7FDD47`, pushes the unchanged EBX at
`0x7FDD4E`, and calls `0x81BB90` at `0x7FDD4F`. The dispatcher loads its
message argument into EDI at `0x81BB95`, reads the DWORD at `[EDI+0x10]` at
`0x81BBB7`, subtracts `0x20`, bounds it through `0x7A`, and indexes the
receiver switch table. Each representative case sets ECX to the receiver and
pushes the unchanged EDI message pointer before its virtual call:

| Construction selector / stored discriminator | Parser stack local | Switch target / slot-call raw | Receiver slot | Vtable method VA / raw |
| --- | --- | --- | --- | --- |
| `0x71` Character `ForcedMovement` | `0x79` at `0x81D560` | `0x81C90B` / `0x81C92A` | `+0x148` | `0xECED30` / `0xACE130` |
| `0x77` Character `NPCInteraction` | `0x7F` at `0x81D74A` | `0x81CA0D` / `0x81CA2C` | `+0x160` | `0xECEEA0` / `0xACE2A0` |
| `0x79` Character `PrivateDialog` | `0x81` at `0x81D7AC` | `0x81CA63` / `0x81CA82` | `+0x168` | `0xECEF00` / `0xACE300` |
| `0x53` Vehicle `AbilityActivated` | `0xD0` at `0x81EF91` | `0x81C401` / `0x81C420` | `+0xD0` | `0xECB180` / `0xACA580` |

For messages that actually reach `0x81BB90`, the slot formula is
`4 + 4*(id-0x20)`. In particular, the `0xD0` parser stack local for Vehicle
`AbilityActivated` is not its discriminator; `0xD0` is that selector's
receiver-vtable byte offset, while the message field is `0x53`. The unchanged
message pointer and primary-vtable calls are proven only for this conditional
dispatcher path, not for every parser family.

### Parser-family collisions and callback route gate

The route-specific parser families below all construct different RTTI types
with the same value, `message+0x10 = 0x53`. The route-local stack DWORDs shown
are separate parser temporaries, not namespace IDs. Character route 0 and
Vehicle route 3 use selector tables; AreaVisualData route 2, Deployable route
4, Turret route 5, and LootStoreExtension route 6 branch directly in their
route dispatchers.

| Parser route | Route dispatcher / selector table | Case raw offset | Message class | Allocation | Parser stack local | Constructor → base helper | `+0x0C` metadata pointer | Primary vtable / RTTI |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Character nested route 0 | `0x81CCAD` / VA `0xC22444` | `0x81CCC3` | `Character::MarketRequestComplete` | `0x48` | `0x5B` at `0x81CCD1` | `0x524C30` → `0x5A0400` | `0x1B5BEAC` (`msg-83…`) | `0x1C67278` / `.?AVMarketRequestComplete@Character@firefall@@` |
| AreaVisualData route 2 | `0x81EE63` / direct case | `0x81EF1C` | `AreaVisualData::LootObjectCollected` | `0x40` | `0xCD` at `0x81EF2A` | `0xADBAA0` → `0xB07660` | `0x1BB0224` (`msg-83…`) | `0x1C723C0` / `.?AVLootObjectCollected@AreaVisualData@firefall@@` |
| Vehicle route 3 | `0x81EF6D` / VA `0xC2260C` | `0x81EF83` | `Vehicle::AbilityActivated` | `0x30` | `0xD0` at `0x81EF91` | `0xB093C0` → `0xB474B0` | `0x1BB04E0` (`msg-83…`) | `0x1C72998` / `.?AVAbilityActivated@Vehicle@firefall@@` |
| Deployable route 4 | `0x81F206` / direct case | `0x81F29E` | `Deployable::TookHit` | `0x60` | `0xD9` at `0x81F2AC` | `0xB27E80` → `0xB47C40` | `0x1BB03C4` (`msg-83…`) | `0x1C727A8` / `.?AVTookHit@Deployable@firefall@@` |
| Turret route 5 | `0x81F2EF` / direct case | `0x81F2F9` | `Turret::WeaponProjectileFired` | `0x48` | `0xDC` at `0x81F307` | `0xB36520` → `0xB47D90` | `0x1BB033C` (`msg-83…`) | `0x1C726E8` / `.?AVWeaponProjectileFired@Turret@firefall@@` |
| LootStoreExtension route 6 | `0x81F34A` / direct case | `0x81F354` | `LootStoreExtension::LootObjectCollected` | `0x40` | `0xDD` at `0x81F362` | `0xB43A70` → `0xB48040` | `0x1BB0280` (`msg-83…`) | `0x1C72608` / `.?AVLootObjectCollected@LootStoreExtension@firefall@@` |

The base helpers temporarily install a common vptr and then a class-specific
base vptr at `+0x00`; the derived constructors replace that with each
route-specific primary vptr. The helpers initialize `+0x08` to 1, store the
route-specific metadata pointer at `+0x0C`, store the compact parser selector
at `+0x10`, copy the stream QWORD to `+0x18/+0x1C`, and zero `+0x20/+0x24`.
They do not write `+0x14` in the observed common initialization sequence; its
meaning is unresolved. The metadata pointers differ, but their strings all
begin `msg-83`; neither that text nor the pointer is proven to be the runtime
namespace selector. The distinct derived RTTI/vptrs and parser-route cases
establish the parser family. A C# snapshot and byte checks for these
constructors and fields live in
`MatrixMessageNamespaceRoutingNativeLayout.cs` and
`StartupBinaryEvidence.cs`.

The stream parser at raw `0x821BA0` reads the envelope QWORD before the compact
selector and passes the QWORD address into `0x81CC50`; each route-specific
constructor passes its two source DWORDs to the base helper, which copies them
to `+0x18/+0x1C`. `tfGameSocketHandler` callback `0x7FDC60` first tests only
the low byte at `message+0x18`. If nonzero, it calls `0x800420`, which consumes
the full `+0x18/+0x1C` key through lookup helper `0x800070` and the socket
handler registries at `+0x24` and `+0x2C`. If the low byte is zero, it compares
`message+0x10` with the nested-message sentinel at `0x1DFC030`; a match takes
the nested path. Otherwise, a non-null callback-context `+8` receiver is
passed the unchanged message pointer at `0x81BB90`. The actual QWORD values
are runtime input and no sample stream was supplied, so none of the six
classes can be assigned to one of these callback branches statically.

The nonzero-key branch is backed by two ordered, QWORD-keyed trees embedded in
the `tfGameSocketHandler` object. At raw `0x7FD670`, its constructor calls
`0x805130` twice and stores the resulting tree headers at `+0x24` and `+0x2C`;
the same constructor stores the factory pointer at `+0x14` (VA `0xC1BDB0` /
raw `0x81B1B0`). `0x800420` copies the message selector and both key DWORDs,
then passes the full key to `0x800070`. That helper searches `this+0x24` and
`this+0x2C` through `0x8013B0`; the tree search compares the key with node
`+0x10/+0x14`, and the mapped pointer is at node `+0x18`. On a first-tree
miss, after the `0x800BF0` creation checks allow it, the callback at context
`+0x14` is called with the QWORD; its dispatcher `0x81B1B0` accepts low-byte
values `0x02..0x35`; the full 52-entry selector mapping is resolved below.
Entry creation uses `0x806910` and tree insertion uses `0x8064C0`; a separate
guarded path at `0x800300` inserts a QWORD-keyed entry into the `+0x2C` tree.
These observations establish the keyed lookup and registration mechanism, not
the meaning of an individual factory selector. The mapped construction cases
are recorded below using their constructors, installed primary vtables, RTTI,
and subsequent receiver call path.

### QWORD-keyed receiver factory: selectors 0x02..0x35

Factory `0x81B1B0` receives the key used by the `0x800070` registry-miss path.
It reads only the low byte of that argument for table selection:
`movzx eax, al; add eax, -2; cmp eax, 0x33; ja 0x81BA93; jmp
[0xC1C6A8+eax*4]`. Thus the table has 52 entries and accepts low-byte values
`0x02..0x35`; table VA `0xC1C6A8` maps to raw `0x81BAA8`. The low byte is a
factory selector, not evidence by itself of a message namespace. The full
QWORD remains the registry key at message `+0x18/+0x1C`. `0x800420` obtains the
registered object through `0x800070`, then uses the returned pointer as the
receiver in its compact-selector dispatch.

The table index is exactly `selector - 2`: selector `0x02` maps to index 0 and
`0x35` to index `0x33`; selector `0x01` underflows the unsigned bound and
`0x36` maps to `0x34`, so both take the out-of-range return. Within the table,
selectors `0x0F` and `0x11` also point to that same zero-return target
`0x81BA93` / VA `0xC1C693`; an in-range selector therefore does not imply
construction.

Each constructing case allocates the listed size through `0x12B7FB0`, calls
its constructor, and the constructor installs the listed RTTI-backed primary
vtable. Text targets use the verified raw-to-VA delta `+0x400C00`; vtable file
offsets use the original PE section map (`.rdata`), not that `.text` delta.
The `.rdata` mapping yields each vtable file offset, and the preceding pointer
resolves its Complete Object Locator (COL) and RTTI name.

| Key low byte | Case raw / VA | Allocation | Constructor raw | Primary vtable VA / raw | Primary slots `+04 / +08 / +0C / +18` (raw) | RTTI |
| --- | --- | ---: | --- | --- | --- | --- |
| `02` | `81B1F2 / C1BDF2` | `B80` | `53F980` | `1C66080 / 1864C80` | `540380 / 541220 / 5441B0 / 54DE00` | `.?AVBaseController@Character@firefall@@` |
| `03` | `81B230 / C1BE30` | `D80` | `54E7B0` | `1C65F1C / 1864B1C` | `54F4A0 / 5503D0 / 5536C0 / 55E160` | `.?AVNPCController@Character@firefall@@` |
| `04` | `81B26E / C1BE6E` | `11D8` | `55E830` | `1C65EF8 / 1864AF8` | `55EAA0 / 55EB10 / 561CF0 / 56B310` | `.?AVMissionAndMarkerController@Character@firefall@@` |
| `05` | `81B2AC / C1BEAC` | `12C0` | `56B970` | `1C65ED4 / 1864AD4` | `56BC20 / 56C0D0 / 56F3A0 / 576C80` | `.?AVCombatController@Character@firefall@@` |
| `06` | `81B2EA / C1BEEA` | `538` | `577320` | `1C65E8C / 1864A8C` | `577410 / 577480 / 5795A0 / 57FBD0` | `.?AVLocalEffectsController@Character@firefall@@` |
| `07` | `81B328 / C1BF28` | `A0` | `57FF80` | `1C65EB0 / 1864AB0` | `5800E0 / 580150 / 580470 / 580F40` | `.?AVSpectatorController@Character@firefall@@` |
| `08` | `81B366 / C1BF66` | `4F8` | `581070` | `1C65E2C / 1864A2C` | `581480 / 5817D0 / 5835C0 / 588500` | `.?AVObserverView@Character@firefall@@` |
| `09` | `81B3A4 / C1BFA4` | `420` | `588830` | `1C65DF4 / 18649F4` | `5889F0 / 588A90 / 589040 / 58A440` | `.?AVEquipmentView@Character@firefall@@` |
| `0A` | `81B3E2 / C1BFE2` | `58` | `58A530` | `1C65DAC / 18649AC` | `58A620 / 58A6E0 / 58AA90 / 58B4C0` | `.?AVAIObserverView@Character@firefall@@` |
| `0B` | `81B41D / C1C01D` | `1258` | `58B5F0` | `1C65D88 / 1864988` | `58B850 / 58BA50 / 58EA40 / 595090` | `.?AVCombatView@Character@firefall@@` |
| `0C` | `81B45B / C1C05B` | `A8` | `595680` | `1C65D58 / 1864958` | `595760 / 595820 / 595E30 / 596250` | `.?AVMovementView@Character@firefall@@` |
| `0D` | `81B499 / C1C099` | `438` | `5962A0` | `1C65D10 / 1864910` | `5963D0 / 596470 / 597590 / 59ACA0` | `.?AVTinyObjectView@Character@firefall@@` |
| `0E` | `81B4D7 / C1C0D7` | `1F8` | `59AEB0` | `1C65D34 / 1864934` | `59AFC0 / 59B060 / 59C600 / 59FF40` | `.?AVDynamicProjectileView@Character@firefall@@` |
| `10` | `81B515 / C1C115` | `88` | `AD9580` | `1C723D8 / 1870FD8` | `AD9790 / AD9830 / AD9BD0 / ADA370` | `.?AVObserverView@Melding@firefall@@` |
| `12` | `81B553 / C1C153` | `58` | `ADA400` | `1C7239C / 1870F9C` | `ADA4D0 / ADA570 / ADAB00 / ADBA10` | `.?AVObserverView@MeldingBubble@firefall@@` |
| `14` | `81B58E / C1C18E` | `158` | `ADBCE0` | `1C72378 / 1870F78` | `ADBE80 / ADBF40 / ADD790 / AE0EF0` | `.?AVObserverView@AreaVisualData@firefall@@` |
| `15` | `81B5CC / C1C1CC` | `628` | `AE10D0` | `1C72334 / 1870F34` | `AE1280 / AE1340 / AE1E80 / AE4170` | `.?AVParticleEffectsView@AreaVisualData@firefall@@` |
| `16` | `81B60A / C1C20A` | `760` | `AE42E0` | `1C722EC / 1870EEC` | `AE44A0 / AE4540 / AE5970 / AE9410` | `.?AVMapMarkerView@AreaVisualData@firefall@@` |
| `17` | `81B648 / C1C248` | `438` | `AE9640` | `1C72310 / 1870F10` | `AE9770 / AE9810 / AEA930 / AEE040` | `.?AVTinyObjectView@AreaVisualData@firefall@@` |
| `18` | `81B686 / C1C286` | `758` | `AEE240` | `1C722C8 / 1870EC8` | `AEE3F0 / AEE4B0 / AEF200 / AF1A60` | `.?AVLootObjectView@AreaVisualData@firefall@@` |
| `19` | `81B6C4 / C1C2C4` | `5C8` | `AF1C00` | `1C722A4 / 1870EA4` | `AF1D30 / AF1DD0 / AF3170 / AF68C0` | `.?AVForceShieldView@AreaVisualData@firefall@@` |
| `1B` | `81B702 / C1C302` | `570` | `B0A990` | `1C72868 / 1871468` | `B0AD20 / B0ADB0 / B0CE40 / B120B0` | `.?AVBaseController@Vehicle@firefall@@` |
| `1C` | `81B740 / C1C340` | `1120` | `B12490` | `1C72824 / 1871424` | `B12650 / B127E0 / B14E20 / B18EB0` | `.?AVCombatController@Vehicle@firefall@@` |
| `1D` | `81B77E / C1C37E` | `478` | `B19380` | `1C727E8 / 18713E8` | `B196E0 / B197A0 / B1B630 / B20100` | `.?AVObserverView@Vehicle@firefall@@` |
| `1E` | `81B7BC / C1C3BC` | `10F8` | `B204A0` | `1C727C4 / 18713C4` | `B20660 / B20720 / B22A80 / B26990` | `.?AVCombatView@Vehicle@firefall@@` |
| `1F` | `81B7FA / C1C3FA` | `B8` | `B26D40` | `1C72784 / 1871384` | `B26E50 / B26F20 / B27670 / B27E20` | `.?AVMovementView@Vehicle@firefall@@` |
| `21` | `81B838 / C1C438` | `78` | `AF6AE0` | `1C72254 / 1870E54` | `AF6C40 / AF6CE0 / AF6E50 / AF7260` | `.?AVAIObserverView@Anchor@firefall@@` |
| `23` | `81B873 / C1C473` | `1710` | `B281A0` | `1C72730 / 1871330` | `B285C0 / B286B0 / B2BCF0 / B34910` | `.?AVObserverView@Deployable@firefall@@` |
| `24` | `81B8B1 / C1C4B1` | `48` | `B34FC0` | `1C72700 / 1871300` | `B35130 / B35200 / B354B0 / B35C40` | `.?AVNPCObserverView@Deployable@firefall@@` |
| `25` | `81B8EC / C1C4EC` | `48` | `B35CC0` | `1C726C4 / 18712C4` | `B35DB0 / B35E50 / B36010 / B364C0` | `.?AVHardpointView@Deployable@firefall@@` |
| `27` | `81B927 / C1C527` | `128` | `B37030` | `1C72648 / 1871248` | `B37220 / B37290 / B37AA0 / B397C0` | `.?AVBaseController@Turret@firefall@@` |
| `28` | `81B965 / C1C565` | `D8` | `B39900` | `1C72618 / 1871218` | `B39AA0 / B39B60 / B3A450 / B3BFA0` | `.?AVObserverView@Turret@firefall@@` |

Each constructor calls the shared helper at raw `0xBCAD70`. It stores the
key's low/high DWORDs at receiver offsets `+0x18/+0x1C`, initializes fields
at `+0x08`, `+0x10`, and `+0x24` to zero, copies its first argument to `+0x20`,
and first writes the helper's unnamed vtable pointer at VA `0x1C660B4` to
offset `+0`. The derived constructor then replaces that pointer with the
listed RTTI-backed vtable. The six added COL offsets for selectors
`0x1B..0x21` are all zero, and no secondary vptr stores were observed in
those constructor bodies. The five constructors at selectors `0x23`, `0x24`,
`0x25`, `0x27`, and `0x28` also call this helper; their vtables map through
`.rdata`, their RTTI TypeDescriptors resolve through the PE section table, and
all five COL subobject offsets are zero.

These are the eight dispatch entries for selectors `0x0A..0x11`; the default
rows have no allocator or constructor:

| Selector | Table index | Entry raw | Target VA / raw | Result |
| --- | ---: | --- | --- | --- |
| `0A` | 8 | `81BAC8` | `C1BFE2 / 81B3E2` | `AIObserverView@Character` |
| `0B` | 9 | `81BACC` | `C1C01D / 81B41D` | `CombatView@Character` |
| `0C` | 10 | `81BAD0` | `C1C05B / 81B45B` | `MovementView@Character` |
| `0D` | 11 | `81BAD4` | `C1C099 / 81B499` | `TinyObjectView@Character` |
| `0E` | 12 | `81BAD8` | `C1C0D7 / 81B4D7` | `DynamicProjectileView@Character` |
| `0F` | 13 | `81BADC` | `C1C693 / 81BA93` | shared zero-return path |
| `10` | 14 | `81BAE0` | `C1C115 / 81B515` | `ObserverView@Melding` |
| `11` | 15 | `81BAE4` | `C1C693 / 81BA93` | shared zero-return path |

The bounded continuation for selectors `0x12..0x19` is:

| Selector | Table index | Entry raw | Target VA / raw | Result |
| --- | ---: | --- | --- | --- |
| `12` | 16 | `81BAE8` | `C1C153 / 81B553` | `MeldingBubble::ObserverView` |
| `13` | 17 | `81BAEC` | `C1C693 / 81BA93` | shared zero-return path |
| `14` | 18 | `81BAF0` | `C1C18E / 81B58E` | `AreaVisualData::ObserverView` |
| `15` | 19 | `81BAF4` | `C1C1CC / 81B5CC` | `AreaVisualData::ParticleEffectsView` |
| `16` | 20 | `81BAF8` | `C1C20A / 81B60A` | `AreaVisualData::MapMarkerView` |
| `17` | 21 | `81BAFC` | `C1C248 / 81B648` | `AreaVisualData::TinyObjectView` |
| `18` | 22 | `81BB00` | `C1C286 / 81B686` | `AreaVisualData::LootObjectView` |
| `19` | 23 | `81BB04` | `C1C2C4 / 81B6C4` | `AreaVisualData::ForceShieldView` |

The eight entries for selectors `0x1A..0x21` are:

| Selector | Table index | Entry raw | Target VA / raw | Result |
| --- | ---: | --- | --- | --- |
| `1A` | 24 | `81BB08` | `C1C693 / 81BA93` | shared zero-return path |
| `1B` | 25 | `81BB0C` | `C1C302 / 81B702` | `Vehicle::BaseController` |
| `1C` | 26 | `81BB10` | `C1C340 / 81B740` | `Vehicle::CombatController` |
| `1D` | 27 | `81BB14` | `C1C37E / 81B77E` | `Vehicle::ObserverView` |
| `1E` | 28 | `81BB18` | `C1C3BC / 81B7BC` | `Vehicle::CombatView` |
| `1F` | 29 | `81BB1C` | `C1C3FA / 81B7FA` | `Vehicle::MovementView` |
| `20` | 30 | `81BB20` | `C1C693 / 81BA93` | shared zero-return path |
| `21` | 31 | `81BB24` | `C1C438 / 81B838` | `Anchor::AIObserverView` |

The eight entries for selectors `0x22..0x29` are:

| Selector | Table index | Entry raw | Target VA / raw | Result |
| --- | ---: | --- | --- | --- |
| `22` | 32 | `81BB28` | `C1C693 / 81BA93` | shared zero-return path |
| `23` | 33 | `81BB2C` | `C1C473 / 81B873` | `Deployable::ObserverView` |
| `24` | 34 | `81BB30` | `C1C4B1 / 81B8B1` | `Deployable::NPCObserverView` |
| `25` | 35 | `81BB34` | `C1C4EC / 81B8EC` | `Deployable::HardpointView` |
| `26` | 36 | `81BB38` | `C1C693 / 81BA93` | shared zero-return path |
| `27` | 37 | `81BB3C` | `C1C527 / 81B927` | `Turret::BaseController` |
| `28` | 38 | `81BB40` | `C1C565 / 81B965` | `Turret::ObserverView` |
| `29` | 39 | `81BB44` | `C1C693 / 81BA93` | shared zero-return path |

Selectors `0x1B..0x1F` install five RTTI-named `Vehicle` receiver types;
selector `0x21` installs `Anchor::AIObserverView`. Each constructor calls
`0xBCAD70` and writes the two key DWORDs at receiver `+0x18/+0x1C`; the
constructor's initial helper vptr at VA `0x1C660B4` is replaced by the
primary vtable listed above. Section-aware `.rdata` mapping resolves each
vtable's Complete Object Locator and TypeDescriptor; all six COL subobject
offsets are zero. This bounded snapshot records the installed primary vptr
and does not claim a hierarchy-wide inventory of secondary subobjects.

This family assignment comes from the concrete constructor-installed vtables
and PE-resolved RTTI names, not from treating the low byte as a namespace ID.
The earlier six additions include five RTTI-named `Character` views and one
`Melding::ObserverView`; the current seven include `MeldingBubble::ObserverView`
and six `AreaVisualData` views; the selector `0x1A..0x21` entries add five
`Vehicle` classes and one `Anchor` class. The latest five add three
`Deployable` classes and two `Turret` classes. These are receiver types, not
parsed-message types. This evidence also corrects the provisional “message factory”
interpretation: these objects are registered against QWORD keys, not created
as per-message parser objects.

The receiver's primary slots are reached through the separate compact selector
at message `+0x10`: selector 1 calls `+0x08`; 2 calls `+0x18`; 3 and 4 call
`+0x0C`; 7 and 8 call `+0x04`; and 9 calls `+0x18`. Selectors 5 and 6 share
the direct helper at `0x800300` and do not call one of these primary slots.
The `+0x04` methods for `BaseController`, `NPCController`, `CombatController`,
and `ObserverView` dispatch message IDs from `message+0x10` over respective
observed ranges `0x53..0xC4`, `0x53..0xC4`, `0x56..0xA8`, and `0x5B..0xAB`,
subject to a class-specific condition. For BaseController and NPCController,
the condition is the non-null virtual-receiver pointer at `+0x2C`, not a
Boolean state flag; the corresponding fields in the other two classes are not
classified here. They first forward to an optional nested object at receiver
`+0x10`; the dispatcher tables themselves are not decoded here. The `+0x04`
methods for `MissionAndMarkerController`,
`LocalEffectsController`, `SpectatorController`, and `EquipmentView` forward
to that optional nested object and conditionally emit the fixed
"this view doesn't have any valid events" message, without a message-ID
argument. These forwarding/logging defaults are not empty stubs and differ
from the dispatchers' invalid-message-ID reporting.

For the newly mapped slots, `Vehicle::BaseController +0x04` handles exactly
message ID `0x5B` when a receiver at `+0x2C` is present; it first forwards to
an optional nested receiver at `+0x10`, and its invalid-ID path calls
`VIBE_LogUnhandledMessageId` with `message+0x10`. The `+0x08` compact-selector
body handler consumes byte data and a length, dispatches byte values through
`0xC1`, and logs values above that bound; individual payload cases remain
unmapped. Its `+0x0C` body handler accepts data and length and may forward to a
nested receiver at `+0x10`, but its large body is not exhaustively mapped. The
`Vehicle::CombatController +0x04` method first forwards to an optional nested
receiver at `+0x10`, then handles message IDs `0x53..0x5A` when a receiver at
`+0x2C` is present; its invalid-ID path calls `VIBE_LogUnhandledMessageId`
with `message+0x10`. `Vehicle::ObserverView`, `CombatView`, and `MovementView`
handle only IDs `0x55`, `0x57`, and `0x56`, respectively. Their invalid IDs
also use the ID-bearing logger; each valid path separately emits
`"received %s"` with `"msg-85"`, `"msg-87"`, or `"msg-86"` and may forward to
a receiver at `+0x2C`. By contrast, `Anchor::AIObserverView` forwards to an
optional nested receiver and may emit the fixed
`"this view doesn't have any valid events"` text through
`VIBE_LogEmitWithArgList`; that log call carries no message ID. The fixed
diagnostic is distinct from an invalid-ID report. These observations describe
the slot code, not message semantics inferred from the class labels.

For the newly mapped `Deployable` and `Turret` receivers, the vtable `+0x04`
targets were read from the installed vtables and traced as actual method bodies.
`Deployable::ObserverView` dispatches only IDs `0x54` and `0x55`, logs
`"received %s"` with `"msg-84"` / `"msg-85"` on those valid paths, and calls
`VIBE_LogUnhandledMessageId` with the supplied ID otherwise.
`Deployable::NPCObserverView` and `Turret::ObserverView` each handle only
`0x53`, using `"msg-83"` on the valid path and the same ID-bearing invalid
logger otherwise. These three methods first forward through an optional
receiver at `+0x10`; when the event receiver at `+0x2C` is present, valid IDs
are forwarded to it. `Deployable::HardpointView` and `Turret::BaseController`
instead forward to the optional `+0x10` receiver and may emit the fixed
`"this view doesn't have any valid events"` diagnostic when `+0x2C` is set;
their method bodies do not inspect a message ID. All five slot `+0x18` methods
store the `dword_1AC4D24` result at receiver `+0x28` and tail-call
`0xBCADF0`. The compact dispatcher routes selectors 1 and 3/4 through slots
`+0x08` and `+0x0C`, respectively; the five receivers' methods at those slots
are mapped by address, while their payload semantics remain outside this
bounded pass.

For selectors `0x0A..0x0C`, the `+0x04` implementations are message-ID
dispatchers: `AIObserverView` handles `0x69`, `CombatView` has a table for
`0x54..0x6C`, and `MovementView` handles `0x6E`; their invalid-ID branches
call `VIBE_LogUnhandledMessageId` with `message+0x10`. The `0x69` and `0x6E`
valid branches separately log `"received %s"` with fixed `"msg-105"` /
`"msg-110"` text, then may forward to an event receiver at `+0x2C`; each also
forwards to a separate optional receiver at `+0x10` before the ID gate.
The new `AreaVisualData::ObserverView`, `ParticleEffectsView`, and
`LootObjectView` `+0x04` methods dispatch only `0x54`, `0x55`, and `0x53`,
respectively; their invalid-ID branches also report `message+0x10` through
`VIBE_LogUnhandledMessageId`, while their valid paths may forward to the
receiver at `+0x2C`. By contrast, the `+0x04` methods for `TinyObjectView`,
`DynamicProjectileView`, `Melding::ObserverView`,
`MeldingBubble::ObserverView`, `AreaVisualData::MapMarkerView`,
`AreaVisualData::TinyObjectView`, and `AreaVisualData::ForceShieldView`
forward to optional nested receivers and conditionally call
`VIBE_LogEmitWithArgList` with the fixed text
`"this view doesn't have any valid events"` without a message-ID argument.
Across the 36 constructing receivers mapped through selector `0x35`, `+0x04`
has nineteen message-ID dispatchers and seventeen forwarding/logging defaults.
The fixed no-events
diagnostics are not invalid-ID reports or empty stubs.

All thirty-six mapped `+0x18` targets are short state-refresh wrappers: they call
the indirect helper at `dword_1AC4D24`, store its result at receiver `+0x28`,
and call `orchestrator_BCADF0`. They ignore payload arguments at this entry
point but are not no-op stubs. The `+0x08` and `+0x0C` targets are compact-selector
body handlers reached with data pointers and lengths; their individual payload
fields remain outside this bounded pass. The
`AIObserverView +0x08` target at `0x58A6E0` has a recovered disassembly
boundary (size `0x394`, ending in `retn 8` at `0x58AA71`); decompilation and
payload semantics remain unresolved. The earlier targets at `0x5536C0` and
`0x5795A0` have a function-boundary limitation, so none are classified
as empty stubs.

The exact table, constructor/vtable/RTTI snapshots, compact-selector call
sites, and original-image fingerprints are kept separately in
`MatrixKeyedReceiverFactoryNativeLayout.cs`,
`MatrixMessageNamespaceRoutingNativeLayout.cs`, and
`StartupBinaryEvidence.cs`. All selectors `0x02..0x35` are now bounded:
36 entries construct RTTI-backed receivers and sixteen table entries share the
zero-return target `0x81BA93` / VA `0xC1C693`.

Each case calls allocator raw `0x12B7FB0` / VA `0x16B8BB0`; its listed
constructor then calls the shared base-key helper raw `0xBCAD70` / VA
`0xFCB970`, which stores the full key at `+0x18/+0x1C`. The final four
constructing cases are:

| Selector | Table entry raw | Case raw / VA | Allocation | Constructor raw / VA | Primary vtable VA / raw | COL VA / raw | TypeDescriptor VA / raw | RTTI |
| --- | --- | --- | ---: | --- | --- | --- | --- | --- |
| `2D` | `81BB54` | `81B9A3 / C1C5A3` | `100` | `AF7300 / EF7F00` | `1C72278 / 1870E78` | `1CCA910 / 18C9510` | `1E29EC0 / 1A282C0` | `.?AVObserverView@Outpost@firefall@@` |
| `30` | `81BB60` | `81B9E1 / C1C5E1` | `11A0` | `B3C0F0 / F3CCF0` | `1C725E4 / 18711E4` | `1CCBA18 / 18CA618` | `1E2A3F0 / 1A287F0` | `.?AVObserverView@ResourceNode@firefall@@` |
| `33` | `81BB6C` | `81BA1F / C1C61F` | `1690` | `AFC100 / EFCD00` | `1C721E0 / 1870DE0` | `1CCA8A0 / 18C94A0` | `1E29EEC / 1A282EC` | `.?AVObserverView@CarryableObject@firefall@@` |
| `35` | `81BB74` | `81BA59 / C1C659` | `758` | `B43B10 / F44710` | `1C72574 / 1871174` | `1CCBA04 / 18CA604` | `1E2A3B4 / 1A287B4` | `.?AVLootObjectView@LootStoreExtension@firefall@@` |

Each derived constructor calls the shared base-key helper `0xBCAD70` before
installing its primary vtable. Text methods use raw+`0x400C00` for VA
conversion; vtable and RTTI addresses use the original PE `.rdata` section map.
The COL subobject offset is zero for each. The four `+0x04` methods are at raw
`AF7420`, `B3C320`, `AFC430`, and `B43CC0`: the first three forward through an
optional receiver at `+0x10` and conditionally emit the fixed
`"this view doesn't have any valid events"` diagnostic, without consuming a
message ID. The `0x35` method accepts exactly message ID `0x53`; invalid IDs
call `VIBE_LogUnhandledMessageId` with the value at `message+0x10`, while the
valid route may log `"received %s"` with `"msg-83"` and forward to a nested
receiver at `+0x2C`.

For these four receivers the `+0x08/+0x0C/+0x18` raw targets are, respectively,
`AF74C0/AF8DC0/AFBBF0`, `B3C3C0/B3EAB0/B43640`,
`AFC4D0/AFF670/B070E0`, and `B43D80/B44AD0/B47330`. The `+0x08/+0x0C`
targets receive compact-body data pointers and lengths; their payload fields
and consumer tails remain unresolved. Each `+0x18` target stores the
`dword_1AC4D24` result at receiver `+0x28` and calls `0xBCADF0`.

The following matrix is the complete 52-entry table summary. A zero-return
entry targets `0x81BA93`; every other selector constructs the RTTI type shown.

| Selector | Factory result | Selector | Factory result | Selector | Factory result | Selector | Factory result |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `02` | `Character::BaseController` | `03` | `Character::NPCController` | `04` | `Character::MissionAndMarkerController` | `05` | `Character::CombatController` |
| `06` | `Character::LocalEffectsController` | `07` | `Character::SpectatorController` | `08` | `Character::ObserverView` | `09` | `Character::EquipmentView` |
| `0A` | `Character::AIObserverView` | `0B` | `Character::CombatView` | `0C` | `Character::MovementView` | `0D` | `Character::TinyObjectView` |
| `0E` | `Character::DynamicProjectileView` | `0F` | zero | `10` | `Melding::ObserverView` | `11` | zero |
| `12` | `MeldingBubble::ObserverView` | `13` | zero | `14` | `AreaVisualData::ObserverView` | `15` | `AreaVisualData::ParticleEffectsView` |
| `16` | `AreaVisualData::MapMarkerView` | `17` | `AreaVisualData::TinyObjectView` | `18` | `AreaVisualData::LootObjectView` | `19` | `AreaVisualData::ForceShieldView` |
| `1A` | zero | `1B` | `Vehicle::BaseController` | `1C` | `Vehicle::CombatController` | `1D` | `Vehicle::ObserverView` |
| `1E` | `Vehicle::CombatView` | `1F` | `Vehicle::MovementView` | `20` | zero | `21` | `Anchor::AIObserverView` |
| `22` | zero | `23` | `Deployable::ObserverView` | `24` | `Deployable::NPCObserverView` | `25` | `Deployable::HardpointView` |
| `26` | zero | `27` | `Turret::BaseController` | `28` | `Turret::ObserverView` | `29` | zero |
| `2A` | zero | `2B` | zero | `2C` | zero | `2D` | `Outpost::ObserverView` |
| `2E` | zero | `2F` | zero | `30` | `ResourceNode::ObserverView` | `31` | zero |
| `32` | zero | `33` | `CarryableObject::ObserverView` | `34` | zero | `35` | `LootStoreExtension::LootObjectView` |

The `0xBD7060` fan-out is a separate conditional side path: `0x7FDC60` reaches
it when the message predicate at `message+0x18` returns zero and context
`+0xB0` is non-null; its selector is `3` for a zero callback argument and `2`
otherwise. `0xBD7060` serializes the message QWORD and message data into an
output stream in this order: raw eight bytes from message `+0x18/+0x1C`, a
compact positive UInt16 encoding of message `+0x10`, then the message vtable's
slot-`+0` serialization. It then passes the selector separately to
`0xBD7460`. The latter stores that selector in the recipient callback's local
header at `0xBD7482`; it is not serialized into the stream. That function
iterates 32-bit recipient pointers in its owner's vector `[+0x2C,+0x30)`
(capacity at `+0x34`), stepping four bytes at a time. For each item it
performs an indirect `__thiscall` through vtable slot `+0x0C`, passing the
output-buffer pointer separately from an eight-byte local header: DWORD
`owner[+0x20]` at header `+0`, WORD output length at `+4`, selector byte at
`+6`, and zero byte at `+7`. The length is narrowed to WORD when the header
is populated. The exact concrete type of every vector entry is not established.

A representative registration path is visible at raw `0x7FE1C0`: it lazily
allocates a `0x48`-byte state object, calls `0xBD6E10`, appends a recipient
pointer to the state's `+0x2C` vector with `0xBD8C90`, then calls the
recipient's vtable slot `+4`. The constructor copies the caller's pair at
`this+0x0C` to state `+0x18/+0x1C`, stores its QWORD argument at `+0x08/+0x0C`,
and zeroes vector fields `+0x2C/+0x30/+0x34`. A concrete recipient on this
observed insertion path is `nl::DefaultReplayWriter`: `0x7FE360` constructs it
at `0xBD7EA0`, whose final vtable VA `0x1C73ADC` has matching RTTI; its slot
`+0x0C` resolves to VA `0xFD9010` / raw `0xBD8410`. That method checks the
record/input lengths through `0x12A0660` and returns a Boolean. This is a
representative recipient of the observed `0x7FE1C0` insertion path, not proof
that every `0xBD7460` vector or the `0x7FDC60` context's `+0xB0` state uses
that class: no direct call from `0x7FDC60` to `0x7FE1C0` was found, and the
shared `+0xB0` offset alone does not establish object identity. The
`context+8` route to `0x81BB90` remains separate; its concrete
`tfGameProtocolHandler` recipient and selector-`0x53` slot mapping below do
not identify the fan-out recipient type or message namespace.

The concrete `tfGameProtocolHandler` has primary vtable VA `0x1C71EC0`, with
COL VA `0x1CCA404` and object offset zero. Its RTTI hierarchy contains
`tfGameProtocolHandler` and `MessageHandler` base descriptors, both at
subobject offset zero; no secondary vtable group was observed for this class.
On the conditional `0x81BB90` path, selector `0x53` indexes slot `+0xD0`,
target VA `0xECB180` / raw `0xACA580`. That method reads `message+0x34`.
Consequently, a `Vehicle::AbilityActivated` object of allocated size `0x30`
would have an apparent extent mismatch if it reached this slot; the unknown
envelope-key branch means that runtime path has not been established. This is
an unresolved conditional observation, not a claim that the parser routes
Vehicle messages to that method.

The original executable (`FirefallClient.exe.original`, 40,197,848 bytes,
SHA-256 `E0E8448393AC1CE05BE73E06F62DD70E3EFF3404CC2E7AAF3F04C79DEC0DE235`)
was read directly; `.text` VAs are raw offsets plus `0x400C00`, and `.rdata`
references are translated using its PE section table. No native execution,
IDB edits, or sample edits were performed. This resolves the static routing
conditions and parser-family collisions; it does not repair or reinterpret
the already-recorded field/extent conflicts.
