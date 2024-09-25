#!/bin/bash

# Exit immediately if a command exits with a non-zero status
set -e

# Get the absolute path to the directory containing this script
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Set the project root directory (assuming it's the parent of Installers/)
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

# Change to the project root directory
cd "$PROJECT_ROOT"

# Variables
APP_NAME="Alkaline"
APP_ID="com.atomix.Alkaline"
BUILD_DIR="$PROJECT_ROOT/build-dir"
DIST_DIR="$PROJECT_ROOT/dist"
FLATPAK_YAML="$PROJECT_ROOT/$APP_ID.yaml"
GNOME_SDK_VERSION="47"

# Create necessary directories
mkdir -p "$BUILD_DIR"
mkdir -p "$DIST_DIR"

echo "Starting the installation and build process for $APP_NAME..."

# Function to check network connectivity
check_network() {
    echo "Checking network connectivity..."
    if ! ping -c 3 archlinux.org &> /dev/null; then
        echo "Error: Unable to reach archlinux.org. Please check your internet connection."
        exit 1
    fi
}

# Call the network check before installing dependencies
check_network

# 1. Install necessary dependencies using pacman

echo "Installing necessary dependencies..."

# Function to check if running as root
check_root() {
    if [ "$(id -u)" != "0" ]; then
        echo "This script must be run as root" 1>&2
        exit 1
    fi
}

# Check if running as root
check_root

# Update package database
pacman -Sy

# Install Python and required packages
pacman -S --noconfirm python python-gobject python-requests

# Function to check if a command exists and install if missing
check_and_install() {
    if ! command -v "$1" &> /dev/null; then
        echo "$1 not found."
        if [ "$1" == "flatpak-builder" ]; then
            read -p "Do you want to install $1? [Y/n]: " choice
            case "$choice" in
                [Nn]* )
                    echo "Skipping $1 installation."
                    ;;
                * )
                    echo "Installing $1 using pacman..."
                    pacman -S --noconfirm flatpak-builder
                    ;;
            esac
        else
            read -p "Do you want to install $1? [Y/n]: " choice
            case "$choice" in
                [Nn]* )
                    echo "Skipping $1 installation."
                    ;;
                * )
                    echo "Installing $1..."
                    wget -O "$1" "$2"
                    chmod +x "$1"
                    mv "$1" /usr/local/bin/
                    ;;
            esac
        fi
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

# Function to check and install Flatpak runtime
check_and_install_flatpak_runtime() {
    RUNTIME=$1
    VERSION=$2
    if ! flatpak info "$RUNTIME//$VERSION" &> /dev/null; then
        echo "$RUNTIME version $VERSION not found."
        read -p "Do you want to install $RUNTIME//$VERSION? [Y/n]: " choice
        case "$choice" in
            [Nn]* )
                echo "Cannot proceed without installing $RUNTIME//$VERSION."
                exit 1
                ;;
            * )
                echo "Installing $RUNTIME//$VERSION..."
                flatpak install -y flathub "$RUNTIME//$VERSION"
                ;;
        esac
    else
        echo "$RUNTIME version $VERSION is already installed."
    fi
}

# Check and install GNOME SDK and Platform version 47
echo "Checking for GNOME SDK and Platform version $GNOME_SDK_VERSION..."

check_and_install_flatpak_runtime "org.gnome.Sdk" "$GNOME_SDK_VERSION"
check_and_install_flatpak_runtime "org.gnome.Platform" "$GNOME_SDK_VERSION"

# 2. Build the Flatpak package

echo "Building the Flatpak package..."

# Check if the Flatpak manifest file exists
if [ ! -f "$FLATPAK_YAML" ]; then
    echo "Flatpak manifest $FLATPAK_YAML not found!"
    exit 1
fi

# Clean previous build
sudo rm -rf "$BUILD_DIR" repo

# Build and install locally for testing
sudo flatpak-builder --user --install --force-clean "$BUILD_DIR" "$FLATPAK_YAML"

# Build the Flatpak package
sudo flatpak-builder --repo=repo --force-clean "$BUILD_DIR" "$FLATPAK_YAML"
sudo flatpak build-bundle repo "$DIST_DIR/alkaline.flatpak" "$APP_ID"

# Change ownership of the built files back to the current user
sudo chown -R $(id -u):$(id -g) "$BUILD_DIR" repo "$DIST_DIR/alkaline.flatpak"

# 3. Build the AppImage package

echo "Building the AppImage package..."

# Clean previous AppDir
rm -rf "$PROJECT_ROOT/$APP_NAME.AppDir"

# Create AppDir structure
mkdir -p "$PROJECT_ROOT/$APP_NAME.AppDir/usr/bin"
mkdir -p "$PROJECT_ROOT/$APP_NAME.AppDir/usr/share/icons/hicolor/48x48/apps"
mkdir -p "$PROJECT_ROOT/$APP_NAME.AppDir/usr/share/applications"

# Copy application files
cp "$PROJECT_ROOT/src/"*.py "$PROJECT_ROOT/$APP_NAME.AppDir/usr/bin/"
cp "$PROJECT_ROOT/assets/icons/alkaline.png" "$PROJECT_ROOT/$APP_NAME.AppDir/usr/share/icons/hicolor/48x48/apps/$APP_ID.png"
cp "$PROJECT_ROOT/data/$APP_ID.desktop" "$PROJECT_ROOT/$APP_NAME.AppDir/usr/share/applications/"

# Create AppRun script
cat > "$PROJECT_ROOT/$APP_NAME.AppDir/AppRun" <<EOL
#!/bin/bash
HERE="\$(dirname "\$(readlink -f "\$0")")"
export PYTHONPATH="\$HERE/usr/bin"
exec alkaline "\$@"
EOL
chmod +x "$PROJECT_ROOT/$APP_NAME.AppDir/AppRun"

# Copy license and other files
cp "$PROJECT_ROOT/LICENSE" "$PROJECT_ROOT/$APP_NAME.AppDir/"

# Use linuxdeploy to bundle dependencies and build AppImage
ARCH=x86_64 linuxdeploy --appdir="$PROJECT_ROOT/$APP_NAME.AppDir" --output appimage

# Move the AppImage to the dist directory
mv "$PROJECT_ROOT/$APP_NAME"-*.AppImage "$DIST_DIR/"

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
