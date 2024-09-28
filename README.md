# More Farm Plots

<p align="center">
  <img alt="Extra farm plots" src="https://raw.githubusercontent.com/DeveloperBlue/ATTMoreFarmPlots/refs/heads/main/previews/1.jpg" width="32%">
  &nbsp;
  <img alt="Extra farm plots (hoe'd and watered)" src="https://raw.githubusercontent.com/DeveloperBlue/ATTMoreFarmPlots/refs/heads/main/previews/2.jpg" width="32%">
  &nbsp;
  <img alt="Extra farm plots (with crops)" src="https://raw.githubusercontent.com/DeveloperBlue/ATTMoreFarmPlots/refs/heads/main/previews/3.jpg" width="32%">
</p>

----

> [!CAUTION]
> THIS MOD DOES NOT WORK.
> Unfortunately, this mod does not work. If you're an experienced Unity developer/modder with some tips, please see the <b>Issues With Development / Help Me</b> section below.

Adds another set of farm plots next to the original farm behind the tavern.

Works in multiplayer (All clients must have the mod installed to see and use the farm).

Good companion mod for the [More Players](https://thunderstore.io/c/ale-and-tale-tavern/p/DeveloperBlue/MorePlayers/) mod.

<p align="left">
    <a href="https://buymeacoffee.com/michaelrooplall" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png" alt="Buy Me A Coffee" style="height: 41px !important;width: 174px !important;box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;-webkit-box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;" ></a>
</p>

## Installation

The only requirement for this mod is [BepInEx](https://thunderstore.io/c/ale-and-tale-tavern/p/BepInEx/BepInExPack/).

### Mod Manager (Recommended)

- Install this mod directly in r2modman or the Thunderstore Launcher by clicking the big blue "Install with Mod Manager" button above.

### Manual Installation

- To manually install this mod, download and unzip the file. Place the folder in your {game}/BepInEx/plugins folder.

## Multiplayer
- This mod works in multiplayer. 

*All clients must have the mod installed to see and use the new farm plots.*

## Uninstalling

Trying to load your world up after uninstalling the mod may crash. This is because the game is trying to load the saved extra plots into slots that don't exist anymore. To fix this, you can open your save file up and delete the extra saved plots.

## Issues With Development / Help Me
I'm having some issues with this mod. Each individial farm plot has a NetworkObject and NetworkBehavior component on them. I am familiar with one NetworkObject in the parent, and the children entities (plots) each having a NetworkBehavior.

I tried just cloning the plots and calling .Spawn() on their NetworkObjects, but because they share the same IDs as the source they are cloned from, Unity doesn't like that.

I tried RemoveImmedidate() on the existing NetworkObjects and creating new NetworkObject components, and then calling .Spawn() on the new NetworkObjects, but that didn't seem to work either.

I tried creating a new parent GameObject with a new NetworkObject, and adding the farm plot instances as children (with original NetworkObjects removed), and that still didn't seem to work.

Frankly, I am just getting issues trying to create a new NetworkObject at runtime. I'm also not sure if calling RemoveImmediate() on existing NetworkObjects messes with the NetworkBehaviors that expect the NetworkObject to exist (but in the runtime inspector, they seem to link to any new NetworkObjects fine, so that might not be an issue.)

----

| Links    |  |
| -------- | ------- |
| GitHub  | https://github.com/DeveloperBlue/ATTMoreFarmPlots/ |
| Thunderstore | https://thunderstore.io/c/ale-and-tale-tavern/p/DeveloperBlue/MoreFarmPlots/ |
