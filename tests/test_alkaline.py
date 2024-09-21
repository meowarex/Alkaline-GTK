# tests/test_alkaline.py

import unittest
from src.config_manager import ConfigManager
from src.utils import unquote_file_path

class TestConfigManager(unittest.TestCase):
    def test_save_and_load_api_key(self):
        config_manager = ConfigManager()
        test_key = 'test_api_key'
        config_manager.save_api_key(test_key)
        loaded_key = config_manager.load_api_key()
        self.assertEqual(test_key, loaded_key)

class TestUtils(unittest.TestCase):
    def test_unquote_file_path(self):
        quoted_path = '/home/user/Documents/Test%20File.txt'
        unquoted_path = unquote_file_path(quoted_path)
        self.assertEqual(unquoted_path, '/home/user/Documents/Test File.txt')

if __name__ == '__main__':
    unittest.main()
