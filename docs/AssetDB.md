# AssetDB (partial, static)

## Observed
- 0x1318400 (`__thiscall`) logs the AssetDB search paths. Output is a banner of asterisks, "Search paths in order of access", "Ordinary search paths" (list at this+8), then "AssetDB search paths" (list at this+16), each printed as `%2i) %s`. Lists are circular doubly-linked, with the head sentinel at the list pointer. It also takes a lock at this+24.
- The singleton is `dword_1DFFF5C`. 0x13178B0 and 0x1319520 are called on it from startup (see round 3 and 4).
- Generic file open candidate: 0x12BA030 (also used by the LMTD pak loader).

## Interpreted
- The object keeps two ordered search-path lists, "ordinary" (loose files) and "AssetDB" (packed database), consulted in order.

## Unresolved
- The asset DB file format, the code that populates the two lists, and the name-to-file open path. Not decompiled this round.