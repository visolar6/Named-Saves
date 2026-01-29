# Named Saves

This mod allows you to assign custom names to your save files in the game, making it easier to organize and identify your saves.

## Features
- Assign and edit custom names for each save file directly from the main menu.
- Edit button and input placeholder text are fully localizable.
- Localization support via `Localizations.xml` for multiple languages.

## Localization
To add or edit translations, open `NamedSaves/Localizations.xml` and add or update the following keys in the appropriate `<LocalizationPackage Lang="...">` section:

- `NamedSaves_EditButton`: The text for the edit button next to each save (e.g., "Edit name").
- `NamedSaves_InputPlaceholder`: The placeholder text for the input field (e.g., "Enter custom name...").

Example:
```xml
<Text key="NamedSaves_EditButton">Edit name</Text>
<Text key="NamedSaves_InputPlaceholder">Enter custom name...</Text>
```

## Example Use-Case: Managing Multiple Vortex Profiles

Suppose you use Vortex to manage several Subnautica mod profiles, each with its own save file. With Named Saves, you can:

- Assign a unique, descriptive name to each save (e.g., "Vanilla Playthrough", "Hardcore Mods", "Creative Sandbox").
- Instantly see which save belongs to which Vortex profile from the in-game menu.
- Edit these names at any time without affecting the actual save data or Vortex profile structure.

This makes it easy to keep track of multiple playthroughs, mod setups, or experimental runs, especially when switching between different Vortex profiles.

## Building
Run `./build.sh` to build the project.

## License
See LICENSE for details.