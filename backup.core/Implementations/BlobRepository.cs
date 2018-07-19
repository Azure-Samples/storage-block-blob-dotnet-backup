// 
// Copyright (c) Microsoft.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
using backup.core.Interfaces;
using backup.core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.DataMovement;

namespace backup.core.Implementations
{
    /// <summary>
    /// BlobRepository
    /// </summary>
    public class BlobRepository : IBlobRepository
    {
        private readonly ILogger<BlobRepository> _logger;

        private readonly IConfigurationRoot _config;

        /// <summary>
        /// Blob Repository
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        public BlobRepository(IConfigurationRoot config, ILogger<BlobRepository> logger)
        {
            _logger = logger;

            _config = config;
        }

        /// <summary>
        /// CopyBlobFromSourceToBackup
        /// </summary>
        /// <returns></returns>
        public async Task<DestinationBlobInfo> CopyBlobFromSourceToBackup(IBlobEvent eventData)
        {
            DestinationBlobInfo destinationBlobInfo = null;

            string destinationStorageAccountConnectionString = _config.GetConnectionString("BackupBlobStorage");

            string sourceStorageAccountConnectionString = _config.GetConnectionString("SourceBlobStorage");

            bool isServerCopy =  bool.Parse(_config.GetSection("AppSettings")["IsServerCopy"]);

            if (eventData is BlobEvent<CreatedEventData>)
            {
                // Retrieve the storage account from the connection string.
                CloudStorageAccount sourceStorageAccount = CloudStorageAccount.Parse(sourceStorageAccountConnectionString);

                CloudBlobClient sourceBlobClient = sourceStorageAccount.CreateCloudBlobClient();

                // Retrieve the storage account from the connection string.
                CloudStorageAccount destinationStorageAccount = CloudStorageAccount.Parse(destinationStorageAccountConnectionString);

                CloudBlobClient destinationBlobClient = destinationStorageAccount.CreateCloudBlobClient();

                BlobEvent<CreatedEventData> createdEventData = (BlobEvent<CreatedEventData>)eventData;

                string url = createdEventData.data.url;

                CloudBlockBlob sourceBlockBlob = new CloudBlockBlob(new Uri(url),sourceBlobClient);

                bool sourceBlobExists = await sourceBlockBlob.ExistsAsync();
                
                if (sourceBlobExists)
                {
                    long blobSize = sourceBlockBlob.Properties.Length;

                    EventDateDetails dateDetails = new EventDateDetails(createdEventData.eventTime);

                    string destinationContaninerName = dateDetails.year.ToString();

                    string destinationBlobName = $"wk{dateDetails.WeekNumber}/dy{(int)dateDetails.DayOfWeek}/{sourceBlockBlob.Container.Name}/{sourceBlockBlob.Name}";

                    CloudBlobContainer destinationContainer = destinationBlobClient.GetContainerReference(destinationContaninerName);

                    bool result = await destinationContainer.CreateIfNotExistsAsync();
                    
                    CloudBlockBlob destinationBlob = destinationContainer.GetBlockBlobReference(destinationBlobName);

                    string copyResult = string.Empty;

                    if (isServerCopy)
                    {
                        string blobToken = GenerateSASBlobToken(sourceBlockBlob);

                        _logger.LogInformation($"About to server copy {sourceBlockBlob.Name}. Blob size {sourceBlockBlob.Properties.Length} bytes");

                        copyResult = await destinationBlob.StartCopyAsync(new Uri(sourceBlockBlob.Uri.AbsoluteUri + blobToken));

                    }
                    else
                    {
                        _logger.LogInformation($"About to sync copy {sourceBlockBlob.Name}. Blob size {sourceBlockBlob.Properties.Length} bytes");

                        copyResult = "SYNCCOPY";

                        await TransferManager.CopyAsync(sourceBlockBlob, destinationBlob, false);
                    }
                   
                    destinationBlobInfo = new DestinationBlobInfo();

                    destinationBlobInfo.ContainerName = destinationContainer.Name;

                    destinationBlobInfo.BlobName = destinationBlobName;

                    destinationBlobInfo.CopyReferenceId = copyResult;

                    destinationBlobInfo.OrgContainerName = sourceBlockBlob.Container.Name;

                    destinationBlobInfo.OrgBlobName = sourceBlockBlob.Name;
                    
                    return destinationBlobInfo;
                }
                else
                {
                    _logger.LogInformation($"Not able to locate the block blob in source storage account---Block blob Name {sourceBlockBlob.Name}");
                }
            }
            else
            {
                _logger.LogInformation($"Input event data is not of Created Event Type.");
            }

            return destinationBlobInfo;
        }

        /// <summary>
        /// CopyBlobFromBackupToRestore
        /// </summary>
        /// <returns></returns>
        public async Task<string> CopyBlobFromBackupToRestore(DestinationBlobInfo backupBlob)
        {
            string destinationStorageAccountConnectionString = _config.GetConnectionString("RestoreBlobStorage");

            string sourceStorageAccountConnectionString = _config.GetConnectionString("BackupBlobStorage");

            bool isServerCopy = bool.Parse(_config.GetSection("AppSettings")["IsServerCopy"]);

            // Retrieve the storage account from the connection string.
            CloudStorageAccount sourceStorageAccount = CloudStorageAccount.Parse(sourceStorageAccountConnectionString);

            CloudBlobClient sourceBlobClient = sourceStorageAccount.CreateCloudBlobClient();

            // Retrieve the storage account from the connection string.
            CloudStorageAccount destinationStorageAccount = CloudStorageAccount.Parse(destinationStorageAccountConnectionString);

            CloudBlobClient destinationBlobClient = destinationStorageAccount.CreateCloudBlobClient();

            CloudBlobContainer sourceContainer = sourceBlobClient.GetContainerReference(backupBlob.ContainerName);

            bool sourceContainerExists = await sourceContainer.ExistsAsync();

            if (sourceContainerExists)
            {
                CloudBlockBlob sourceBlockBlob = sourceContainer.GetBlockBlobReference(backupBlob.BlobName);

                bool sourceBlobExists = await sourceBlockBlob.ExistsAsync();

                if (sourceBlobExists)
                {
                    CloudBlobContainer destinationContainer = destinationBlobClient.GetContainerReference(backupBlob.OrgContainerName);

                    await destinationContainer.CreateIfNotExistsAsync();

                    CloudBlockBlob destinationBlob = destinationContainer.GetBlockBlobReference(backupBlob.OrgBlobName);

                    string copyResult = string.Empty;

                    if (isServerCopy)
                    {
                        string blobToken = GenerateSASBlobToken(sourceBlockBlob);

                        _logger.LogInformation($"About to server copy {sourceBlockBlob.Name}. Blob size {sourceBlockBlob.Properties.Length} bytes");

                        copyResult = await destinationBlob.StartCopyAsync(new Uri(sourceBlockBlob.Uri.AbsoluteUri + blobToken));

                    }
                    else
                    {
                        _logger.LogInformation($"About to sync copy {sourceBlockBlob.Name}. Blob size {sourceBlockBlob.Properties.Length} bytes");

                        copyResult = "SYNCCOPY";

                        await TransferManager.CopyAsync(sourceBlockBlob, destinationBlob, false);
                    }

                    _logger.LogInformation($"Copy Scheduled. Source Blob Name: {backupBlob.BlobName} Destination Blob Name: {backupBlob.OrgBlobName} Copy Id {copyResult}.");

                    return copyResult;
                }
                else
                {
                    _logger.LogInformation($"Not able to locate the blob {backupBlob.BlobName} in source storage account.");
                }
            }
            else
            {
                _logger.LogInformation($"Not able to locate the container {backupBlob.ContainerName} in source storage account.");
            }

            return string.Empty;
        }

        /// <summary>
        /// Deletes the blob from the target / restore storage account
        /// </summary>
        /// <param name="eventData"></param>
        /// <returns></returns>
        public async Task<bool> DeleteBlobFromRestore(IBlobEvent eventData)
        {
            if (eventData is BlobEvent<DeletedEventData>)
            {
                string destinationStorageAccountConnectionString = _config.GetConnectionString("RestoreBlobStorage");

                // Retrieve the storage account from the connection string.
                CloudStorageAccount destinationStorageAccount = CloudStorageAccount.Parse(destinationStorageAccountConnectionString);

                CloudBlobClient destinationBlobClient = destinationStorageAccount.CreateCloudBlobClient();

                BlobEvent<DeletedEventData> deletedEventData = (BlobEvent<DeletedEventData>)eventData;

                //creating sourceBlockBlob to get the container name and blob name
                CloudBlockBlob sourceBlockBlob = new CloudBlockBlob(new Uri(deletedEventData.data.url));

                CloudBlobContainer destinationContainer = destinationBlobClient.GetContainerReference(sourceBlockBlob.Container.Name);

                bool destinationContainerExists = await destinationContainer.ExistsAsync();

                if (destinationContainerExists)
                {
                    CloudBlockBlob blockBlobToDelete = destinationContainer.GetBlockBlobReference(sourceBlockBlob.Name);

                    bool blobDeleted = await blockBlobToDelete.DeleteIfExistsAsync();

                    if (blobDeleted)
                    {
                        _logger.LogInformation($"Successfully able to delete the blob {sourceBlockBlob.Name} in destination (restore) storage account.");
                    }
                    else
                    {
                        _logger.LogInformation($"Failed to delete blob {sourceBlockBlob.Name} in destination (restore) storage account.");
                    }

                    return blobDeleted;
                }
                else
                {
                    _logger.LogInformation($"Not able to locate the container {sourceBlockBlob.Container.Name} in destination (restore) storage account.");
                }
            }

            return false;
        }
        
        /// <summary>
        /// GenerateSASBlobToken
        /// </summary>
        /// <param name="sourceBlockBlob"></param>
        /// <returns></returns>
        private string GenerateSASBlobToken(CloudBlockBlob sourceBlockBlob)
        {
            int blobSASExpiryInMinutes = int.Parse(_config.GetSection("AppSettings")["BlobSASExpiryInMts"]);

            //Set the expiry time and permissions for the blob.
            //In this case, the start time is specified as a few minutes in the past, to mitigate clock skew.
            //The shared access signature will be valid immediately.
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();

            sasConstraints.SharedAccessStartTime = DateTimeOffset.UtcNow.AddMinutes(-5);

            sasConstraints.SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddMinutes(blobSASExpiryInMinutes);

            sasConstraints.Permissions = SharedAccessBlobPermissions.Read;

            //Generate the shared access signature on the blob, setting the constraints directly on the signature.
            return sourceBlockBlob.GetSharedAccessSignature(sasConstraints);
        }
    }
}
