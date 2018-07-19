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
using System;
using System.Collections.Generic;
using System.Text;

namespace backup.core.Models
{
    /// <summary>
    /// Deleted Event Data
    /// </summary>
    public class DeletedEventData
    {
        /// <summary>
        /// api
        /// </summary>
        public string api { get; set; }

        /// <summary>
        /// requestid
        /// </summary>
        public string requestId { get; set; }

        /// <summary>
        /// contentType
        /// </summary>
        public string contentType { get; set; }

        /// <summary>
        /// blobType
        /// </summary>
        public string blobType { get; set; }

        /// <summary>
        /// url
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// sequencer
        /// </summary>
        public string sequencer { get; set; }

        /// <summary>
        /// storageDiagnostics
        /// </summary>
        public StorageDiagnostics storageDiagnostics { get; set; }

    }
}
