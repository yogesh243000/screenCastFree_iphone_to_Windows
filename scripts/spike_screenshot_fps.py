"""
Throwaway Milestone-4 spike: measure the real, sustained frame rate of looping
the DVT single-shot screenshot service over an already-established no-root
userspace RSD tunnel (the only capture path available on this device until
iOS 27 unlocks the continuous HEVC media-stream service).

Not part of the shipped product - just answers "what FPS can we actually get."
"""

import asyncio
import time

from pymobiledevice3.remote.userspace_tunnel import establish_userspace_rsd
from pymobiledevice3.services.dvt.instruments.dvt_provider import DvtProvider
from pymobiledevice3.services.dvt.instruments.screenshot import Screenshot

FRAME_COUNT = 15


async def main() -> None:
    print("Establishing no-root userspace RSD tunnel...")
    t0 = time.perf_counter()
    rsd = await establish_userspace_rsd()
    print(f"Tunnel established in {time.perf_counter() - t0:.2f}s")

    async with DvtProvider(rsd) as dvt, Screenshot(dvt) as screenshot:
        print("Warming up (first frame, includes DVT channel setup)...")
        t_warm = time.perf_counter()
        first = await screenshot.get_screenshot()
        print(f"  first frame: {time.perf_counter() - t_warm:.3f}s, {len(first)} bytes")

        print(f"Capturing {FRAME_COUNT} frames back-to-back...")
        timings = []
        sizes = []
        for i in range(FRAME_COUNT):
            t1 = time.perf_counter()
            data = await screenshot.get_screenshot()
            dt = time.perf_counter() - t1
            timings.append(dt)
            sizes.append(len(data))
            print(f"  frame {i + 1:2d}: {dt * 1000:7.1f} ms, {len(data):>9,} bytes")

        avg = sum(timings) / len(timings)
        fps = 1.0 / avg
        print()
        print(f"Average per-frame time: {avg * 1000:.1f} ms")
        print(f"Sustained rate:         {fps:.2f} FPS")
        print(f"Average frame size:     {sum(sizes) / len(sizes):,.0f} bytes")


if __name__ == "__main__":
    asyncio.run(main())
