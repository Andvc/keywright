# Round 2 in the game — complete

2026-11-01. C2 / 4x / meaningful detail / no outlines, following the owner's final correction and authorization
to complete the remaining art and integration. All 242 redraws are delivered and used by the game. The full
267-asset manifest passes strict validation with no warnings.

## Actual game captures

These are 1280x720 captures from the current game, not painted mock-ups. They use controlled in-memory fixtures,
so the deliberately repeated waiting visitors and supplied inventory are review data. No run was played or saved.
All twelve were visually inspected, along with the complete head-crop sheet.

| Capture | What to inspect |
|---|---|
| [1. Stall, morning](round2_integration/art_01_stall_morning_short.png) | Tam's short figure, face below the bubble, paper and counter alignment |
| [2. Thornwick, midday](round2_integration/art_02_thornwick_midday.png) | Hob, shelf goods, full daylight and dark ink on the second sheet |
| [3. Crownhold, evening](round2_integration/art_03_crownhold_evening_tall.png) | Orrin's tall figure, warm tint, lit lanterns and readable appraisal |
| [4. Emporium, all tools](round2_integration/art_04_emporium_imp_tools.png) | Vexis on the counter, nine Heartsongs, seven props, marked values and Bram's tip |
| [5. Nim, morning](round2_integration/art_05_nim_morning.png) | Full figure, unlit lanterns, three curio cards and their icons |
| [6. Vane, night](round2_integration/art_06_vane_night.png) | Night palette, lantern glow, five-week track and three-of-four key progress |
| [7. Story figures](round2_integration/art_07_story_figures.png) | Hood and Bram at 4x, speaking emphasis, background and dialogue placement |
| [8. Notebook](round2_integration/art_08_notebook_heads.png) | Head crops in rows and Orrin's larger character card |
| [9. Rusty lock](round2_integration/art_09_lock_1.png) | Rust patches and a plain track behind the pick and timing zone |
| [10. Guild lock](round2_integration/art_10_lock_2.png) | Gold eye and plain track, readable pin indicators |
| [11. Dwarven lock](round2_integration/art_11_lock_3.png) | Bronze knot, thin outer rim and clear four-pin presentation |
| [12. Upgrade cards](round2_integration/art_12_upgrade_cards.png) | Imported upgrade icons and prices in the end-of-day interface |
| [All 38 head crops](round2_integration/heads.png) | Identity, hats, horns and shoulders at the actual 76px card size |

The separate [batch-15 guide](round2_batch15_README.md) and validator's `counter_*_times.png` sheets cover all
four hours at every shop; earlier batch guides record the individual assets and candidate decisions.

## Integration details

- `ShopView` composes native layers at exactly 4x, with nearest filtering. Morning, midday and evening follow
  customer thirds; changing customers crossfades the sky and tint over 0.7 seconds. The Guild uses night lighting.
- Full figures keep their common baseline and individual heights. Short folk are lifted behind the counter;
  Vexis stands in front of it. Speech and interface text retain their own lighting.
- Sheet art is 2x with dark ink and filled count boxes. Tool-button pictures no longer enlarge the buttons over
  the totals. The Guild's weekly boxes leave enough width for the key.
- Shelf goods stand on the board at screen row 196. Nine Heartsongs fit between the speech bubble and paper;
  the full row uses native 1x object pixels, while smaller rows use 2x. No fractional raster filtering is used.
- Story scenes show full figures. The 38px queue and 76px card/tip crops come from the same full-figure sources,
  using nearest-neighbour resampling. `assets/figures/layout.json` is included in exported builds.
- The 0.25 code-drawn chest, retained UI, effects and branding keep their specified role. The no-outline correction
  applies to the 242 round-2 redraws. No gameplay or economy rules were changed.

## Verification and replay

| Check | Result |
|---|---|
| Final strict art validation | 267 OK; no failures, missing assets or warnings |
| Runtime import audit | 242 redraws; 364 exact RGBA comparisons; 38 figure layouts; all pass |
| .NET build | 0 warnings, 0 errors |
| Godot headless import | Completed without errors or warnings |
| Source-project visual fixture | All 12 captures complete; no stderr; visually reviewed |
| Windows release export | Completed; executable and .NET data folder present; no logged errors or warnings |
| Exported-program launch | Same 12-capture fixture complete; no stderr; crowded Emporium capture reviewed |

The [asset audit](../_workflow/round2_integration_assets.json) lists each destination and its source hash.
The [verification record](../_workflow/round2_integration_verification.json) hashes the code, logs, screenshots
and local export. Full logs and the local Windows build are archived outside git at
`../records/integration_c2_no_outline/` relative to the repository root.

From a configured checkout with Python/Pillow, .NET 8 and Godot 4.7.2 .NET:

```sh
python3 tools/import_art.py --cache <kenney-cache>
python3 art/generated/_workflow/round2_integration_check.py
dotnet build Keywright.csproj
godot --headless --path . --import
godot --path . --rendering-method gl_compatibility --audio-driver Dummy --windowed --resolution 1280x720 -- --art-review --shots=<new-absolute-directory>
```

Use a fresh screenshot directory: `Main.Moment` preserves existing PNGs. Run the visual fixture with isolated
`APPDATA` and `LOCALAPPDATA`, as done for this delivery. It exits after twelve review captures; it does not write
a saved run. On this Windows workspace, `DOTNET_ROOT` and `PATH` point through the existing `.build-tools/dotnet`
junction so Godot can find the SDK reliably.

All authorized work is complete. Stop after pushing the final integration.
