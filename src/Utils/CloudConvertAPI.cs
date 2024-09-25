using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json.Linq;
using System.Net;

namespace AlkalineGTK.Utils
{
    public class CloudConvertApi
    {
        private readonly string _apiUrl = "https://api.cloudconvert.com/v2";
        private readonly string _apiToken;

        public CloudConvertApi()
        {
            _apiToken = Environment.GetEnvironmentVariable("CLOUDCONVERT_API_TOKEN");
            if (string.IsNullOrEmpty(_apiToken))
            {
                throw new InvalidOperationException("CloudConvert API token is not set.");
            }
            Console.WriteLine("CloudConvertApi initialized with API token.");
        }

        public async Task<List<string>> GetSupportedFormatsAsync(string filePath)
        {
            Console.WriteLine($"GetSupportedFormatsAsync called with filePath: {filePath}");

            var client = new RestClient(_apiUrl);
            var request = new RestRequest("formats", Method.Get);
            request.AddHeader("Authorization", $"Bearer {_apiToken}");

            Console.WriteLine($"Sending GET request to {_apiUrl}/formats");
            var response = await client.ExecuteAsync(request);
            Console.WriteLine($"Response status code: {response.StatusCode}");
            Console.WriteLine($"Response content: {response.Content}");

            if (response.IsSuccessful)
            {
                var json = JObject.Parse(response.Content);
                var formats = new List<string>();

                foreach (var format in json["data"])
                {
                    formats.Add(format["output"]["format"].ToString());
                }

                Console.WriteLine($"Parsed formats: {string.Join(", ", formats)}");
                return formats;
            }
            else
            {
                Console.WriteLine($"API request failed with status {response.StatusCode}: {response.Content}");
                throw new Exception($"API request failed with status {response.StatusCode}: {response.Content}");
            }
        }

        public async Task<bool> ConvertFileAsync(string inputFilePath, string outputFilePath, string targetFormat)
        {
            Console.WriteLine($"ConvertFileAsync called with inputFilePath: {inputFilePath}, outputFilePath: {outputFilePath}, targetFormat: {targetFormat}");

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

            Console.WriteLine($"Sending POST request to {_apiUrl}/jobs");
            var response = await client.ExecuteAsync(request);
            Console.WriteLine($"Response status code: {response.StatusCode}");
            Console.WriteLine($"Response content: {response.Content}");

            if (!response.IsSuccessful)
            {
                Console.WriteLine($"Failed to create conversion job: {response.StatusCode} - {response.Content}");
                throw new Exception($"Failed to create conversion job: {response.StatusCode} - {response.Content}");
            }

            var job = JObject.Parse(response.Content);
            var importTask = job["data"]["tasks"].First(t => t["name"].ToString() == "import")["result"]["form"];
            string uploadUrl = importTask["url"].ToString();
            string uploadToken = importTask["parameters"]["token"].ToString();

            Console.WriteLine($"Upload URL: {uploadUrl}");
            Console.WriteLine($"Upload token: {uploadToken}");

            // Upload the file
            var uploadClient = new RestClient(uploadUrl);
            var uploadRequest = new RestRequest(uploadUrl, Method.Post);
            uploadRequest.AddFile("file", inputFilePath);
            uploadRequest.AddParameter("token", uploadToken);

            Console.WriteLine($"Uploading file to {uploadUrl}");
            var uploadResponse = await uploadClient.ExecuteAsync(uploadRequest);
            Console.WriteLine($"Upload response status code: {uploadResponse.StatusCode}");
            Console.WriteLine($"Upload response content: {uploadResponse.Content}");

            if (!uploadResponse.IsSuccessful)
            {
                Console.WriteLine($"Failed to upload file: {uploadResponse.StatusCode} - {uploadResponse.Content}");
                throw new Exception($"Failed to upload file: {uploadResponse.StatusCode} - {uploadResponse.Content}");
            }

            // Wait for the job to complete
            while (true)
            {
                var statusRequest = new RestRequest($"jobs/{job["data"]["id"]}", Method.Get);
                statusRequest.AddHeader("Authorization", $"Bearer {_apiToken}");
                var statusResponse = await client.ExecuteAsync(statusRequest);

                Console.WriteLine($"Job status response status code: {statusResponse.StatusCode}");
                Console.WriteLine($"Job status response content: {statusResponse.Content}");

                if (!statusResponse.IsSuccessful)
                {
                    Console.WriteLine($"Failed to get job status: {statusResponse.StatusCode} - {statusResponse.Content}");
                    throw new Exception($"Failed to get job status: {statusResponse.StatusCode} - {statusResponse.Content}");
                }

                var statusJob = JObject.Parse(statusResponse.Content);
                string status = statusJob["data"]["status"].ToString();
                Console.WriteLine($"Job status: {status}");

                if (status == "finished")
                {
                    // Download the converted file
                    var exportTask = statusJob["data"]["tasks"].First(t => t["name"].ToString() == "export");
                    var fileUrl = exportTask["result"]["files"][0]["url"].ToString();
                    Console.WriteLine($"Downloading converted file from: {fileUrl}");
                    var fileResponse = await client.DownloadDataAsync(new RestRequest(fileUrl, Method.Get));

                    System.IO.File.WriteAllBytes(outputFilePath, fileResponse);
                    Console.WriteLine($"File saved to: {outputFilePath}");
                    return true;
                }
                else if (status == "error")
                {
                    Console.WriteLine("Conversion job failed.");
                    throw new Exception("Conversion job failed.");
                }

                // Wait before polling again
                await Task.Delay(2000);
            }
        }
    }
}