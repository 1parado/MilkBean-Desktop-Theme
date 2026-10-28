# Theme visual assets

## Wallpapers (generate / download then place here)

| File | Description |
| --- | --- |
| `Wallpaper/milkbean-main-studio.jpg` | Soft cream studio, character on the right |
| `Wallpaper/milkbean-main-desk.jpg` | Desk scene with plant and notebook |
| `Wallpaper/milkbean-main-minimal.jpg` | Minimal floating character |
| `Wallpaper/milkbean-main-1920x1080.jpg` | Alias / export for 1080p |
| `Wallpaper/milkbean-main-2560x1440.jpg` | Alias / export for 1440p |
| `Wallpaper/milkbean-main-3840x2160.jpg` | Alias / export for 4K |

## Character

| File | Description |
| --- | --- |
| `Assets/milkbean-character.png` | Original MilkBean reference |

## Why binaries may be missing in git

Remote pushes from the cloud agent use a **text-only** GitHub API path, so JPEG/PNG cannot be uploaded reliably from that environment.

### Add assets on your machine

```powershell
cd MilkBean-Desktop-Theme

# Put downloaded wallpapers into:
#   themes/Desktop-01/Wallpaper/
# Put character png into:
#   themes/Desktop-01/Assets/milkbean-character.png

git add themes/Desktop-01/Wallpaper/*.jpg
git add themes/Desktop-01/Assets/milkbean-character.png
git commit -m "assets: MilkBean wallpapers and character"
git push
```

After files exist locally you can also run:

```powershell
python scripts/decode_assets.py
```

if you use the optional base64 `encoded/` pipeline.
