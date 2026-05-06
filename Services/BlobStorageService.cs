using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EventEase.Services
{
    public class BlobStorageService
    {
        private readonly string _connectionString;
        private const string ContainerName = "venue-images";

        public BlobStorageService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AzureStorage");
        }

        public async Task<string> UploadImageAsync(Stream fileStream, string fileName)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;
            BlobClient blobClient = containerClient.GetBlobClient(uniqueFileName);
            await blobClient.UploadAsync(fileStream, true);

            return blobClient.Uri.ToString();
        }

        public async Task DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return;

            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);

            Uri uri = new Uri(imageUrl);
            string blobName = Path.GetFileName(uri.LocalPath);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}