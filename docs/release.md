# Build and publish Level Cap Remover

From the repository root, with the .NET 8 SDK and Python 3 on `PATH`:

```sh
python3 scripts/package.py
```

The script restores the pinned build references, compiles in Release mode, and writes `dist/Level_Cap_Remover-<version>.zip`. Only the plugin DLL, README, manifest, and 256×256 icon are packed. To build just the DLL, use `dotnet build src/LevelCapRemover.csproj -c Release`.

## Test the ZIP

1. Make a fresh Risk of Rain 2 profile in Gale or r2modman. Install the BepInEx packs listed in `thunderstore/manifest.json`, then import the **local ZIP**. Launch modded and check `BepInEx/LogOutput.log` for `Removed the level-99 ambient cap.`
2. Play a standard run. For a quick A/B check, add [DebugToolkit](https://thunderstore.io/c/riskofrain2/p/IHarbHD/DebugToolkit/) to the test profile. In a fresh solo run, use `run_set_stages_cleared 26` in the console. The enemy level on the HUD should exceed 99. Raise the stage count again and check that the level keeps rising. At lower stage counts it should match vanilla at the same time and difficulty.
3. As a control, disable Level Cap Remover, restart, and repeat: the enemy level should stop at 99. Do not use `team_set_level` to test this; team XP is a different mechanic.
4. For multiplayer, repeat with matching installs. Check the **remote client's** HUD as well as the host's. For an enemy-stat check, compare the max health of the same newly spawned non-elite monster at the same time and stage count across the two setups. Use identical difficulty, items and artifacts.

## Upload

- For a new release, set the same three-part version in `src/LevelCapRemover.csproj` and `thunderstore/manifest.json`. Rebuild and test that ZIP. Set `website_url` to your public source or issue tracker when available; empty is valid.
- On [Thunderstore's upload form](https://thunderstore.io/package/create/), pick your publishing team, **Risk of Rain 2**, and the **Mods** and **AI Generated** categories. The team name is selected on upload, not in the manifest. Upload the ZIP without a wrapping folder.
- Keep the README and assembly AI disclosures. See [source notes and AI policy links](modding-notes.md) for the RoR2-specific rule and similar mods.

## Game build checked

On Steam build `25921972` (RoR2 `1.5.0#1224`), `Run.RecalculateDifficultyCoefficentInternal` clamps ambient level with the private static `int Run.ambientLevelCap`, then floors it to an `int`. `CharacterBody.RecalculateStats` uses that whole level for bodies flagged to use ambient scaling or holding the game's ambient-level item. `AmbientLevelDisplay` reads the same whole level. `InfiniteTowerRun` overrides the calculation with a separate `9999f` constant.

The plugin sets the standard-run field once when BepInEx loads it. If it cannot find the field or discovers a non-vanilla value, it logs the reason and leaves the game alone. A mod that rewrites the cap later can still override it.

**Historical reference:** Moffein's [Raise Monster Level Cap](https://thunderstore.io/c/riskofrain2/p/Moffein/Raise_Monster_Level_Cap/) launched in 2021. Its [1.0.8 implementation](https://github.com/Moffein/RaiseMonsterLevelCap/blob/42b39dddd50d9382393dffdb29dc8ab429219066/RaiseMonsterLevelCap/Class1.cs#L25-L51) both wrote `Run.ambientLevelCap` and replaced the read of that field inside the difficulty calculation with its configurable float (default 9,999). It separately suppressed the ambient level-up effect above a configurable threshold (default 300). On the checked build, modifying the field alone suffices, as the A/B results below show. Keeping the standard calculation also preserves vanilla level-up effects. When playtesting co-op, check a remote client's **enemy health bars** above level 99: the [old mod's README](https://thunderstore.io/c/riskofrain2/p/Moffein/Raise_Monster_Level_Cap/1.0.8/) reported inaccurate bars for clients without that mod. That was the old mod's observation, not a verified result for this one.

**Runtime check (1.0.0):** BepInEx loaded the DLL extracted from the release ZIP in an isolated, headless copy of this game build. A test-only plugin called the game's own `Run.RecalculateDifficultyCoefficent()` with one simulated player, an empty inventory, and identical inputs for the vanilla and modded cap. With zero elapsed time, stage-clear counts 24, 26, and 32 produced ambient levels **84, 99, 99** with the vanilla cap and **84, 112, 263** with Level Cap Remover. At 180 clears the modded result was **2,147,483,520**, without integer rollover. This exercises the in-engine calculation; the HUD, live monster stats, local-mod import, and remote co-op still need a playtest.
