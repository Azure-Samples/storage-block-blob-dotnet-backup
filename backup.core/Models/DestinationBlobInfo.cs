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
    /// Destination Blob Info
    /// </summary>
    public class DestinationBlobInfo
    {
        /// <summary>
        /// Blob Name
        /// </summary>
        public String BlobName { get; set; }

        /// <summary>
        /// Container Name
        /// </summary>
        public String ContainerName { get; set; }

        /// <summary>
        /// Original Blob Name
        /// </summary>
        public String OrgBlobName { get; set; }

        /// <summary>
        /// Original Container Name
        /// </summary>
        public String OrgContainerName { get; set; }

        /// <summary>
        /// Copy Reference Id. This can be used to track the copy status
        /// </summary>
        public string CopyReferenceId { get; set; }

    }
}
