# src/utils.py

from urllib.parse import unquote

def unquote_file_path(file_path):
    return unquote(file_path)
