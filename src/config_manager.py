# config_manager.py

import json
import os
from gi.repository import GLib

class ConfigManager:
    def __init__(self):
        config_dir = os.path.join(GLib.get_user_config_dir(), "alkaline")
        os.makedirs(config_dir, exist_ok=True)
        self.config_file = os.path.join(config_dir, "config.json")
        self.config = {}
        self.load_config()

    def load_config(self):
        if os.path.exists(self.config_file):
            with open(self.config_file, "r") as f:
                self.config = json.load(f)
        else:
            self.config = {"api_key": ""}

    def save_config(self):
        with open(self.config_file, "w") as f:
            json.dump(self.config, f, indent=4)

    def get_api_key(self):
        return self.config.get("api_key", "")

    def set_api_key(self, api_key):
        self.config["api_key"] = api_key
        self.save_config()
