using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DigitalCap.Persistence.Repositories
{
    public class BlobStorageRepository : IBlobStorageRepository
    {
        private readonly BlobContainerClient blobContainer;
        private readonly TimeSpan shareValidityDuration;
        private readonly string storageAccountName;
        private readonly string storageAccountKey;

        public BlobStorageRepository(
            BlobContainerClient blobContainer,
            string storageAccountName,
            string storageAccountKey,
            TimeSpan shareValidityDuration)
        {
            blobContainer = blobContainer;
            storageAccountName = storageAccountName;
            storageAccountKey = storageAccountKey;
            shareValidityDuration = shareValidityDuration;
        }

        public async Task<string> SaveFile(IFormFile file, string id = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                id = Guid.NewGuid().ToString();

            await blobContainer.CreateIfNotExistsAsync();

            var blobClient = blobContainer.GetBlobClient(id);

            var headers = new BlobHttpHeaders
            {
                ContentDisposition = file.ContentDisposition
            };

            await using var stream = file.OpenReadStream();

            var options = new BlobUploadOptions
            {
                HttpHeaders = headers
            };

            await blobClient.UploadAsync(stream, options);

            return id;
        }

        public async Task<string> GetFileDownloadUrl(string id)
        {
            var blobClient = blobContainer.GetBlobClient(id);

            if (!await blobClient.ExistsAsync())
                return null;

            var expiry = DateTimeOffset.UtcNow.Add(shareValidityDuration);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = blobContainer.Name,
                BlobName = id,
                Resource = "b",
                ExpiresOn = expiry
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasToken = sasBuilder.ToSasQueryParameters(
                new StorageSharedKeyCredential(storageAccountName, storageAccountKey)
            ).ToString();

            return $"{blobClient.Uri}?{sasToken}";
        }

        public async Task DeleteFile(string id)
        {
            var blobClient = blobContainer.GetBlobClient(id);
            await blobClient.DeleteIfExistsAsync();
        }

        public async Task<byte[]> GetContent(string id)
        {
            var blobClient = blobContainer.GetBlobClient(id);

            if (!await blobClient.ExistsAsync())
                return Array.Empty<byte>();

            var properties = await blobClient.GetPropertiesAsync();
            var content = new byte[properties.Value.ContentLength];

            await using var ms = new MemoryStream(content);
            await blobClient.DownloadToAsync(ms);
            return ms.ToArray();
        }

        public async Task<MemoryStream> GetZipAsync(IEnumerable<ProjectFile> files)
        {
            var ms = new MemoryStream();

            using (var zipOutputStream = new ZipOutputStream(ms))
            {
                zipOutputStream.IsStreamOwner = false;
                zipOutputStream.SetLevel(3);

                foreach (var file in files)
                {
                    var blobClient = blobContainer.GetBlobClient(file.StorageKey);

                    if (!await blobClient.ExistsAsync())
                        continue;

                    var entry = new ZipEntry(file.FullName);
                    zipOutputStream.PutNextEntry(entry);

                    await blobClient.DownloadToAsync(zipOutputStream);

                    zipOutputStream.CloseEntry();
                }

                zipOutputStream.Finish();
            }

            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }
}
