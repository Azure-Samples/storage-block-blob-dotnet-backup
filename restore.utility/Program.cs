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
using backup.core.Implementations;
using backup.core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Serilog;
using System;
using System.Threading.Tasks;

namespace restore.utility
{
    class Program
    {
        private static ServiceProvider _serviceProvider;

        /// <summary>
        /// Main for restore.
        /// </summary>
        /// <param name="args"></param>
        static async Task Main(string[] args)
        {
            try
            {
                if (args == null || args.Length < 2)
                {
                    Console.WriteLine("In order to restore the backup please provide start and end date. Date format is mm/dd/yyyy!");

                    Console.ReadKey();

                    return;
                }

                DateTime startDate = DateTime.MinValue;

                DateTime endDate = DateTime.MinValue;

                bool startDateParsed = false;

                bool endDateParsed = false;

                if (args.Length == 2)
                {
                    startDateParsed = DateTime.TryParse(args[0], out startDate);

                    endDateParsed = DateTime.TryParse(args[1], out endDate);
                }

                if (!startDateParsed || !endDateParsed)
                {
                    Console.WriteLine($"Unable to parse start and end date. Provide dates in mm/dd/yyyy format. Start date value {args[0]} End date value {args[1]}. ");

                    Console.ReadKey();

                    return;
                }

                if (startDate > endDate)
                {
                    Console.WriteLine($"Start date cannot be greater than end date.");

                    Console.ReadKey();

                    return;
                }

                Console.WriteLine($"Here are the captured values. Start date : {startDate.ToString("MM/dd/yyyy")} End date {endDate.ToString("MM/dd/yyyy")}.");

                Console.WriteLine($"Please enter \"Y\" to continue performing the restore'");

                string response = Console.ReadKey().Key.ToString();

                if (!response.ToLower().Equals("y"))
                {
                    Console.WriteLine($"Press any key to exit!");

                    Console.ReadKey();

                }

                //to do start the restore process.

                // Create service collection
                var serviceCollection = new ServiceCollection();

                ConfigureServices(serviceCollection);

                // Create service provider
                _serviceProvider = serviceCollection.BuildServiceProvider();

                var config = _serviceProvider.GetService<IConfigurationRoot>();

                var logger = _serviceProvider.GetService<ILogger<RestoreBackupWorker>>();

                IRestoreBackup restoreBackup = _serviceProvider.GetService<IRestoreBackup>();

                // Run the restore process
                await restoreBackup.Run(startDate, endDate);

                Console.WriteLine($"Press any key to exit!");

                Console.ReadLine();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"An exception occurred. {ex.ToString()}");
            }
        }

        /// <summary>
        /// Configure Configuration, Logger and Other Services in Dependency Injection
        /// </summary>
        /// <param name="serviceCollection"></param>
        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // add logging
            serviceCollection.AddSingleton(new LoggerFactory()
            .AddConsole()
            .AddSerilog());

            serviceCollection.AddLogging();

            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", false)
                .Build();

            Log.Logger = new LoggerConfiguration()
              .ReadFrom.Configuration(configuration)
              .CreateLogger();


            // Add access to generic IConfigurationRoot
            serviceCollection.AddSingleton(configuration);

            // Add services
            serviceCollection.AddTransient<IStorageRepository, TableRepository>();

            serviceCollection.AddTransient<IBlobRepository, BlobRepository>();

            serviceCollection.AddTransient<IRestoreBackup, RestoreBackupWorker>();
        }
    }
}
