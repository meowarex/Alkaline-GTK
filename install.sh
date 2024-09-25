#!/bin/bash

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Unicode characters for tick and cross
TICK="✓"
CROSS="✗"

# Function to check if a file exists and print result
check_file() {
    if [ -f "$1" ]; then
        echo -e "${GREEN}${TICK} $1 exists${NC}"
    else
        echo -e "${RED}${CROSS} $1 is missing${NC}"
        return 1
    fi
}

# Function to check if a directory exists and print result
check_directory() {
    if [ -d "$1" ]; then
        echo -e "${GREEN}${TICK} $1 directory exists${NC}"
    else
        echo -e "${RED}${CROSS} $1 directory is missing${NC}"
        return 1
    fi
}

# Array of files to check
files=(
    "batteries.sh"
    "build.sh"
    "src/Program.cs"
    "src/MainWindow.cs"
    "src/Utils/CloudConvertAPI.cs"
    "src/AlkalineGTK.csproj"
    "Components/MainWindow.ui"
)

# Array of directories to check
directories=(
    "src"
    "Components"
)

echo "Checking project structure..."

# Check directories
for dir in "${directories[@]}"; do
    if ! check_directory "$dir"; then
        echo -e "${RED}Invalid project structure. Aborting installation.${NC}"
        exit 1
    fi
done

# Check files
for file in "${files[@]}"; do
    if ! check_file "$file"; then
        echo -e "${RED}Invalid project structure. Aborting installation.${NC}"
        exit 1
    fi
done

echo -e "${GREEN}All required files and directories are present.${NC}"

# Make scripts executable
echo "Making scripts executable..."
chmod +x batteries.sh build.sh
echo -e "${GREEN}${TICK} Scripts are now executable${NC}"

echo -e "${GREEN}Installation process completed.${NC}"

echo -e "\e[3;37mWaiting for 3 seconds...\e[0m"
sleep 3

# Run batteries.sh
echo "Running batteries.sh..."
./batteries.sh