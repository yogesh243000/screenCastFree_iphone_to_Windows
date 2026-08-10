# Troubleshooting

## "No iPhone detected"

- Unlock your iPhone
- Connect using a USB data cable (not a charge-only cable)
- Tap "Trust" if prompted on the iPhone
- Make sure Apple Mobile Device Support is installed (comes with iTunes or
  the "Apple Devices" app from the Microsoft Store)

## "Connection lost" / pairing stuck on "waiting for trust"

Click Connect again after responding to the Trust dialog and passcode
prompt on the iPhone. If it keeps timing out, unlock the phone and make
sure no other tool (Xcode, Finder, another copy of this app) is already
using the USB connection.

## Manual test procedure: fresh pairing (not covered by automated tests)

The automated tests cover every pairing outcome (`Paired`,
`WaitingForUserTrust`, `NoDeviceConnected`, `Failed`) against fakes, but the
*specific* case of a completely fresh device that has never trusted this
computer - where iOS actually blocks on the on-screen Trust dialog until a
human taps it - can't be exercised in CI. To test it manually:

1. `python -m pymobiledevice3 lockdown unpair` (removes the pairing record)
2. Lock the iPhone, then open the app and click Connect
3. Confirm the app shows the pairing guidance and does not hang past the
   20s timeout - it should return to "waiting for trust" with a retry option
4. Unlock the iPhone, tap Trust when it appears, enter the passcode
5. Click Connect again and confirm it reaches "iPhone connected"

Note observed during development: on this test device, re-pairing shortly
after an unpair sometimes completed without the Trust dialog reappearing at
all (iOS appears to grant a brief grace period). To reliably force the full
dialog, wait a minute or reboot the device between steps 1 and 2.

## Continuous screen mirroring isn't available

As of this writing, continuous screen streaming requires **iOS 27 or
later** on the iPhone (Apple-side restriction, confirmed by the device's own
error message: "Remote control requires iOS 27.0 or later on this device").
Check Settings -> General -> Software Update. See `docs/FEASIBILITY.md` for
the full investigation.

## Guaranteed-reliable fallback: HDMI capture

If you need a stream to work tonight regardless of software issues, a
Lightning/USB-C-to-HDMI adapter plus any HDMI capture card lets OBS capture
the iPhone screen directly (Window/Video Capture Device source) with no
reverse-engineered software in the path at all. This bypasses this app
entirely for the video path.
