# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Fork of [BepInEx.ConfigurationManager](https://github.com/BepInEx/BepInEx.ConfigurationManager) adapted for Lobotomy Corporation's mod loader (LMM) and Harmony 1. Provides an in-game ImGUI settings window (F1) for LMM and BepInEx mods. Targets **net35** (Unity 2017-era Mono runtime).

Upstream remote: `upstream` → BepInEx/BepInEx.ConfigurationManager
Origin: `origin` → open-lobotomy/LobCorp.ConfigurationManager

## Build

```bash
dotnet build
```

Output goes to `bin/net35/`. Dependencies in `lib/` are symlinks to `../lobotomy-corporation-mods/external/LobotomyCorp_Data/Managed/` — the game's managed assemblies plus Harmony must be present there.

No test projects exist yet. The test infrastructure (xunit.v3, Moq, AwesomeAssertions) is configured in `Directory.Packages.props` for future use.

## Architecture

**Entry flow:**
1. `Harmony_Patch.cs` — static initializer creates Harmony instance (`com.lobcorp.configurationmanager`), patches all
2. `Patches/IntroPlayerPatchAwake.cs` — Harmony postfix on `IntroPlayer.Awake()` injects `ConfigManagerBehaviour` MonoBehaviour
3. `Implementations/ConfigManagerBehaviour.cs` — Unity lifecycle wrapper, delegates to `ConfigurationManager`
4. `Implementations/ConfigurationManager.cs` — main UI controller, handles hotkey, window rendering

**Settings discovery (`Implementations/SettingSearcher.cs`):**
- LMM mods register via `Config/LmmConfigRegistration.cs` static API
- Auto-scans `BaseMods/{modId}/config.cfg` files
- Discovers BepInEx plugins via reflection (`Implementations/BepInExInterop.cs`) — no hard dependency

**Configuration model (`Config/`):**
- `LmmConfigFile` — file I/O and parsing for `config.cfg` files
- `LmmConfigEntry<T>` — generic typed entries with change events and auto-save
- `LmmConfigDefinition` — section + key identity
- `AcceptableValueRange<T>` / `AcceptableValueList<T>` — value constraints

**UI rendering (`Implementations/SettingFieldDrawer.cs`):**
- ImGUI immediate-mode rendering
- Type-specific controls: checkboxes, sliders, dropdowns, color pickers, hotkey capture
- `ConfigurationManagerAttributes` controls display (order, visibility, custom drawers)

## Key Constraints

- **net35 target**: no LINQ extensions beyond what's available, no `System.ValueTuple`, limited BCL. `LangVersion` is set to `latest` so C# syntax features work but BCL APIs are restricted.
- **RootNamespace is `ConfigurationManager`** (not `LobCorp.ConfigurationManager`) — follows original BepInEx conventions.
- **All references are `Private=false`** — none are copied to output since they exist in the game's managed folder at runtime.
- **Implicit usings and nullable are disabled.**
- `Microsoft.NETFramework.ReferenceAssemblies` is pulled in implicitly by the SDK for net35 — do not add it to `Directory.Packages.props`.

## CI/CD

GitHub Actions workflow (`.github/workflows/nuget.yml`) packs and pushes to IllusionMods Azure DevOps NuGet feed on release publish.

## Analyzers

Global analyzers (`LobotomyCorporation.Mods.Analyzers`, `OpenLobotomy.Standards`) are configured in `Directory.Packages.props`. There is existing analyzer debt (~117 style warnings) that needs a separate cleanup pass.
