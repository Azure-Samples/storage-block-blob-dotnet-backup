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
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace backup.core.Models
{
    /// <summary>
    /// Event Data
    /// </summary>
    public class EventData: IEventData
    {
        public EventData()
        {

        }

        /// <summary>
        /// Event recieved from storage Queue
        /// </summary>
        public IBlobEvent RecievedEventData { get; set; }

        /// <summary>
        /// Destination Blob Info. Populated only in case of CREATE
        /// </summary>
        public DestinationBlobInfo DestinationBlobInfo { get; set; }

        /// <summary>
        /// DestinationBlobInfoJSON
        /// Destination Blob Info. Populated only in case of CREATE
        /// </summary>
        public string DestinationBlobInfoJSON { get; set; }

        /// <summary>
        /// RecievedEventDataJSON
        /// Destination Blob Info. Populated only in case of CREATE
        /// </summary>
        public string RecievedEventDataJSON { get; set; }


        /// <summary>
        /// Event Data Constructor
        /// </summary>
        /// <param name="eventData"></param>
        public EventData(string eventData)
        {
            string eventId;

            DateTime eventDateTime;

            RecievedEventData = IBlobEvent.ParseBlobEvent(eventData, out eventId, out eventDateTime);

            if (RecievedEventData != null)
            {
                RecievedEventDataJSON = JsonConvert.SerializeObject(RecievedEventData);

                string partitionKey = string.Empty;

                string rowKey = string.Empty;

                EventDateDetails dateDetails = new EventDateDetails(eventDateTime);

                base.PartitionKey = $"{dateDetails.year}_{dateDetails.WeekNumber}";

                base.RowKey = $"{dateDetails.formattedDate}_{eventId.Replace("-", "")}";

            }
        }
    }
}
