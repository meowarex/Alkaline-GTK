# src/cloudconvert_api.py

import threading
import requests
import time
import os
from gi.repository import GLib

class CloudConvertAPI:
    def __init__(self, api_key):
        self.api_key = api_key

    def fetch_formats(self, file_path, callback):
        threading.Thread(target=self._fetch_formats_thread, args=(file_path, callback)).start()

    def _fetch_formats_thread(self, file_path, callback):
        headers = {
            'Authorization': f'Bearer {self.api_key}',
        }
        response = requests.get('https://api.cloudconvert.com/v2/convert/formats', headers=headers)
        if response.status_code == 200:
            data = response.json()
            input_format = os.path.splitext(file_path)[1][1:].lower()
            formats = [item['output_format'] for item in data['data'] if item['input_format'] == input_format]
            if formats:
                GLib.idle_add(callback, sorted(set(formats)), None)
            else:
                GLib.idle_add(callback, None, "No conversion formats available for this file type.")
        elif response.status_code == 401:
            GLib.idle_add(callback, None, "Invalid API key. Please change it.")
        else:
            GLib.idle_add(callback, None, "Failed to fetch formats.")

    def convert_file(self, file_path, output_format, callback):
        threading.Thread(target=self._convert_file_thread, args=(file_path, output_format, callback)).start()

    def _convert_file_thread(self, file_path, output_format, callback):
        headers = {
            'Authorization': f'Bearer {self.api_key}',
            'Content-Type': 'application/json',
        }
        job_data = {
            'tasks': {
                'import-my-file': {
                    'operation': 'import/upload'
                },
                'convert-my-file': {
                    'operation': 'convert',
                    'input': 'import-my-file',
                    'output_format': output_format
                },
                'export-my-file': {
                    'operation': 'export/url',
                    'input': 'convert-my-file'
                }
            }
        }
        response = requests.post('https://api.cloudconvert.com/v2/jobs', headers=headers, json=job_data)
        if response.status_code == 201:
            job = response.json()['data']
            job_id = job['id']
            upload_task = next(task for task in job['tasks'] if task['name'] == 'import-my-file')
            upload_url = upload_task['result']['form']['url']
            # Upload file
            with open(file_path, 'rb') as f:
                files = {'file': f}
                upload_response = requests.post(upload_url, files=files)
                if upload_response.status_code == 200:
                    # Monitor conversion
                    output_url = self._monitor_conversion(job_id)
                    if output_url:
                        GLib.idle_add(callback, output_url, None)
                    else:
                        GLib.idle_add(callback, None, "Conversion failed.")
                else:
                    GLib.idle_add(callback, None, "Failed to upload file.")
        else:
            GLib.idle_add(callback, None, "Failed to create conversion job.")

    def _monitor_conversion(self, job_id):
        headers = {
            'Authorization': f'Bearer {self.api_key}',
        }
        while True:
            response = requests.get(f'https://api.cloudconvert.com/v2/jobs/{job_id}', headers=headers)
            if response.status_code == 200:
                job = response.json()['data']
                status = job['status']
                if status == 'finished':
                    export_task = next(task for task in job['tasks'] if task['name'] == 'export-my-file')
                    output_url = export_task['result']['files'][0]['url']
                    return output_url
                elif status == 'error':
                    return None
            else:
                return None
            time.sleep(2)

    def download_file(self, output_url, file_path, output_format, callback):
        threading.Thread(target=self._download_file_thread, args=(output_url, file_path, output_format, callback)).start()

    def _download_file_thread(self, output_url, file_path, output_format, callback):
        response = requests.get(output_url, stream=True)
        if response.status_code == 200:
            output_dir = os.path.dirname(file_path)
            base_name = os.path.splitext(os.path.basename(file_path))[0]
            output_path = os.path.join(output_dir, f"{base_name}.{output_format}")
            with open(output_path, 'wb') as f:
                for chunk in response.iter_content(chunk_size=8192):
                    f.write(chunk)
            GLib.idle_add(callback, output_path, None)
        else:
            GLib.idle_add(callback, None, "Failed to download file.")
