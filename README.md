# Alkaline File Converter

Alkaline is a file conversion tool integrated with the Dolphin file manager, allowing users to convert files using the CloudConvert API directly from the context menu.

## Features

- Right-click context menu integration with Dolphin.
- Converts files to various formats supported by CloudConvert.
- Modern GTK interface with easy-to-use dialogs.
- Stores and manages CloudConvert API key securely.

## Batteries

- CloudConvert Pro Account + API Key
- Dolphin File Manager
- GTK+ 3.0
- Python 3.x
- PyGObject
- requests

## Installation

1. **Clone the Repository:**

   ```bash
   git clone https://github.com/yourusername/alkaline.git

2. **Navigate to the Installers Directory:**

   ```bash
   cd alkaline/installers
   ```

3. **Run the Installer:**

   ```bash
   chmod +x install.sh
   ./install.sh
   ```

4. **Restart Dolphin File Manager:**
   
   Close all instances of Dolphin File Manager and reopen it.

## Usage
- Convert a File:
  1. Right-click on a file in Dolphin.
  2. Navigate to `Actions > Convert with Alkaline...`.
  3. The Alkaline application will launch.
  4. If prompted, enter your CloudConvert API key.
  5. Select the desired output format from the dropdown menu.
  6. Click `Convert` to start the conversion process.
  7. Once conversion is complete, click `Download` to save the converted file.

- Set or Change API Key:
  1. Click on the key icon in the title bar of the Alkaline window.
  2. Enter your CloudConvert API key and click `OK`.

## Configuration

Alkaline stores your CloudConvert API key in ~/.config/alkaline/config.ini. The file is created automatically after entering your API key for the first time.

Security Note: It's recommended to restrict access to the configuration file:

```bash
chmod 600 ~/.config/alkaline/config.ini
```

## Uninstallation

1. **Navigate to the Installers Directory:**

   ```bash
   cd alkaline/installers
   ```

2. **Run the Uninstaller:**

   ```bash
   chmod +x uninstall.sh
   ./uninstall.sh
   ```

3. **Restart Dolphin File Manager:**

   Close all instances of Dolphin File Manager and reopen it.

