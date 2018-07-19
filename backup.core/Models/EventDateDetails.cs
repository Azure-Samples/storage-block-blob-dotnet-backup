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
using System.Globalization;
using System.Text;

namespace backup.core.Models
{
    /// <summary>
    /// Event Date Details
    /// https://msdn.microsoft.com/en-us/library/system.globalization.calendarweekrule.aspx
    /// https://blogs.msdn.microsoft.com/shawnste/2006/01/24/iso-8601-week-of-year-format-in-microsoft-net/
    /// </summary>
    public class EventDateDetails
    {
        /// <summary>
        /// Event Date Time
        /// </summary>
        private DateTime _eventDateTime;

        /// <summary>
        /// EventDateDetails
        /// </summary>
        /// <param name="eventDateTime"></param>
        public EventDateDetails(DateTime eventDateTime)
        {
            _eventDateTime = eventDateTime;
        }

        /// <summary>
        /// Formatted date
        /// </summary>
        public string formattedDate
        {
            get
            {
                return string.Format("{0:D19}", _eventDateTime.Ticks);
            }
        }

        /// <summary>
        /// Returns the year part of the year
        /// </summary>
        public int year
        {
            get
            {
                return  int.Parse(_eventDateTime.ToString("yyyy"));
            }
        }


        /// <summary>
        /// Day of the week
        /// </summary>
        public DayOfWeek DayOfWeek
        {
            get
            {
                return CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(_eventDateTime);
            }
        }

        /// <summary>
        /// Week Number
        /// </summary>
        public int WeekNumber
        {
            get
            {
                return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(_eventDateTime,
                                                                           CalendarWeekRule.FirstFullWeek,
                                                                           CultureInfo.InvariantCulture.DateTimeFormat.FirstDayOfWeek);
            }
        }
    }
}
