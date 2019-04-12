/*
 * Copyright 2019 marksman Contributors (https://github.com/Scope-IT/marksman)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Marksman.Types;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Diagnostics;
using SnipeSharp;
using SnipeSharp.Endpoints.Models;
using SnipeSharp.Endpoints.SearchFilters;

namespace Marksman
{
    /// <summary>
    /// 
    /// </summary>
    class Marksman
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Trace.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + ": Started application.");

            var debugTimer = new Stopwatch();
            System.Collections.Specialized.NameValueCollection appSettings = System.Configuration.ConfigurationManager.AppSettings;
            debugTimer.Start();

            SnipeItApi snipe = new SnipeItApi();
            snipe.ApiSettings.ApiToken = appSettings["API"];
            snipe.ApiSettings.BaseUrl = new Uri(appSettings["BaseURI"]);

            Sentry mySentry = new Sentry(appSettings); // creating new Sentry (we can have multiple for parallel execution at a later point)

            // Adding what we want
            mySentry.AddQuery("WMI", "SELECT Name, Manufacturer, Model, PCSystemType FROM Win32_ComputerSystem");
            mySentry.AddQuery("WMI", "SELECT IdentifyingNumber FROM Win32_ComputerSystemProduct");
            mySentry.AddQuery("WMI", "SELECT Name FROM Win32_BIOS");
            mySentry.AddQuery("WMI", "SELECT Manufacturer,Name,MACAddress FROM Win32_NetworkAdapter WHERE NetEnabled=true AND AdapterTypeId=0 AND netConnectionStatus=2");
            mySentry.AddQuery("WMI", "SELECT Manufacturer,Model,SerialNumber FROM Win32_DiskDrive");
            mySentry.AddQuery("WMI", "SELECT EndingAddress FROM Win32_MemoryArray");
            mySentry.AddQuery("WMI", "SELECT Name FROM Win32_DesktopMonitor");
            mySentry.AddQuery("WMI", "SELECT Manufacturer,Product,SerialNumber FROM Win32_BaseBoard");
            mySentry.AddQuery("WMI", "SELECT Name,NumberOfCores,NumberOfLogicalProcessors FROM Win32_Processor");

            bool getOU = false;
            bool getOUSuccess = Boolean.TryParse(appSettings["OUEnabled"], out getOU);
            if (getOUSuccess && getOU)
            {
                mySentry.AddQuery("Location", "OU");
            }
            else
            {
                mySentry.AddQuery("Location", "Config");
            }

            mySentry.Run();

            Asset currentAsset = mySentry.GetAsset(appSettings, snipe);
            Model currentModel = mySentry.GetModel(appSettings, snipe);
            Manufacturer currentManufacturer = mySentry.GetManufacturer(appSettings, snipe);
            Category currentCategory = mySentry.GetCategory(appSettings, snipe);
            Company currentCompany = mySentry.GetCompany(appSettings, snipe);
            StatusLabel currentStatusLabel = mySentry.GetStatusLabel(appSettings, snipe);
            Location currentLocation = mySentry.GetLocation(appSettings, snipe);

            //Broker.syncAsset(snipe, currentComputer);
            Broker snipeBroker = new Broker();
            bool connectionStatus = snipeBroker.CheckConnection(appSettings);

            if (connectionStatus)
            {
                snipeBroker.SyncAll(snipe, currentAsset, currentModel, currentManufacturer, currentCategory,
                                    currentCompany, currentStatusLabel, currentLocation);
            }
            else
            {
                Console.WriteLine("ERROR: Could not connect to SnipeIT database instance.");
                // Until a standardized logging framework is set up, quick way to make user see crash message.
                Console.ReadKey();
            }

            debugTimer.Stop();
            Trace.WriteLine("Total program execution time " + debugTimer.ElapsedMilliseconds + "ms.");
            Trace.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + ": Exiting application.");
            Trace.WriteLine(" ");
        }
    }
}
