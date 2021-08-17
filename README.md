# Bannerlord.BUTRLoader
<p align="center">
  <a href="https://github.com/BUTR/Bannerlord.BUTRLoader" alt="Lines Of Code">
    <img src="https://tokei.rs/b1/github/BUTR/Bannerlord.BUTRLoader?category=code" />
  </a>
  <a href="https://www.codefactor.io/repository/github/butr/bannerlord.butrloader">
    <img src="https://www.codefactor.io/repository/github/butr/bannerlord.butrloader/badge" alt="CodeFactor" />
  </a>
  <a href="https://codeclimate.com/github/BUTR/Bannerlord.BUTRLoader/maintainability">
    <img alt="Code Climate maintainability" src="https://img.shields.io/codeclimate/maintainability-percentage/BUTR/Bannerlord.BUTRLoader">
  </a>
  </br>
  <a href="https://github.com/BUTR/Bannerlord.BUTRLoader/actions/workflows/test.yml?query=branch%3Amaster">
    <img alt="GitHub Workflow Status (event)" src="https://img.shields.io/github/workflow/status/BUTR/Bannerlord.BUTRLoader/Test?branch=master&label=Game%20Stable%20and%20Beta">
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

## Installation
Download the file and extract it's contents into the game's root folder (e.g. `C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord`).

## Troubleshooting
### Unblocking DLL's
You may need to right click on Bannerlord.BUTRLoader.dll  file, click Properties, and click `Unblock` if you extracted the zip file with Windows Explorer or other programs that try to secure extracted files.

## For Modders
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
