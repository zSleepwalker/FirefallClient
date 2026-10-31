# DumpTruck.exe

Static IDA analysis only. SHA256 `53796D440D586A306C36ADAE86744C580B9F9ECD3987D2C0D3CAB0EA77ADD6E4`, 16,675,032 bytes, x86 GUI, PE stamp 2016-05-05. Reconstruction: `src\DumpTruck`.

## Observed
- Init/options `sub_404BD0`: `nofear debug userlangid forge dump log auth app msg` (plus `url autosubmit debuguser nodelete dump-config` strings). Loads string-table resources, creates a dialog (resource 0x853).
- Submit flow `sub_40A3F0`: logs `Submit: dumpfile/logfile`, compresses the core dump, the log file and `awesomium.log`, runs `HWStats.exe` through a pipe (`sub_42AB90`) and compresses its output, optionally writes a "poo" file (`save_poo`).
- `sub_409A60`: POSTs `application/xml` `<report-crash><auth>%s</auth></report-crash>` to the `--url` value (fallback `http://crash.firefall.com/crash_report.xml`); response must contain url, date, auth tags.
- Upload `PUT/POST %s/%s` with an `Authorization` header (S3-style), then `sub_40B100` posts `<confirm-upload><path/><date/><awsauth/><auth/></confirm-upload>`.
- Request signing strings: `ver=%d&tc=%u&nonce=...&uid=%s&host=%s&path=%s&hbody=%s&cid=%llu`, `Red5 %s %s %s`, header `X-Red5-Signature`, salts `red5salt-...` (see binary). Hosts `.r5internal/.r5external/.r5test .com/.net`.
- Own death handler writes `%s%s.dmp` via dbghelp_red5; embeds pugixml, zlib 1.2.8, slLog framework, DPAPI.

## Interpreted
- Crash-report uploader launched by RedHanded; shows UI unless `--autosubmit`.

## Unresolved
- Meaning of `forge`, `debuguser`, `dump-config`, `nodelete`; exact Authorization/signature derivation; the HTTP server's real hostname beyond the hardcoded fallback.
