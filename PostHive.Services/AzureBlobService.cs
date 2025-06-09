using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Threading.Tasks;
namespace PostHive.Services
{
    public class AzureBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public AzureBlobService(string connectionString)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task UploadFileAsync(string fileName, Stream fileStream, string contentType)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient("posthivecontainer");
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(fileName);
            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            };
            await blobClient.UploadAsync(fileStream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });
        }


        public async Task DownloadFileAsync(string containerName, string blobName, string downloadPath)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            BlobDownloadInfo download = await blobClient.DownloadAsync();
            using FileStream downloadFileStream = File.OpenWrite(downloadPath);
            await download.Content.CopyToAsync(downloadFileStream);
        }

        public async Task DeleteFileAsync(string containerName, string blobName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}
