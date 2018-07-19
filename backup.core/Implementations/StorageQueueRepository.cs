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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace backup.core.Implementations
{
    /// <summary>
    /// Storage Queue Repository
    /// </summary>
    public class StorageQueueRepository: IStorageQueueRepository
    {
        private readonly ILogger<StorageQueueRepository> _logger;

        private readonly IConfigurationRoot _config;

        /// <summary>
        /// StorageQueueRepository
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        public StorageQueueRepository(IConfigurationRoot config, ILogger<StorageQueueRepository> logger)
        {
            _logger = logger;

            _config = config;
        }

        /// <summary>
        /// GetCloudQueue
        /// </summary>
        /// <returns></returns>
        private CloudQueue GetCloudQueue()
        {
            string _storageAccountConnectionString = _config.GetConnectionString("EventQueueStorage");

            string _storageQueueName = _config.GetSection("AppSettings")["EventQueueName"];

            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_storageAccountConnectionString);

            // Create the table client.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            //Get the table reference
            CloudQueue queue = queueClient.GetQueueReference(_storageQueueName);

            return queue;
        }

        /// <summary>
        /// Returns blob events
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<CloudQueueMessage>> GetBLOBEvents()
        {
            int queueVisibilityTimeOutInMS = int.Parse(_config.GetSection("AppSettings")["QueueVisibilityTimeOutInMS"]);

            int queueMessageCountToRead = int.Parse(_config.GetSection("AppSettings")["QueueMessageCountToRead"]);

            CloudQueue queue = GetCloudQueue();

            QueueRequestOptions options = new QueueRequestOptions();

            return await queue.GetMessagesAsync(queueMessageCountToRead, new TimeSpan(0, 0, 0, 0, queueVisibilityTimeOutInMS),null,null);
        }

        /// <summary>
        /// DeleteBLOBEventAsync
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task DeleteBLOBEventAsync(CloudQueueMessage message)
        {
            CloudQueue queue = GetCloudQueue();

            await queue.DeleteMessageAsync(message);

        }
    }
}
