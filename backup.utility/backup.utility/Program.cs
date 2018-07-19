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
using backup.core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Timers;

namespace backup.utility
{
    /// <summary>
    /// Main class to start storage listener
    /// </summary>
    class Program
    {
        private static Timer _timer = null;

        private static ServiceProvider _serviceProvider;
        
        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task Main(string[] args)
        {
            // Create service collection
            var serviceCollection = new ServiceCollection();

            ConfigureServices(serviceCollection);

            // Create service provider
            _serviceProvider = serviceCollection.BuildServiceProvider();

            var config = _serviceProvider.GetService<IConfigurationRoot>();

            var logger = _serviceProvider.GetService<ILogger<StorageBackupWorker>>();

            _timer = new Timer(int.Parse(config.GetSection("AppSettings")["TimerElapsedInMS"]));

            _timer.Elapsed += OnTimerElapsed;

            _timer.Start();

            logger.LogInformation("Listener started!!!");

            Console.ReadLine();
        }

        /// <summary>
        /// On Timer Elapseed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async static void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            var logger = _serviceProvider.GetService<ILogger<StorageBackupWorker>>();

            try
            {
                _timer.Stop();

                logger.LogDebug("Inside timer elapsed.");

                //Get the storage back up provider
                IStorageBackup storageBackup = _serviceProvider.GetService<IStorageBackup>();

                // Run the storage process
                await storageBackup.Run();
            }
            catch(Exception ex)
            {
                logger.LogError($"Exception occurred in OnTimerElapsed . Exception : {@ex.ToString()}");
            }
            finally
            {
                _timer.Start();

                logger.LogDebug("Timer Started Again.");
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
            serviceCollection.AddTransient<IStorageBackup, StorageBackupWorker>();

            serviceCollection.AddTransient<IStorageQueueRepository, StorageQueueRepository>();

            serviceCollection.AddTransient<IStorageRepository, TableRepository>();

            serviceCollection.AddTransient<IBlobRepository, BlobRepository>();

        }

    }

}