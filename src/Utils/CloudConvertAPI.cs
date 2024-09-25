using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace AlkalineGTK.Utils
{
    public class CloudConvertApi
    {
        private readonly string _apiUrl = "https://api.cloudconvert.com/v2"; // CloudConvert API base URL
        private readonly string _apiToken = Environment.GetEnvironmentVariable("CLOUDCONVERT_API_TOKEN") ?? "YOUR_CLOUDCONVERT_API_TOKEN"; // Securely retrieve your API token

        public CloudConvertApi()
        {
            if (_apiToken == "YOUR_CLOUDCONVERT_API_TOKEN")
            {
                throw new InvalidOperationException("Please set the CLOUDCONVERT_API_TOKEN environment variable with your CloudConvert API token.");
            }
        }

        /// <summary>
        /// Retrieves supported output formats for the given input file.
        /// </summary>
        /// <param name="filePath">Path to the input file.</param>
        /// <returns>List of supported output formats.</returns>
        public async Task<List<string>> GetSupportedFormatsAsync(string filePath)
        {
            var client = new RestClient(_apiUrl);
            var request = new RestRequest("formats", Method.Get);
            request.AddHeader("Authorization", $"Bearer {_apiToken}");

            var response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                var json = JObject.Parse(response.Content);
                var formats = new List<string>();

                foreach (var format in json["data"])
                {
                    // Assuming we want to list all possible output formats
                    formats.Add(format["output"]["format"].ToString());
                }

                return formats;
            }
            else
            {
                throw new Exception($"API request failed with status {response.StatusCode}: {response.Content}");
            }
        }

        /// <summary>
        /// Converts a file using CloudConvert API.
        /// </summary>
        /// <param name="inputFilePath">Path to the input file.</param>
        /// <param name="outputFilePath">Path where the converted file will be saved.</param>
        /// <param name="targetFormat">Desired output format.</param>
        /// <returns>Boolean indicating success or failure.</returns>
        public async Task<bool> ConvertFileAsync(string inputFilePath, string outputFilePath, string targetFormat)
        {
            var client = new RestClient(_apiUrl);
            var request = new RestRequest("jobs", Method.Post);
            request.AddHeader("Authorization", $"Bearer {_apiToken}");
            request.AddJsonBody(new
            {
                tasks = new
                {
                    import = new
                    {
                        operation = "import/upload"
                    },
                    convert = new
                    {
                        operation = "convert",
                        input = "import",
                        output_format = targetFormat
                    },
                    export_ = new
                    {
                        operation = "export/url",
                        input = "convert"
                    }
                }
            });

            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
            {
                throw new Exception($"Failed to create conversion job: {response.StatusCode} - {response.Content}");
            }

            var job = JObject.Parse(response.Content);
            var importTask = job["data"]["tasks"].First(t => t["name"].ToString() == "import")["result"]["form"];
            string uploadUrl = importTask["url"].ToString();
            string uploadToken = importTask["parameters"]["token"].ToString();

            // Upload the file
            var uploadClient = new RestClient(uploadUrl);
            var uploadRequest = new RestRequest(Method.Post);
            foreach (var file in System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(inputFilePath)))
            {
                uploadRequest.AddFile("file", inputFilePath);
                break; // Only upload the selected file
            }

            var uploadResponse = await uploadClient.ExecuteAsync(uploadRequest);
            if (!uploadResponse.IsSuccessful)
            {
                throw new Exception($"Failed to upload file: {uploadResponse.StatusCode} - {uploadResponse.Content}");
            }

            // Wait for the job to complete
            while (true)
            {
                var statusRequest = new RestRequest($"jobs/{job["data"]["id"]}", Method.Get);
                statusRequest.AddHeader("Authorization", $"Bearer {_apiToken}");
                var statusResponse = await client.ExecuteAsync(statusRequest);

                if (!statusResponse.IsSuccessful)
                {
                    throw new Exception($"Failed to get job status: {statusResponse.StatusCode} - {statusResponse.Content}");
                }

                var statusJob = JObject.Parse(statusResponse.Content);
                string status = statusJob["data"]["status"].ToString();

                if (status == "finished")
                {
                    // Download the converted file
                    var exportTask = statusJob["data"]["tasks"].First(t => t["name"].ToString() == "export");
                    var fileUrl = exportTask["result"]["files"][0]["url"].ToString();
                    var fileResponse = await client.DownloadDataAsync(new RestRequest(fileUrl, Method.Get));

                    System.IO.File.WriteAllBytes(outputFilePath, fileResponse);
                    return true;
                }
                else if (status == "error")
                {
                    throw new Exception("Conversion job failed.");
                }

                // Wait before polling again
                await Task.Delay(2000);
            }
        }
    }
}