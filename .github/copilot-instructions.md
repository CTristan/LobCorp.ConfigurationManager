# Instructions

This file provides guidance when working with code in this repository.

## Project Overview

Fork of [BepInEx.ConfigurationManager](https://github.com/BepInEx/BepInEx.ConfigurationManager) adapted for Lobotomy Corporation's mod loader (LMM) and Harmony 1. Provides an in-game ImGUI settings window (F1) for LMM and BepInEx mods. Targets **net35** (Unity 2017-era Mono runtime).

Upstream remote: `upstream` → BepInEx/BepInEx.ConfigurationManager
Origin: `origin` → open-lobotomy/LobCorp.ConfigurationManager

## Build

```bash
dotnet build
```

Output goes to `bin/net35/`. Dependencies in `lib/` are symlinks to `../lobotomy-corporation-mods/external/LobotomyCorp_Data/Managed/` — the game's managed assemblies plus Harmony must be present there.

Tests live in `LobCorp.ConfigurationManager.Test` (xunit.v3, Moq, AwesomeAssertions). Run with `dotnet test`.

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
- Source-gen-based optional-dependency path for mod authors is planned (see `/Users/chris/.claude/plans/cheeky-cooking-puzzle.md`); this repo no longer carries a `LobotomyCorporation.Mods.Common`-based bridge.

**Configuration model (`Config/`):**
- `LmmConfigFile` — file I/O and parsing for `config.cfg` files
- `LmmConfigEntry<T>` — generic typed entries with change events and auto-save
- `LmmConfigDefinition` — section + key identity
- `AcceptableValueRange<T>` / `AcceptableValueList<T>` — value constraints implementing `IAcceptableValue`

**UI rendering (`Implementations/SettingFieldDrawer.cs`):**
- ImGUI immediate-mode rendering
- Type-specific controls: checkboxes, sliders, dropdowns, color pickers, hotkey capture
- `ConfigurationManagerAttributes` controls display (order, visibility, custom drawers)

**`ConfigurationManagerAttributes` is a copy-paste template, not a referenced API.** Each plugin bundles its own copy of the class and assigns values to instances that get passed as tags to setting descriptions. `SettingEntryBase.SetFromAttributes()` reads these via reflection (`Type.GetProperties`, matching by simple type name — not assembly identity). This fork uses **public auto-properties**; upstream BepInEx.ConfigurationManager uses **public fields**, so upstream's template is not directly compatible — if copying from upstream, convert the fields to auto-properties.

## Key Constraints

- **net35 target**: no LINQ extensions beyond what's available, no `System.ValueTuple`, limited BCL. `LangVersion` is set to `latest` so C# syntax features work but BCL APIs are restricted.
- **RootNamespace and AssemblyName are both `ConfigurationManager`** (not `LobCorp.ConfigurationManager`) — intentionally matches upstream BepInEx.ConfigurationManager. This is a **DLL-name / namespace collision prevention** mechanism only: the identical DLL name stops both from loading simultaneously, and the shared root namespace avoids dual-load conflicts (double UI entries, duplicate `ConfigurationManagerAttributes` processing). This is **not** a public-API-compatibility contract — the fork can freely change its internal shape (e.g. `ConfigurationManagerAttributes` was moved from fields to properties). Do not change `RootNamespace` or `AssemblyName` without accounting for the loader-collision implications.
- **`Harmony_Patch` class name is load-bearing** — every LMM mod must expose an entry type named `Harmony_Patch`. The analyzer package (`LobotomyCorporation.Mods.Analyzers` globalconfig) suppresses S101 and CA1707 repo-wide so this pattern doesn't trip naming rules.
- **Game assembly references are `Private=false`** — none are copied to output since they exist in the game's managed folder at runtime. No other runtime DLLs are copied alongside `ConfigurationManager.dll` today (the previous `LobotomyCorporation.Mods.Common` bridge has been removed).
- **Implicit usings and nullable are disabled.**
- `Microsoft.NETFramework.ReferenceAssemblies` is pulled in implicitly by the SDK for net35 — do not add it to `Directory.Packages.props`.

## CI/CD

- **CI** (`.github/workflows/ci.yml`) — builds and tests on push to `main` and PRs.
- **Release** (`.github/workflows/release.yml`) — triggered when a GitHub Release is published. Builds, tests, packages the mod as a zip (matching the `BaseMods/ConfigurationManager/` folder structure), and uploads it as a release asset.

NuGet package publishing is planned for v1.0.0 but not yet implemented.

## Analyzers

Global analyzers (`LobotomyCorporation.Mods.Analyzers`, `OpenLobotomy.Standards`) are configured in `Directory.Packages.props`. All Sonar rules run at their default severity — there are no global suppressions in `.editorconfig`. Rule exceptions that apply to all LMM mods (e.g. S101/CA1707 for the `Harmony_Patch` entry point) live in the shared `LobotomyCorporation.Mods.Analyzers` globalconfig, not here. Fix violations rather than suppressing them; if a suppression is truly needed, scope it as narrowly as possible (file-local `#pragma` or per-member `[SuppressMessage]`).

## Audience & Language

**Assume the reader is a first-time mod author whose first language is not English.** Most consumers of this repo — both the `ConfigurationManager.dll` end-user install and the `Integration` NuGet package — are Korean-speaking modders reading English as a second language or through machine translation, and many have no prior professional development experience. Every error message, diagnostic, README, and code comment that an author will see must pass that bar before shipping.

### Project facts that shape documentation

- **Lobotomy Corporation itself will never update.** The base game is final. Do not pitch wrappers, adapters, or analyzers on "survives game updates" or "keeps working when the game changes" — those claims are factually wrong and will mislead readers. The honest value props for typed wrappers over reflection are: (a) the compiler checks names and types at build time, so typos fail before you run the game; (b) typed code is shorter and easier to read; (c) the package is community-maintained, so fixes land once for everyone. What *does* still change is LMM (the mod loader) and other mods that patch the same game code via Harmony — if a doc needs to explain why a wrapper helps mods coexist, that is the real reason, not game updates.
- **The Integration package exists so authors can hook into ConfigurationManager *if it is installed*, without bundling or redistributing `ConfigurationManager.dll` themselves.** The mod ships as a single DLL. If the player has ConfigurationManager, settings appear in the F1 menu. If not, the mod still runs and bindings serve defaults from an in-memory store. Never write docs, diagnostics, or examples that imply the mod author has to ship `ConfigurationManager.dll`, reference it at compile time, or detect its presence by hand — the generator's emitted reflection probe handles that. If a reader walks away thinking they need to copy a DLL into their mod folder or add a hard `Reference Include="ConfigurationManager"`, the doc has failed.

### Package Audiences

- **`ConfigurationManager.dll`** — shipped to players as a BaseMod. End-user audience (installers, not coders); release notes and the in-game UI should be readable without developer vocabulary.
- **`LobotomyCorporation.Mods.ConfigurationManager.Integration`** — consumed by mod authors via a single NuGet reference. The optional-dependency story above is the central value proposition: assume the author found this package because they want settings UI *when available* but are not willing to take a hard runtime dependency. Error messages, analyzer diagnostics, README samples, and generated-code comments should all reinforce that. Explain *why*, not just *what*. Do not assume knowledge of dependency injection, mocking, reflection, source generators, Roslyn analyzers, or build-system internals like `PrivateAssets`/`ReferenceOutputAssembly` — when those terms are unavoidable, define them inline or link to a one-paragraph explainer. Surface failures as clear, actionable messages, not stack traces.
- **`samples/` directory (e.g. `samples/SampleMod/`)** — this bar applies here too. Samples are copy-paste reference material for mod authors; every comment, naming choice, and implicit convention must be readable in isolation on GitHub by someone who has never opened the rest of this repo. Expand acronyms the first time they appear (LMM → Lobotomy Mod Manager), add inline comments on any assembly attribute or pattern that a first-timer would not recognize (e.g. `Fallback = ConfigFallback.InMemory`, the static-initializer entry-point idiom), and ship a README in each sample that states the optional-dependency promise upfront.

### Writing Style

**User-facing text** (README, error messages, release notes): use short sentences with active voice and explicit subjects. Avoid idioms, slang, and culturally specific references. Define technical terms inline or use simpler words. Write in a style that survives machine translation — no ambiguous pronouns, no noun stacking.

**Developer-facing text** (code comments, commit messages): technical terminology is fine, but prefer direct, concise phrasing over unnecessarily complex language.
