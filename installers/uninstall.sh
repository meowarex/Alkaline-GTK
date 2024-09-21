#!/bin/bash

# Remove the application script
sudo rm /usr/local/bin/alkaline

# Remove the service menu entry
rm "$HOME/.local/share/kservices5/ServiceMenus/alkaline.desktop"

# Remove the icon
rm "$HOME/.local/share/icons/hicolor/48x48/apps/alkaline.png"

# Update icon cache
gtk-update-icon-cache "$HOME/.local/share/icons/hicolor"

# Remove configuration files
rm -rf "$HOME/.config/alkaline"

echo "Uninstallation complete."
