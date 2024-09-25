# AlkalineGTK

AlkalineGTK is a cross-platform desktop application built with GTK# that allows users to convert files between different formats using the CloudConvert API.

## Features

- Select input files using a file chooser
- Choose output directory for converted files
- Dynamically load supported output formats based on the input file
- Convert files to selected formats using CloudConvert API
- Simple and intuitive user interface

## Prerequisites

- CloudConvert Pro API key
- Dolphin
- Compatible System with GTK 3-4



# Installation

### Clone the Repository

- Clone the repository:
   ```bash
   git clone https://github.com/meowarex/AlkalineGTK.git
   cd AlkalineGTK
   ```



# Building from source <3

## Make Script Executable

1. Open a terminal in the project root directory.
2. Make the scripts executable:
   ```bash
   chmod +x install.sh
   ```

## Run the Installation Script

3. Run the installation script:
   ```bash
   ./install.sh
   ```

This script will detect your distribution, install .NET SDK, GTK#, and other required batteries using the appropriate package manager. It will then prompt you if you want to build the application as an AppImage.

### Batteries Included <3

- GtkSharp (3.24.24.95)
- RestSharp (110.2.0)
- Newtonsoft.Json (13.0.3)


## Usage

1. Run the application:
   ```bash
   cd Release
   ./AlkalineGTK
   ```

2. Use the file chooser to select an input file
3. Choose an output directory
4. Select the desired output format from the dropdown
5. Click "Convert" to start the conversion process



# Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [CloudConvert](https://cloudconvert.com/) for providing the file conversion API
- [GTK#](https://github.com/GtkSharp/GtkSharp) for the cross-platform GUI framework

