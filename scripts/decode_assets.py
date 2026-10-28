#!/usr/bin/env python3
"""Decode base64 theme assets into Wallpaper/ and Assets/."""
from __future__ import annotations

import base64
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / "themes" / "Desktop-01" / "encoded" / "manifest.json"


def main() -> int:
    if not MANIFEST.exists():
        print("manifest not found:", MANIFEST)
        return 1

    items = json.loads(MANIFEST.read_text(encoding="utf-8"))
    for item in items:
        enc = ROOT / item["encoded"]
        target = ROOT / item["target"]
        if not enc.exists():
            print("missing:", enc)
            continue
        target.parent.mkdir(parents=True, exist_ok=True)
        data = base64.b64decode(enc.read_text(encoding="ascii"))
        target.write_bytes(data)
        print(f"wrote {target} ({len(data)} bytes)")

    studio = ROOT / "themes/Desktop-01/Wallpaper/milkbean-main-studio.jpg"
    if studio.exists():
        for name in (
            "milkbean-main-1920x1080.jpg",
            "milkbean-main-2560x1440.jpg",
            "milkbean-main-3840x2160.jpg",
        ):
            dest = ROOT / "themes/Desktop-01/Wallpaper" / name
            dest.write_bytes(studio.read_bytes())
            print(f"alias {dest.name}")

    print("done")
    return 0


if __name__ == "__main__":
    sys.exit(main())
