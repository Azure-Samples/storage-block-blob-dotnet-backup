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
    /// Repostiory to store the back up files logs.
    /// It contains methods to store and read the the backup logs.
    /// </summary>
    public interface IStorageRepository
    {
        /// <summary>
        /// Inserts the blob event in storage
        /// </summary>
        /// <param name="blobEvent"></param>
        /// <returns></returns>
        Task InsertBLOBEvent(IEventData eventData);

        /// <summary>
        /// Returns the blob events data from storage basis weeknumber, start date and end date
        /// </summary>
        /// <param name="yearNumber"></param>
        /// <param name="weekNumber"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        Task<List<IEventData>> GetBLOBEvents(int yearNumber,int weekNumber, DateTime startDate, DateTime endDate);
    }
}
