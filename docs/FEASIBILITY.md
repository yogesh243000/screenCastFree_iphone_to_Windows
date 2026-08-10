# Feasibility Report

This documents what was actually verified on real hardware (a Lightning/USB-C
iPhone 16 Pro Max, `iPhone17,2`), not just researched. Update this file
whenever a new device/iOS version changes any of these findings.

## Bottom line

There is no public Apple API for a third-party Windows app to pull an
iPhone's screen over USB. Every working approach — including Apple's own
QuickTime/Xcode mirroring — relies on protocols the community had to
reverse-engineer. We chose the path built on **pymobiledevice3** (GPL-3.0,
invoked as an external process, never linked in-process, so its license does
not apply to this codebase).

## What's confirmed working today (iOS 26.5.2)

| Capability | Mechanism | Status |
|---|---|---|
| USB device detection | `pymobiledevice3 usbmux list` | Works, wired into `iPhoneMirror.USB` |
| Pairing / trust | Standard "Trust This Computer" + usbmux pairing record | Works (no code needed - it's the OS-level dialog) |
| Developer Mode enable | `amfi enable-developer-mode` | Only works on a passcode-free device. With a passcode set (normal case), it must be toggled manually on-device: Settings -> Privacy & Security -> Developer Mode. If the toggle isn't visible yet, run `amfi reveal-developer-mode` first (mirrors what Xcode does on first launch) |
| Developer Disk Image mount | `mounter auto-mount` | Works, but makes one network call to Apple's TSS server to fetch a device-personalized image. Needs internet at least once per iOS version |
| RSD tunnel (needed for all `developer`/`core-device` commands on iOS 17+) | No-root **userspace** tunnel (pure-Python `pytcp` stack, see `remote/userspace_tunnel.py`) | **Works without admin, without Bonjour.** This is the path actually used automatically by `developer`/`dvt`/`core-device` commands |
| RSD tunnel via `remote start-tunnel` / `remote pair` | Kernel tunnel + Bonjour/mDNS discovery | **Does not work on Windows.** Traced to `browse_remoted()` doing a real mDNS browse for a network interface that macOS's Apple USB driver creates over the cable and Windows' driver does not. Installing Apple's Bonjour service (`winget install Apple.Bonjour`) does not fix this - there is no network path to browse on. Avoid this code path entirely; use the userspace tunnel instead |
| Single-shot screenshot (`developer dvt screenshot`) | DVT Instruments `com.apple.instruments.server.services.screenshot` channel | **Works**, ~2-3 FPS sustained if looped (435ms/frame average, measured; full-res 16-bit PNGs, several MB each) - unusable for mirroring, fine for a manual screenshot feature |
| Continuous HEVC screen stream (`developer core-device display serve-web` / `serve-vnc` / `start-video-stream`) | `com.apple.coredevice.feature.startmediastream`, reverse-engineered byte-exact from Xcode's own Mirror feature (RTP/HEVC depacketization, RTCP rate-control feedback, stall recovery, HID input injection - see `remote/core_device/screen_stream.py`) | **Blocked on this device.** Device returns: `"Remote control requires iOS 27.0 or later on this device."` iOS 27 was not yet available as of this test. This is an Apple-side version gate, not a bug in our code or in pymobiledevice3 |

## Prerequisites checklist (fresh machine)

1. .NET 10 SDK (LTS) - `winget install Microsoft.DotNet.SDK.8` or newer
2. Python 3.12 specifically - `winget install Python.Python.3.12` (3.14 fails to
   build pymobiledevice3's `lzfse`/`pylzss` C extensions; no prebuilt wheels
   exist yet for such a new interpreter, and building them needs a C++
   compiler we don't otherwise require)
3. Apple Mobile Device Support - installed via iTunes or the "Apple Devices"
   Microsoft Store app. Provides the Windows USB driver
4. Run `scripts/setup-pmd3-venv.ps1` to create the pymobiledevice3 venv
5. On the iPhone: enable Developer Mode (Settings -> Privacy & Security ->
   Developer Mode). If it's not listed, the app needs to run
   `amfi reveal-developer-mode` first
6. First run needs internet once, to fetch the personalized Developer Disk
   Image from Apple

Bonjour and Administrator elevation are **not** required for anything that
currently works - only for the broken `remote start-tunnel` path, which
should not be used.

## Planned architecture for the video pipeline (blocked on iOS 27, but built against the known protocol)

`serve-web` already implements everything: it opens `DisplayService`, starts
the device's own HEVC encoder via `start_video_stream`, and serves the
resulting access units over HTTP at `/stream.bin` as
`[4-byte length][1-byte type][length-prefixed HEVC NALUs]`. Rather than
re-implement RTP/HEVC depacketization and Xcode's reverse-engineered
rate-control feedback loop ourselves, `iPhoneMirror.Video` will:

1. Launch `pymobiledevice3 developer core-device display serve-web --bind
   127.0.0.1 --http-port <port>` as a subprocess (same external-process
   pattern as device discovery).
2. Connect to `http://127.0.0.1:<port>/stream.bin` with `HttpClient` and
   parse the same framing.
3. Feed the HEVC NALUs to Windows Media Foundation's hardware HEVC decoder
   in-process (no ffmpeg in the hot path - lower latency, one fewer process
   hop) for the live preview.

This cannot be validated end-to-end until the test device is on iOS 27+.

## Licensing notes

- pymobiledevice3: GPL-3.0. Invoked as a subprocess only - never referenced
  as a library from C#. This keeps the GPL from applying to this repository.
- Bonjour is an optional Apple-signed Windows service; not currently a
  runtime dependency (see table above).
