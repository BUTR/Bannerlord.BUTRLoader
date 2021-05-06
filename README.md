# Bannerlord.BUTRLoader
<p align="center">
  <a href="https://github.com/BUTR/Bannerlord.BUTRLoader" alt="Lines Of Code">
  <img src="https://tokei.rs/b1/github/BUTR/Bannerlord.BUTRLoader?category=code" /></a>
  <a href="https://www.codefactor.io/repository/github/butr/bannerlord.butrloader"><img src="https://www.codefactor.io/repository/github/butr/bannerlord.butrloader/badge" alt="CodeFactor" /></a>
  </br>
  <!--
  <a href="https://github.com/BUTR/Bannerlord.BUTRLoader/actions?query=workflow%3ATest"><img src="https://github.com/BUTR/Bannerlord.BUTRLoader/workflows/Test/badge.svg?branch=dev&event=push" alt="Test" /></a>
  <a href="https://codecov.io/gh/BUTR/Bannerlord.BUTRLoader"><img src="https://codecov.io/gh/BUTR/Bannerlord.BUTRLoader/branch/dev/graph/badge.svg" />
   </a>
  </br>
  -->
  <a href="https://www.nexusmods.com/mountandblade2bannerlord/mods/2513" alt="Nexus BUTRLoader">
  <img src="https://img.shields.io/badge/Nexus-BUTRLoader-yellow.svg" /></a>
  <a href="https://www.nexusmods.com/mountandblade2bannerlord/mods/2513" alt="BUTRLoader">
  <img src="https://img.shields.io/endpoint?url=https%3A%2F%2Fnexusmods-version-pzk4e0ejol6j.runkit.sh%3FgameId%3Dmountandblade2bannerlord%26modId%3D2513" /></a>
  <a href="https://www.nexusmods.com/mountandblade2bannerlord/mods/2513" alt="Nexus BUTRLoader">
  <img src="https://img.shields.io/endpoint?url=https%3A%2F%2Fnexusmods-downloads-ayuqql60xfxb.runkit.sh%2F%3Ftype%3Dunique%26gameId%3D3174%26modId%3D2513" /></a>
  <a href="https://www.nexusmods.com/mountandblade2bannerlord/mods/2513" alt="Nexus BUTRLoader">
  <img src="https://img.shields.io/endpoint?url=https%3A%2F%2Fnexusmods-downloads-ayuqql60xfxb.runkit.sh%2F%3Ftype%3Dtotal%26gameId%3D3174%26modId%3D2513" /></a>
  <a href="https://www.nexusmods.com/mountandblade2bannerlord/mods/2513" alt="Nexus BUTRLoader">
  <img src="https://img.shields.io/endpoint?url=https%3A%2F%2Fnexusmods-downloads-ayuqql60xfxb.runkit.sh%2F%3Ftype%3Dviews%26gameId%3D3174%26modId%3D2513" /></a>
  </br>
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
