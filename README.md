# In-game configuration manager for Lobotomy Corporation (LMM)

Fork of [BepInEx.ConfigurationManager](https://github.com/BepInEx/BepInEx.ConfigurationManager) adapted for Lobotomy Corporation's mod loader (LMM) and Harmony 1. Provides an in-game ImGUI settings window for LMM and BepInEx mods. Press **F1** to open. Hover over setting names to see their descriptions.

![Configuration manager](Screenshot.PNG)

## Installation

Copy `ConfigurationManager.dll` into your Lobotomy Corporation mods folder. The configuration manager will load automatically via LMM.

## How to make my mod compatible

### Registering settings

Use `LmmConfigRegistration` to register your mod's settings. ConfigurationManager will display them automatically, including any metadata (descriptions, value ranges, acceptable value lists).

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
