# DeploymentCommon WOW64/OS-bitness detection

Static, read-only analysis of the identified DeploymentCommon.dll
(76800 bytes, SHA256
`33848E8CA80DFAD7B6883D5A57FC55834411E574CEE6467C753B77AD4523DAE0`).
The assembly was not loaded or executed. Its `.text` RVA `0x2000` maps
to file offset `0x200`; addresses below are managed body RVAs or file
offsets, not the original client's raw IDB addresses.

| Method | MethodDef token | Body RVA / file offset | IL file offset / length |
|---|---|---|---|
| `Wow.get_Is64BitProcess` | `0x06000053` | `0x2F60 / 0x1160` | `0x1161 / 9` |
| `Wow.get_Is64BitOperatingSystem` | `0x06000054` | `0x2F6C / 0x116C` | `0x1178 / 44` |
| `Wow.ModuleContainsFunction` | `0x06000055` | `0x2FA4 / 0x11A4` | `0x11B0 / 40` |

The first getter compares `IntPtr.Size` with8. The OS getter returns true
immediately for that result. Otherwise it checks whether the loaded
kernel32.dll exports IsWow64Process. It returns false if the export is
absent, or if IsWow64Process(GetCurrentProcess(), out wow64) fails;
only a successful call's out flag is returned. Therefore a false result
can mean detection failed, not solely that the OS is32-bit.

The availability helper calls GetModuleHandle, checks against IntPtr.Zero,
then GetProcAddress and checks against IntPtr.Zero. It does not call
LoadLibrary or attempt to repair a missing module/export. This also shows
why the COFF I386 header alone cannot establish this managed assembly's
runtime process bitness.

## P/Invoke metadata

All four imports below reference kernel32.dll. Signatures are raw
MethodDef signature-blob contents; import flags are ImplMap field values,
not physical offsets.

| Token | Import | Signature bytes | Import flags |
|---|---|---|---|
| `0x06000056` | IsWow64Process | `00 02 02 18 10 02` | `0x0140` |
| `0x06000057` | GetCurrentProcess | `00 00 18` | `0x0146` |
| `0x06000058` | GetModuleHandle | `00 01 18 0E` | `0x0106` |
| `0x06000059` | GetProcAddress | `00 02 18 18 0E` | `0x0142` |

IsWow64Process returns bool, takes a native-int handle and a bool by-ref;
parameter2 is marked Out. Its flags specify WinApi calling convention
and SetLastError. The GetCurrentProcess/GetModuleHandle rows carry the
CharSetMask value (`0x0006`); GetProcAddress specifies ANSI (`0x0002`).

`src\DeploymentCommon\WowNativeLayout.cs` preserves these identities
and pure Boolean decisions without adding actual P/Invoke calls.
Companion checks compare all three complete IL byte sequences after
the sample's identity guard and exercise the Boolean input combinations.
No runtime probe, credential access, assembly loading, or network activity
was necessary. NativeMethods.ClientToScreen/MoveWindow and
Utils.SHGetSpecialFolderPath remain independent follow-on traces.
