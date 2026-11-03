# Runtime contract and verification

The new source textures target 3×3 physical-pixel cells at 2560×1440. The game keeps its 1280×720 logical layout, text sizes, anchors, hit areas, and existing animation scales. `ArtTexture.Load` creates a full-resolution `ImageTexture` and applies `SetSizeOverride` so the texture reports the original logical dimensions without discarding its image detail.

A real Godot 4.7.2 OpenGL diagnostic drew a 2560×1440 3px checker with a 320×180 logical override and the existing 4× sprite scale. The screenshot matched the source exactly: zero differing RGB pixels. The full game captures use the same rendering path.

| Manifest category | Source multiplier | Runtime logical size | Reference draw scale |
| --- | ---: | --- | ---: |
| Sky, place, frame, shelf, counter, prop, figure | 8 | Original native size | 4 |
| Story scene | 8 | 640×360 | 2 |
| Sheet, lock face | 4 | Original native size | 2 |
| Item, Heartsong | 4 | Native alpha crop | 2 |
| Curio, upgrade/kind icon, emblem, key | 4 | Native size ×2 | 1 |
| Portrait / queue head | Derived 152×152 / 76×76 backing | 76×76 / 38×38 | 1 |

At the reference display size, every runtime backing image has exactly `logical_size × reference_scale × 2` physical pixels. Smaller/larger UI reuse intentionally changes the effective cell size. Head portraits retain fine facial detail; they are not resampled to a new 3px grid inside the tiny UI slots.

## Import and runtime files

- `tools/import_density.py` consumes completed records from `art/density3/catalog.json`, preserves the legacy baseline under `art/generated`, derives heads/unlit lanterns, and writes runtime textures under `assets`.
- `tools/import_art.py --density-only` performs only that overlay. `--density-check` validates without writing. A normal full art rebuild also applies the overlay after rebuilding legacy assets. Pending/retained catalog rows are skipped.
- `art/density3/runtime.json` and exported `assets/art_density.json` contain identical path/size/source-fingerprint mappings. `art/density3/import-report.json` records import counts and item crop adjustments.
- `scripts/Ui/ArtTexture.cs` owns texture loading, caching, logical sizes, and the package outline policy. Existing texture consumers in Ui/Phases use this helper; their coordinates and scales remain unchanged.
- `UiKit.cs`, `ShopView.cs`, `SettingsView.cs`, and `Settings.cs` implement the no-outline default and effective package policy. The package suppresses figure rings and the counter-edge stroke even if an older settings file enabled them. The settings row displays Off and is disabled for this style.
- `export_presets.cfg` includes the runtime metadata in exported resources.
- `scripts/Debug/DensityReview.cs`, dispatched from `Main.cs`, provides controlled review captures without trades, progression, or saved games.

The new sampling grid slightly crosses some old item trim boundaries. The importer expands those crop rectangles to whole logical units, preserving every opaque contour pixel and the exact physical pixel pitch. This affects 42 item/Heartsong sources and their 84 item/icon outputs, adding at most 2 logical pixels in width and 1 in height. It preserves 909 otherwise-cut physical pixels. Slot sizes and anchors do not change. All other runtime logical dimensions match the git baseline.

## Verification completed

- Final .NET build: 0 warnings, 0 errors.
- Final Godot asset import and full capture run: empty stderr logs, no runtime errors or warnings.
- Final density check and independent runtime audit: 242 source assets, 364 runtime textures, 0 pending sources, 0 errors. The audit checks backing dimensions, reference display scaling, current source hashes, unchanged logical geometry outside the recorded item crops, and complete item opacity.
- 66 distinct PNG screenshots, each exactly 2560×1440: all 38 figures with card/queue derivatives, all 16 shop/time combinations, 4 story scenes, 4 notebook spreads, 3 locks, and upgrade cards.
- `capture-index.json` records each capture's ID, Chinese title, category, relative filename, dimensions, optional figure ID, and SHA-256. The offline gallery contains the exact captured PNGs.
- Both face reviewers examined final head contact sheets at the actual 152px/76px display sizes. Facial structure survives the smaller slots; Skrae's existing ear-tip framing is recorded below.
- Tests used isolated APPDATA/LOCALAPPDATA. No `save.json` or `profile.json` was created, and no real player save was accessed. No new executable/exported playtest package was made.

The earlier six-screen smoke run logged two ObjectDB instances at shutdown; the final 66-screen run did not repeat that warning. A cold initial editor import reported the missing font cache before building it; final import/runtime logs are clean.

## Reproduction

Run from the repository root with Python/Pillow, Godot 4.7.2 .NET, and .NET 8. Use a new screenshot directory on each run: the harness refuses to overwrite existing captures. On Windows, use an ASCII SDK path or the existing local tool junction when the .NET SDK otherwise lives under a non-ASCII path.

```powershell
python tools/import_art.py --density-check
python tools/import_art.py --density-only
$env:DOTNET_ROOT = 'PATH/TO/ASCII/DOTNET8'
$env:PATH = $env:DOTNET_ROOT + ';' + $env:PATH
$env:APPDATA = 'PATH/TO/ISOLATED/TEST-DATA/Roaming'
$env:LOCALAPPDATA = 'PATH/TO/ISOLATED/TEST-DATA/Local'
$densityGodot = 'PATH/TO/Godot_v4.7.2-stable_mono_win64_console.exe'
$densityShots = 'PATH/TO/NEW_CAPTURE_DIRECTORY'
dotnet build --nologo
& $densityGodot --headless --path . --editor --import
& $densityGodot --path . -- --density-review "--shots=$densityShots"
```

`--density-review=smoke` captures one representative from each consumer group. The actual automated runs used hidden background processes with separate stdout/stderr logs. The external integration record includes `audit_runtime.py`, `finalize_captures.py`, and `make_head_contacts.py` for the independent contract audit, hash index, and native-size head sheets, plus their reports and logs.

## Known limits

- Exact 3px cells are calibrated for 2560×1440 at the reference draw sizes. Other display resolutions, window sizes, and intentional UI rescaling change displayed cell sizes.
- Legacy typography, procedural chest/effect sprites, and the 25 retained manifest assets keep their existing density. This change adds no new character poses or animations.
- Skrae's full figure retains both complete ears. Its card/queue portraits use the exact existing native crop `[29, 48, 67, 86]`, which clips the wide ear tips in both the baseline and new art. This is retained portrait framing, not a new density regression. The external `skrae_legacy_crop_comparison.png` and `skrae_crop_audit.json` document the comparison.
- The existing Master Restorer description can wrap below its fixed-height upgrade card in the tier-4 fixture. Fonts, card geometry, and the icon's logical size are unchanged; this baseline text-layout issue was left outside the density integration scope.
- Gerda's long display name extends past the fixed speech-panel edge in her review fixture. The existing name font and panel width are unchanged; the figure face and both portraits remain unobscured.
- The fixtures verify presentation and asset consumption, not balance, multi-day progression, exported packaging, or antivirus behavior.
- Source PNGs remain under `art/density3/assets`; exported runtime consumes `assets` and its metadata. Original `art/generated` files remain available as the legacy baseline.

The owner approved the complete reviewed package on2026-11-04 with “都通过，上传资产”. `approval.json` binds that approval to the exact asset, capture, and runtime-texture hashes. The publication target is `art/round2`.
