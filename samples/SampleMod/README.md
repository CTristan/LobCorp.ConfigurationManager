# Adding ConfigurationManager settings to your mod

This page shows how to add in-game settings to your Lobotomy Corporation
mod using the
`LobotomyCorporation.Mods.ConfigurationManager.Integration` package.

The files in this `SampleMod/` folder are a complete, working example.
**You do not need to copy the whole folder into your mod.** Follow the
steps below to add the integration to a mod you already have, and open
the sample files when you want to compare your code with a finished
version.

## How it works with and without ConfigurationManager

Your mod is distributed as **one DLL**. You never copy
`ConfigurationManager.dll` into your mod's folder. You never add a
runtime reference to it.

- **If the player has ConfigurationManager installed**, your settings
  appear in the in-game F1 menu and save to disk.
- **If the player does not have ConfigurationManager installed**, your
  mod still loads. Your settings keep their default values in memory.
  Nothing crashes. Nothing logs errors.

You do not write the code that detects ConfigurationManager. The
Integration package generates that for you. You only call
`Config.RegisterAll()` once, from your mod's entry point.

## Four steps

### 1. Add the Integration package to your `.csproj`

Add this inside any `<ItemGroup>` in your mod's `.csproj`:

```xml
<PackageReference
    Include="LobotomyCorporation.Mods.ConfigurationManager.Integration"
    Version="1.0.0"
    PrivateAssets="all" />
```

`PrivateAssets="all"` means "use this package at build time only; do
not include it in my output DLL." That is how your mod stays a single
DLL.

> The sample here uses a `ProjectReference` instead, because it builds
> from inside this repository. See `SampleMod.csproj` for the full
> project file. Your own mod uses the `PackageReference` shown above.

### 2. Identify your mod to the integration

Add this once per mod, at the top of any `.cs` file:

```c#
using LobotomyCorporation.Mods.ConfigurationManager;

[assembly: ConfigManagerMod(
    ModId = "com.example.mymod",
    ModName = "My Mod",
    ModVersion = "1.0.0",
    Fallback = ConfigFallback.InMemory
)]
```

- `ModId` — a unique identifier for your mod. Reverse-domain format
  (for example `com.yourname.yourmod`) keeps it unique across every
  mod a player installs.
- `ModName` — the heading players see for your mod in the F1 menu.
- `ModVersion` — shown next to the mod name in the F1 menu.
- `Fallback = ConfigFallback.InMemory` — what happens when
  ConfigurationManager is not installed. `InMemory` means "keep values
  in memory with their defaults." This is the only supported value
  today.

> See `MyConfig.cs` in this folder for this attribute in context.

### 3. Declare your settings

Create a static class that holds your settings. Each setting is one
call to `Config.Bind`:

```c#
internal static class MyConfig
{
    public static readonly IConfigValue<int> Damage = Config.Bind(
        section: "Combat",
        key: "Damage",
        defaultValue: 100,
        description: "Damage per hit",
        minValue: 1,
        maxValue: 1000);

    public static readonly IConfigValue<bool> GodMode = Config.Bind(
        section: "Cheats",
        key: "GodMode",
        defaultValue: false,
        isAdvanced: true);
}
```

Read a setting at runtime with `MyConfig.Damage.Value`.

Common options:
- `minValue` / `maxValue` — for numeric settings. Shows a slider in
  the F1 menu when both are set.
- `isAdvanced: true` — hides the setting behind an "Advanced" toggle.
- `order` — higher numbers appear earlier within a section.

> See `MyConfig.cs` for a complete example with three different
> setting shapes.

### 4. Register your settings when the mod loads

Your mod's entry point is a class named `Harmony_Patch`. LMM
(Lobotomy Mod Manager — the base game's mod loader) instantiates this
class when your mod loads. Call `Config.RegisterAll()` from its static
constructor:

```c#
public sealed class Harmony_Patch
{
    static Harmony_Patch()
    {
        var harmony = HarmonyInstance.Create("com.example.mymod");
        harmony.PatchAll(typeof(Harmony_Patch).Assembly);

        Config.RegisterAll();
    }
}
```

That one call:

- Finds every `Config.Bind` declaration in your assembly.
- Checks whether ConfigurationManager is installed.
- If it is, registers your settings so they appear in the F1 menu.
- If it is not, returns without error. Your settings keep their
  defaults in memory.

> See `Harmony_Patch.cs` for a complete example, including a
> `try`/`catch` wrapper so that a failure during load appears in the
> game's log instead of failing silently.

## The full sample in this folder

| File | What to look at |
|---|---|
| `MyConfig.cs` | The `[assembly: ConfigManagerMod]` attribute and three settings: a numeric range, a boolean flag, and a plain string. |
| `Harmony_Patch.cs` | The entry-point pattern and how to wrap setup in `try`/`catch`. |
| `SampleMod.csproj` | The project structure, including how to reference the Integration package at build time only. |

## Building the sample (optional)

If you want to build the sample directly from this repository to see
the output:

```sh
dotnet build samples/SampleMod/SampleMod.csproj
```

The output is `samples/SampleMod/bin/net35/SampleMod.dll`. To install
it as a real mod, copy that one file into your LMM
`BaseMods/SampleMod/` folder.
