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
using backup.core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace backup.core.Interfaces
{
    public class IBlobEvent
    {
        /// <summary>
        /// Parse Blob Event
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="eventId"></param>
        /// <param name="eventDateTime"></param>
        /// <returns></returns>
        public static IBlobEvent ParseBlobEvent(string eventData, out string eventId, out DateTime eventDateTime)
        {
            JObject jsonEventData = JObject.Parse(eventData);

            string eventType = jsonEventData["eventType"].ToString();

            eventId = string.Empty; eventDateTime = DateTime.MinValue;

            if (string.Equals(eventType, Constants.Constants.BlobEventType.BLOBCREATED, StringComparison.InvariantCultureIgnoreCase))
            {
                BlobEvent<CreatedEventData> createdEvent = jsonEventData.ToObject<BlobEvent<CreatedEventData>>();

                eventId = createdEvent.id;

                eventDateTime = createdEvent.eventTime;

                return createdEvent;
            }
            else if (string.Equals(eventType, Constants.Constants.BlobEventType.BLOBDELETED, StringComparison.InvariantCultureIgnoreCase))
            {
                BlobEvent<DeletedEventData> deletedEvent = jsonEventData.ToObject<BlobEvent<DeletedEventData>>();

                eventId = deletedEvent.id;

                eventDateTime = deletedEvent.eventTime;

                return deletedEvent;
            }

            return null;
        }

        /// <summary>
        /// ParseBlobEvent
        /// </summary>
        /// <param name="eventData"></param>
        /// <returns></returns>
        public static IBlobEvent ParseBlobEvent(string eventData)
        {
            string eventId;

            DateTime eventDateTime;

            return ParseBlobEvent(eventData, out eventId, out eventDateTime);
        }
    }
}
