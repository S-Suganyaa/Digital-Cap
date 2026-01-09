using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Persistence.Repositories
{
    public class BlobStorageRepository : IBlobStorageRepository
    {
        private readonly TimeSpan _shareValidityDuration;
        protected CloudBlobContainer BlobContainer { get; private set; }

        public async Task<string> SaveFile(IFormFile file, string id = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                id = Guid.NewGuid().ToString();

            await BlobContainer.CreateIfNotExistsAsync();

            var blockBlob = BlobContainer.GetBlockBlobReference(id);
            blockBlob.Properties.ContentDisposition = file.ContentDisposition;

            await using var fileStream = file.OpenReadStream();
            await blockBlob.UploadFromStreamAsync(fileStream);

            return id;
        }
        public async Task<string> GetFileDownloadUrl(string id)
        {
            var blockBlob = BlobContainer.GetBlockBlobReference(id);

            if (!await blockBlob.ExistsAsync())
                return null;

            var token = blockBlob.GetSharedAccessSignature(
                new SharedAccessBlobPolicy
                {
                    SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-1),
                    SharedAccessExpiryTime = DateTime.UtcNow.Add(_shareValidityDuration),
                    Permissions = SharedAccessBlobPermissions.Read
                });

            return blockBlob.Uri + token;
        }
        public async Task DeleteFile(string id)
        {
            var blockBlob = BlobContainer.GetBlockBlobReference(id);
            await blockBlob.DeleteIfExistsAsync();
        }

        public async Task<byte[]> GetContent(string id)
        {
            var blockBlob = BlobContainer.GetBlockBlobReference(id);

            if (!await blockBlob.ExistsAsync())
                return Array.Empty<byte>();

            await blockBlob.FetchAttributesAsync();

            var content = new byte[blockBlob.Properties.Length];
            await blockBlob.DownloadToByteArrayAsync(content, 0);

            return content;
        }
        public async Task<MemoryStream> GetZipAsync(
            IEnumerable<ProjectFile> files)
        {
            var ms = new MemoryStream();

            using (var zipOutputStream = new ZipOutputStream(ms))
            {
                zipOutputStream.IsStreamOwner = false;
                zipOutputStream.SetLevel(3);

                foreach (var file in files)
                {
                    var blob = BlobContainer.GetBlockBlobReference(file.StorageKey);

                    if (!await blob.ExistsAsync())
                        continue;

                    var entry = new ZipEntry(file.FullName);
                    zipOutputStream.PutNextEntry(entry);

                    await blob.DownloadToStreamAsync(zipOutputStream);

                    zipOutputStream.CloseEntry();
                }

                zipOutputStream.Finish();
            }

            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

    }
}
