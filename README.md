### This ReadMe is AI Generated for reference purposes only
- This will be completely rewritten in the future

# AlkalineGTK

AlkalineGTK is a cross-platform desktop application built with GTK# that allows users to convert files between different formats using the CloudConvert API.

## Features

- Select input files using a file chooser
- Choose output directory for converted files
- Dynamically load supported output formats based on the input file
- Convert files to selected formats using CloudConvert API
- Simple and intuitive user interface

## Prerequisites

- Latest .NET SDK (targeting .NET 9 when available)
- GTK# 4.6.5 or later
- CloudConvert API key

## Installation

1. Clone the repository:
   ```
   git clone https://github.com/yourusername/AlkalineGTK.git
   cd AlkalineGTK
   ```

2. Set up your CloudConvert API key:
   - Sign up for a CloudConvert account and obtain an API key
   - Set the API key as an environment variable:
     ```
     export CLOUDCONVERT_API_TOKEN=your_api_key_here
     ```

3. Build the project:
   ```
   dotnet build
   ```

## Usage

1. Run the application:
   ```
   dotnet run
   ```

2. Use the file chooser to select an input file
3. Choose an output directory
4. Select the desired output format from the dropdown
5. Click "Convert" to start the conversion process

## Dependencies

- GtkSharp (4.6.5)
- RestSharp (110.4.0)
- Newtonsoft.Json (13.0.3)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [CloudConvert](https://cloudconvert.com/) for providing the file conversion API
- [GTK#](https://github.com/GtkSharp/GtkSharp) for the cross-platform GUI framework

## Installation on Linux

For Linux users, we provide a script that sets up the necessary dependencies and optionally builds the application. This script supports various Linux distributions, including Debian-based, Red Hat-based, Arch Linux, and openSUSE.

1. Open a terminal in the project root directory.
2. Make the scripts executable:
   ```
   chmod +x batteries.sh build.sh
   ```
3. Run the installation script:
   ```
   ./batteries.sh
   ```

This script will detect your distribution, install .NET SDK, GTK#, and other required dependencies using the appropriate package manager. It will then prompt you if you want to build the application as an AppImage.

If you choose to build the application later, you can do so by running:
```
./build.sh
```

This will create an AppImage of AlkalineGTK in your project directory.
