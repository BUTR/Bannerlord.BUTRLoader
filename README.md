# Bannerlord.BUTRLoader
<p align="center">
  <a href="https://github.com/BUTR/Bannerlord.BUTRLoader" alt="Lines Of Code">
    <img src="https://aschey.tech/tokei/github/BUTR/Bannerlord.BUTRLoader?category=code" />
  </a>
  <a href="https://www.codefactor.io/repository/github/butr/bannerlord.butrloader">
    <img src="https://www.codefactor.io/repository/github/butr/bannerlord.butrloader/badge" alt="CodeFactor" />
  </a>
  <a href="https://codeclimate.com/github/BUTR/Bannerlord.BUTRLoader/maintainability">
    <img alt="Code Climate maintainability" src="https://img.shields.io/codeclimate/maintainability-percentage/BUTR/Bannerlord.BUTRLoader">
  </a>
  <a title="Crowdin" target="_blank" href="https://crowdin.com/project/butrloader">
    <img src="https://badges.crowdin.net/butrloader/localized.svg">
  </a>
  </br>
  <a href="https://github.com/BUTR/Bannerlord.BUTRLoader/actions/workflows/test.yml?query=branch%3Amaster">
    <img alt="GitHub Workflow Status" src="https://img.shields.io/github/actions/workflow/status/BUTR/Bannerlord.BUTRLoader/test.yml?branch=master&label=Game%20Stable%20and%20Beta">
  </a>
  </br>
  <a href="https://www.nexusmods.com/mountandblade2bannerlord/mods/2513" alt="NexusMods BUTRLoader">
    <img src="https://img.shields.io/badge/NexusMods-BUTRLoader-yellow.svg" />
  </a>
  <a href="https://www.nexusmods.com/mountandblade2bannerlord/mods/2513" alt="NexusMods BUTRLoader">
    <img src="https://img.shields.io/endpoint?url=https%3A%2F%2Fnexusmods-version-pzk4e0ejol6j.runkit.sh%3FgameId%3Dmountandblade2bannerlord%26modId%3D2513" />
  </a>
  <a href="https://www.nexusmods.com/mountandblade2bannerlord/mods/2513" alt="NexusMods BUTRLoader">
    <img src="https://img.shields.io/endpoint?url=https%3A%2F%2Fnexusmods-downloads-ayuqql60xfxb.runkit.sh%2F%3Ftype%3Dunique%26gameId%3D3174%26modId%3D2513" />
  </a>
  <a href="https://www.nexusmods.com/mountandblade2bannerlord/mods/2513" alt="NexusMods BUTRLoader">
    <img src="https://img.shields.io/endpoint?url=https%3A%2F%2Fnexusmods-downloads-ayuqql60xfxb.runkit.sh%2F%3Ftype%3Dtotal%26gameId%3D3174%26modId%3D2513" />
  </a>
  <a href="https://www.nexusmods.com/mountandblade2bannerlord/mods/2513" alt="NexusMods BUTRLoader">
    <img src="https://img.shields.io/endpoint?url=https%3A%2F%2Fnexusmods-downloads-ayuqql60xfxb.runkit.sh%2F%3Ftype%3Dviews%26gameId%3D3174%26modId%3D2513" />
  </a>
  </br>
  <img src="https://staticdelivery.nexusmods.com/mods/3174/images/2513/2513-1612129311-35018174.png" width="800">
</p>

Extends the native launcher.  
Adds support for community used mod metadata that fixes issues with mod load order sorting! It will sort correctly Harmony, UIExtender, ButterLib and MCM. 

## Features
**BUTRLoader** consists of two modules - **BUTRLoader** itself and **LauncherEx**.

**BUTRLoader** expands the game launch with the following features:

* **Interceptor** - BUTRLoader checks if the is a class with a custom attribute named ***BUTRLoaderInterceptorAttribute***. If it's found it checks if there are the following signatures:
  *  **void OnInitializeSubModulesPrefix()** - will execute just before the game starts to initialize the SubModules. This gives us the ability to add SubModules declared in other programming languages like [Python](https://github.com/BUTR/Bannerlord.Python)﻿ and [Lua](https://github.com/BUTR/Bannerlord.Lua)﻿
  * **void OnLoadSubModulesPostfix()** - will execute just after all SubModules were initialized
* **LoadReferencesOnLoad** - gives the ability to add <Tag key="LoadReferencesOnLoad" value="false" /> that will disable explicit dependency load. Will be useful after switch to .NET Core runtime.


**LauncherEx** is the UI module. It expands the native launcher with the following features:

* **Option Screen** - provides various options that will be listed below.
* **Extended Sorting** - the launcher now respects the community metadata when sorting. Available in Options. Enabled by default.
* **Scrollbar** - the launcher before e1.7.2 didn't had a way to scroll without the mouse wheel. We added a scrollbar to fix this.
* **Enable/Disable All Mods Checkbox** - added the ability to enable and disable all mods with one click.
* **Expanded Dependencies Hint** - added our community metadata to be displayed in the Hints added in e1.7.0.
* **Issue Hint System** - the launcher displays an arrow that when expanded, will display why a mod can't be enabled. The issue can be a wrong dependency module version, binary incompatibility with the current game version
* **Compact Module List** - allows a more compact display of the Module List. Available in Options. Disabled by default.
* **Fix Common Issues** - the launcher checks if 0Harmony.dll is present in the main /bin folder. If there is one, will prompt the user whether t delete it.
* **File Unblocking** - the launcher will unblock the .dll's if they are locked itself. Available in Options. Enabled by default.
* **Reset Module List** - will forcefully reset the module list and force the raw loaded list to be sorted. Available in Options. Will be disabled after restart.
* **Binary Compatibility Check** - the launcher will check whether the are ABI issues in the module with the current game version. ABI issues mean the module won't work in the game and will need a new updated version.
* **Import/Export Mod List** - provides a way to export and import Mod Lists with the correct load order and module versions. If a module version is incorrect, with highlight that.


## Installation
Download the file and extract it's contents into the game's root folder (e.g. `C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord`).

## Troubleshooting
### Unblocking DLL's
You may need to right click on Bannerlord.BUTRLoader.dll  file, click Properties, and click `Unblock` if you extracted the zip file with Windows Explorer or other programs that try to secure extracted files.

## For Modders
See [BLSE](https://github.com/BUTR/Bannerlord.BLSE#community-dependency-metadata) for a newer version!  
BUTRLoader adds support for a new tag DependedModuleMetadatas that allows you to better define your load order, see the example below
```xml
<DependedModuleMetadatas>
  <!-- order: [ "LoadBeforeThis", "LoadAfterThis" ] -->
  <!-- optional: [ "true", "false" ] -->
  <!-- version: [ "e1.0.0.0", "e1.*", "e1.0.*", "e1.0.0.*" ] -->
  <!-- incompatible: [ "true", "false" ] -->

  <DependedModuleMetadata id="Bannerlord.Harmony" order="LoadBeforeThis" />

  <DependedModuleMetadata id="Native" order="LoadAfterThis" version="e1.4.3.*" />
  <DependedModuleMetadata id="SandBoxCore" order="LoadAfterThis" version="e1.5.*" />
  <DependedModuleMetadata id="Sandbox" order="LoadAfterThis" />
  <DependedModuleMetadata id="StoryMode" order="LoadAfterThis" version="e1.*" optional="true" />
  <DependedModuleMetadata id="CustomBattle" order="LoadAfterThis" optional="true" />

  <DependedModuleMetadata id="MyCustomMod" incompatible="true" />
</DependedModuleMetadatas>
```
