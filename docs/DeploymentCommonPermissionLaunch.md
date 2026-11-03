# DeploymentCommon permissions launch boundary

Static, read-only reconstruction of Utils.ResetInstallFolderPermissions
in the identified DeploymentCommon.dll, MethodDef `0x0600012F`,
body RVA `0x6E18` (file offset `0x5018`). Sample size76800,
SHA256 `33848E8CA80DFAD7B6883D5A57FC55834411E574CEE6467C753B77AD4523DAE0`.
No process was started, ACL read, or permission changed during analysis.

The method constructs a Process and a ProcessStartInfo. Its executable
is Path.Combine(Environment.GetFolderPath((SpecialFolder)37), "icacls.exe"):
it selects the Windows System folder, rather than looking up an arbitrary
PATH executable. Arguments are String.Format of the literal
`"{0}" /RESET /T /Q /C` with its input directory.

Explicit properties and subsequent observations:

| IL offset | Observation |
|---|---|
| `0x002A..002B` | CreateNoWindow=true |
| `0x0031..0032` | ErrorDialog=false |
| `0x0038..0039` | UseShellExecute=false |
| `0x0047..004C` | Process.Start() then pop its Boolean return |
| `0x004D` onward | No WaitForExit or ExitCode read |

This is a launch attempt, **not verification that permissions were reset**.
It neither waits for the child nor checks its eventual exit code.
The directory is surrounded by quotes but the method does not separately
escape quotes or sanitize the supplied path.

Its System.Exception catch covers IL `[0,79)` (construction through
launch), with handler `[79,129)`. The handler combines the exception
message with `Failed to set permissions: ` and calls Logger.LogLine,
then Logger.LogPersistentEvent with category `Permissions`.
No catch can observe a later nonzero child exit, because the method
does not wait or read that exit status. Both paths return void.

`src\DeploymentCommon\PermissionLaunchNativeLayout.cs` saves a pure
descriptor and the observed flags. It performs no process, filesystem,
registry, or permission operations. CompanionTools verifies the complete
IL, literal user strings, catch bounds/type, and synthetic descriptors.
It does not execute this descriptor or treat it as successful ACL repair.
