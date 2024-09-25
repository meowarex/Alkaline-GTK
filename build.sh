#!/bin/bash

# Exit immediately if a command exits with a non-zero status
set -e

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to detect the package manager
detect_package_manager() {
    if [ -x "$(command -v apt-get)" ]; then
        echo "apt"
    elif [ -x "$(command -v dnf)" ]; then
        echo "dnf"
    elif [ -x "$(command -v yum)" ]; then
        echo "yum"
    elif [ -x "$(command -v pacman)" ]; then
        echo "pacman"
    elif [ -x "$(command -v zypper)" ]; then
        echo "zypper"
    else
        echo "unknown"
    fi
}

# Detect the package manager
PKG_MANAGER=$(detect_package_manager)

# Function to install packages
install_packages() {
    case $PKG_MANAGER in
        apt)
            sudo apt-get update
            sudo apt-get install -y "$@"
            ;;
        dnf)
            sudo dnf install -y "$@"
            ;;
        yum)
            sudo yum install -y "$@"
            ;;
        pacman)
            sudo pacman -Syu --noconfirm "$@"
            ;;
        zypper)
            sudo zypper install -y "$@"
            ;;
        *)
            echo "Unsupported package manager. Please install the following packages manually: $@"
            exit 1
            ;;
    esac
}

# Clean the Release directory
echo "Cleaning the Release directory..."
rm -rf Release/*

# Navigate to the src directory where the .csproj file is located
cd src

echo "Building the project..."

# Build the project with warnings suppressed
dotnet publish -c Release -r linux-x64 --self-contained true /p:WarningLevel=0

# Navigate back to the root directory
cd ..

echo "Installing AppImageKit..."

# Create Batteries directory if it doesn't exist
mkdir -p Batteries

# Check if appimagetool is already downloaded
if [ ! -f Batteries/appimagetool-x86_64.AppImage ]; then
    echo "Downloading appimagetool..."
    install_packages wget
    wget https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage -O Batteries/appimagetool-x86_64.AppImage
    chmod +x Batteries/appimagetool-x86_64.AppImage
else
    echo "Using existing appimagetool..."
fi

echo "Creating AppDir structure..."

# Create AppDir structure
mkdir -p AlkalineGTK.AppDir/usr/bin
mkdir -p AlkalineGTK.AppDir/usr/share/applications
mkdir -p AlkalineGTK.AppDir/usr/share/icons/hicolor/256x256/apps

echo "Copying files to AppDir..."

# Debug: List the contents of the publish directory
echo "Contents of src/bin/Release/net8.0/linux-x64/publish/:"
ls -l src/bin/Release/net8.0/linux-x64/publish/

# Ensure the AlkalineGTK executable exists and is executable
if [ ! -f src/bin/Release/net8.0/linux-x64/publish/AlkalineGTK ]; then
    echo -e "${RED}AlkalineGTK executable not found!${NC}"
    exit 1
fi

if [ ! -x src/bin/Release/net8.0/linux-x64/publish/AlkalineGTK ]; then
    echo -e "${RED}AlkalineGTK executable is not executable!${NC}"
    chmod +x src/bin/Release/net8.0/linux-x64/publish/AlkalineGTK
    echo -e "${GREEN}AlkalineGTK executable permissions fixed.${NC}"
fi

# Copy files to AppDir
cp -r src/bin/Release/net8.0/linux-x64/publish/* AlkalineGTK.AppDir/usr/bin/

# Debug: List the contents of the usr/bin directory
echo "Contents of AlkalineGTK.AppDir/usr/bin/:"
ls -l AlkalineGTK.AppDir/usr/bin/

echo "Creating .desktop file..."

# Create .desktop file
cat > AlkalineGTK.AppDir/alkalinegtk.desktop << EOL
[Desktop Entry]
Name=AlkalineGTK
Exec=AlkalineGTK
Icon=alkalinegtk
Type=Application
Categories=Utility;
EOL

# Copy .desktop file
cp AlkalineGTK.AppDir/alkalinegtk.desktop AlkalineGTK.AppDir/usr/share/applications/

echo "Creating AppRun file..."

# Create AppRun file
cat > AlkalineGTK.AppDir/AppRun << EOL
#!/bin/bash
HERE="$(dirname "$(readlink -f "${0}")")"
export PATH="${HERE}/usr/bin/:${PATH}"
export LD_LIBRARY_PATH="${HERE}/usr/lib/:${LD_LIBRARY_PATH}"
exec "${HERE}/usr/bin/AlkalineGTK" "$@"
EOL

chmod +x AlkalineGTK.AppDir/AppRun

echo "Copying icon file..."

# Copy icon file from Assets directory
if [ -f Assets/alkalinegtk.png ]; then
    cp Assets/alkalinegtk.png AlkalineGTK.AppDir/usr/share/icons/hicolor/256x256/apps/
    cp Assets/alkalinegtk.png AlkalineGTK.AppDir/alkalinegtk.png  # Add this line to copy the icon to the root of AppDir
    echo -e "${GREEN}Icon file copied successfully!${NC}"
else
    echo -e "${RED}Icon file not found in Assets directory!${NC}"
    exit 1
fi

echo "Verifying AppDir structure..."

# List the contents of the AppDir structure for debugging
ls -R AlkalineGTK.AppDir

echo "Deleting old AppImage files..."

# Delete old AppImage files
rm -f Release/AlkalineGTK*.AppImage

echo "Creating AppImage..."

# Create Release directory if it doesn't exist
mkdir -p Release

# Create AppImage
./Batteries/appimagetool-x86_64.AppImage AlkalineGTK.AppDir Release/AlkalineGTK.AppImage

echo -e "${GREEN}AppImage created successfully!${NC}"
echo -e "${GREEN}You can find the AppImage at: $(pwd)/Release/AlkalineGTK.AppImage${NC}"
echo -e "${GREEN}Build process completed.${NC}"

echo -e "\e[3;37mWaiting for 3 seconds...\e[0m"
sleep 3

# Prompt to execute the AppImage
echo -e "${YELLOW}Do you want to execute the AppImage now? (yes/no)${NC}"
read -r response
if [[ "$response" == "yes" ]]; then
    echo "Executing the AppImage with verbose output..."
    ./Release/AlkalineGTK.AppImage --verbose
fi