# In-game settings menu for Lobotomy Corporation mods

ConfigurationManager is a Lobotomy Corporation mod that adds an in-game
settings window for other mods. Players press **F1** to open the
window, change values, and save them.

It is a fork of
[BepInEx.ConfigurationManager](https://github.com/BepInEx/BepInEx.ConfigurationManager)
adapted for Lobotomy Mod Manager (LMM — the base game's mod loader) and
Harmony 1 (the patching library LMM mods use).

![Configuration manager](Screenshot.PNG)

## Who this page is for

- **Players**: see [Installation](#installation).
- **Mod authors**: see
  [Adding settings to your mod](#adding-settings-to-your-mod).

If you have never written a Lobotomy Corporation mod before, the
[`samples/SampleMod/`](samples/SampleMod/) folder in this repository
walks through the integration step by step and includes a complete
working example.

## Installation

Copy `ConfigurationManager.dll` into your Lobotomy Corporation mods
folder. LMM loads it automatically the next time you start the game.

## Adding settings to your mod

The **Integration package**
(`LobotomyCorporation.Mods.ConfigurationManager.Integration`) is the
recommended way to add settings to your mod.

Your mod is distributed as one DLL. If the player has
ConfigurationManager installed, your settings appear in the F1 menu.
If not, your mod still runs and your settings keep their default
values in memory. You never copy `ConfigurationManager.dll` into your
mod's folder, and you never add a runtime reference to it.

→ For a four-step walkthrough with a working example, see
**[samples/SampleMod/README.md](samples/SampleMod/)**.

A short version follows for readers already familiar with the pattern.

### Short version — Integration package

Add the package to your `.csproj` with `PrivateAssets="all"`
(build-time-only, so the package does not ship inside your mod):

```xml
<PackageReference Include="LobotomyCorporation.Mods.ConfigurationManager.Integration"
                  Version="1.0.0"
                  PrivateAssets="all" />
```

Mark your assembly and declare your settings:

```c#
using LobotomyCorporation.Mods.ConfigurationManager;

[assembly: ConfigManagerMod(
    ModId = "com.example.mymod",
    ModName = "My Mod",
    ModVersion = "1.0.0",
    Fallback = ConfigFallback.InMemory)]

internal static class MyConfig
{
    public static readonly IConfigValue<int> Damage = Config.Bind(
        "Combat", "Damage", 100,
        description: "Damage per hit", minValue: 1, maxValue: 1000, order: 10);

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

The Integration package writes the supporting runtime code — a local
`ConfigurationManagerAttributes` copy and a `Config.RegisterAll()`
method that probes for ConfigurationManager at runtime — directly
into your mod's own DLL. The output is a single DLL.

A few things to know:

- **Do not alias `Config`.** `using X = Config;` breaks the scanner,
  which matches the literal identifier `Config`.
- **If your mod already has a class named `Config`, rename it.** The
  `using LobotomyCorporation.Mods.ConfigurationManager;` directive
  brings the Integration package's own `Config` class into scope. Two
  classes with the same name cause a compile error.
- **`ConfigurationManager.dll` is a fork of
  [BepInEx.ConfigurationManager](https://github.com/BepInEx/BepInEx.ConfigurationManager).**
  Both share the same DLL name on purpose — only one can load at a
  time. If a player installs BepInEx.ConfigurationManager instead of
  this fork, your mod still runs, but the F1 menu does not show your
  settings. Install LobCorp.ConfigurationManager to see them.

### Option 2 — Direct dependency (requires `ConfigurationManager.dll` at runtime)

Use `LmmConfigRegistration` directly. This is a harder dependency — your mod will fail to load if ConfigurationManager is not installed.

```c#
using ConfigurationManager.Config;

public class Harmony_Patch
{
    private static LmmConfigEntry<int> _volume;
    private static LmmConfigEntry<bool> _enabled;

    static Harmony_Patch()
    {
        var config = LmmConfigRegistration.GetConfigFile("MyMod", "My Mod", "1.0.0");
        _volume = config.Bind("Audio", "Volume", 50, "Master volume level");
        _enabled = config.Bind("General", "Enabled", true, "Enable the mod");
    }
}
```

You can also register individual settings without managing the config file directly:

```c#
var volume = LmmConfigRegistration.Register<int>(
    "MyMod", "My Mod", "Audio", "Volume", 50, "Master volume level", "1.0.0"
);
```

### How to make a slider

Specify `AcceptableValueRange` when creating your setting. If the range is 0f–1f or 0–100 the slider will be shown as a percentage (this can be overridden with `ConfigurationManagerAttributes`).

```c#
var volume = config.Bind(
    "Audio", "Volume", 50,
    new LmmConfigDescription("Master volume", new AcceptableValueRange<int>(0, 100))
);
```

### How to make a drop-down list

Specify `AcceptableValueList` when creating your setting. If you use an enum you don't need to specify `AcceptableValueList` — all enum values will be shown automatically.

You can add `System.ComponentModel.DescriptionAttribute` to enum items to override their displayed names:

```c#
public enum Difficulty
{
    Easy,
    [Description("Standard difficulty")]
    Normal,
    Hard
}
```

### How to use keyboard shortcuts

Add a setting of type `KeyboardShortcut`. Use `IsDown()` in your `Update` method to check for presses. The class handles modifier keys (Shift, Control, Alt) properly.

```c#
var hotkey = config.Bind("Hotkeys", "Toggle", new KeyboardShortcut(KeyCode.U, KeyCode.LeftShift));

// In Update():
if (hotkey.Value.IsDown()) { /* handle press */ }
```

## Overriding default display behavior

You can customize how a setting appears in the configuration manager by passing a `ConfigurationManagerAttributes` instance as a tag. Copy the [`ConfigurationManagerAttributes.cs`](LobCorp.ConfigurationManager/ConfigurationManagerAttributes.cs) file into your project and use it like this:

```c#
config.Bind("X", "1", 1, new LmmConfigDescription("", null,
    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 3 }));
config.Bind("X", "2", 2, new LmmConfigDescription("", null,
    new ConfigurationManagerAttributes { Order = 1 }));
```

Important notes about the attributes class:

- You do **not** need to reference ConfigurationManager.dll for this to work — it is read via reflection.
- This fork uses **public auto-properties** (not public fields). If copying from upstream BepInEx.ConfigurationManager, convert fields to auto-properties.
- Keep the class name `ConfigurationManagerAttributes` unchanged. You can remove properties you don't use.
- Avoid making the class public to prevent conflicts with other mods.

### Custom setting editors

For unsupported types, you can provide a custom ImGUI drawer:

```c#
config.Bind("Section", "Key", "value",
    new LmmConfigDescription("Description", null,
        new ConfigurationManagerAttributes { CustomDrawer = MyDrawer }));

static void MyDrawer(LmmConfigEntryBase entry)
{
    GUILayout.Label((string)entry.BoxedValue, GUILayout.ExpandWidth(true));
}
```

## BepInEx plugin compatibility

ConfigurationManager also discovers BepInEx plugins via reflection — no hard dependency is required. BepInEx plugins using `Config.Bind` will have their settings shown automatically.
