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
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backup.core.Implementations
{
    /// <summary>
    /// Table Repository
    /// https://docs.microsoft.com/en-us/azure/visual-studio/vs-storage-aspnet5-getting-started-tables
    /// </summary>
    public class TableRepository : IStorageRepository
    {
        private readonly ILogger<TableRepository> _logger;

        private readonly IConfigurationRoot _config;

        /// <summary>
        /// Table Repository
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        public TableRepository(IConfigurationRoot config, ILogger<TableRepository> logger)
        {
            _config = config;

            _logger = logger;
        }

        /// <summary>
        /// Get Cloud Table
        /// </summary>
        /// <returns></returns>
        private CloudTable GetCloudTable()
        {
            string _storageAccountConnectionString = _config.GetConnectionString("BackupTableStorage"); 

            string _storageTableName = _config.GetSection("AppSettings")["BackupTableName"];

            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_storageAccountConnectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            //Get the table reference
            CloudTable table = tableClient.GetTableReference(_storageTableName);

            return table;
        }

        /// <summary>
        /// Returns the blob event basis weeknumber, startdate and endDate
        /// </summary>
        /// <param name="weekNumber"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public async Task<List<IEventData>> GetBLOBEvents(int year, int weekNumber, DateTime startDate, DateTime endDate)
        {
            //Get the table reference
            CloudTable employeeAuditTable = GetCloudTable();

            var startDateTimeTicks = string.Format("{0:D19}", startDate.Ticks) + "_" + "ID";

            var endDateTimeTicks = string.Format("{0:D19}", endDate.Ticks) + "_" + "ID";

            var whereCondition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, $"{year.ToString()}_{weekNumber.ToString()}");

            var lessThanCondition = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, endDateTimeTicks);

            var greaterThanCondition = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, startDateTimeTicks);

            string query = TableQuery.CombineFilters(whereCondition, TableOperators.And, lessThanCondition);

            query = TableQuery.CombineFilters(query, TableOperators.And, greaterThanCondition);

            TableQuery<EventData> rangeQuery = new TableQuery<EventData>().Where(query);

            List<IEventData> blobEvents = new List<IEventData>();

            // Print the fields for each customer.
            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<EventData> resultSegment = await employeeAuditTable.ExecuteQuerySegmentedAsync(rangeQuery, token);

                token = resultSegment.ContinuationToken;

                foreach (EventData entity in resultSegment.Results)
                {
                    if (!string.IsNullOrEmpty(entity.DestinationBlobInfoJSON))
                    {
                        entity.DestinationBlobInfo = JsonConvert.DeserializeObject<DestinationBlobInfo>(entity.DestinationBlobInfoJSON);
                    }

                    entity.RecievedEventData = IBlobEvent.ParseBlobEvent(entity.RecievedEventDataJSON);

                    blobEvents.Add(entity);
                }

            } while (token != null);

            return blobEvents;

        }

        /// <summary>
        /// Insert Blob Event
        /// </summary>
        /// <param name="blobEvent"></param>
        /// <returns></returns>
        public async Task InsertBLOBEvent(IEventData blobEvent)
        {
            CloudTable eventsTable = GetCloudTable();

            // Create the TableOperation object that inserts the customer entity.
            TableOperation insertOperation = TableOperation.Insert(blobEvent);

            // Execute the insert operation.
            await eventsTable.ExecuteAsync(insertOperation);

        }
    }
}
