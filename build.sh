#!/bin/sh

dotnet build -c Release
if [ $? -ne 0 ]; then
    echo "✗ Build failed"
    exit 1
fi

# Copy DLL and assets to Subnautica BepInEx plugins folder
# Set SUBNAUTICA_PATH environment variable or edit here for your system
SUBNAUTICA_PATH="${SUBNAUTICA_PATH:-D:/SteamLibrary/steamapps/common/Subnautica}"
PLUGIN_DIR="$SUBNAUTICA_PATH/BepInEx/plugins/NamedSaves"
DLL_PATH="NamedSaves/bin/Release/net472/NamedSaves.dll"
LOCALIZATION_PATH="NamedSaves/Localizations.xml"
if [ -f "$DLL_PATH" ]; then
    mkdir -p "$PLUGIN_DIR"
    cp "$DLL_PATH" "$PLUGIN_DIR/"
    echo "✓ DLL copied to $PLUGIN_DIR"

    if [ -f "$LOCALIZATION_PATH" ]; then
        cp "$LOCALIZATION_PATH" "$PLUGIN_DIR/"
        echo "✓ Localizations.xml copied to $PLUGIN_DIR"
    else
        echo "✗ Localizations.xml not found at $LOCALIZATION_PATH"
    fi
else
    echo "✗ DLL not found at $DLL_PATH"
    echo "Make sure you have built the project in Release mode"
    exit 1
fi

echo "✓ Build and deployment complete!"