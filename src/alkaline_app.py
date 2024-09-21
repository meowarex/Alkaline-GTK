# src/alkaline_app.py

#!/usr/bin/env python3

import sys
from gi.repository import Gtk, GLib
from config_manager import ConfigManager
from cloudconvert_api import CloudConvertAPI
from ui_components import UIComponents
from utils import unquote_file_path

class AlkalineApp(Gtk.Window):
    def __init__(self, file_path):
        super().__init__(title="Alkaline File Converter")
        self.file_path = unquote_file_path(file_path)
        self.config_manager = ConfigManager()
        self.api_client = CloudConvertAPI(self.config_manager.api_key)
        self.selected_format = None

        self.set_border_width(10)
        self.set_default_size(400, 150)

        self.ui = UIComponents(self)
        self.init_ui()

        if self.api_client.api_key:
            self.get_formats()
        else:
            self.prompt_api_key()

    def init_ui(self):
        # Build the UI components
        self.ui.build_header_bar()
        self.ui.build_main_layout()

    def prompt_api_key(self):
        # Show a dialog to prompt the API key
        self.ui.show_api_key_prompt(self.on_api_key_entered)

    def on_api_key_entered(self, api_key):
        if api_key:
            self.config_manager.save_api_key(api_key)
            self.api_client.api_key = api_key
            self.get_formats()
        else:
            self.ui.show_error("API key cannot be empty.")
            self.close()

    def get_formats(self):
        # Fetch supported formats
        self.ui.set_loading_state(True)
        self.api_client.fetch_formats(self.file_path, self.on_formats_fetched)

    def on_formats_fetched(self, formats, error):
        self.ui.set_loading_state(False)
        if error:
            self.ui.show_error(error)
            self.close()
        else:
            self.ui.populate_formats(formats)

    def on_format_selected(self, format):
        self.selected_format = format
        self.ui.enable_convert_button(True)

    def on_convert_clicked(self):
        if self.selected_format:
            self.ui.set_loading_state(True)
            self.api_client.convert_file(
                self.file_path,
                self.selected_format,
                self.on_conversion_complete
            )

    def on_conversion_complete(self, output_url, error):
        self.ui.set_loading_state(False)
        if error:
            self.ui.show_error(error)
        else:
            self.output_url = output_url
            self.ui.show_download_button(True)

    def on_download_clicked(self):
        self.ui.set_loading_state(True)
        self.api_client.download_file(
            self.output_url,
            self.file_path,
            self.selected_format,
            self.on_download_complete
        )

    def on_download_complete(self, output_path, error):
        self.ui.set_loading_state(False)
        if error:
            self.ui.show_error(error)
        else:
            self.ui.show_info(f"File downloaded to {output_path}")
            self.close()

def main():
    if len(sys.argv) != 2:
        print("Usage: alkaline_app.py <file_path>")
        sys.exit(1)

    app = AlkalineApp(sys.argv[1])
    app.connect("destroy", Gtk.main_quit)
    app.show_all()
    Gtk.main()

if __name__ == "__main__":
    main()
