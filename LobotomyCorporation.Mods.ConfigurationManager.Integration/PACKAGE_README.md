# LobotomyCorporation.Mods.ConfigurationManager.Integration

Build-time integration package for
[LobCorp.ConfigurationManager](https://github.com/open-lobotomy/LobCorp.ConfigurationManager).
It lets your Lobotomy Corporation mod expose settings in the in-game F1 menu
without taking a hard dependency on `ConfigurationManager.dll`. Your mod ships
as a single DLL and runs whether or not ConfigurationManager is installed.

## What it does

Reference this package with `PrivateAssets="all"` and write fluent config
declarations:

```csharp
using LobotomyCorporation.Mods.ConfigurationManager;

[assembly: ConfigManagerMod(
    ModId = "com.example.mymod",
    ModName = "My Mod",
    ModVersion = "1.0.0")]

public static class MyConfig
{
    public static readonly IConfigValue<int> Damage = Config.Bind(
        "Combat", "Damage", 100,
        description: "Damage per hit", minValue: 1, maxValue: 1000);

    public static readonly IConfigValue<bool> GodMode = Config.Bind(
        "Cheats", "GodMode", false, isAdvanced: true);
}

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

The Integration package scans your source for `Config.Bind(...)` invocations and emits into your mod's assembly:

- A `Config` static class with `Bind` overloads
- `IConfigValue<T>` / `ConfigValue<T>` value types
- A local `ConfigurationManagerAttributes` class for UI hints
- A `Config.RegisterAll()` method that uses reflection to register with
  ConfigurationManager if it is installed, or falls back to an in-memory store if not

## Runtime behavior

- **ConfigurationManager present**: settings show up in the in-game F1 menu and persist to disk.
- **ConfigurationManager absent**: bindings return their default values from an in-memory store. Your mod still runs.

## Things to know

- **Aliased uses (`using X = Config;`) are not supported.** The scanner matches on the literal identifier `Config`.
- **If your mod already has a class named `Config`, rename it.** The `using LobotomyCorporation.Mods.ConfigurationManager;` directive brings this package's own `Config` class into scope, and two classes with the same name cause a compile error.
- **ConfigurationManager is a fork of [BepInEx.ConfigurationManager](https://github.com/BepInEx/BepInEx.ConfigurationManager).** Both ship as `ConfigurationManager.dll` on purpose — only one can be loaded at a time. If a user has BepInEx.ConfigurationManager instead of LobCorp.ConfigurationManager, your mod still runs, but settings fall back to in-memory.

## Installation

```sh
dotnet add package LobotomyCorporation.Mods.ConfigurationManager.Integration
```

Make sure the `PackageReference` has `PrivateAssets="all"` so the generator
doesn't flow through to consumers of your mod:

```xml
<PackageReference Include="LobotomyCorporation.Mods.ConfigurationManager.Integration" Version="..." PrivateAssets="all" />
```

Your mod still targets **.NET Framework 3.5** — this package contains no runtime
code; it only emits source into your assembly.
