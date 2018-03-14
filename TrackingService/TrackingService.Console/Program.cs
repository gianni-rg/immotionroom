namespace ImmotionAR.ImmotionRoom.TrackingService
{
    using System;
    using ControlApi;
    using Helpers;
    using Logger;
    using Logger.Log4Net;
    using Model;

    internal class Program
    {
        private static void Main(string[] args)
        {
            LoggerService.LoggerFactory = new LoggerFactory();
            LoggerService.Configuration = Configuration.LoadLoggerConfiguration();

            HelpersPlatformSetup();

            Console.WriteLine("Immotionar ImmotionRoom TrackingService - Version: " + AppVersions.RetrieveExecutableVersion());
            Console.WriteLine("Copyright (C) 2017-2018 Gianni Rosa Gallina.");
            Console.WriteLine("Copyright (C) 2014-2017 Immotionar.");
            Console.WriteLine();
            Console.WriteLine("This program is free software: you can redistribute it and/or modify\nit under the terms of the GNU General Public License as published by\nthe Free Software Foundation, either version 3 of the License, or\n(at your option) any later version.");
            Console.WriteLine();
            Console.WriteLine("This program is distributed in the hope that it will be useful,\nbut WITHOUT ANY WARRANTY; without even the implied warranty of\nMERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the\nGNU General Public License for more details.");
            Console.WriteLine();
            Console.WriteLine("You should have received a copy of the GNU General Public License\nalong with this program. If not, see <https://www.gnu.org/licenses/>.");
            Console.WriteLine();
            Console.WriteLine("Press 'q' key to stop...");
            Console.WriteLine();

            var currentConfiguration = Configuration.LoadConfigurationFromAppConfig();
            var dataSources = Configuration.LoadConfigurationItemFromFile<DataSourceCollection>("DataSources.txt");

            var trackingServiceFactory = new TrackingServiceFactory();

            var trackingService = trackingServiceFactory.Create(currentConfiguration, dataSources);
            var trackingServiceControlApiServer = new TrackingServiceControlApiServer(currentConfiguration);

            // NetworkConfigTool is required in order to enable Firewall rules
            if (!trackingServiceControlApiServer.Start())
            {
                return;
            }

            trackingService.Start().Wait();

            // Wait for exit command
            while (true)
            {
                var k = Console.ReadKey(true);

                if (k.Key == ConsoleKey.Q)
                {
                    break;
                }
            }

            trackingServiceControlApiServer.Stop();
            trackingService.Stop().Wait();
        }

        private static void HelpersPlatformSetup()
        {
            AppVersions.PlatformHelpers = new HelpersAppVersions();
            NetworkTools.PlatformHelpers = new HelpersNetworkTools();
            SystemManagement.PlatformHelpers = new HelpersSystemManagement();
            RegistryTools.PlatformHelpers = new HelpersRegistry();
        }
    }
}
