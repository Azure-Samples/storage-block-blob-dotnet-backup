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
using System.Collections.Generic;

namespace backup.core.Implementations
{
    /// <summary>
    /// Restore Backup Worker
    /// </summary>
    public class RestoreBackupWorker: IRestoreBackup
    {
        private readonly ILogger<RestoreBackupWorker> _logger;

        private readonly IConfigurationRoot _config;

        private readonly IStorageRepository _storageRepository;

        private readonly IBlobRepository _blobRepository;

        /// <summary>
        /// Storage back up
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        public RestoreBackupWorker(IConfigurationRoot config, 
                             ILogger<RestoreBackupWorker> logger, 
                             IStorageRepository storageRepository,
                             IBlobRepository blobRepository)
        {
            _logger = logger;

            _config = config;

            _storageRepository = storageRepository;

            _blobRepository = blobRepository;
        }

        /// <summary>
        /// Run method
        /// 1: Reads the messgaes from the table storage in ascending order.
        /// 2: Peform the delete or create operation on the destination blob.
        /// </summary>
        /// <returns></returns>
        public async Task Run(DateTime startDate, DateTime endDate)
        {
            _logger.LogDebug("Inside RestoreBackupWorker.Run");

            //Since there can be many records around 84K for a day, let's read the records day by day and perform the restore operation

            List<Tuple<int,int, DateTime>> dates = GetDatesForDateRange(startDate, endDate);

            _logger.LogInformation($"Number of dates determined.---{dates.Count()}");

            int totalSuccessCount = 0;

            int totalFailureCount = 0;

            foreach (Tuple<int,int, DateTime> dateData in dates)
            {
                _logger.LogInformation($"Starting restore for Year {dateData.Item1} Week {dateData.Item2} and Date {dateData.Item3.ToString("MM/dd/yyyy")}");

                var blobEvents = await _storageRepository.GetBLOBEvents(dateData.Item1, dateData.Item2, dateData.Item3, dateData.Item3.AddDays(1));

                _logger.LogInformation($"Found {blobEvents.Count} for {dateData.Item1} and Date {dateData.Item3.ToString("MM/dd/yyyy")}");

                if (blobEvents != null && blobEvents.Count > 0)
                {
                    foreach(EventData eventData in blobEvents)
                    {
                        try
                        {
                            if (eventData.RecievedEventData is BlobEvent<CreatedEventData>)
                            {
                                BlobEvent<CreatedEventData> createdBlob = (BlobEvent<CreatedEventData>)eventData.RecievedEventData;

                                if (eventData.DestinationBlobInfo != null)
                                {
                                    _logger.LogInformation($"Going to perform copy as it is a created event {createdBlob.data.url}");

                                    await _blobRepository.CopyBlobFromBackupToRestore(eventData.DestinationBlobInfo);
                                }
                                else
                                {
                                    _logger.LogInformation($"Copy of the blob will be ignored as at the time of backup the blob was not present at source. One of the cause can be , a delete has been performed already on this blob. {createdBlob.data.url}");
                                }
                            }
                            else if (eventData.RecievedEventData is BlobEvent<DeletedEventData>)
                            {
                                BlobEvent<DeletedEventData> deletedBlob = (BlobEvent<DeletedEventData>)eventData.RecievedEventData;

                                _logger.LogInformation($"Going to delete as it is a deleted event {deletedBlob.data.url}");

                                await _blobRepository.DeleteBlobFromRestore(eventData.RecievedEventData);
                            }
                            else
                            {
                                _logger.LogInformation($"Currently Created and Deleted events are supported. Event Data: {eventData.RecievedEventDataJSON}");
                            }

                            totalSuccessCount++;
                        }
                        catch(Exception ex)
                        {
                            totalFailureCount++;
                            _logger.LogError($"Exception while restoring event {eventData.RecievedEventDataJSON}. Exception {ex.ToString()}");
                        }
                    }
                }
            }

            _logger.LogInformation($" Restore Success records count {totalSuccessCount}. Restore Failure records count {totalFailureCount}.");

            _logger.LogDebug("Completed RestoreBackupWorker.Run");
        }


        /// <summary>
        /// Returns all the dates between date range and with the corresponding week number.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="EndDate"></param>
        /// <returns></returns>
        public List<Tuple<int,int,DateTime>> GetDatesForDateRange(DateTime startDate, DateTime EndDate)
        {
            List<Tuple<int,int, DateTime>> dates = new List<Tuple<int,int, DateTime>>();

            Tuple<int,int, DateTime> dateData;

            EventDateDetails dateDetails;

            while (startDate <= EndDate)
            {
                dateDetails = new EventDateDetails(startDate);

                dateData = new Tuple<int,int, DateTime>(dateDetails.year, dateDetails.WeekNumber, startDate);

                dates.Add(dateData);

                startDate = startDate.AddDays(1);
            }

            return dates;
        }
    }
}
