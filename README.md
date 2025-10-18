# KenshiLib

Core library for Kenshi mod management, providing comprehensive functionality for mod discovery, playset management, and load order handling.

## Features

- **Mod Discovery** - Automatic detection of Steam Workshop and local mods
- **Playset Management** - Create, load, save, and switch between mod configurations
- **Load Order** - Handle mod ordering and dependencies
- **Mod Metadata** - Extract mod information (name, author, description, preview image)
- **Steam Integration** - Read Steam Workshop subscriptions and library paths
- **Translation Support** - Infrastructure for mod translation and glossary management
- **File Persistence** - Read/write `.cfg` playset files with enabled/disabled state

## Installation

Add as NuGet package reference in your `.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="KenshiLib" Version="1.0.0" />
</ItemGroup>
```

Or via dotnet CLI:

```bash
dotnet add package KenshiLib
```

## Basic Usage

### Initialize Mod Manager

```csharp
using KenshiLib.Core;

// Initialize reverse engineer (for mod file parsing)
var reverseEngineer = new ReverseEngineer();

// Create mod manager
var modManager = new ModManager(reverseEngineer);

// Load all available mods with details
var allMods = await modManager.LoadAllModsWithDetailsAsync();

foreach (var mod in allMods)
{
    Console.WriteLine($"{mod.Name} by {mod.Author}");
    Console.WriteLine($"  Enabled: {mod.IsEnabled}, LoadOrder: {mod.LoadOrder}");
}
```

### Manage Playsets

```csharp
using KenshiLib.Core;

string kenshiPath = ModManager.KenshiPath; // Auto-detected or custom

// Create playset repository
var playsetRepo = new PlaysetRepository(kenshiPath);

// Get all playsets
var playsets = playsetRepo.GetAllPlaysets();

// Create new playset
var newPlayset = playsetRepo.CreateNewPlayset("MyModpack");

// Load mods from playset
var modEntries = playsetRepo.LoadPlaysetModsWithState(newPlayset.FilePath);

// Save mods to playset (with enabled/disabled state)
playsetRepo.SaveModsToPlaysetWithState(newPlayset.FilePath, modsList);
```

### Detect Kenshi Installation

```csharp
using KenshiLib.Core;

// Auto-detect Kenshi path
string? kenshiPath = ModManager.FindKenshiInstallDir();

if (kenshiPath != null)
{
    Console.WriteLine($"Kenshi found at: {kenshiPath}");

    // Find workshop mods path
    string? workshopPath = ModManager.FindWorkshopPath();
    Console.WriteLine($"Workshop mods: {workshopPath}");
}
```

## Core Classes

### `ModManager`
Main class for mod discovery and loading. Handles both Steam Workshop and local mods.

### `PlaysetRepository`
Manages playset files (`.cfg` format). CRUD operations for playlists of mods.

### `ModInfo`
Data class representing a mod with properties:
- `Name`, `Author`, `Description`
- `IsEnabled`, `LoadOrder`
- `ModPath`, `ImagePath`
- `IsSteamWorkshop`

### `PlaysetInfo`
Metadata about a playset:
- `Name`, `FilePath`
- `ModCount`
- `CreatedDate`, `ModifiedDate`

### `ReverseEngineer`
Low-level mod file parser (reads `.mod` files).

## Requirements

- **.NET 9.0** or later
- **Windows** (uses Windows Registry for Steam path detection)
- **Kenshi** installation (Steam or standalone)

## Dependencies

- `Google.Cloud.Translate.V3` - Translation API support
- `DeepMorphy` - Russian morphology for translation post-processing
- `TensorFlowSharp` - ML model support for DeepMorphy
- Other translation-related packages

## Platform Notes

This library is Windows-specific due to:
- Windows Registry usage for Steam path detection
- File path conventions (`C:\Program Files\...`)
- Kenshi game is Windows-only

Methods using Registry are marked with `[SupportedOSPlatform("windows")]`.

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Contributing

This library is part of the Kenshi mod management ecosystem. For issues or feature requests, please open an issue on GitHub.

## Related Projects

- **KenshiModManager** - WPF application using this library for mod management UI
