#!/bin/bash

# Exit immediately if a command exits with a non-zero status
set -e

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

echo "Detected package manager: $PKG_MANAGER"

# Install dependencies
install_packages wget

# Download and install the Microsoft package signing key
case $PKG_MANAGER in
    apt)
        wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
        sudo dpkg -i packages-microsoft-prod.deb
        rm packages-microsoft-prod.deb
        sudo apt-get update
        ;;
    dnf|yum)
        sudo rpm -Uvh https://packages.microsoft.com/config/rhel/7/packages-microsoft-prod.rpm
        ;;
    pacman)
        # Arch Linux uses its own repositories for .NET
        ;;
    zypper)
        sudo rpm -Uvh https://packages.microsoft.com/config/opensuse/15/packages-microsoft-prod.rpm
        ;;
esac

# Install .NET SDK (latest version)
case $PKG_MANAGER in
    apt|dnf|yum|zypper)
        install_packages dotnet-sdk
        ;;
    pacman)
        install_packages dotnet-sdk
        ;;
esac

# Install GTK# dependencies
case $PKG_MANAGER in
    apt)
        install_packages gtk-sharp3
        ;;
    dnf|yum)
        install_packages gtk-sharp3
        ;;
    pacman)
        install_packages gtk-sharp-3
        ;;
    zypper)
        install_packages gtk-sharp3
        ;;
esac

# Install additional dependencies (if needed)
# install_packages <additional-package-names>

# Verify installation
dotnet --version

echo "${GREEN}Batteries Included successfully!${NC}"

echo -e "\e[3;37mWaiting for 3 seconds...\e[0m"
sleep 3


# Prompt to run build script
read -p "Do you want to build the application now? (y/n) " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]
then
    ./build.sh
fi