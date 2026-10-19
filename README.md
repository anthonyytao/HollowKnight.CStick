# Hollow Knight C-Stick

Right-stick nail attacks for Hollow Knight 1.5.12620, in the style of a Smash Bros C-stick. Look up and down moves to the D-pad, so an Xbox controller can still peek.

Hollow Knight on Steam is the Windows build (app 367520). The released Hollow Knight Modding API still targets 1.5.78, so this mod loads with BepInEx 5 instead of Scarab.

## Controls

During play:

| Input | Result |
|---|---|
| Right stick | One slash in that direction. A second flick during that swing comes out on the first frame the nail is free. Then return the stick below halfway before the next one. Holding it does not charge a Nail Art. |
| Attack (X) | Unchanged. A tap swings. Holding it still charges a Nail Art. |
| Left stick | Move, and aim slashes, spells, and the Crystal Heart. |
| D-pad up / down | Instant look up / down. |
| D-pad left / right | Move, same as before. |

A diagonal flick prefers up or down. A left or right flick swings that way and the Knight keeps running. Up-slash and down-slash do not use the left stick.

The D-pad and the right stick go back to their normal jobs while the game is paused, in the inventory, in a cutscene, while you do not have control, and while the map can pan.

Look still only happens when the Knight is standing still. Hollow Knight does not pan the camera up or down while you run, and this mod does not add that. There is no look left or right.

## Install

1. Install [BepInEx 5](https://github.com/BepInEx/BepInEx/releases), Windows x64, into the Hollow Knight folder (the folder that contains `hollow_knight.exe`). On a Steam Deck that folder is `~/.local/share/Steam/steamapps/common/Hollow Knight`. Use the Windows build, because the game runs under Proton.
2. Start the game once so BepInEx creates its folders, then quit.
3. Build this repo (see below) and copy `CStick.dll` and `CStick.Core.dll` into `BepInEx/plugins/`.

Removing those two dlls turns the feature off. BepInEx stays until you delete it or verify the game files. The mod does not change save data.

## Build

```bash
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"
dotnet test -c Release
dotnet build src/CStick/CStick.csproj -c Release
```

The plugin looks for the game's `Managed` folder at the default Steam Deck path. Override it with `-p:HollowKnightManaged=/path/to/Managed` if the install is somewhere else.

Release output is `src/CStick/bin/Release/net472/`. Copy `CStick.dll` and `CStick.Core.dll` only.

## Limits

- Made for Hollow Knight 1.5.12620. A later patch can rename a private method and the swing will stop until the mod is updated.
- A flick while the nail is cooling down swings once on the first frame the nail is free, if the stick is still held that way. Holding it there does not swing again. The next one needs the stick back below half deflection.
- A flick during a dash, a hard landing, or a hit is remembered for about a tenth of a second, the same window as the attack button. If the stick is still held out when that window is still open and the nail is free, it swings once. A flick from the start of the dash does not.
- No swing from the right stick while wall-sliding, while holding the attack button, or while a Nail Art is charging.
- D-pad up and down no longer aim slashes, spells, or the Crystal Heart. The left stick still does.
- Holding the left stick up or down while standing still still uses the game's slow look.
