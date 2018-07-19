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
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace backup.core.Interfaces
{
    /// <summary>
    /// CopyBlobData
    /// </summary>
    public interface IBlobRepository
    {
        /// <summary>
        /// Copy blobs from source to backup storage account.
        /// </summary>
        /// <param name="eventData"></param>
        /// <returns></returns>
        Task<DestinationBlobInfo> CopyBlobFromSourceToBackup(IBlobEvent eventData);

        /// <summary>
        /// Copy blobs from backup to restore storage account
        /// </summary>
        /// <param name="backupBlob"></param>
        /// <returns></returns>
        Task<string> CopyBlobFromBackupToRestore(DestinationBlobInfo backupBlob);

        /// <summary>
        /// Deletes the blob from restore storage account
        /// </summary>
        /// <param name="eventData"></param>
        /// <returns></returns>
        Task<bool> DeleteBlobFromRestore(IBlobEvent eventData);
    }
}
