# cloudconvert_api.py

import requests

class CloudConvertAPI:
    def __init__(self, config_manager):
        self.api_key = config_manager.get_api_key()
        self.base_url = "https://api.cloudconvert.com/v2"

    def set_api_key(self, api_key):
        self.api_key = api_key

    def convert_file(self, input_file_path, output_format):
        headers = {
            "Authorization": f"Bearer {self.api_key}",
            "Content-Type": "application/json",
        }

        # Create a new job
        job_data = {
            "tasks": {
                "import-my-file": {
                    "operation": "import/upload"
                },
                "convert-my-file": {
                    "operation": "convert",
                    "input": "import-my-file",
                    "output_format": output_format
                },
                "export-my-file": {
                    "operation": "export/url",
                    "input": "convert-my-file"
                }
            }
        }

        response = requests.post(f"{self.base_url}/jobs", json=job_data, headers=headers)
        if response.status_code != 201:
            print("Failed to create job:", response.text)
            return

        job = response.json()["data"]
        upload_task = next(task for task in job["tasks"] if task["name"] == "import-my-file")
        upload_url = upload_task["result"]["form"]["url"]

        # Upload the file
        with open(input_file_path, "rb") as f:
            files = {'file': f}
            upload_response = requests.post(upload_url, files=files)
            if upload_response.status_code not in (200, 201):
                print("Failed to upload file:", upload_response.text)
                return

        print("File conversion started. Check your CloudConvert dashboard for progress.")
