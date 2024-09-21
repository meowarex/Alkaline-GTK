#!/bin/bash

# Install Python dependencies
pip3 install --user -r ../requirements.txt

# Copy the Python script to /usr/local/bin/
sudo cp ../src/alkaline_app.py /usr/local/bin/alkaline
sudo chmod +x /usr/local/bin/alkaline

# Copy the service menu file
SERVICE_MENU_DIR="$HOME/.local/share/kservices5/ServiceMenus"
mkdir -p "$SERVICE_MENU_DIR"
cp ../service_menu/alkaline.desktop "$SERVICE_MENU_DIR/"

# Copy icons if necessary
ICON_DIR="$HOME/.local/share/icons/hicolor/48x48/apps"
mkdir -p "$ICON_DIR"
cp ../assets/icons/alkaline.png "$ICON_DIR/"

# Update icon cache
gtk-update-icon-cache "$HOME/.local/share/icons/hicolor"

echo "Installation complete. Restart Dolphin to see the 'Convert with Alkaline...' option."
