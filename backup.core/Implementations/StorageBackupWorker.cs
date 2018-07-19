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
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;

namespace backup.core.Implementations
{
    /// <summary>
    /// Storage Backup
    /// </summary>
    public class StorageBackupWorker:IStorageBackup
    {
        private readonly ILogger<StorageBackupWorker> _logger;

        private readonly IConfigurationRoot _config;

        private readonly IStorageQueueRepository _queueRepository;

        private readonly IStorageRepository _storageRepository;

        private readonly IBlobRepository _blobRepository;

        /// <summary>
        /// Storage back up
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        public StorageBackupWorker(IConfigurationRoot config, 
                             ILogger<StorageBackupWorker> logger, 
                             IStorageQueueRepository queueRepository, 
                             IStorageRepository storageRepository,
                             IBlobRepository blobRepository)
        {
            _logger = logger;

            _config = config;

            _queueRepository = queueRepository;

            _storageRepository = storageRepository;

            _blobRepository = blobRepository;
        }

        /// <summary>
        /// Run method
        /// 1: Reads the messgaes from the queue.
        /// 2: Stores the messages to the table storage.
        /// 3: Deletes the messages from the queue
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            _logger.LogDebug("Inside StorageBackup.Run");

            var blobEvents = await _queueRepository.GetBLOBEvents();

            _logger.LogInformation($"Number of messages found in queue.---{blobEvents.Count()}");

            EventData eventData = null;

            string eventString = string.Empty;

            foreach (CloudQueueMessage blobEvent in blobEvents)
            {
                try
                {
                    eventString = blobEvent.AsString;

                    eventData = new EventData(eventString);

                    if (eventData.RecievedEventData != null)
                    {
                        //In case file has been added, copy the file from source storage to destination storage
                        if (eventData.RecievedEventData is BlobEvent<CreatedEventData>)
                        {
                            _logger.LogDebug($"Going to write to blob---{@eventString}");

                            DestinationBlobInfo destinationBlobInfo = await _blobRepository.CopyBlobFromSourceToBackup(eventData.RecievedEventData);

                            eventData.DestinationBlobInfo = destinationBlobInfo;

                            eventData.DestinationBlobInfoJSON  = JsonConvert.SerializeObject(destinationBlobInfo);

                            if (eventData.DestinationBlobInfo == null)
                            {
                                _logger.LogDebug($"DestinationBlobInfo is null. File not copied---{@eventString}");
                            }
                        }
                        else
                        {
                            _logger.LogDebug($"Skipping copying blob as it is not blob created event.---{@eventString}");
                        }

                        _logger.LogDebug($"Going to insert to storage---{@eventString}");

                        await _storageRepository.InsertBLOBEvent(eventData);

                        _logger.LogDebug($"Going to delete message from queue---{@eventString}");

                        //delete the message from queue after success insert only
                        await _queueRepository.DeleteBLOBEventAsync(blobEvent);
                        
                    }
                    else
                    {
                        _logger.LogDebug($"EventData.RecievedEventData is null. Currently the utility understands Created and Deleted Events only.---{@eventString}");
                    }
                }catch (Exception ex)
                {
                    _logger.LogError($"Error while inserting to storage repository for this event. Event should come back to queue.Exception : {@ex.ToString()} Event Data :{@eventString}");
                }
            }

            _logger.LogDebug("Completed StorageBackup.Run");
        }
    }
}
