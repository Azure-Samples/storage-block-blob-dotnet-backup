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

namespace backup.core.Models
{
    /// <summary>
    /// Event representing the event recieved from BLOB Events
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BlobEvent<T> : IBlobEvent
    {
        /// <summary>
        /// topic
        /// </summary>
        public string topic { get; set; }

        /// <summary>
        /// subject
        /// </summary>
        public string subject { get; set; }

        /// <summary>
        /// eventType
        /// </summary>
        public string eventType { get; set; }

        /// <summary>
        /// eventTime
        /// </summary>
        public DateTime eventTime { get; set; }

        /// <summary>
        /// id
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// data
        /// </summary>
        public T data { get; set; }

        /// <summary>
        /// dataVersion
        /// </summary>
        public string dataVersion { get; set; }

        /// <summary>
        /// metadataVersion
        /// </summary>
        public string metadataVersion { get; set; }
    }
}
