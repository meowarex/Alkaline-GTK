#!/usr/bin/env python3
# alkaline_app.py

import gi
gi.require_version('Gtk', '4.0')
from gi.repository import Gtk, Gio

from config_manager import ConfigManager
from cloudconvert_api import CloudConvertAPI
from ui_components import create_main_ui

class AlkalineApp(Gtk.Application):
    def __init__(self):
        super().__init__(application_id="com.atomix.Alkaline")
        self.config_manager = ConfigManager()
        self.cloudconvert_api = CloudConvertAPI(self.config_manager)
        self.window = None

    def do_activate(self):
        if not self.window:
            self.window = Gtk.ApplicationWindow(application=self)
            self.window.set_title("Alkaline")
            self.window.set_default_size(800, 600)

            # Set margins
            self.window.set_margin_top(10)
            self.window.set_margin_bottom(10)
            self.window.set_margin_start(10)
            self.window.set_margin_end(10)

            # Create main UI
            main_ui = create_main_ui(self)
            self.window.set_child(main_ui)

        self.window.present()

def main():
    app = AlkalineApp()
    app.run(None)

if __name__ == "__main__":
    main()
