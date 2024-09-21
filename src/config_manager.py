# src/config_manager.py

import configparser
from pathlib import Path

class ConfigManager:
    def __init__(self):
        self.config_dir = Path.home() / '.config' / 'alkaline'
        self.config_file = self.config_dir / 'config.ini'
        self.config = configparser.ConfigParser()
        self.api_key = self.load_api_key()

    def load_api_key(self):
        if self.config_file.exists():
            self.config.read(self.config_file)
            return self.config.get('API', 'key', fallback=None)
        return None

    def save_api_key(self, key):
        if not self.config_dir.exists():
            self.config_dir.mkdir(parents=True)
        if 'API' not in self.config.sections():
            self.config.add_section('API')
        self.config.set('API', 'key', key)
        with open(self.config_file, 'w') as configfile:
            self.config.write(configfile)
        self.api_key = key
