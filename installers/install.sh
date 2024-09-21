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
#!/bin/bash

# Exit immediately if a command exits with a non-zero status
set -e

# Variables
APP_NAME="Alkaline"
APP_ID="com.example.Alkaline"
BUILD_DIR="build-dir"
DIST_DIR="dist"
FLATPAK_YAML="$APP_ID.yaml"

# Create necessary directories
mkdir -p "$BUILD_DIR"
mkdir -p "$DIST_DIR"

echo "Starting the installation and build process for $APP_NAME..."

# 1. Install necessary dependencies

echo "Installing necessary dependencies..."

# Install Python dependencies
pip3 install --user -r requirements.txt

# Function to check if a command exists and install if missing
check_and_install() {
    if ! command -v "$1" &> /dev/null; then
        echo "$1 not found."
        read -p "Do you want to install $1? [Y/n]: " choice
        case "$choice" in
            [Nn]* )
                echo "Skipping $1 installation."
                ;;
            * )
                if [ "$(id -u)" -ne 0 ]; then
                    SUDO='sudo'
                fi
                if [[ "$1" == "flatpak-builder" ]]; then
                    echo "Installing $1..."
                    $SUDO apt-get update
                    $SUDO apt-get install -y flatpak-builder
                else
                    echo "Installing $1..."
                    wget -O "$1" "$2"
                    chmod +x "$1"
                    $SUDO mv "$1" /usr/local/bin/
                fi
                ;;
        esac
    else
        echo "$1 is already installed."
    fi
}

# Check and install flatpak-builder
check_and_install "flatpak-builder"

# Check and install appimagetool
check_and_install "appimagetool" "https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage"

# Check and install linuxdeploy
check_and_install "linuxdeploy" "https://github.com/linuxdeploy/linuxdeploy/releases/download/continuous/linuxdeploy-x86_64.AppImage"

# 2. Build the Flatpak package

echo "Building the Flatpak package..."

# Check if the Flatpak manifest file exists
if [ ! -f "$FLATPAK_YAML" ]; then
    echo "Flatpak manifest $FLATPAK_YAML not found!"
    exit 1
fi

# Clean previous build
rm -rf "$BUILD_DIR"

# Build and install locally for testing
flatpak-builder --user --install --force-clean "$BUILD_DIR" "$FLATPAK_YAML"

# Build the Flatpak package
flatpak-builder --repo=repo --force-clean "$BUILD_DIR" "$FLATPAK_YAML"
flatpak build-bundle repo "$DIST_DIR/alkaline.flatpak" "$APP_ID"

# 3. Build the AppImage package

echo "Building the AppImage package..."

# Clean previous AppDir
rm -rf "$APP_NAME.AppDir"

# Create AppDir structure
mkdir -p "$APP_NAME.AppDir/usr/bin"
mkdir -p "$APP_NAME.AppDir/usr/share/icons/hicolor/48x48/apps"
mkdir -p "$APP_NAME.AppDir/usr/share/applications"

# Copy application files
cp src/*.py "$APP_NAME.AppDir/usr/bin/"
cp assets/icons/alkaline.png "$APP_NAME.AppDir/usr/share/icons/hicolor/48x48/apps/$APP_ID.png"
cp data/$APP_ID.desktop "$APP_NAME.AppDir/usr/share/applications/"

# Create AppRun script
cat > "$APP_NAME.AppDir/AppRun" <<EOL
#!/bin/bash
HERE="\$(dirname "\$(readlink -f "\$0")")"
export PYTHONPATH="\$HERE/usr/bin"
exec python3 "\$HERE/usr/bin/alkaline_app.py" "\$@"
EOL
chmod +x "$APP_NAME.AppDir/AppRun"

# Copy license and other files
cp LICENSE "$APP_NAME.AppDir/"

# Use linuxdeploy to bundle dependencies and build AppImage
linuxdeploy --appdir="$APP_NAME.AppDir" --output appimage

# Move the AppImage to the dist directory
mv "$APP_NAME"-*.AppImage "$DIST_DIR/"

echo "Build process completed. Packages are available in the $DIST_DIR directory."

# 4. Ask the user if they want to run the test

read -p "Do you want to test the application now? [Y/n]: " run_test
case "$run_test" in
    [Nn]* )
        echo "Skipping test run."
        ;;
    * )
        echo "Running the application..."

        # Ask whether to run Flatpak or AppImage version
        echo "Which version do you want to test?"
        select version in "Flatpak" "AppImage" "Cancel"; do
            case $version in
                Flatpak)
                    flatpak run "$APP_ID"
                    break
                    ;;
                AppImage)
                    chmod +x "$DIST_DIR/$APP_NAME"-*.AppImage
                    "$DIST_DIR/$APP_NAME"-*.AppImage
                    break
                    ;;
                Cancel)
                    echo "Test run cancelled."
                    break
                    ;;
                *)
                    echo "Invalid option. Please select 1, 2, or 3."
                    ;;
            esac
        done
        ;;
esac

echo "Installation and build process for $APP_NAME completed successfully!"
