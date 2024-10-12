# <img src="https://github.com/TheodoreChristianRadu/ModManager/blob/master/ModManager/Icon.ico" width="40"> Dark Souls Mod Manager <img src="https://github.com/TheodoreChristianRadu/ModManager/blob/master/ModManager/Icon.ico" width="40">

A generic mod manager created for Dark Souls Remastered.

Should work with Dark Souls: Prepare to Die Edition, Demon Souls (PS3) and mostly any game as the main function of the program is to perform file replacement and restoration.

Based on code decompiled from AinTunez' [Mod Manager](https://www.nexusmods.com/darksoulsremastered/mods/75) for Dark Souls Remastered

## Instructions

+ Download `ModManager.exe` from the [Releases](https://github.com/TheodoreChristianRadu/ModManager/releases/latest) page
+ Add `ModManager.exe` to the game folder (by default `C:\Program Files (x86)\Steam\steamapps\common\DARK SOULS REMASTERED`)
+ Run the program, on first launch it should create a `mods` directory; a `ModManager.json` settings file will also be created after mods are installed
+ To add a mod to your list, you can drag and drop the mod's folder into the program's window or move said folder to the newly created `mods` directory
+ To install one or multiple mods into the game, select the mods you want and click `INSTALL`
+ Any conflict should raise a warning and proceeding will prioritize mods in the order they appear, topmost having the highest priority; mods are ordered alphabetically so to control the load order you can precede your mod's name with a number (for example `01 - My Mod`)
+ After the mods have been successfully installed, you can now launch the game which should be modded
+ To remove all mods, exit the game, open the Mod Manager and press `RESTORE`; changing the mods and pressing `INSTALL` will also restore the game files before installing the new mods

## Mod structure

A mod consists of a main directory bearing the name of the mod, which should itself contain various files and folders meant to be replaced or added into the game's folder when the mod is installed. A typical example of mod structure for Dark Souls Remastered is given below:

```
My Mod
├── chr
│   └── c0000.anibnd.dcx
├── event
│   └── common.emevd.dcx
├── map
│   └── MapStudio
│       └── m10_00_00_00.msb
├── menu
│   └── menu.drb.dcx
├── msg
│   └── ENGLISH
│       └── item.msgbnd.dcx
├── param
│   └── GameParam
│       └── GameParam.parambnd.dcx
├── script
│   ├── m10_00_00_00.luabnd.dcx
│   └── talk
│       └── m10_00_00_00.talkesdbnd
└── sound
    └── frpg_main.fsb
```
