# src/alkaline_app.py

#!/usr/bin/env python3

import sys
from gi.repository import Gtk, GLib
from config_manager import ConfigManager
from cloudconvert_api import CloudConvertAPI
from ui_components import UIComponents
from utils import unquote_file_path

class AlkalineApp(Gtk.Window):
    def __init__(self):
        super().__init__(title="Alkaline File Converter")
        self.file_paths = []
        self.config_manager = ConfigManager()
        self.api_client = CloudConvertAPI(self.config_manager.api_key)
        self.selected_format = None

        self.set_border_width(10)
        self.set_default_size(600, 400)

        self.ui = UIComponents(self)
        self.init_ui()

        if self.api_client.api_key:
            pass  # Wait for user to select files
        else:
            self.prompt_api_key()

    def init_ui(self):
        self.ui.build_header_bar()
        self.ui.build_main_layout()

    def prompt_api_key(self):
        self.ui.show_api_key_prompt(self.on_api_key_entered)

    def on_api_key_entered(self, api_key):
        if api_key:
            self.config_manager.save_api_key(api_key)
            self.api_client.api_key = api_key
        else:
            self.ui.show_error("API key cannot be empty.")
            self.close()

    def on_file_selected(self, file_paths):
        self.file_paths = file_paths
        self.get_formats()

    def get_formats(self):
        if not self.file_paths:
            return
        self.ui.set_loading_state(True)
        self.api_client.fetch_formats(self.file_paths[0], self.on_formats_fetched)

    def on_formats_fetched(self, formats, error):
        self.ui.set_loading_state(False)
        if error:
            self.ui.show_error(error)
        else:
            self.ui.populate_formats(formats)

    def on_format_selected(self, format):
        self.selected_format = format
        self.ui.enable_convert_button(True)

    def on_convert_clicked(self):
        if self.selected_format and self.file_paths:
            self.ui.set_loading_state(True)
            self.api_client.convert_files(
                self.file_paths,
                self.selected_format,
                self.on_conversion_complete
            )

    def on_conversion_complete(self, results, error):
        self.ui.set_loading_state(False)
        if error:
            self.ui.show_error(error)
        else:
            self.results = results  # List of (input_file, output_url) tuples
            self.ui.show_download_button(True)

    def on_download_clicked(self):
        self.ui.set_loading_state(True)
        self.api_client.download_files(
            self.results,
            self.selected_format,
            self.on_download_complete
        )

    def on_download_complete(self, output_paths, error):
        self.ui.set_loading_state(False)
        if error:
            self.ui.show_error(error)
        else:
            message = "Files downloaded:\n" + "\n".join(output_paths)
            self.ui.show_info(message)
            self.ui.reset_ui()

def main():
    app = AlkalineApp()
    app.connect("destroy", Gtk.main_quit)
    app.show_all()
    Gtk.main()

if __name__ == "__main__":
    main()
