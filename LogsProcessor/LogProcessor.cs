using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Blobs;


namespace LogsProcessor
{
    public static class LogProcessor
    {
        [FunctionName("SplitFile")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            string file = req.Query["f"];
            log.LogInformation($"Requesting file: {file}");


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            file = file ?? data?.file;

            string responseMessage = string.IsNullOrEmpty(file)
                ? $"Function called specifiying an empty file: {file}."
                : $"Function called specifiying the following file: {file}.";

            var bIsValid = await IsFileValid(file);

            return bIsValid ? new OkObjectResult(responseMessage) : new NotFoundObjectResult("File does not exists");
        }

        static async Task<bool>IsFileValid( string file)
        {
            // Replace with your connection string
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=logsidx;AccountKey=4E0PsICSSB5SdY+NiHICNL+6YGGEUlFE8uDyTqk0ISInQClwsQNGQ0WGsSHzm6vZ15SGgIGuiIcy+AStqT19+A==;EndpointSuffix=core.windows.net";
            // Replace with your container name
            string containerName = "logsdata";
            // Replace with your file name
            string fileName = file;

            // Create a blob container client
            BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);

            // Create a blob client
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            // Check if the blob exists
            bool exists = await blobClient.ExistsAsync();

            // Print the result
            Console.WriteLine($"The file {fileName} {(exists ? "exists" : "does not exist")} in the container {containerName}.");
            return exists;
        }
    }
}
